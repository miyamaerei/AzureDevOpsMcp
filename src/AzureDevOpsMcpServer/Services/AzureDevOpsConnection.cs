using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// Azure DevOps 连接管理服务
/// 负责创建和管理与 Azure DevOps 的连接
/// </summary>
public class AzureDevOpsConnection : IDisposable
{
    private VssConnection? _connection;
    private readonly string _organizationUrl;
    private readonly string _personalAccessToken;
    private bool _disposed;

    /// <summary>
    /// 创建 Azure DevOps 连接实例
    /// </summary>
    /// <param name="organizationUrl">组织 URL (例如: https://dev.azure.com/myorg)</param>
    /// <param name="personalAccessToken">个人访问令牌</param>
    public AzureDevOpsConnection(string organizationUrl, string personalAccessToken)
    {
        _organizationUrl = organizationUrl;
        _personalAccessToken = personalAccessToken;
    }

    /// <summary>
    /// 获取或创建 VssConnection 实例
    /// </summary>
    public VssConnection GetConnection()
    {
        if (_connection == null)
        {
            var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
        }
        return _connection;
    }

    /// <summary>
    /// 获取指定类型的客户端
    /// </summary>
    public T GetClient<T>() where T : VssHttpClientBase
    {
        return GetConnection().GetClient<T>();
    }

    /// <summary>
    /// 验证连接是否有效
    /// </summary>
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            var connection = GetConnection();
            await connection.ConnectAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
