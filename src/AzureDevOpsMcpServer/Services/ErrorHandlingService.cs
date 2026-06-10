using AzureDevOpsMcpServer.Exceptions;
using Microsoft.Extensions.Logging;
using System;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 错误处理服务
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// 处理异常并返回用户友好消息
    /// </summary>
    ErrorResponse HandleException(Exception ex);

    /// <summary>
    /// 记录错误日志
    /// </summary>
    void LogError(Exception ex, string context);
}

/// <summary>
/// 错误响应
/// </summary>
public class ErrorResponse
{
    public int ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErrorHandlingService : IErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;

    public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
    {
        _logger = logger;
    }

    public ErrorResponse HandleException(Exception ex)
    {
        return ex switch
        {
            AuthenticationException authEx => HandleAuthenticationException(authEx),
            AuthorizationException authzEx => HandleAuthorizationException(authzEx),
            ResourceNotFoundException notFoundEx => HandleNotFoundException(notFoundEx),
            ValidationException validationEx => HandleValidationException(validationEx),
            AzureDevOpsApiException apiEx => HandleAzureDevOpsApiException(apiEx),
            DatabaseOperationException dbEx => HandleDatabaseException(dbEx),
            McpServerException serverEx => HandleMcpServerException(serverEx),
            _ => HandleUnknownException(ex)
        };
    }

    public void LogError(Exception ex, string context)
    {
        var errorResponse = HandleException(ex);
        
        _logger.LogError(ex, "[{Context}] ErrorCode: {ErrorCode}, Message: {Message}", 
            context, errorResponse.ErrorCode, errorResponse.Message);
    }

    private ErrorResponse HandleAuthenticationException(AuthenticationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = "请检查您的认证凭据"
        };
    }

    private ErrorResponse HandleAuthorizationException(AuthorizationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = "请联系管理员获取访问权限"
        };
    }

    private ErrorResponse HandleNotFoundException(ResourceNotFoundException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = null
        };
    }

    private ErrorResponse HandleValidationException(ValidationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = null
        };
    }

    private ErrorResponse HandleAzureDevOpsApiException(AzureDevOpsApiException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = $"Azure DevOps API 返回错误: {ex.HttpStatusCode}"
        };
    }

    private ErrorResponse HandleDatabaseException(DatabaseOperationException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = "数据库操作失败，请稍后重试"
        };
    }

    private ErrorResponse HandleMcpServerException(McpServerException ex)
    {
        return new ErrorResponse
        {
            ErrorCode = ex.ErrorCode,
            Message = ex.UserMessage,
            Details = null
        };
    }

    private ErrorResponse HandleUnknownException(Exception ex)
    {
        return new ErrorResponse
        {
            ErrorCode = 500,
            Message = "服务器内部错误",
            Details = "请联系管理员查看详细日志"
        };
    }
}