using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

public class AzureDevOpsTool
{
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly IUserContext _userContext;

    public AzureDevOpsTool(IAzureDevOpsService azureDevOpsService, IUserContext userContext)
    {
        _azureDevOpsService = azureDevOpsService;
        _userContext = userContext;
    }

    [McpServerTool]
    [Description("获取指派给当前用户的任务列表")]
    public async Task<IEnumerable<TaskItem>> GetAssignedTasks(
        [Description("项目ID")] string? projectId = null)
    {
        // 自动获取当前用户
        var userId = await _userContext.GetCurrentAzureDevOpsUserAsync()
            ?? throw new InvalidOperationException("无法获取当前用户");
        
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentException("项目ID不能为空");
        }
        
        return await _azureDevOpsService.GetAssignedTasksAsync(userId, projectId);
    }

    [McpServerTool]
    [Description("更新任务状态")]
    public async Task<TaskItem?> UpdateTaskStatus(
        [Description("任务ID")] string taskId,
        [Description("任务状态 (NotStarted, InProgress, Blocked, Archived)")] string status)
    {
        if (!Enum.TryParse<Models.TaskStatus>(status, out var taskStatus))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }
        return await _azureDevOpsService.UpdateTaskStatusAsync(taskId, taskStatus);
    }

    [McpServerTool]
    [Description("获取任务详细信息")]
    public async Task<TaskItem?> GetTaskDetails(
        [Description("任务ID")] string taskId)
    {
        return await _azureDevOpsService.GetTaskDetailsAsync(taskId);
    }

    [McpServerTool]
    [Description("获取当前用户可访问的项目列表")]
    public async Task<IEnumerable<Project>> GetProjects()
    {
        // 自动获取当前用户
        var userId = await _userContext.GetCurrentAzureDevOpsUserAsync();
        
        return await _azureDevOpsService.GetProjectsAsync(userId ?? string.Empty);
    }

    [McpServerTool]
    [Description("获取任务状态变更历史")]
    public async Task<IEnumerable<TaskHistory>> GetTaskHistory(
        [Description("任务ID")] string taskId)
    {
        return await _azureDevOpsService.GetTaskHistoryAsync(taskId);
    }
}