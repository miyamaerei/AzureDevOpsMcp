using AzureDevOpsMcpServer.Services;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure DevOps Project 和 Repository 工具类（已废弃，功能已合并到其他工具类）
/// </summary>
public class ProjectRepositoryTool
{
    private readonly IAzureDevOpsApiService _apiService;
    private readonly IUserContext? _userContext;

    public ProjectRepositoryTool(IAzureDevOpsApiService apiService, IUserContext? userContext = null)
    {
        _apiService = apiService;
        _userContext = userContext;
    }
}
