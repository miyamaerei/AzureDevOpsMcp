# 开发流程规范

## 概述

本文档定义了团队使用开发管理平台进行任务开发的标准流程。所有团队成员应遵循此流程进行日常开发工作。

## 流程总览

```
┌─────────────────────────────────────────────────────────────────┐
│                    开发流程总览                                  │
├─────────────────────────────────────────────────────────────────┤
│  1. 初始化项目                                                   │
│     └── 运行初始化脚本配置开发环境                                  │
├─────────────────────────────────────────────────────────────────┤
│  2. 配置仓库映射                                                  │
│     └── 建立本地 Git 仓库与 Azure Boards 项目的映射关系                │
├─────────────────────────────────────────────────────────────────┤
│  3. 拉取当前仓库任务                                               │
│     └── 通过 GitHub 关联对象获取指派给当前用户且关联当前仓库的任务        │
├─────────────────────────────────────────────────────────────────┤
│  4. 需求分析                                                     │
│     └── 使用 /grill-with-docs 技能分析需求                          │
├─────────────────────────────────────────────────────────────────┤
│  5. TDD 开发                                                     │
│     └── 使用 /tdd 技能进行测试驱动开发                              │
├─────────────────────────────────────────────────────────────────┤
│  6. 代码分析                                                     │
│     └── 使用 GitNexus 分析代码影响范围                              │
├─────────────────────────────────────────────────────────────────┤
│  7. 提交代码                                                     │
│     └── 遵循提交规范提交代码                                       │
├─────────────────────────────────────────────────────────────────┤
│  8. 同步状态                                                     │
│     └── 任务完成后自动同步状态到 Azure DevOps                        │
└─────────────────────────────────────────────────────────────────┘
```

## 详细流程

### 1. 项目初始化

**目的**：快速配置新项目的开发环境

**操作步骤**：

```bash
# Windows
Invoke-Expression (Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -UseBasicParsing).Content

# Linux/Mac
curl -sSL https://platform.company.com/init-project.sh | bash
```

**脚本执行内容**：
- 检查系统依赖（Node.js ≥ 18.0.0、Git）
- 安装 GitNexus 并运行代码索引
- 安装 mattpocock skills
- 配置 MCP 连接（启用 Windows 集成认证）
- 提示配置仓库映射
- 生成初始 CONTEXT.md
- 创建 docs/adr/ 目录

### 2. 仓库映射配置

**目的**：建立本地代码仓库、远程仓库与 Azure Boards 项目的对应关系。Azure DevOps Project 只是 WorkItem 查询边界，当前任务归属边界是 WorkItem 上已建立关联的仓库。

**操作方式**：

```mcp
SetRepositoryMapping(
  localProject,
  azureProjectId,
  azureProjectName,
  repositoryId,
  repositoryName,
  remoteUrl,
  organization,
  workingDirectory,
  isDefault,
  repositoryProvider,
  repositoryOwner
)
```

**参数说明**：
- `localProject`: 本地项目/仓库名称
- `workingDirectory`: 本地 Git 仓库工作目录
- `repositoryProvider`: 仓库提供方，例如 `GitHub` 或 `AzureRepos`
- `repositoryOwner`: GitHub owner 或 Azure DevOps organization/project owner
- `repositoryName`: GitHub repo 名称或 Azure Repo 名称
- `remoteUrl`: 仓库远程地址
- `azureProjectId`: Azure DevOps Project ID，用于限定 Azure Boards 查询范围
- `azureProjectName`: Azure DevOps Project 名称

**配置文件位置**：`templates/project-mapping-template.json`

### 3. 当前仓库任务拉取

**目的**：获取指派给当前用户，并且在 Azure DevOps WorkItem 中通过仓库关联关系指向当前仓库的任务列表。

**操作方式**：

```mcp
GetAssignedWorkItemsForRepository(userId, repositoryProvider, repositoryOwner, repositoryName, projectId)
```

或在已配置默认仓库映射时：

```mcp
GetAssignedWorkItemsForCurrentRepository(userId)
```

**关联依据**：
- Azure Boards WorkItem 链接的 `GitHub Branch` ArtifactLink
- Azure Boards WorkItem 链接的 `GitHub Commit` ArtifactLink
- Azure Boards WorkItem 链接的 `GitHub Pull Request` ArtifactLink
- Azure Boards WorkItem 链接的 `GitHub Issue` ArtifactLink
- Azure Boards WorkItem 链接的 Azure Repos Branch / Commit / Pull Request ArtifactLink

> 当前仓库任务拉取只以 Azure DevOps WorkItem 上已存在的关系为准，不通过 Git 或 GitHub API 扫描远程仓库对象。

**参数说明**：
- `userId`: 用户标识（可选，默认使用当前登录用户）
- `repositoryProvider`: 仓库提供方，例如 `GitHub`
- `repositoryOwner`: GitHub owner
- `repositoryName`: GitHub repo 名称
- `projectId`: Azure DevOps Project ID（可选，默认使用仓库映射中的项目）

**返回内容**：
- 任务 ID
- 任务标题
- 任务描述
- 指派用户
- 项目信息
- 关联仓库信息
- 仓库解析来源
- 任务状态
- 创建时间
- 更新时间

> 注意：`GetAssignedWorkItems(userId, projectId)` 只按 Azure DevOps Project 拉取任务，不能代表“当前仓库任务”。

### 4. 需求分析

**目的**：深入理解任务需求，建立共享语言

**操作方式**：

```
/grill-with-docs
```

**执行内容**：
- 分析任务需求文档
- 澄清模糊需求
- 建立领域语言
- 更新 CONTEXT.md

### 5. TDD 开发

**目的**：使用测试驱动开发方法实现功能

**操作方式**：

```
/tdd <需求描述>
```

**执行流程**：
1. **Red** - 编写失败的测试
2. **Green** - 编写使测试通过的代码
3. **Refactor** - 重构代码

**测试要求**：
- 单元测试覆盖率 ≥ 80%
- 所有核心功能必须有测试覆盖
- 测试必须通过才能提交代码

### 6. 代码分析

**目的**：分析代码修改的影响范围，避免破坏依赖

**操作方式**：

```
gitnexus impact()
gitnexus context()
```

**分析内容**：
- 依赖关系分析
- 影响范围评估
- 代码复杂度检查
- 潜在问题识别

### 7. 代码提交

**目的**：将代码变更提交到版本控制系统

**提交规范**：

```
<type>(<scope>): <description>

<optional body>

<optional footer>
```

**类型说明**：
- `feat`: 新功能
- `fix`: 修复 Bug
- `docs`: 文档更新
- `style`: 代码格式（不影响代码逻辑）
- `refactor`: 重构（既不新增功能也不修复 Bug）
- `test`: 测试相关
- `chore`: 构建/工具相关

**示例**：
```
feat(auth): add Windows integrated authentication

- Implement Negotiate authentication middleware
- Add user context service
- Update MCP transport configuration
```

### 8. 状态同步

**目的**：保持任务状态在本地和 Azure DevOps 之间同步

**自动同步**：
- 任务状态变更为 "Archived" 时自动同步到 Azure DevOps
- 后台定时同步（默认 5 分钟间隔）

**手动同步**：

```mcp
SyncTaskToAzureDevOps(workItemId)
```

**同步内容**：
- 任务状态
- 完成时间
- 备注信息

## 任务状态模型

### 状态定义

| 状态 | 代码值 | 说明 |
|------|--------|------|
| **Current** | `current` | 当前任务（正在开发） |
| **Blocked** | `blocked` | 阻塞中（外部依赖） |
| **NotImplemented** | `notImplemented` | 未实现（搁置） |
| **Archived** | `archived` | 归档（完成验证） |

### 状态流转规则

```
                    ┌──────────────────┐
                    │  NotImplemented │
                    └────────┬─────────┘
                             │ 开始开发
                             ▼
                    ┌──────────────────┐
        ┌───────────│    Current      │───────────┐
        │           └────────┬─────────┘           │
        │                    │                    │
        │ 遇到阻塞           │ 完成开发            │ 暂停开发
        ▼                    ▼                    ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│    Blocked       │  │    Archived      │  │  NotImplemented │
└────────┬─────────┘  └──────────────────┘  └──────────────────┘
         │ 阻塞解除
         ▼
┌──────────────────┐
│    Current      │
└──────────────────┘
```

### 流转规则说明

| 从状态 | 到状态 | 条件 |
|--------|--------|------|
| NotImplemented | Current | 开始开发任务 |
| Current | Blocked | 遇到外部依赖阻塞 |
| Current | NotImplemented | 暂停或回退任务 |
| Current | Archived | 完成任务开发 |
| Blocked | Current | 阻塞问题解决 |
| Archived | - | 最终状态，不可变更 |

## 分支管理策略

### 分支类型

| 分支类型 | 命名规范 | 用途 |
|----------|----------|------|
| **main** | `main` | 主分支，稳定版本 |
| **develop** | `develop` | 开发分支，集成所有功能 |
| **feature** | `feature/<feature-name>` | 功能开发分支 |
| **bugfix** | `bugfix/<bug-description>` | Bug 修复分支 |
| **hotfix** | `hotfix/<issue-description>` | 紧急修复分支 |

### 分支流程

```
main ──┬── 合并 hotfix ───────────────────────────────────┐
       │                                                  │
       ▼                                                  ▼
develop ── feature/* ──→ PR ──→ 审核 ──→ 合并 ──→ 测试 ──→
       │                                                  │
       └── bugfix/* ──────────────────────────────────────┘
```

### 合并策略

- **feature → develop**: 使用 Squash Merge
- **bugfix → develop**: 使用 Squash Merge
- **hotfix → main**: 使用 Merge Commit
- **develop → main**: 使用 Merge Commit（版本发布）

## 代码审查要求

### 审查标准

1. **代码正确性**：功能实现正确，无逻辑错误
2. **代码质量**：遵循编码规范，易于理解和维护
3. **测试覆盖**：单元测试覆盖核心逻辑
4. **性能考虑**：无明显性能问题
5. **安全考虑**：无安全漏洞
6. **文档完善**：必要的注释和文档

### 审查流程

1. 创建 Pull Request
2. 分配至少 1 位审查者
3. 审查者进行代码审查
4. 修复审查意见
5. 审查通过后合并

## 质量保障

### 自动化检查

- ✅ 代码构建验证
- ✅ 单元测试执行
- ✅ 代码格式检查
- ✅ 静态代码分析
- ✅ GitNexus 依赖分析

### 代码质量指标

| 指标 | 目标值 |
|------|--------|
| 单元测试覆盖率 | ≥ 80% |
| 代码复杂度 | < 15 (平均) |
| 代码重复率 | < 5% |
| 技术债务 | 持续减少 |

## 故障处理

### 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| MCP Server 连接失败 | 服务未启动或端口占用 | 检查服务状态，确保端口可用 |
| 认证失败 | Windows 用户未映射 | 使用 SetUserMapping 配置映射 |
| 任务拉取为空 | 仓库映射未配置，或 WorkItem 未在 Azure DevOps 中链接当前仓库对象 | 配置仓库映射，并确认 WorkItem 已建立 GitHub/Azure Repos Branch、Commit、PR 或 Issue ArtifactLink |
| 状态同步失败 | Azure DevOps API 错误 | 检查 PAT 权限和网络连接 |

### 问题上报流程

1. 收集错误日志
2. 检查文档中的故障排查指南
3. 在团队沟通渠道报告问题
4. 创建 Issue 跟踪问题
5. 问题解决后更新文档

## 工具支持

### 核心工具

| 工具 | 用途 | 文档链接 |
|------|------|----------|
| **GitNexus** | 代码分析和上下文理解 | https://gitnexus.dev |
| **MCP Server** | Azure DevOps 集成服务 | 项目文档 |
| **mattpocock skills** | 工程化开发技能 | https://github.com/mattpocock/skills |

### 辅助工具

| 工具 | 用途 |
|------|------|
| **/tdd** | 测试驱动开发 |
| **/grill-with-docs** | 需求分析 |
| **/diagnose** | 结构化调试 |
| **/improve-codebase-architecture** | 架构优化 |

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0 | 2024-01-15 | 初始版本 |
| 1.1 | 2024-02-01 | 添加状态同步流程 |
| 1.2 | 2024-03-01 | 更新分支管理策略 |