# GitNexus Pre-commit Hook 配置文件

## 概述

本文档说明了如何配置 GitNexus Pre-commit Hook，实现 100% 强制执行 impact 分析。

## 文件说明

```
.git/hooks/
├── pre-commit           # Shell 版本的 pre-commit hook (Unix/Linux/Mac)
├── pre-commit.ps1      # PowerShell 版本的 pre-commit hook (Windows)
└── Install-GitNexusHook.ps1  # Hook 安装/管理脚本
```

## 自动安装

### Windows (PowerShell)

```powershell
# 运行安装脚本
.\.git\hooks\Install-GitNexusHook.ps1

# 或者手动复制 hook 文件
Copy-Item ".git\hooks\pre-commit.ps1" ".git\hooks\pre-commit.ps1"
```

### Unix/Linux/Mac

```bash
# 添加执行权限
chmod +x .git/hooks/pre-commit

# 创建符号链接（可选）
ln -sf ../../.git/hooks/pre-commit .git/hooks/pre-commit
```

## 启用 Hook

配置 Git 使用自定义 hooks 目录：

```bash
# 全局启用（所有项目）
git config --global core.hooksPath .git/hooks

# 仅当前项目
git config core.hooksPath .git/hooks
```

## 验证安装

```powershell
# 检查 hook 状态
.\.git\hooks\Install-GitNexusHook.ps1 -Test
```

## 使用方法

### 正常工作流程

```bash
# 1. 提交代码
git add .
git commit -m "Fix: Update login logic"

# 2. Hook 自动运行 impact 分析
# 如果有问题，会提示并阻止提交
```

### 强制跳过检查（不推荐）

```powershell
# Windows
.\.git\hooks\pre-commit.ps1 -Force

# Unix/Linux
.git/hooks/pre-commit --force
```

## GitHub Actions 集成

项目已配置 `.github/workflows/gitnexus-impact.yml`，会自动：

1. 在 PR 和 push 时运行 impact 分析
2. 检查代码质量
3. 阻止高风险变更合并

## Hook 工作流程

```
git commit
    ↓
Pre-commit Hook 触发
    ↓
检查是否有代码文件变更
    ↓
[有] → 运行 gitnexus detect_changes
    ↓
[发现风险] → 阻止提交，报告影响
[无风险] → 继续提交
    ↓
[没有] → 直接提交
```

## 故障排除

### Hook 没有运行

```bash
# 检查 hooksPath 配置
git config core.hooksPath

# 如果没有输出，手动配置
git config core.hooksPath .git/hooks
```

### GitNexus 未安装

```bash
# 安装 GitNexus
npm install -g gitnexus

# 验证安装
gitnexus --version
```

### Hook 权限问题

```bash
# Unix/Linux: 添加执行权限
chmod +x .git/hooks/pre-commit
```

## 卸载 Hook

```powershell
# Windows
.\.git\hooks\Install-GitNexusHook.ps1 -Uninstall

# Unix/Linux
rm .git/hooks/pre-commit

# 取消 hooksPath 配置
git config --unset core.hooksPath
```

## 相关文档

- [CLAUDE.md](CLAUDE.md) - AI 协作规则
- [AGENTS.md](AGENTS.md) - Agent 行为规范
- [.claude/skills/gitnexus/](.claude/skills/gitnexus/) - GitNexus 技能文档
