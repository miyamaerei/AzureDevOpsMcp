using AzureDevOpsMcpServer.Services;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure DevOps 工具类（已废弃，功能已合并到其他工具类）
/// </summary>
public class AzureDevOpsTool
{
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly IUserContext _userContext;

    public AzureDevOpsTool(IAzureDevOpsService azureDevOpsService, IUserContext userContext)
    {
        _azureDevOpsService = azureDevOpsService;
        _userContext = userContext;
    }
}
