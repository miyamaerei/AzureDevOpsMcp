namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 当前用户上下文接口
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// 获取当前 Windows 用户名
    /// </summary>
    string? CurrentWindowsUsername { get; }
    
    /// <summary>
    /// 获取当前 Azure DevOps 用户（通过映射获取）
    /// </summary>
    Task<string?> GetCurrentAzureDevOpsUserAsync();
    
    /// <summary>
    /// 设置当前 Windows 用户名（通常由认证中间件设置）
    /// </summary>
    void SetCurrentUser(string windowsUsername);
}

/// <summary>
/// 当前用户上下文实现
/// </summary>
public class UserContext : IUserContext
{
    private readonly IUserMappingService _userMappingService;
    private string? _currentWindowsUsername;

    public UserContext(IUserMappingService userMappingService)
    {
        _userMappingService = userMappingService;
    }

    /// <summary>
    /// 获取当前 Windows 用户名
    /// </summary>
    public string? CurrentWindowsUsername => _currentWindowsUsername;

    /// <summary>
    /// 获取当前 Azure DevOps 用户（通过映射获取）
    /// </summary>
    public async Task<string?> GetCurrentAzureDevOpsUserAsync()
    {
        if (string.IsNullOrEmpty(_currentWindowsUsername))
        {
            return null;
        }
        
        return await _userMappingService.GetAzureDevOpsUserFromWindowsUserAsync(_currentWindowsUsername);
    }

    /// <summary>
    /// 设置当前 Windows 用户名
    /// </summary>
    public void SetCurrentUser(string windowsUsername)
    {
        _currentWindowsUsername = windowsUsername;
    }
}