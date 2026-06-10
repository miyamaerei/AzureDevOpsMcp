using AzureDevOpsMcpServer.Models;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 任务状态流转验证服务
/// 基于 PRD 定义的状态流转规则进行验证
/// </summary>
public interface ITaskStatusTransitionService
{
    /// <summary>
    /// 验证状态转换是否合法
    /// </summary>
    bool CanTransition(Models.TaskStatus currentStatus, Models.TaskStatus targetStatus);

    /// <summary>
    /// 获取状态转换的描述信息
    /// </summary>
    string GetTransitionDescription(Models.TaskStatus currentStatus, Models.TaskStatus targetStatus);

    /// <summary>
    /// 获取允许转换到的目标状态列表
    /// </summary>
    IEnumerable<Models.TaskStatus> GetAllowedTransitions(Models.TaskStatus currentStatus);

    /// <summary>
    /// 验证状态转换并抛出异常（如果不合法）
    /// </summary>
    void ValidateTransition(Models.TaskStatus currentStatus, Models.TaskStatus targetStatus);
}

/// <summary>
/// 任务状态流转验证服务实现
/// 
/// PRD 定义的状态流转规则：
/// 1. 任务从 Azure DevOps 拉取后进入"当前任务"
/// 2. 当前任务可转为"阻塞中"或回退至"未实现"
/// 3. 任务完成后进入"归档"状态并自动同步到 Azure DevOps
/// 
/// 四状态任务模型：
/// - 当前任务(Current)：正在积极开发中的任务
/// - 阻塞中(Blocked)：因外部依赖或问题无法继续推进的任务
/// - 未实现(NotImplemented)：尚未开始或被搁置的任务
/// - 归档(Archived)：已完成所有验证步骤的任务
/// </summary>
public class TaskStatusTransitionService : ITaskStatusTransitionService
{
    private static readonly Dictionary<Models.TaskStatus, HashSet<Models.TaskStatus>> _transitionMatrix = new()
    {
        {
            Models.TaskStatus.NotImplemented,
            new HashSet<Models.TaskStatus>
            {
                Models.TaskStatus.NotImplemented,
                Models.TaskStatus.Current,
                Models.TaskStatus.Blocked,
                Models.TaskStatus.Archived
            }
        },
        {
            Models.TaskStatus.Current,
            new HashSet<Models.TaskStatus>
            {
                Models.TaskStatus.Current,
                Models.TaskStatus.Blocked,
                Models.TaskStatus.NotImplemented,
                Models.TaskStatus.Archived
            }
        },
        {
            Models.TaskStatus.Blocked,
            new HashSet<Models.TaskStatus>
            {
                Models.TaskStatus.Blocked,
                Models.TaskStatus.Current,
                Models.TaskStatus.NotImplemented,
                Models.TaskStatus.Archived
            }
        },
        {
            Models.TaskStatus.Archived,
            new HashSet<Models.TaskStatus>
            {
                Models.TaskStatus.Archived,
                Models.TaskStatus.Current,
                Models.TaskStatus.NotImplemented,
                Models.TaskStatus.Blocked
            }
        }
    };

    private static readonly Dictionary<(Models.TaskStatus, Models.TaskStatus), string> _transitionDescriptions = new()
    {
        { (Models.TaskStatus.NotImplemented, Models.TaskStatus.Current), "开始任务开发" },
        { (Models.TaskStatus.NotImplemented, Models.TaskStatus.Blocked), "任务因依赖问题被阻塞" },
        { (Models.TaskStatus.NotImplemented, Models.TaskStatus.Archived), "任务被取消或搁置" },
        
        { (Models.TaskStatus.Current, Models.TaskStatus.Blocked), "任务因外部依赖或问题被阻塞" },
        { (Models.TaskStatus.Current, Models.TaskStatus.NotImplemented), "任务被搁置，回退至未实现状态" },
        { (Models.TaskStatus.Current, Models.TaskStatus.Archived), "任务完成并归档，将自动同步到 Azure DevOps" },
        
        { (Models.TaskStatus.Blocked, Models.TaskStatus.Current), "阻塞问题已解决，恢复任务开发" },
        { (Models.TaskStatus.Blocked, Models.TaskStatus.NotImplemented), "任务被搁置，回退至未实现状态" },
        { (Models.TaskStatus.Blocked, Models.TaskStatus.Archived), "阻塞任务被归档" },
        
        { (Models.TaskStatus.Archived, Models.TaskStatus.Current), "任务被重新激活" },
        { (Models.TaskStatus.Archived, Models.TaskStatus.NotImplemented), "归档任务被回退至未实现状态" },
        { (Models.TaskStatus.Archived, Models.TaskStatus.Blocked), "归档任务转为阻塞状态" }
    };

    public bool CanTransition(Models.TaskStatus currentStatus, Models.TaskStatus targetStatus)
    {
        if (!_transitionMatrix.TryGetValue(currentStatus, out var allowedTargets))
        {
            return false;
        }
        
        return allowedTargets.Contains(targetStatus);
    }

    public string GetTransitionDescription(Models.TaskStatus currentStatus, Models.TaskStatus targetStatus)
    {
        if (currentStatus == targetStatus)
        {
            return "保持当前状态";
        }
        
        var key = (currentStatus, targetStatus);
        return _transitionDescriptions.TryGetValue(key, out var description)
            ? description
            : "状态转换";
    }

    public IEnumerable<Models.TaskStatus> GetAllowedTransitions(Models.TaskStatus currentStatus)
    {
        if (_transitionMatrix.TryGetValue(currentStatus, out var allowedTargets))
        {
            return allowedTargets.ToList();
        }
        
        return Enumerable.Empty<Models.TaskStatus>();
    }

    public void ValidateTransition(Models.TaskStatus currentStatus, Models.TaskStatus targetStatus)
    {
        if (!CanTransition(currentStatus, targetStatus))
        {
            throw new InvalidOperationException(
                $"不允许从状态 '{currentStatus}' 转换到状态 '{targetStatus}'");
        }
    }
}