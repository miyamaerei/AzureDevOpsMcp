# CONTEXT.md - 领域语言定义

## 项目概述

简要描述项目的目的和核心价值。

## 核心概念

### 任务状态模型

| 状态 | 说明 | 触发条件 |
|------|------|----------|
| **Current** | 当前任务（正在开发） | 任务从 Azure DevOps 拉取后自动进入此状态 |
| **Blocked** | 阻塞中（外部依赖） | 当前任务因外部依赖无法继续 |
| **NotImplemented** | 未实现（搁置） | 当前任务暂停或回退 |
| **Archived** | 归档（完成验证） | 任务完成后进入此状态并自动同步到 Azure DevOps |

### 状态流转规则

```
NotImplemented → Current → Blocked
                   ↓           ↓
                  Archived ←───┘
```

## 术语表

### 项目相关

| 术语 | 定义 |
|------|------|
| **Local Project** | 本地开发项目 |
| **Azure Project** | Azure DevOps 中的项目 |
| **Project Mapping** | 本地项目与 Azure DevOps 项目的映射关系 |

### 任务相关

| 术语 | 定义 |
|------|------|
| **Work Item** | Azure DevOps 中的工作项 |
| **Task Item** | 本地任务模型 |
| **Task History** | 任务状态变更历史 |

### 用户相关

| 术语 | 定义 |
|------|------|
| **Windows User** | Windows 域用户 |
| **Azure DevOps User** | Azure DevOps 用户 |
| **User Mapping** | Windows 用户与 Azure DevOps 用户的映射 |

## 标准流程

### 任务开发流程

1. **初始化** → 使用项目初始化脚本配置环境
2. **配置映射** → 配置本地项目与 Azure DevOps 项目的映射关系
3. **拉取任务** → 通过 MCP 获取指派给当前用户的任务
4. **需求分析** → 使用 /grill-with-docs 技能分析需求
5. **TDD 开发** → 使用 /tdd 技能进行测试驱动开发
6. **代码分析** → 使用 GitNexus 分析代码影响
7. **提交代码** → 遵循提交规范提交代码
8. **同步状态** → 任务完成后自动同步状态到 Azure DevOps

## 引用文档

- PRD: `PRD.md`
- 开发流程规范: `docs/development-process.md`
- 分支策略: `docs/branching-strategy.md`
- 提交规范: `docs/commit-conventions.md`