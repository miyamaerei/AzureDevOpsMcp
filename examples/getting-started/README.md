# Azure DevOps MCP Server - 入门示例

## 概述

本示例展示如何使用 Azure DevOps MCP Server 进行任务管理。

## 快速开始

### 1. 启动 MCP Server

```powershell
# 进入项目目录
cd src/AzureDevOpsMcpServer

# 设置环境变量
$env:AZURE_DEVOPS_ORG_URL = "https://dev.azure.com/your-organization"
$env:AZURE_DEVOPS_PAT = "your-pat-token"
$env:MCP_TRANSPORT_MODE = "Http"
$env:MCP_HTTP_PORT = "5000"

# 启动服务
dotnet run
```

### 2. 配置客户端

#### Claude Code 配置

在 `.claude/settings.json` 中添加：

```json
{
  "mcpServers": [
    {
      "id": "azure-devops-mcp",
      "name": "Azure DevOps MCP Server",
      "url": "http://localhost:5000",
      "transport": "http",
      "authType": "windows"
    }
  ]
}
```

#### Cursor 配置

在 `.cursor/mcp.json` 中添加：

```json
{
  "servers": [
    {
      "id": "azure-devops-mcp",
      "url": "http://localhost:5000",
      "auth": {
        "type": "windows-integrated"
      }
    }
  ]
}
```

### 3. 使用 MCP 工具

```mcp
# 获取项目列表
GetProjects()

# 获取指派任务
GetAssignedTasks()

# 获取任务详情
GetTaskDetails(taskId: "12345")

# 更新任务状态
UpdateTaskStatus(taskId: "12345", status: "archived")

# 配置项目映射
SetProjectMapping(
    localProjectName: "MyProject",
    azureDevOpsProjectId: "abc123"
)

# 配置用户映射
SetUserMapping(
    windowsUsername: "DOMAIN\\user",
    azureDevOpsUser: "user@company.com"
)
```

## 完整示例

### 示例 1：获取并处理任务

```mcp
# 1. 获取指派给我的任务
GetAssignedTasks()

# 输出示例：
# ┌─────────┬───────────────────────┬──────────┐
# │ Task ID │ Title                 │ Status   │
# ├─────────┼───────────────────────┼──────────┤
# │ 12345   │ 实现用户登录功能       │ Current  │
# │ 12346   │ 修复数据同步 Bug      │ New      │
# └─────────┴───────────────────────┴──────────┘

# 2. 获取任务详情
GetTaskDetails(taskId: "12345")

# 3. 更新任务状态
UpdateTaskStatus(taskId: "12345", status: "archived")

# 4. 同步到 Azure DevOps
SyncTaskToAzureDevOps(workItemId: "12345")
```

### 示例 2：项目管理

```mcp
# 获取项目列表
GetProjects()

# 获取项目详情
GetProject(projectId: "abc123")

# 获取项目仓库
GetRepositories(projectId: "abc123")

# 获取仓库详情
GetRepository(projectId: "abc123", repositoryId: "def456")
```

### 示例 3：任务历史

```mcp
# 获取任务状态变更历史
GetTaskHistory(taskId: "12345")

# 获取同步历史
GetSyncHistory()
```

## 技能使用

### 开发工作流

```
# 使用任务工作流技能
/task-workflow

# 使用需求分析技能
/grill-with-docs

# 使用 TDD 开发技能
/tdd 实现用户登录功能

# 使用代码分析技能
/code-analysis impact src/services/auth.ts
```

## 环境变量参考

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `AZURE_DEVOPS_ORG_URL` | Azure DevOps 组织 URL | - |
| `AZURE_DEVOPS_PAT` | Azure DevOps PAT 令牌 | - |
| `MCP_TRANSPORT_MODE` | 传输模式（Stdio/Http） | Stdio |
| `MCP_HTTP_PORT` | HTTP 端口 | 5000 |
| `MCP_REQUIRE_AUTH` | 是否需要认证 | true |
| `TASK_SYNC_INTERVAL_MINUTES` | 同步间隔（分钟） | 5 |
| `TASK_SYNC_AUTO_ON_ARCHIVE` | 归档时自动同步 | true |

## 故障排查

### 常见问题

**Q: 无法连接到 MCP Server？**

```powershell
# 检查服务是否运行
dotnet run --project src/AzureDevOpsMcpServer

# 检查端口占用
netstat -ano | findstr :5000
```

**Q: 认证失败？**

```mcp
# 检查用户映射
GetAllUserMappings()

# 配置用户映射
SetUserMapping(
    windowsUsername: "DOMAIN\\user",
    azureDevOpsUser: "user@company.com"
)
```

**Q: 任务同步失败？**

```mcp
# 检查同步历史
GetSyncHistory()

# 手动同步
SyncTaskToAzureDevOps(workItemId: "12345")
```

## 更多资源

- [开发流程规范](../../docs/workflow-specification.md)
- [技能使用指南](../../docs/skill-usage-guide.md)
- [故障排查指南](../../docs/troubleshooting.md)
- [项目初始化指南](../../docs/initialization-guide.md)