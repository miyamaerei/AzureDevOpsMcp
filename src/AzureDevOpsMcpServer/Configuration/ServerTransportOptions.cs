namespace AzureDevOpsMcpServer.Configuration;

/// <summary>
/// MCP 服务器传输配置
/// </summary>
public class ServerTransportOptions
{
    /// <summary>
    /// 传输模式: "Stdio" 或 "Http"
    /// </summary>
    public string TransportMode { get; set; } = "Stdio";
    
    /// <summary>
    /// HTTP 监听端口（仅 Http 模式）
    /// </summary>
    public int HttpPort { get; set; } = 5000;
    
    /// <summary>
    /// 是否使用 HTTPS
    /// </summary>
    public bool UseHttps { get; set; } = true;
    
    /// <summary>
    /// 是否需要认证（仅 Http 模式）
    /// </summary>
    public bool RequireAuthentication { get; set; } = true;
    
    /// <summary>
    /// HTTPS 证书路径（可选）
    /// </summary>
    public string? CertificatePath { get; set; }
    
    /// <summary>
    /// HTTPS 证书密码（可选）
    /// </summary>
    public string? CertificatePassword { get; set; }
}