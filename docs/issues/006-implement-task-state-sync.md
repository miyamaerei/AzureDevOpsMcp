# Issue #6: 状态同步到 Azure DevOps

## 父级

N/A - 新功能

## 需要构建的内容

实现任务完成时自动同步状态到 Azure DevOps。

### PRD 要求

根据 PRD 四状态模型：
- **当前任务**: 正在开发的任务
- **阻塞中**: 因外部依赖而暂停的任务
- **未实现**: 搁置的任务
- **归档**: 完成验证的任务（需要同步到 Azure DevOps）

### 状态映射

| 内部状态 | Azure DevOps 状态 |
|---------|------------------|
| 当前任务 | To Do / In Progress |
| 阻塞中 | To Do / In Progress |
| 未实现 | Removed |
| 归档 | Closed |

### 实现方案

创建状态同步服务：

```csharp
public interface ITaskSyncService
{
    Task<bool> SyncTaskToAzureDevOpsAsync(Guid taskId);
    Task<IEnumerable<TaskSyncRecord>> GetSyncHistoryAsync(Guid taskId);
    Task<bool> SyncAllPendingTasksAsync();
}

public class TaskSyncRecord
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Models.TaskStatus InternalStatus { get; set; }
    public string AzureDevOpsState { get; set; }
    public bool SyncSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SyncedAt { get; set; }
}
```

### 自动同步触发

在以下情况触发同步：
1. 任务状态变为"归档"
2. 定时同步（可配置间隔）
3. 手动触发同步

### 配置

```json
{
  "TaskSync": {
    "AutoSyncOnArchive": true,
    "SyncIntervalMinutes": 5,
    "RetryAttempts": 3
  }
}
```

## 验收标准

- [ ] 创建 TaskSyncRecord 数据模型
- [ ] 创建 ITaskSyncService 接口
- [ ] 实现状态同步逻辑
- [ ] 实现自动同步触发器
- [ ] 创建 SyncTaskTool MCP 工具
- [ ] 编写测试
- [ ] 更新文档

## 阻塞

Issue #1: 实现 GetTaskHistory 工具（需要记录历史）

## 优先级

高

## 来源

根据 PRD 用户故事 #28: "我希望任务完成时自动同步状态到 Azure DevOps，以便保持任务状态一致"
