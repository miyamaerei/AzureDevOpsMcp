using AzureDevOpsMcpServer.Configuration;
using AzureDevOpsMcpServer.Services;
using AzureDevOpsMcpServer.Tools;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Get transport mode from environment
var transportMode = Environment.GetEnvironmentVariable("MCP_TRANSPORT_MODE") ?? "Stdio";

// Add database context
builder.Services.AddDbContext<AppDbContext>();

// Add Azure DevOps configuration from environment variables
var orgUrl = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG_URL") ?? string.Empty;
var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") ?? string.Empty;

// Get HTTP configuration
var httpPort = int.TryParse(Environment.GetEnvironmentVariable("MCP_HTTP_PORT"), out var port) ? port : 5000;
var useHttps = Environment.GetEnvironmentVariable("MCP_USE_HTTPS") == "true";
var requireAuth = Environment.GetEnvironmentVariable("MCP_REQUIRE_AUTH") != "false"; // Default true

// Create a single IMcpServerBuilder instance
var mcpServerBuilder = builder.Services.AddMcpServer();

// Configure transport based on mode
if (transportMode.Equals("Http", StringComparison.OrdinalIgnoreCase))
{
    // Use HTTP transport (port and HTTPS configured via Kestrel)
    mcpServerBuilder.WithHttpTransport();
}
else
{
    // Use stdio transport (default)
    mcpServerBuilder.WithStdioServerTransport();
}

// Register tools and services
if (!string.IsNullOrEmpty(orgUrl) && !string.IsNullOrEmpty(pat))
{
    builder.Services.AddSingleton(new AzureDevOpsConnection(orgUrl, pat));
    builder.Services.AddScoped<IAzureDevOpsApiService, AzureDevOpsApiService>();
    
    // Register MCP tools with real API
    mcpServerBuilder
        .WithTools<WorkItemTool>()
        .WithTools<ProjectRepositoryTool>()
        .WithTools<TaskHistoryTool>()
        .WithTools<ProjectMappingTool>()
        .WithTools<UserMappingTool>();
}
else
{
    // Fallback to mock service for testing
    builder.Services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
    
    mcpServerBuilder
        .WithTools<AzureDevOpsTool>()
        .WithTools<ProjectMappingTool>()
        .WithTools<UserMappingTool>();
}

// Add services
builder.Services.AddScoped<MappingService>();
builder.Services.AddScoped<UserMappingService>();
builder.Services.AddScoped<IUserContext, UserContext>();

// Configure HTTP authentication if using HTTP transport with auth required
if (transportMode.Equals("Http", StringComparison.OrdinalIgnoreCase) && requireAuth)
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();
    builder.Services.AddAuthorization();
}

var app = builder.Build();

// Initialize database and test data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    
    // Only initialize test data if using mock service
    var azureDevOpsService = scope.ServiceProvider.GetService<IAzureDevOpsService>();
    if (azureDevOpsService != null)
    {
        await ((AzureDevOpsService)azureDevOpsService).InitializeTestDataAsync();
    }
    
    // Initialize UserContext
    var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
    var currentUser = Environment.GetEnvironmentVariable("AZURE_DEVOPS_CURRENT_USER") 
                      ?? Environment.UserName;
    if (!string.IsNullOrEmpty(currentUser))
    {
        userContext.SetCurrentUser(currentUser);
    }
}

await app.RunAsync();