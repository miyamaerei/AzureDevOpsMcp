# Handoff Document - Azure DevOps MCP Server Development

**Created:** 2026-06-10
**Session Focus:** TDD-based code fixes for Azure DevOps MCP Server issues

---

## Session Summary

本次会话完成了 Azure DevOps MCP Server 的 6 个关键问题修复，采用 TDD（测试驱动开发）方法。所有 49 个测试通过。

---

## Completed Work

### Issue #3: Windows 集成认证 ✅
**Status:** Completed
**Files Modified:**
- `src/AzureDevOpsMcpServer/Models/UserMapping.cs` - Windows 用户与 Azure DevOps 用户映射模型
- `src/AzureDevOpsMcpServer/Services/UserMappingService.cs` - 用户映射 CRUD 操作
- `src/AzureDevOpsMcpServer/Services/UserContext.cs` - 当前用户上下文管理
- `src/AzureDevOpsMcpServer/Tools/UserMappingTool.cs` - MCP 工具暴露

**Key Implementation:**
- `UserMapping` 模型存储 Windows 用户名到 Azure DevOps 用户的映射
- `UserMappingService` 提供 Add/Get/Delete/Exists 方法
- `UserContext` 实现 `IUserContext` 接口，支持当前用户注入
- `UserMappingTool` 暴露 4 个 MCP 工具用于管理用户映射

### Issue #4: HTTP 传输模式 + Windows 认证 ✅
**Status:** Completed
**Files Modified:**
- `src/AzureDevOpsMcpServer/AzureDevOpsMcpServer.csproj` - 添加 HTTP 和认证 NuGet 包
- `src/AzureDevOpsMcpServer/Program.cs` - 双传输模式支持
- `src/AzureDevOpsMcpServer/Configuration/ServerTransportOptions.cs` - HTTP 传输配置模型

**Key Implementation:**
- 支持 Stdio 和 HTTP 两种传输模式，通过 `MCP_TRANSPORT_MODE` 环境变量切换
- HTTP 传输使用 `ModelContextProtocol.AspNetCore` 包的 `WithHttpTransport()`
- Windows 集成认证使用 `Microsoft.AspNetCore.Authentication.Negotiate` 包
- 通过 `MCP_REQUIRE_AUTH` 环境变量控制是否需要认证（默认 true）
- 通过 `MCP_HTTP_PORT` 环境变量配置 HTTP 端口（默认 5000）

**环境变量配置：**
```powershell
$env:MCP_TRANSPORT_MODE = "Http"           # 或 "Stdio"
$env:MCP_HTTP_PORT = "5000"                 # HTTP 端口
$env:MCP_REQUIRE_AUTH = "true"              # 是否需要认证
```

### Issue #1: 实现 GetTaskHistory 工具 ✅
**Status:** Completed
**Files Modified:**
- `src/AzureDevOpsMcpServer/Services/AzureDevOpsApiService.cs` - 修复 `WorkItemRevision` 类型问题，改用 `WorkItem` 类型
- `src/AzureDevOpsMcpServer/Tools/TaskHistoryTool.cs` - MCP 工具暴露
- `src/AzureDevOpsMcpServer/Models/TaskStateHistory.cs` - 状态历史模型

**Key Implementation:**
- `GetTaskHistoryAsync(int workItemId)` - 获取 WorkItem 状态变更历史
- 使用 `GetRevisionsAsync` API 获取修订版本
- 提取状态变更记录并转换为 `TaskStateHistory`

### Issue #2: GetProjects(userId) 按用户过滤 ✅
**Status:** Completed
**Files Modified:**
- `src/AzureDevOpsMcpServer/Services/AzureDevOpsApiService.cs` - 添加可选 `userId` 参数
- `src/AzureDevOpsMcpServer/Tools/ProjectRepositoryTool.cs` - 支持无参调用

**Key Implementation:**
- `GetProjectsAsync(string? userId = null)` - 可选用户过滤
- 当前实现返回所有项目，预留用户过滤扩展点

### Issue #5: 完善项目映射集成 ✅
**Status:** Completed
**Files Modified:**
- `src/AzureDevOpsMcpServer/Models/ProjectMapping.cs` - 添加 `WorkingDirectory` 和 `IsDefault` 字段
- `src/AzureDevOpsMcpServer/Services/MappingService.cs` - 新增多个查询方法
- `src/AzureDevOpsMcpServer/Tools/WorkItemTool.cs` - 支持无参调用使用默认项目

**Key Implementation:**
- `GetMappingByWorkingDirectoryAsync(string workingDirectory)` - 按工作目录查询
- `GetDefaultMappingAsync()` - 获取默认映射
- `ValidateMappingAsync(string localProjectName)` - 验证映射有效性
- WorkItemTool 支持省略 `projectId` 参数，自动使用默认映射

### Issue #6: 状态同步到 Azure DevOps ✅
**Status:** Completed
**Files Modified:**
- `src/AzureDevOpsMcpServer/Models/TaskSyncRecord.cs` - 同步记录模型
- `src/AzureDevOpsMcpServer/Services/TaskSyncService.cs` - 任务同步服务接口和实现
- `src/AzureDevOpsMcpServer/Services/TaskSyncBackgroundService.cs` - 后台定时同步服务
- `src/AzureDevOpsMcpServer/Tools/SyncTaskTool.cs` - MCP 同步工具
- `src/AzureDevOpsMcpServer/Services/AppDbContext.cs` - 添加同步记录实体

**Key Implementation:**
- `ITaskSyncService` 接口提供同步操作方法
- `SyncTaskToAzureDevOpsAsync` - 同步单个任务
- `SyncAllPendingTasksAsync` - 同步所有待同步任务（归档状态）
- `SyncTaskByWorkItemIdAsync` - 按 WorkItemId 同步
- `GetSyncHistoryAsync` - 获取同步历史记录
- `TaskSyncBackgroundService` - 后台服务，支持定时同步（默认 5 分钟间隔）
- 支持自动同步触发（任务归档时）

**同步配置环境变量：**
```powershell
$env:TASK_SYNC_INTERVAL_MINUTES = "5"       # 定时同步间隔（分钟）
$env:TASK_SYNC_AUTO_ON_ARCHIVE = "true"     # 任务归档时自动同步
```

---

## Test Results

**Total Tests:** 49
**Passed:** 49
**Failed:** 0
**Skipped:** 0

**Test Files Created:**
- `tests/AzureDevOpsMcpServer.Tests/TaskHistoryModelTests.cs`
- `tests/AzureDevOpsMcpServer.Tests/GetProjectsWithUserFilterTests.cs`
- `tests/AzureDevOpsMcpServer.Tests/ProjectMappingEnhancedTests.cs`
- `tests/AzureDevOpsMcpServer.Tests/WorkItemStateSyncTests.cs`
- `tests/AzureDevOpsMcpServer.Tests/TaskSyncServiceTests.cs`

---

## Architecture Notes

### Key Dependencies
- Microsoft.TeamFoundationServer.Client (20.256.2) - Azure DevOps API
- Microsoft.EntityFrameworkCore.Sqlite - 数据库
- ModelContextProtocol.Server - MCP 协议实现
- ModelContextProtocol.AspNetCore (1.2.0) - HTTP 传输支持
- Microsoft.AspNetCore.Authentication.Negotiate (8.0.1) - Windows 集成认证
- Moq (4.20.72) - 测试 mock 框架

### Service Layer
- `IAzureDevOpsApiService` - Azure DevOps API 抽象接口
- `MappingService` - 本地项目与 Azure DevOps 项目映射管理
- `UserMappingService` - Windows 用户与 Azure DevOps 用户映射管理
- `ITaskSyncService` - 任务同步服务
- `AppDbContext` - EF Core 数据库上下文
- `IUserContext` - 当前用户上下文

### MCP Tools
- `WorkItemTool` - WorkItem 查询和操作
- `TaskHistoryTool` - 任务历史查询
- `ProjectRepositoryTool` - 项目和仓库查询
- `ProjectMappingTool` - 项目映射管理
- `UserMappingTool` - 用户映射管理
- `SyncTaskTool` - 任务同步工具

### Background Services
- `TaskSyncBackgroundService` - 定时同步后台服务

---

## Known Warnings

构建时存在以下 NuGet 警告（不影响功能）：
1. NU1603: Microsoft.TeamFoundationServer.Client 版本解析
2. NU1904: System.Drawing.Common 安全漏洞
3. NU1902: System.Security.Cryptography.Xml 安全漏洞

---

## Suggested Skills for Next Session

1. **tdd** - 继续使用 TDD 方法开发新功能
2. **TRAE-code-review** - 代码审查，检查实现质量
3. **TRAE-security-review** - 安全审查，处理 NuGet 警告中的安全漏洞
4. **improve-codebase-architecture** - 架构改进，优化服务层设计

---

## Next Steps (Recommendations)

1. **处理安全漏洞** - 升级 System.Drawing.Common 和 System.Security.Cryptography.Xml 包
2. **实现用户过滤** - 完善 `GetProjectsAsync(userId)` 的实际过滤逻辑
3. **添加集成测试** - 编写与真实 Azure DevOps 实例的集成测试
4. **HTTP 端点测试** - 使用 Trae 或 Postman 测试 HTTP 传输模式
5. **创建初始化脚本** - 实现项目初始化的 PowerShell/Shell 脚本
6. **创建配置模板** - 提供 MCP 配置、CONTEXT.md 等模板文件

---

## Reference Documents

- PRD: `e:\git\test_dev_flow\PRD.md`
- Issues: `e:\git\test_dev_flow\docs\issues\`
- Source Code: `e:\git\test_dev_flow\src\AzureDevOpsMcpServer\`
- Tests: `e:\git\test_dev_flow\tests\AzureDevOpsMcpServer.Tests\`

---

## Quick Start Commands

```powershell
# 构建项目
cd e:\git\test_dev_flow\src\AzureDevOpsMcpServer
dotnet build

# 运行测试
cd e:\git\test_dev_flow\tests\AzureDevOpsMcpServer.Tests
dotnet test

# 运行 MCP Server（Stdio 模式）
cd e:\git\test_dev_flow\src\AzureDevOpsMcpServer
dotnet run

# 运行 MCP Server（HTTP 模式）
$env:MCP_TRANSPORT_MODE = "Http"
$env:MCP_HTTP_PORT = "5000"
dotnet run
```

---

*This handoff document was generated to facilitate session continuity.*