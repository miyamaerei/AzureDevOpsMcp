# 故障排查指南

## 概述

本文档提供开发管理平台常见问题的排查方法和解决方案。

## 目录

1. [MCP Server 问题](#mcp-server-问题)
2. [认证问题](#认证问题)
3. [初始化脚本问题](#初始化脚本问题)
4. [GitNexus 问题](#gitnexus-问题)
5. [Skills 问题](#skills-问题)
6. [项目映射问题](#项目映射问题)
7. [任务同步问题](#任务同步问题)
8. [日志分析](#日志分析)

---

## MCP Server 问题

### 问题：无法连接到 MCP Server

**现象**：
- MCP 工具调用失败
- 连接超时错误
- "Server not available" 错误

**排查步骤**：

1. **检查服务状态**：
   ```powershell
   # 检查服务是否运行
   dotnet run --project src/AzureDevOpsMcpServer
   ```

2. **检查端口占用**：
   ```powershell
   # Windows
   netstat -ano | findstr :5000

   # Linux/Mac
   lsof -i :5000
   ```

3. **检查防火墙设置**：
   - 确保 5000 端口已开放
   - 检查 Windows Defender 防火墙规则

4. **检查配置**：
   - 确认 `MCP_TRANSPORT_MODE` 设置为 `Http`
   - 确认 `MCP_HTTP_PORT` 配置正确

**解决方案**：
- 确保 MCP Server 正在运行
- 释放被占用的端口
- 配置防火墙规则允许入站连接

---

## 认证问题

### 问题：Windows 认证失败

**现象**：
- "Authentication failed" 错误
- "Unauthorized" 错误
- 无法获取当前用户

**排查步骤**：

1. **检查用户上下文**：
   ```mcp
   # 检查当前用户
   GetCurrentUser()
   ```

2. **检查用户映射**：
   ```mcp
   GetAllUserMappings()
   ```

3. **检查 Windows 用户**：
   ```powershell
   whoami
   ```

**解决方案**：
- 配置用户映射：
  ```mcp
  SetUserMapping(
      windowsUsername: "DOMAIN\\username",
      azureDevOpsUser: "user@company.com"
  )
  ```
- 确保使用域账号登录

### 问题：Azure DevOps API 认证失败

**现象**：
- PAT 令牌无效
- API 请求返回 401/403 错误

**排查步骤**：

1. **检查 PAT 权限**：
   - 确保 PAT 具有 Work Items 读写权限
   - 确保 PAT 未过期

2. **检查环境变量**：
   ```powershell
   $env:AZURE_DEVOPS_PAT
   ```

**解决方案**：
- 在 Azure DevOps 生成新的 PAT
- 设置环境变量：
  ```powershell
  $env:AZURE_DEVOPS_PAT = "your-pat-token"
  ```

---

## 初始化脚本问题

### 问题：脚本执行失败

**现象**：
- PowerShell 脚本无法执行
- Shell 脚本权限不足

**排查步骤**：

1. **检查执行策略**（Windows）：
   ```powershell
   Get-ExecutionPolicy
   ```

2. **检查脚本权限**（Linux/Mac）：
   ```bash
   ls -la scripts/init-project.sh
   ```

**解决方案**：

**Windows**：
```powershell
# 设置执行策略
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Linux/Mac**：
```bash
# 添加执行权限
chmod +x scripts/init-project.sh
```

### 问题：依赖安装失败

**现象**：
- Node.js 安装失败
- GitNexus 安装失败

**排查步骤**：

1. **检查网络连接**：
   ```bash
   ping npmjs.com
   ```

2. **检查 Node.js 版本**：
   ```bash
   node --version
   ```

**解决方案**：
- 确保网络可以访问 npm 仓库
- 升级 Node.js 到 ≥ 18.0.0

---

## GitNexus 问题

### 问题：代码索引失败

**现象**：
- `gitnexus analyze` 失败
- 代码上下文为空

**排查步骤**：

1. **检查 GitNexus 状态**：
   ```bash
   gitnexus status
   ```

2. **检查项目目录**：
   - 确保在 Git 仓库目录中
   - 确保有代码文件

**解决方案**：
```bash
# 重新运行索引
gitnexus analyze --force

# 检查索引状态
gitnexus index status
```

### 问题：context()/impact() 返回空

**现象**：
- `gitnexus context()` 返回空
- `gitnexus impact()` 返回空

**排查步骤**：

1. **检查索引是否完成**：
   ```bash
   gitnexus index status
   ```

2. **检查文件类型**：
   - GitNexus 支持的文件类型有限
   - 确保代码文件在支持列表中

**解决方案**：
```bash
# 重新索引
gitnexus analyze
```

---

## Skills 问题

### 问题：技能无法加载

**现象**：
- `/tdd`、`/grill-with-docs` 等技能不可用
- "Skill not found" 错误

**排查步骤**：

1. **检查技能安装**：
   ```bash
   skills list
   ```

2. **检查技能配置**：
   - 检查 `.skills` 目录
   - 检查技能锁文件

**解决方案**：
```bash
# 重新安装技能
npx skills@latest add mattpocock/skills

# 同步技能
skills sync
```

---

## 项目映射问题

### 问题：任务拉取为空

**现象**：
- `GetAssignedTasks()` 返回空列表
- 无法获取项目任务

**排查步骤**：

1. **检查项目映射**：
   ```mcp
   GetAllMappings()
   ```

2. **检查默认映射**：
   ```mcp
   GetDefaultMapping()
   ```

3. **检查用户权限**：
   - 确保用户有权访问 Azure DevOps 项目

**解决方案**：
```mcp
# 配置项目映射
SetProjectMapping(
    localProjectName: "MyProject",
    azureDevOpsProjectId: "project-id"
)

# 设置默认映射
SetDefaultMapping("MyProject")
```

---

## 任务同步问题

### 问题：状态同步失败

**现象**：
- 任务状态变更后未同步到 Azure DevOps
- 同步日志显示错误

**排查步骤**：

1. **检查同步服务**：
   - 确认 `TaskSyncBackgroundService` 正在运行
   - 检查同步间隔配置

2. **检查网络连接**：
   - 确保可以访问 Azure DevOps API

3. **检查任务状态**：
   - 只有 "Archived" 状态会自动同步
   - 检查任务状态是否正确

**解决方案**：
- 检查网络连接
- 手动触发同步：
  ```mcp
  SyncTaskToAzureDevOps(workItemId: "12345")
  ```
- 检查同步日志

---

## 日志分析

### 查看 MCP Server 日志

```powershell
# 运行时日志（控制台输出）
dotnet run --project src/AzureDevOpsMcpServer

# 检查日志文件
Get-Content logs/mcp-server.log
```

### 查看同步日志

```mcp
# 获取同步历史
GetSyncHistory()
```

### 常见日志错误

| 错误类型 | 解决方法 |
|----------|----------|
| `Authentication failed` | 检查用户映射和 PAT |
| `Connection refused` | 检查 MCP Server 是否运行 |
| `Timeout` | 检查网络和 Azure DevOps API |
| `Permission denied` | 检查 PAT 权限 |

---

## 问题上报

如果以上方法无法解决问题，请按照以下步骤上报：

1. **收集信息**：
   - 错误截图或日志
   - 操作系统版本
   - Node.js 版本
   - 复现步骤

2. **创建 Issue**：
   - 在项目仓库创建 Issue
   - 描述问题现象和复现步骤
   - 附上相关日志

3. **联系支持**：
   - 通过团队沟通渠道联系平台管理员
   - 提供详细信息以便快速定位

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0 | 2024-01-15 | 初始版本 |