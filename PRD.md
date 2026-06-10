# AI辅助开发管理平台 - 基础设施建设 PRD

## 问题陈述

开发团队在使用 Azure DevOps 进行任务管理时，缺乏统一的开发流程规范和基础设施支持。每个项目都需要重复配置 GitNexus、Skills 等工具，导致：

1. **配置不一致**：不同项目的开发流程和工具配置存在差异
2. **重复劳动**：每个项目都需要重新设置开发环境和工作流程
3. **缺乏规范**：没有统一的任务管理、代码分析和质量保障流程
4. **协作困难**：团队难以共享最佳实践和开发经验

团队需要一个**开发管理平台**，提供：
- 标准化的 MCP 服务（Azure DevOps 集成）
- 统一的开发流程规范
- 可复用的技能模板
- 一键初始化脚本

## 解决方案

构建一个**开发管理平台项目**（test_dev_flow），作为团队的基础设施，提供：

1. **Azure DevOps MCP Server**：基于 .NET Core 的 MCP 服务，提供任务管理能力
2. **流程规范文档**：定义标准的开发流程和最佳实践
3. **自定义技能模板**：提供团队特定的开发技能
4. **项目初始化脚本**：一键配置新项目的开发环境

团队其他项目通过使用这个平台，可以：
- 快速初始化开发环境（GitNexus、Skills 等）
- 使用统一的 MCP 服务进行任务管理
- 遵循标准的开发流程规范
- 自动集成代码分析和质量保障工具

## 用户故事

### 平台管理员用户故事

1. 作为平台管理员，我希望部署 MCP Server，以便团队可以通过 MCP 协议访问 Azure DevOps 功能
2. 作为平台管理员，我希望定义开发流程规范，以便团队遵循统一的开发标准
3. 作为平台管理员，我希望创建自定义技能模板，以便团队使用特定的开发工作流
4. 作为平台管理员，我希望提供项目初始化脚本，以便新项目可以快速配置开发环境
5. 作为平台管理员，我希望发布 MCP 配置模板，以便团队项目可以轻松连接到平台
6. 作为平台管理员，我希望维护流程文档，以便团队了解最佳实践
7. 作为平台管理员，我希望提供 CONTEXT.md 模板，以便新项目建立共享语言
8. 作为平台管理员，我希望监控 MCP Server 的运行状态，以便及时发现和解决问题
9. 作为平台管理员，我希望提供技能使用指南，以便团队成员快速上手
10. 作为平台管理员，我希望配置 MCP Server 使用 Windows 用户认证，以便识别调用者身份

### 团队开发者用户故事

11. 作为开发者，我希望使用初始化脚本配置新项目，以便快速开始开发工作
12. 作为开发者，我希望初始化脚本自动安装 GitNexus，以便进行代码分析
13. 作为开发者，我希望初始化脚本自动运行 gitnexus analyze，以便索引代码库
14. 作为开发者，我希望初始化脚本自动安装 mattpocock skills，以便使用工程化技能
15. 作为开发者，我希望初始化脚本自动运行 /setup-matt-pocock-skills，以便配置项目
16. 作为开发者，我希望初始化脚本自动配置 MCP 连接，以便访问平台的 Azure DevOps MCP Server
17. 作为开发者，我希望使用 Windows 用户认证访问 MCP Server，以便无需手动配置凭证
18. 作为开发者，我希望配置项目与 Azure DevOps 项目的映射关系，以便正确拉取对应项目的任务
19. 作为开发者，我希望通过 MCP 拉取指派给我的任务，以便开始开发工作
20. 作为开发者，我希望通过 MCP 按项目过滤任务，以便只查看特定项目的工作
21. 作为开发者，我希望使用 /task-workflow 技能，以便遵循标准的任务开发流程
22. 作为开发者，我希望使用 /grill-with-docs 技能，以便进行需求分析和建立共享语言
23. 作为开发者，我希望使用 /tdd 技能，以便进行测试驱动开发
24. 作为开发者，我希望使用 GitNexus 的 impact() 工具，以便分析代码修改的影响范围
25. 作为开发者，我希望使用 GitNexus 的 context() 工具，以便查看代码依赖关系
26. 作为开发者，我希望在遇到问题时使用 /diagnose 技能，以便进行结构化调试
27. 作为开发者，我希望在代码提交前使用 GitNexus 分析影响，以便避免破坏依赖
28. 作为开发者，我希望在任务完成时自动同步状态到 Azure DevOps，以便保持任务状态一致
29. 作为开发者，我希望使用 /improve-codebase-architecture 技能，以便优化代码架构
30. 作为开发者，我希望在项目根目录看到 CONTEXT.md，以便理解领域语言
31. 作为开发者，我希望在 docs/adr/ 目录看到架构决策记录，以便了解历史决策

### 项目经理用户故事

32. 作为项目经理，我希望看到任务状态仪表板，以便跟踪项目进度
33. 作为项目经理，我希望看到高亮显示的阻塞任务，以便解决瓶颈问题
34. 作为项目经理，我希望看到团队使用平台的情况统计，以便评估平台价值
35. 作为项目经理，我希望看到项目的初始化成功率，以便优化初始化流程

### 系统集成用户故事

36. 作为 CI/CD 系统，我希望通过 MCP API 更新任务状态，以便自动化工作流
37. 作为监控系统，我希望获取 MCP Server 的健康状态，以便进行运维监控
38. 作为日志系统，我希望收集 MCP Server 的运行日志，以便问题排查

## 实现决策

### 核心模块架构

**1. Azure DevOps MCP Server (.NET Core)**
- 使用微软官方 MCP Template：`dotnet new mcp` 初始化项目
- 使用 `Microsoft.Extensions.AI` 和 `Microsoft.Extensions.AI.Server` 包
- 采用 HTTP 传输模式，支持远程调用
- 提供以下 MCP 工具：
  - `GetAssignedTasks(userId, projectId)`：按用户和项目拉取指派的任务
  - `UpdateTaskStatus(taskId, status)`：更新任务状态
  - `GetTaskDetails(taskId)`：获取任务详细信息
  - `GetProjects(userId)`：获取用户可访问的项目列表
  - `GetTaskHistory(taskId)`：获取任务状态变更历史
  - `SetProjectMapping(localProject, azureProjectId)`：配置本地项目与 Azure 项目的映射

**2. 认证与安全模块**
- MCP Server 服务端 PAT：用于访问 Azure DevOps API（管理员配置，后台配置）
- Windows 用户认证：MCP Server 使用 Windows 集成认证识别调用者身份，开发者无需手动配置凭证
- API 调用使用 HTTPS 加密传输
- 实现请求频率限制，防止 API 滥用
- 支持项目级隔离（不同项目使用不同的 Azure DevOps 项目）
- 通过 Windows 用户名自动映射到 Azure DevOps 用户

**3. 流程规范模块**
- 定义标准开发流程：初始化 → 配置项目映射 → 拉取任务 → 需求分析 → TDD 开发 → 代码分析 → 提交 → 同步状态
- 定义四状态任务模型：当前任务（正在开发）→ 阻塞中（外部依赖）/未实现（搁置）→ 归档（完成验证）
- 状态流转规则：任务从 Azure DevOps 拉取后进入"当前任务"；当前任务可转为"阻塞中"或回退至"未实现"；任务完成后进入"归档"状态并自动同步到 Azure DevOps
- 提供流程文档模板
- 定义代码提交规范
- 定义分支管理策略

**4. 自定义技能模块**
- `/task-workflow`：完整的任务开发工作流（包含状态同步到 Azure DevOps）
- `/project-init`：项目初始化技能
- `/code-analysis`：代码分析集成技能（基于 GitNexus）
- `/configure-project`：项目映射配置技能

自定义技能作为 Matt Pocock skills 的补充，不重复实现通用工程技能（如 `/grill-with-docs`、`/tdd`、`/diagnose`、`/improve-codebase-architecture` 等）。

**5. 项目初始化模块**
- 采用模块化设计，主脚本 + 功能子脚本结构
- 提供 Shell 脚本和 PowerShell 脚本两种版本

**主入口脚本**：
- `init-project.sh` / `init-project.ps1`：交互式向导，引导用户完成初始化

**模块化子脚本**（scripts/modules/）：
- `check-deps.sh` / `Check-Deps.ps1`：检查系统依赖（Node.js、Git）
- `install-gitnexus.sh` / `Install-GitNexus.ps1`：安装 GitNexus 并运行代码索引
- `install-skills.sh` / `Install-Skills.ps1`：安装 mattpocock skills
- `config-mcp.sh` / `Config-Mcp.ps1`：配置 MCP 连接（启用 Windows 集成认证）
- `config-project.sh` / `Config-Project.ps1`：配置项目与 Azure DevOps 项目的映射关系
- `setup-docs.sh` / `Setup-Docs.ps1`：生成初始 CONTEXT.md 和创建 docs/adr/ 目录

**使用方式**：
1. **一键快速初始化**（推荐）：
   ```bash
   # Shell
   curl -sSL https://platform.company.com/init-project.sh | bash
   
   # PowerShell
   Invoke-Expression (Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -UseBasicParsing).Content
   ```

2. **分步执行**（高级用户）：
   ```bash
   # 仅检查依赖
   ./scripts/modules/check-deps.sh
   
   # 仅配置项目映射
   ./scripts/modules/config-project.sh
   ```

3. **跳过步骤**（使用参数）：
   ```bash
   # 跳过 GitNexus 安装
   ./init-project.sh --skip-gitnexus
   
   # 跳过 Skills 安装
   ./init-project.sh --skip-skills
   ```

**脚本功能**：
  - 检查系统依赖（Node.js、Git）
  - 安装 GitNexus（npm install -g gitnexus）并运行代码索引
  - 安装 mattpocock skills（npx skills@latest add mattpocock/skills）
  - 配置 MCP 连接（写入 .claude/settings.json 或 .cursor/mcp.json，启用 Windows 集成认证）
  - 提示用户配置项目与 Azure DevOps 项目的映射关系
  - 运行 /setup-matt-pocock-skills
  - 生成初始 CONTEXT.md（基于模板）
  - 创建 docs/adr/ 目录

**6. 配置模板模块**
- `mcp-config.json`：MCP Server 连接配置模板
- `context-template.md`：领域语言定义模板
- `gitignore-template`：.gitignore 模板（包含 .gitnexus/ 等）
- `claude-settings-template.json`：Claude Code 配置模板
- `cursor-mcp-template.json`：Cursor MCP 配置模板
- `project-mapping-template.json`：项目映射配置模板

### 数据流设计

```
团队项目                    开发管理平台                 Azure DevOps
   │                            │                          │
   ├─ 初始化脚本 ──────────────→│                          │
   │  (下载并执行)               │                          │
   │                            │                          │
   ├─ 配置 PAT ─────────────────→│                          │
   │  (用户个人 PAT)              │                          │
   │                            │                          │
   ├─ 配置项目映射 ─────────────→│                          │
   │  (本地项目 → Azure 项目)      │                          │
   │                            │                          │
   ├─ 调用 MCP 工具 ────────────→│                          │
   │  (GetAssignedTasks)        ├─ API 调用 ──────────────→│
   │  (带 userId + projectId)    │                          │
   │                            │                          │
   │←─ 返回任务列表 ─────────────┤←─ 返回数据 ──────────────┤
   │  (仅返回指定项目的任务)      │                          │
   │                            │                          │
   ├─ 开发完成                  │                          │
   ├─ 调用 MCP 工具 ────────────→│                          │
   │  (UpdateTaskStatus)        ├─ API 调用 ──────────────→│
   │                            │                          │
   │←─ 确认更新 ─────────────────┤←─ 确认更新 ─────────────┤
```

### 技术栈选择

- **MCP Server**：.NET Core 10.0 + 微软官方 MCP Template（`dotnet new mcp`）+ `Microsoft.Extensions.AI.Server`
- **脚本语言**：Shell (Bash) + PowerShell
- **包管理**：npm（GitNexus、Skills）
- **文档格式**：Markdown
- **配置格式**：JSON

### 部署架构

```
开发管理平台部署：
├─ MCP Server (HTTP)
│  ├─ 端点：https://platform.company.com/mcp
│  ├─ 认证：服务端 PAT 令牌
│  └─ 监控：健康检查端点
│
├─ 文档站点
│  ├─ 流程规范文档
│  ├─ 技能使用指南
│  └─ 最佳实践
│
└─ 初始化脚本托管
   ├─ https://platform.company.com/init-project.sh
   └─ https://platform.company.com/init-project.ps1
```

### 文件结构

```
test_dev_flow/
├─ src/
│  └─ AzureDevOpsMcpServer/
│     ├─ Tools/
│     │  ├─ AzureDevOpsTool.cs     # Azure DevOps MCP 工具实现
│     │  └─ ProjectMappingTool.cs  # 项目映射工具
│     ├─ Services/
│     │  ├─ AzureDevOpsService.cs  # Azure API 封装
│     │  └─ MappingService.cs      # 映射管理服务
│     ├─ Models/
│     │  ├─ Task.cs                # 任务模型
│     │  ├─ Project.cs             # 项目模型
│     │  └─ ProjectMapping.cs      # 项目映射模型
│     ├─ Properties/
│     │  └─ launchSettings.json    # 启动配置（MCP Template 默认生成）
│     ├─ appsettings.json          # 应用配置（包含 PAT、连接字符串等）
│     └─ Program.cs                # MCP Server 启动（使用官方 Template 模式）
│
├─ skills/
│  ├─ task-workflow/
│  │  └─ SKILL.md                  # 任务工作流技能（包含状态同步）
│  ├─ project-init/
│  │  └─ SKILL.md                  # 项目初始化技能
│  ├─ code-analysis/
│  │  └─ SKILL.md                  # 代码分析技能（基于 GitNexus）
│  ├─ configure-pat/
│  │  └─ SKILL.md                  # PAT 配置技能
│  └─ configure-project/
│     └─ SKILL.md                  # 项目配置技能
│
├─ scripts/
│  ├─ init-project.sh              # Linux/Mac 初始化主脚本（交互式向导）
│  ├─ init-project.ps1             # Windows 初始化主脚本（交互式向导）
│  └─ modules/                     # 模块化子脚本目录
│     ├─ check-deps.sh             # Shell 依赖检查模块
│     ├─ install-gitnexus.sh       # Shell GitNexus 安装模块
│     ├─ install-skills.sh         # Shell Skills 安装模块
│     ├─ config-mcp.sh             # Shell MCP 配置模块
│     ├─ config-project.sh         # Shell 项目映射配置模块
│     ├─ setup-docs.sh             # Shell 文档初始化模块
│     ├─ Check-Deps.ps1            # PowerShell 依赖检查模块
│     ├─ Install-GitNexus.ps1      # PowerShell GitNexus 安装模块
│     ├─ Install-Skills.ps1        # PowerShell Skills 安装模块
│     ├─ Config-Mcp.ps1            # PowerShell MCP 配置模块
│     ├─ Config-Project.ps1        # PowerShell 项目映射配置模块
│     └─ Setup-Docs.ps1            # PowerShell 文档初始化模块
│
├─ templates/
│  ├─ mcp-config.json              # MCP 配置模板
│  ├─ context-template.md          # CONTEXT.md 模板
│  ├─ gitignore-template           # .gitignore 模板
│  ├─ claude-settings-template.json
│  ├─ cursor-mcp-template.json
│  └─ project-mapping-template.json # 项目映射配置模板
│
├─ docs/
│  ├─ workflow-specification.md    # 开发流程规范
│  ├─ initialization-guide.md      # 项目初始化指南
│  ├─ best-practices.md           # 最佳实践
│  ├─ skill-usage-guide.md        # 技能使用指南
│  └─ troubleshooting.md          # 故障排查指南
│
└─ README.md                       # 平台说明文档
```

## 测试决策

### 测试策略

**1. 单元测试**
- 测试 MCP Server 的各个工具方法
- 使用 Azure DevOps API mock 进行隔离测试
- 测试认证和授权逻辑
- 测试数据转换和验证逻辑
- 测试项目映射逻辑

**2. 集成测试**
- 测试 MCP Server 与 Azure DevOps API 的集成
- 测试初始化脚本的执行流程
- 测试技能模板的正确性
- 测试配置文件的生成
- 测试项目映射功能

**3. 端到端测试**
- 模拟完整的用户流程：初始化项目 → 配置 PAT → 配置项目映射 → 拉取任务 → 开发 → 提交 → 同步状态
- 测试 MCP 客户端与服务端的通信
- 测试 GitNexus 和 Skills 的集成
- 测试多项目隔离

**4. 性能测试**
- 测试 MCP Server 的并发处理能力
- 测试大量任务拉取的性能
- 测试初始化脚本的执行时间

### 测试模块

- `AzureDevOpsMcpServer.Tests`：MCP Server 单元测试和集成测试
- `scripts.Tests`：初始化脚本测试
- `skills.Tests`：技能模板验证测试

### 测试标准

- 单元测试覆盖率 ≥ 80%
- 所有 MCP 工具必须有集成测试
- 初始化脚本必须在 Linux、Mac、Windows 上测试通过
- 技能模板必须符合 SKILL.md 格式规范
- 项目映射功能必须正确隔离不同项目的任务

## 范围外

以下功能不在当前 PRD 范围内：

1. **Web 管理界面**：不开发图形化管理界面，仅提供命令行和 MCP 接口
2. **统计分析功能**：不实现详细的数据分析和报表功能
3. **多语言支持**：初始化脚本仅支持 Shell 和 PowerShell，不支持其他脚本语言
4. **自定义工作流引擎**：不开发可配置的工作流引擎，使用固定的流程规范
5. **实时协作功能**：不实现多人实时协作编辑功能
6. **移动端支持**：不支持移动端访问
7. **第三方集成**：不集成 Jira、Trello 等其他项目管理工具
8. **AI 代码生成**：不提供 AI 自动代码生成功能
9. **高级监控告警**：不实现复杂的监控和告警系统
10. **多租户管理界面**：不提供租户管理界面，租户隔离通过配置实现

## 补充说明

### 实施阶段

**第一阶段（核心功能）**：
- Azure DevOps MCP Server 开发和部署
- 基础配置模板创建
- 初始化脚本开发（包含 PAT 配置和项目映射）
- 基础文档编写

**第二阶段（流程规范）**：
- 自定义技能开发
- 流程规范文档完善
- 最佳实践总结
- 技能使用指南编写

**第三阶段（优化增强）**：
- 性能优化
- 错误处理增强
- 文档完善
- 示例项目创建

### 依赖关系

- 需要 Azure DevOps 组织和服务端 PAT 令牌
- 需要部署 MCP Server 的服务器环境
- 需要 Node.js ≥ 18.0.0（用于 GitNexus 和 Skills）
- 需要 Git 环境
- 需要网络访问 Azure DevOps API

### 安全考虑

- MCP Server 服务端 PAT 必须安全存储，使用环境变量或密钥管理服务
- MCP Server 使用 Windows 集成认证，无需用户手动配置凭证
- MCP Server 必须使用 HTTPS
- 初始化脚本必须从可信源下载
- 配置文件中的敏感信息必须加密或使用环境变量
- API 调用必须实现频率限制，防止滥用
- Windows 用户认证确保只有域内用户可以访问 MCP Server

### 扩展性考虑

- MCP Server 设计为可扩展，支持添加新的工具
- 技能模板支持自定义和扩展
- 初始化脚本支持参数化配置
- 流程规范文档支持版本管理
- 项目映射支持灵活配置

### 成功标准

1. 团队可以在 5 分钟内完成新项目的初始化（包含项目映射配置）
2. MCP Server 可以稳定支持 100+ 并发请求
3. 所有团队成员都能使用标准流程进行开发
4. 代码质量指标（测试覆盖率、代码复杂度）得到改善
5. 任务状态同步准确率达到 99% 以上
6. 项目映射准确率达到 100%（任务与项目正确对应）
