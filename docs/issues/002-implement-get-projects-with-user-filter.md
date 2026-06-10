# Issue #2: 实现 GetProjects(userId) 按用户过滤

## 父级

N/A - 新功能

## 需要构建的内容

修改现有的 `GetProjects` 工具，使其支持按用户 ID 过滤可访问的项目。

### 当前问题

当前的 `GetProjects()` 无参方法返回组织下的所有项目，但根据 PRD，应该：
- `GetProjects(userId)` - 获取用户可访问的项目列表
- 仅返回该用户有权限访问的项目

### 端到端行为

1. 客户端调用 `GetProjects(userId)` 工具
2. MCP Server 查询 Azure DevOps 用户权限
3. 返回用户有权限访问的项目列表

### Azure DevOps API

使用 `TeamSecurityHttpClient` 或 Graph API 查询用户的项目访问权限。

```csharp
// 接口变更
Task<IEnumerable<Project>> GetProjectsAsync(string userId);
```

## 验收标准

- [ ] 修改 IAzureDevOpsApiService.GetProjectsAsync 接受 userId 参数
- [ ] 在 AzureDevOpsApiService 实现用户权限过滤
- [ ] 更新 ProjectRepositoryTool 的 GetProjects 方法
- [ ] 编写测试
- [ ] 更新文档

## 阻塞

无 - 可以立即开始

## 优先级

中

## 来源

根据 PRD MCP 工具定义: `GetProjects(userId)`：获取用户可访问的项目列表
