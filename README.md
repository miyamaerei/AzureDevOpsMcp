# AI 辅助开发管理平台

一个基于 MCP（Model Context Protocol）的开发管理平台，提供标准化的 Azure DevOps 集成服务、开发流程规范和项目初始化能力。

## 功能特性

### 🔧 核心功能

- **Azure DevOps MCP Server**：基于 .NET Core 的 MCP 服务，提供任务管理能力
- **项目初始化脚本**：一键配置新项目开发环境
- **配置模板**：提供 MCP 配置、CONTEXT.md、.gitignore 等模板
- **流程规范文档**：定义标准的开发流程和最佳实践

### 🛠️ MCP 工具

| 工具 | 功能 |
|------|------|
| `GetAssignedTasks()` | 获取指派给用户的任务 |
| `GetTaskDetails()` | 获取任务详细信息 |
| `UpdateTaskStatus()` | 更新任务状态 |
| `GetProjects()` | 获取用户可访问的项目列表 |
| `GetTaskHistory()` | 获取任务状态变更历史 |
| `SetProjectMapping()` | 配置项目映射 |
| `SetUserMapping()` | 配置用户映射 |
| `SyncTaskToAzureDevOps()` | 同步任务状态到 Azure DevOps |

### 🔐 认证方式

- **Windows 集成认证**：自动识别域用户身份
- **用户映射**：Windows 用户与 Azure DevOps 用户映射

## 快速开始

### 1. 启动 MCP Server

```powershell
# 进入项目目录
cd src/AzureDevOpsMcpServer

# Stdio 模式（默认）
dotnet run

# HTTP 模式
$env:MCP_TRANSPORT_MODE = "Http"
$env:MCP_HTTP_PORT = "5000"
dotnet run
```

### 2. 初始化新项目

```powershell
# Windows
Invoke-Expression (Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -UseBasicParsing).Content

# Linux/Mac
curl -sSL https://platform.company.com/init-project.sh | bash
```

### 3. 使用 MCP 工具

```mcp
# 获取项目列表
GetProjects()

# 获取指派任务
GetAssignedTasks()

# 更新任务状态
UpdateTaskStatus(taskId: "123", status: "archived")
```

## 项目结构

```
test_dev_flow/
├─ src/
│  └─ AzureDevOpsMcpServer/       # MCP Server 源代码
├─ scripts/                        # 初始化脚本
│  ├─ Initialize-McpServer.ps1    # Windows 初始化脚本
│  └─ initialize-mcp-server.sh    # Linux/Mac 初始化脚本
├─ templates/                      # 配置模板
│  ├─ mcp-config.json             # MCP 配置模板
│  ├─ context-template.md         # CONTEXT.md 模板
│  ├─ gitignore-template          # .gitignore 模板
│  ├─ claude-settings-template.json
│  ├─ cursor-mcp-template.json
│  └─ project-mapping-template.json
├─ docs/                           # 文档
│  ├─ workflow-specification.md    # 开发流程规范
│  ├─ skill-usage-guide.md        # 技能使用指南
│  ├─ initialization-guide.md     # 项目初始化指南
│  └─ troubleshooting.md          # 故障排查指南
├─ tests/                          # 测试
└─ .agents/                        # 技能配置
```

## 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `MCP_TRANSPORT_MODE` | 传输模式（Stdio/Http） | Stdio |
| `MCP_HTTP_PORT` | HTTP 端口 | 5000 |
| `MCP_REQUIRE_AUTH` | 是否需要认证 | true |
| `AZURE_DEVOPS_PAT` | Azure DevOps PAT 令牌 | - |
| `TASK_SYNC_INTERVAL_MINUTES` | 同步间隔（分钟） | 5 |
| `TASK_SYNC_AUTO_ON_ARCHIVE` | 归档时自动同步 | true |

## 开发流程

```
初始化 → 配置项目映射 → 拉取任务 → 需求分析 → TDD 开发 → 代码分析 → 提交 → 同步状态
```

### 推荐技能

| 技能 | 用途 |
|------|------|
| `/grill-with-docs` | 需求分析 |
| `/tdd` | 测试驱动开发 |
| `/diagnose` | 结构化调试 |
| `/improve-codebase-architecture` | 架构优化 |
| `gitnexus impact()` | 代码影响分析 |

## 技术栈

- **MCP Server**: .NET Core 10.0 + Microsoft.Extensions.AI.Server
- **脚本语言**: PowerShell 5.1+, Bash
- **包管理**: npm
- **文档格式**: Markdown
- **配置格式**: JSON

## 测试

```powershell
# 运行所有测试
cd tests/AzureDevOpsMcpServer.Tests
dotnet test

# 测试覆盖率
dotnet test --collect:"XPlat Code Coverage"
```

## 部署

### 本地开发

```powershell
# 构建
dotnet build --configuration Release

# 运行
dotnet run --project src/AzureDevOpsMcpServer
```

### 生产部署

```powershell
# 发布
dotnet publish --configuration Release --output publish

# 运行
dotnet AzureDevOpsMcpServer.dll
```

## 文档

- [开发流程规范](docs/workflow-specification.md)
- [技能使用指南](docs/skill-usage-guide.md)
- [项目初始化指南](docs/initialization-guide.md)
- [故障排查指南](docs/troubleshooting.md)
- [分支策略](docs/branching-strategy.md)
- [提交规范](docs/commit-conventions.md)
- [开发流程](docs/development-process.md)

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

MIT License

## 联系方式

如有问题，请联系平台管理员。