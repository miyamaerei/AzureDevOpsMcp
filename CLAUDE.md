<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **AzureDevOpsMcp** (1798 symbols, 3770 relationships, 113 execution flows).

## ⚡ PreToolUse Hook — 修改代码前的自动门卫

**一句话规则：** 准备改代码前，先用 GitNexus 查一下影响范围，拿到结果再动手。

### 🔗 Hook 执行流程

```
用户: "改一下用户登录逻辑"
  ↓
AI 准备调用 Edit/Write 工具
  ↓
【PreToolUse Hook 拦截】
  ↓
自动提取要修改的符号名 (如: validateUser)
  ↓
自动触发: gitnexus_impact({target: "validateUser", direction: "upstream"})
  ↓
AI 拿到完整影响分析:
  - d=1: loginHandler, apiMiddleware (会直接破坏)
  - d=2: authRouter, sessionManager (可能受影响)
  - 风险等级: MEDIUM
  ↓
基于完整信息做决策
```

### 🚨 触发条件

当检测到以下工具调用时，自动执行 impact 分析：

| 工具 | 触发动作 |
|------|---------|
| `Edit` | 分析目标文件中的符号影响 |
| `Write` | 分析目标文件中的符号影响 |
| `DeleteFile` | 分析被删除符号的破坏范围 |

### ✅ Edit/Write/DeleteFile 之前必须完成检查清单

```
- [ ] 识别要修改的符号名称 (函数名、类名、方法名)
- [ ] 运行 gitnexus_impact({target: "符号名", direction: "upstream", maxDepth: 2})
- [ ] 报告 blast radius:
      - d=1 直接调用者 (会直接破坏)
      - d=2 间接依赖 (可能受影响)
      - 涉及的 execution flows
- [ ] 评估风险等级:
      - <5 符号, 无关键流程 → LOW
      - 5-15 符号, 2-5 流程 → MEDIUM
      - >15 符号 或 关键路径 → HIGH/CRITICAL
- [ ] 如果是 HIGH/CRITICAL, 询问用户确认后再继续
- [ ] 报告改动计划 (改什么、影响什么、需要测试什么)
```

### 🔒 强制执行

**NEVER 跳过 impact 分析直接修改代码。**

修改代码时，必须先完成 Hook 检查：

```
当用户要求修改代码时:
  1. 识别目标符号 (从用户的描述或文件内容中提取)
  2. 调用 gitnexus_impact() 获取影响范围
  3. 如果有 d=1 调用者，报告哪些文件会直接被破坏
  4. 询问用户: "发现 N 个直接调用者，是否继续？"
  5. 获得确认后才执行修改
```

### 🎯 快捷命令

| 任务 | 命令 |
|------|------|
| 分析符号影响 | `gitnexus_impact({target: "符号名", direction: "upstream"})` |
| 查看符号上下文 | `gitnexus_context({name: "符号名"})` |
| 提交前检查 | `gitnexus_detect_changes({scope: "staged"})` |
| 概念搜索 | `gitnexus_query({query: "登录 OR auth"})` |

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/AzureDevOpsMcp/context` | Codebase overview, check index freshness |
| `gitnexus://repo/AzureDevOpsMcp/clusters` | All functional areas |
| `gitnexus://repo/AzureDevOpsMcp/processes` | All execution flows |
| `gitnexus://repo/AzureDevOpsMcp/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

## 🚀 强制执行机制

为确保 100% 遵守 impact 分析规则，本项目配置了多层强制执行：

### 1. Pre-commit Hook (本地)

```bash
# 启用 hooks
git config core.hooksPath .git/hooks

# 测试 hook 状态
.\.git\hooks\Install-GitNexusHook.ps1 -Test
```

**文件位置：**
- `.git/hooks/pre-commit.ps1` - Windows PowerShell 版本
- `.git/hooks/pre-commit` - Unix Shell 版本

**功能：**
- 提交前自动运行 `gitnexus detect_changes`
- 检测代码变更的影响范围
- 高风险变更阻止提交

### 2. GitHub Actions (CI/CD)

配置文件：`.github/workflows/gitnexus-impact.yml`

**触发条件：**
- Push 到 main/develop 分支
- Pull Request 创建/更新

**检查项：**
- [x] GitNexus impact 分析
- [x] 代码质量检查
- [x] 自动化测试

### 3. 详细配置文档

参考 [GITNEXUS-HOOK.md](GITNEXUS-HOOK.md) 了解完整的安装和配置流程。

### ⚠️ 强制程度

| 层级 | 强制程度 | 说明 |
|------|---------|------|
| Pre-commit Hook | **100% 强制** | 本地提交必须通过 |
| GitHub Actions | **100% 强制** | PR/Merge 必须通过 CI |
| AI 规则 (本文件) | ⚠️ **建议遵守** | 需要 AI 主动遵守 |
| 代码审查 | 📋 **人工检查** | PR 中的评审要点 |

### 🎯 快速开始

**首次设置：**
```bash
# 1. 启用 pre-commit hook
git config core.hooksPath .git/hooks

# 2. 测试配置
.\.git\hooks\Install-GitNexusHook.ps1 -Test

# 3. 提交代码
git add .
git commit -m "Your changes"
```

<!-- gitnexus:end -->
