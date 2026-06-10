using AzureDevOpsMcpServer.Exceptions;
using AzureDevOpsMcpServer.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureDevOpsMcpServer.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IErrorHandlingService _errorHandlingService;
    private readonly ILoggingService _loggingService;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        IErrorHandlingService errorHandlingService,
        ILoggingService loggingService)
    {
        _next = next;
        _errorHandlingService = errorHandlingService;
        _loggingService = loggingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _loggingService.LogError(ex, "GlobalExceptionMiddleware");

        var errorResponse = _errorHandlingService.HandleException(ex);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.ErrorCode;

        var response = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(response);
    }
}

/// <summary>
/// MCP 工具异常处理包装器
/// </summary>
public static class McpToolExceptionHandler
{
    public static T HandleToolException<T>(Func<T> operation, IErrorHandlingService errorHandlingService, ILoggingService loggingService)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "McpToolExceptionHandler");
            throw;
        }
    }

    public static async Task<T> HandleToolExceptionAsync<T>(Func<Task<T>> operation, IErrorHandlingService errorHandlingService, ILoggingService loggingService)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "McpToolExceptionHandler");
            throw;
        }
    }
}