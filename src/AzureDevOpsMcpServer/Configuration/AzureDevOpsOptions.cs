namespace AzureDevOpsMcpServer.Configuration;

/// <summary>
/// Azure DevOps 连接配置
/// </summary>
public class AzureDevOpsOptions
{
    /// <summary>
    /// Azure DevOps 组织 URL (例如: https://dev.azure.com/myorg)
    /// </summary>
    public string OrganizationUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 个人访问令牌 (PAT)
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// 默认项目名称
    /// </summary>
    public string DefaultProject { get; set; } = string.Empty;
}
