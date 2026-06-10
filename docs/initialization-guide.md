# 项目初始化指南

## 概述

本文档详细介绍如何使用开发管理平台初始化新项目。

## 前置条件

### 系统要求

| 依赖 | 最低版本 | 说明 |
|------|----------|------|
| Node.js | ≥ 18.0.0 | 用于安装 GitNexus 和 Skills |
| Git | ≥ 2.0.0 | 版本控制 |
| .NET | ≥ 10.0.0 | 用于运行 MCP Server（服务端） |

### 权限要求

- Windows 用户需要域账号（用于 Windows 集成认证）
- Azure DevOps 账号（用于配置项目映射）

---

## 快速开始

### 方式一：一键初始化（推荐）

**Windows**：

```powershell
Invoke-Expression (Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -UseBasicParsing).Content
```

**Linux/Mac**：

```bash
curl -sSL https://platform.company.com/init-project.sh | bash
```

### 方式二：手动执行

```bash
# 1. 克隆模板仓库
git clone https://github.com/company/dev-platform-template.git
cd dev-platform-template

# 2. 运行初始化脚本
./scripts/Initialize-McpServer.ps1   # Windows
./scripts/initialize-mcp-server.sh  # Linux/Mac
```

---

## 初始化流程详解

### 步骤 1：依赖检查

脚本会自动检查以下依赖：

```powershell
# 检查 Node.js
node --version

# 检查 Git
git --version

# 检查 .NET（服务端）
dotnet --version
```

如果依赖缺失，脚本会提示安装。

### 步骤 2：GitNexus 安装

```bash
# 安装 GitNexus
npm install -g gitnexus

# 运行代码索引
gitnexus analyze
```

### 步骤 3：Skills 安装

```bash
# 安装 mattpocock skills
npx skills@latest add mattpocock/skills

# 配置项目
/setup-matt-pocock-skills
```

### 步骤 4：MCP 连接配置

脚本会自动配置 MCP 服务器连接：

**Claude Code**（`.claude/settings.json`）：

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

**Cursor**（`.cursor/mcp.json`）：

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

### 步骤 5：项目映射配置

配置本地项目与 Azure DevOps 项目的映射关系：

```mcp
SetProjectMapping(
    localProjectName: "MyProject",
    azureDevOpsProjectId: "abc123"
)
```

### 步骤 6：文档初始化

脚本会自动生成：

- `CONTEXT.md` - 领域语言定义
- `docs/adr/` - 架构决策记录目录

---

## 配置选项

### 跳过步骤

```bash
# 跳过 GitNexus 安装
./scripts/init-project.sh --skip-gitnexus

# 跳过 Skills 安装
./scripts/init-project.sh --skip-skills

# 跳过文档初始化
./scripts/init-project.sh --skip-docs
```

### 自定义配置

```bash
# 指定 MCP 服务器地址
./scripts/init-project.sh --mcp-url http://mcp.company.com:5000

# 指定 Azure DevOps 组织
./scripts/init-project.sh --org my-organization
```

---

## 验证初始化

### 检查 MCP 连接

```mcp
GetProjects()
```

如果返回项目列表，说明 MCP 连接配置成功。

### 检查 GitNexus

```bash
gitnexus status
```

### 检查 Skills

```bash
skills list
```

---

## 常见问题

### Q: 初始化脚本无法下载？

**A**：检查网络连接，或手动下载脚本：

```bash
# Windows
Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -OutFile init-project.ps1
.\init-project.ps1

# Linux/Mac
wget https://platform.company.com/init-project.sh
chmod +x init-project.sh
./init-project.sh
```

### Q: MCP Server 连接失败？

**A**：确保 MCP Server 正在运行：

```powershell
# 启动 MCP Server
cd src/AzureDevOpsMcpServer
dotnet run
```

### Q: GitNexus 安装失败？

**A**：检查 Node.js 版本，确保 ≥ 18.0.0：

```bash
node --version
npm install -g n
n 18
```

---

## 后续步骤

1. **配置用户映射**（首次使用）：
   ```mcp
   SetUserMapping(windowsUsername: "DOMAIN\\user", azureDevOpsUser: "user@company.com")
   ```

2. **拉取任务**：
   ```mcp
   GetAssignedTasks()
   ```

3. **开始开发**：
   ```
   /grill-with-docs
   /tdd
   ```

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0 | 2024-01-15 | 初始版本 |