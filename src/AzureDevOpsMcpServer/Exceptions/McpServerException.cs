using System;

namespace AzureDevOpsMcpServer.Exceptions;

/// <summary>
/// MCP Server 基础异常类
/// </summary>
public abstract class McpServerException : Exception
{
    public int ErrorCode { get; }
    public string UserMessage { get; }

    protected McpServerException(string message, int errorCode, string userMessage, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }
}

/// <summary>
/// 认证异常
/// </summary>
public class AuthenticationException : McpServerException
{
    public AuthenticationException(string message, string userMessage, Exception? innerException = null)
        : base(message, 401, userMessage, innerException) { }
}

/// <summary>
/// 授权异常
/// </summary>
public class AuthorizationException : McpServerException
{
    public AuthorizationException(string message, string userMessage, Exception? innerException = null)
        : base(message, 403, userMessage, innerException) { }
}

/// <summary>
/// 资源未找到异常
/// </summary>
public class ResourceNotFoundException : McpServerException
{
    public ResourceNotFoundException(string message, string userMessage)
        : base(message, 404, userMessage) { }
}

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : McpServerException
{
    public ValidationException(string message, string userMessage)
        : base(message, 400, userMessage) { }
}

/// <summary>
/// Azure DevOps API 异常
/// </summary>
public class AzureDevOpsApiException : McpServerException
{
    public int HttpStatusCode { get; }

    public AzureDevOpsApiException(string message, int httpStatusCode, string userMessage, Exception? innerException = null)
        : base(message, 503, userMessage, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}

/// <summary>
/// 数据库操作异常
/// </summary>
public class DatabaseOperationException : McpServerException
{
    public DatabaseOperationException(string message, string userMessage, Exception? innerException = null)
        : base(message, 500, userMessage, innerException) { }
}