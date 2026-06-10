# 领域语言定义

## 核心概念

### 任务 (Task)

代表一个工作项，对应 Azure DevOps 的 Work Item。

**属性**：
- `Id` - 任务唯一标识
- `AzureDevOpsId` - Azure DevOps Work Item ID
- `Title` - 任务标题
- `Status` - 任务状态（New/Current/Archived）
- `ProjectId` - 所属项目 ID

**状态转换**：
```
New → Current → Archived
     ↖     ↙
   Reopened ←
```

### 项目 (Project)

代表 Azure DevOps 项目。

**属性**：
- `Id` - 项目唯一标识
- `AzureDevOpsId` - Azure DevOps 项目 ID
- `Name` - 项目名称
- `Description` - 项目描述

### 项目映射 (Project Mapping)

本地项目与 Azure DevOps 项目的映射关系。

**属性**：
- `LocalProjectName` - 本地项目名称
- `AzureDevOpsProjectId` - Azure DevOps 项目 ID
- `WorkingDirectory` - 本地工作目录
- `IsDefault` - 是否默认映射

### 用户映射 (User Mapping)

Windows 用户与 Azure DevOps 用户的映射关系。

**属性**：
- `WindowsUsername` - Windows 用户名（格式：DOMAIN\username）
- `AzureDevOpsUser` - Azure DevOps 用户（格式：email）

## MCP 工具

### 任务管理工具

| 工具 | 功能 |
|------|------|
| `GetAssignedTasks()` | 获取指派给当前用户的任务 |
| `GetTaskDetails()` | 获取任务详细信息 |
| `UpdateTaskStatus()` | 更新任务状态 |
| `GetTaskHistory()` | 获取任务状态变更历史 |

### 项目管理工具

| 工具 | 功能 |
|------|------|
| `GetProjects()` | 获取项目列表 |
| `GetProject()` | 获取项目详情 |
| `GetRepositories()` | 获取项目仓库列表 |
| `GetRepository()` | 获取仓库详情 |

### 配置工具

| 工具 | 功能 |
|------|------|
| `SetProjectMapping()` | 配置项目映射 |
| `GetAllMappings()` | 获取所有项目映射 |
| `SetUserMapping()` | 配置用户映射 |
| `GetAllUserMappings()` | 获取所有用户映射 |

### 同步工具

| 工具 | 功能 |
|------|------|
| `SyncTaskToAzureDevOps()` | 同步任务状态到 Azure DevOps |
| `GetSyncHistory()` | 获取同步历史记录 |

## 技能

### 核心技能

| 技能 | 用途 |
|------|------|
| `/tdd` | 测试驱动开发 |
| `/grill-with-docs` | 需求分析 |
| `/diagnose` | 结构化调试 |
| `/improve-codebase-architecture` | 架构优化 |

### 平台技能

| 技能 | 用途 |
|------|------|
| `/task-workflow` | 任务开发工作流 |
| `/project-init` | 项目初始化 |
| `/code-analysis` | 代码分析 |
| `/configure-pat` | PAT 配置 |
| `/configure-project` | 项目配置 |

## 工作流

### 任务开发流程

```
1. 拉取任务 → GetAssignedTasks()
2. 需求分析 → /grill-with-docs
3. TDD 开发 → /tdd
4. 代码分析 → /code-analysis
5. 提交代码 → git commit
6. 更新状态 → UpdateTaskStatus()
7. 同步 → SyncTaskToAzureDevOps()
```

### 项目初始化流程

```
1. 运行初始化脚本
2. 配置 MCP 连接
3. 配置项目映射
4. 配置用户映射（可选）
5. 开始开发
```

## 术语表

| 术语 | 定义 |
|------|------|
| MCP | Model Context Protocol，模型上下文协议 |
| PAT | Personal Access Token，个人访问令牌 |
| Work Item | Azure DevOps 工作项 |
| Domain Account | Windows 域账号 |
| Sync | 任务状态同步 |
| Mapping | 项目/用户映射关系 |

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0 | 2024-01-15 | 初始版本 |