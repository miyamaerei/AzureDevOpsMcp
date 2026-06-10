# Issue #1: 实现 GetTaskHistory 工具

## 父级

N/A - 新功能

## 需要构建的内容

实现 `GetTaskHistory` MCP 工具，用于获取任务的状态变更历史记录。

根据 PRD 定义的任务历史记录模型，需要记录：
- 任务 ID (TaskId)
- 旧状态 (OldStatus)
- 新状态 (NewStatus)
- 变更时间 (ChangedAt)
- 变更人 (ChangedBy)

### 端到端行为

1. 客户端调用 `GetTaskHistory(taskId)` 工具
2. MCP Server 调用 Azure DevOps API 获取工作项的历史记录
3. 返回状态变更历史列表

### 数据模型

```csharp
public class TaskHistory
{
    public Guid Id { get; set; }
    public string WorkItemId { get; set; }      // Azure DevOps WorkItem ID
    public Models.TaskStatus OldStatus { get; set; }
    public Models.TaskStatus NewStatus { get; set; }
    public string ChangedBy { get; set; }        // 用户名或邮箱
    public DateTime ChangedAt { get; set; }
}
```

### Azure DevOps API

使用 `WorkItemTrackingHttpClient.GetRevisionsAsync()` 获取所有修订版本，然后提取状态变更记录。

## 验收标准

- [ ] 创建 TaskHistory 数据模型
- [ ] 在 IAzureDevOpsApiService 添加 GetTaskHistoryAsync 方法
- [ ] 在 AzureDevOpsApiService 实现该方法
- [ ] 创建 TaskHistoryTool MCP 工具
- [ ] 编写单元测试
- [ ] 更新 Program.cs 注册新工具

## 阻塞

无 - 可以立即开始

## 优先级

高

## 来源

根据 PRD 用户故事 #5: "我希望获取任务状态变更历史"
