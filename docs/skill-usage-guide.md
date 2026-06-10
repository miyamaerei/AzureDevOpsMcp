# 技能使用指南

## 概述

本文档详细介绍团队常用技能的使用方法和最佳实践。

## 目录

1. [核心技能](#核心技能)
   - [/tdd](#tdd)
   - [/grill-with-docs](#grill-with-docs)
   - [/diagnose](#diagnose)
   - [/improve-codebase-architecture](#improve-codebase-architecture)

2. [辅助技能](#辅助技能)
   - [/grill-me](#grill-me)
   - [/prototype](#prototype)
   - [/to-issues](#to-issues)
   - [/to-prd](#to-prd)
   - [/handoff](#handoff)

3. [代码分析技能](#代码分析技能)
   - [GitNexus](#gitnexus)

4. [平台特有技能](#平台特有技能)
   - [项目初始化](#项目初始化)
   - [任务工作流](#任务工作流)

---

## 核心技能

### /tdd

**用途**：测试驱动开发，遵循 Red-Green-Refactor 流程

**使用方式**：

```
/tdd <需求描述>
```

**示例**：

```
/tdd 实现用户登录功能，支持用户名密码验证和 JWT 令牌生成
```

**执行流程**：

1. **Red** - 编写失败的测试用例
2. **Green** - 编写使测试通过的最小代码
3. **Refactor** - 优化代码结构

**最佳实践**：
- 测试用例应覆盖核心业务逻辑
- 保持测试隔离，避免测试间相互依赖
- 使用 Mock 隔离外部依赖

---

### /grill-with-docs

**用途**：深入分析需求文档，建立共享语言

**使用方式**：

```
/grill-with-docs
```

**执行流程**：

1. 读取项目中的文档（CONTEXT.md、PRD.md 等）
2. 识别关键概念和术语
3. 澄清模糊需求
4. 建立领域语言
5. 更新 CONTEXT.md

**最佳实践**：
- 在任务开始前使用此技能进行需求分析
- 确保团队成员对需求有共同理解
- 及时更新文档反映最新理解

---

### /diagnose

**用途**：结构化调试，定位和解决复杂问题

**使用方式**：

```
/diagnose
```

**执行流程**：

1. **Reproduce** - 复现问题
2. **Minimize** - 最小化问题范围
3. **Hypothesize** - 提出假设
4. **Instrument** - 添加日志和调试信息
5. **Fix** - 实施修复
6. **Regression-test** - 回归测试

**最佳实践**：
- 遇到难以定位的 Bug 时使用
- 记录调试过程和解决方案
- 将解决方案整理为文档

---

### /improve-codebase-architecture

**用途**：优化代码架构，识别重构机会

**使用方式**：

```
/improve-codebase-architecture
```

**执行流程**：

1. 分析代码库结构
2. 识别架构问题（耦合度、内聚度等）
3. 提出重构建议
4. 提供实施路径

**最佳实践**：
- 定期对代码库进行架构审查
- 在功能迭代间隙进行重构
- 重构前确保有足够的测试覆盖

---

## 辅助技能

### /grill-me

**用途**：对计划或设计进行深度审查

**使用方式**：

```
/grill-me
```

**执行流程**：

1. 用户描述计划或设计
2. 系统提出挑战性问题
3. 用户回答问题
4. 逐步完善计划

**适用场景**：
- 新功能设计审查
- 技术方案讨论
- 架构决策验证

---

### /prototype

**用途**：快速构建原型验证设计

**使用方式**：

```
/prototype <设计描述>
```

**示例**：

```
/prototype 用户管理界面的三种设计方案
```

**特点**：
- 快速生成可运行的原型
- 支持多种设计方案对比
- 可用于验证交互逻辑

---

### /to-issues

**用途**：将计划分解为可追踪的 Issue

**使用方式**：

```
/to-issues
```

**执行流程**：

1. 分析需求或计划
2. 分解为独立的任务单元
3. 创建 Issue 追踪每个任务
4. 建立任务间的依赖关系

**最佳实践**：
- 任务应遵循单一职责原则
- 每个任务应在合理时间内完成（建议 1-2 天）
- 明确任务的验收标准

---

### /to-prd

**用途**：将对话内容转换为产品需求文档

**使用方式**：

```
/to-prd
```

**输出内容**：
- 问题陈述
- 用户故事
- 功能需求
- 非功能需求
- 验收标准

---

### /handoff

**用途**：生成交接文档，便于会话连续性

**使用方式**：

```
/handoff
```

**输出内容**：
- 会话摘要
- 已完成工作
- 待办事项
- 关键决策
- 参考文档

---

## 代码分析技能

### GitNexus

**用途**：代码分析和上下文理解

**核心命令**：

```
gitnexus impact()
gitnexus context()
```

**impact()**：分析代码修改的影响范围

```
// 分析当前变更的影响
gitnexus impact()

// 分析特定文件的影响
gitnexus impact("src/services/auth.ts")
```

**context()**：查看代码依赖关系

```
// 查看当前文件的上下文
gitnexus context()

// 查看特定函数的上下文
gitnexus context("validateUser")
```

**最佳实践**：
- 代码提交前使用 impact() 检查影响
- 理解陌生代码时使用 context()
- 定期运行分析确保代码质量

---

## 平台特有技能

### 项目初始化

**用途**：一键配置新项目开发环境

**使用方式**：

```bash
# Windows
Invoke-Expression (Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -UseBasicParsing).Content

# Linux/Mac
curl -sSL https://platform.company.com/init-project.sh | bash
```

**功能模块**：
- 依赖检查（Node.js、Git）
- GitNexus 安装和索引
- Skills 安装配置
- MCP 连接配置
- 项目映射配置
- 文档初始化

---

### 任务工作流

**用途**：遵循标准流程完成任务开发

**使用方式**：

```
/task-workflow
```

**执行流程**：

1. **拉取任务** → GetAssignedTasks
2. **需求分析** → /grill-with-docs
3. **TDD 开发** → /tdd
4. **代码分析** → GitNexus
5. **提交代码** → 遵循规范
6. **同步状态** → 自动同步到 Azure DevOps

---

## 技能组合推荐

### 开发新功能

```
/grill-with-docs    # 需求分析
    ↓
/tdd               # 测试驱动开发
    ↓
gitnexus impact()   # 代码分析
    ↓
/to-issues         # 任务分解（如需要）
```

### 修复 Bug

```
/diagnose          # 结构化调试
    ↓
/tdd               # 编写测试用例
    ↓
gitnexus context() # 理解代码上下文
```

### 架构优化

```
/improve-codebase-architecture
    ↓
/to-issues         # 创建重构任务
    ↓
/tdd               # 重构时保持测试覆盖
```

---

## 快捷键建议

| 场景 | 推荐技能 |
|------|----------|
| 开始新任务 | `/grill-with-docs` |
| 实现功能 | `/tdd` |
| 遇到问题 | `/diagnose` |
| 代码审查 | `/grill-me` |
| 架构问题 | `/improve-codebase-architecture` |

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 1.0 | 2024-01-15 | 初始版本 |
| 1.1 | 2024-02-01 | 添加 GitNexus 说明 |