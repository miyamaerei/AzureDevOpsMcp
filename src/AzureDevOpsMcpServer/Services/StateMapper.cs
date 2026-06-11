using TaskStatusEnum = AzureDevOpsMcpServer.Models.TaskStatus;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 状态映射器
/// 统一管理内部任务状态与 Azure DevOps 状态的相互转换
/// </summary>
public static class StateMapper
{
    /// <summary>
    /// 将 Azure DevOps 状态转换为内部任务状态
    /// </summary>
    /// <param name="azureState">Azure DevOps 状态字符串</param>
    /// <returns>内部任务状态</returns>
    public static TaskStatusEnum ToInternalStatus(string? azureState)
    {
        return azureState?.ToLowerInvariant() switch
        {
            "new" => TaskStatusEnum.NotImplemented,
            "to do" => TaskStatusEnum.NotImplemented,
            "active" => TaskStatusEnum.Current,
            "in progress" => TaskStatusEnum.Current,
            "resolved" => TaskStatusEnum.Current,
            "blocked" => TaskStatusEnum.Blocked,
            "closed" => TaskStatusEnum.Archived,
            "done" => TaskStatusEnum.Archived,
            "removed" => TaskStatusEnum.Archived,
            _ => TaskStatusEnum.NotImplemented
        };
    }

    /// <summary>
    /// 将内部任务状态转换为 Azure DevOps 状态
    /// </summary>
    /// <param name="internalStatus">内部任务状态</param>
    /// <returns>Azure DevOps 状态字符串</returns>
    public static string ToAzureDevOpsState(TaskStatusEnum internalStatus)
    {
        return internalStatus switch
        {
            TaskStatusEnum.NotImplemented => "New",
            TaskStatusEnum.Current => "Active",
            TaskStatusEnum.Blocked => "Active",
            TaskStatusEnum.Archived => "Closed",
            _ => "New"
        };
    }
}
