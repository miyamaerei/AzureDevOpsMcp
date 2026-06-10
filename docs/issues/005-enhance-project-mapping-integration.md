# Issue #5: 完善项目映射集成

## 父级

N/A - 新功能

## 需要构建的内容

增强项目映射功能，使其与 MCP 工具深度集成。

### 当前问题

当前的 ProjectMappingTool 实现了基本的 CRUD 操作，但没有与 WorkItem 查询深度集成。

### PRD 要求

1. **自动项目识别**: 根据当前工作目录自动识别本地项目
2. **映射验证**: 调用 MCP 工具时自动验证项目映射
3. **跨项目查询**: 支持在多个映射项目间查询

### 端到端行为

1. 用户调用 `GetAssignedTasks` 不带 projectId
2. MCP Server 自动检测当前工作目录
3. 根据工作目录查找对应的项目映射
4. 使用映射的 Azure DevOps 项目 ID 查询任务

### 增强接口

```csharp
public interface IProjectMappingService
{
    // 新增: 根据工作目录自动获取映射
    Task<ProjectMapping?> GetMappingByWorkingDirectoryAsync(string workingDirectory);
    
    // 增强: 验证映射是否有效
    Task<bool> ValidateMappingAsync(string localProjectName);
    
    // 增强: 获取当前用户的所有映射项目
    Task<IEnumerable<ProjectMapping>> GetUserMappingsAsync(string userId);
}
```

### 工作目录映射

```csharp
public class ProjectMapping
{
    // ... 现有字段 ...
    
    public string? WorkingDirectory { get; set; }  // 可选的工作目录路径
    public bool IsDefault { get; set; }            // 是否为默认项目
}
```

## 验收标准

- [ ] 添加 WorkingDirectory 和 IsDefault 字段
- [ ] 实现 GetMappingByWorkingDirectoryAsync 方法
- [ ] 修改 WorkItemTool 支持无参数调用
- [ ] 实现映射验证逻辑
- [ ] 编写测试
- [ ] 更新文档

## 阻塞

无 - 可以立即开始

## 优先级

中

## 来源

根据 PRD 用户故事 #18: "我希望配置项目与 Azure DevOps 项目的映射关系，以便正确拉取对应项目的任务"
