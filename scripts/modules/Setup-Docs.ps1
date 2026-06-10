#Requires -Version 5.1
<#
.SYNOPSIS
    初始化项目文档
.DESCRIPTION
    创建必要的文档目录和初始文件，包括 ADR、CONTEXT.md 等
.EXAMPLE
    .\Setup-Docs.ps1
#>

param(
    [switch]$SkipContextGen,
    [switch]$Silent
)

$ErrorActionPreference = "Stop"

# 颜色定义
$Cyan = [ConsoleColor]::Cyan
$Green = [ConsoleColor]::Green
$Yellow = [ConsoleColor]::Yellow
$Red = [ConsoleColor]::Red
$White = [ConsoleColor]::White

function Write-ColorOutput {
    param(
        [string]$Message,
        [ConsoleColor]$Color = $White
    )
    if (-not $Silent) {
        $originalColor = $Host.UI.RawUI.ForegroundColor
        $Host.UI.RawUI.ForegroundColor = $Color
        Write-Host $Message
        $Host.UI.RawUI.ForegroundColor = $originalColor
    }
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    初始化项目文档" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$projectRoot = $PSScriptRoot | Split-Path -Parent | Split-Path -Parent

# 1. 创建 docs 目录结构
Write-ColorOutput "创建文档目录结构..." $Yellow

$docsDir = Join-Path $projectRoot "docs"
$adrDir = Join-Path $docsDir "adr"
$issuesDir = Join-Path $docsDir "issues"
$templatesDir = Join-Path $docsDir "templates"

# 创建目录
$directories = @($docsDir, $adrDir, $issuesDir, $templatesDir)
foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
}
Write-ColorOutput "  ✅ 文档目录结构创建完成" $Green

# 2. 创建 ADR 目录
Write-ColorOutput "`n创建 ADR (Architecture Decision Records)..." $Yellow

$adrIndexPath = Join-Path $adrDir "README.md"
$adrIndexContent = @"
# Architecture Decision Records

## 什么是 ADR?

ADR (Architecture Decision Records) 是记录重要架构决策的文档。每个 ADR 包含：
- **背景**: 做出决策的原因
- **决策**: 具体的架构选择
- **结果**: 决策的后果

## ADR 列表

| ID | 标题 | 日期 | 状态 |
|----|------|------|------|
| 000 | ADR 模板 | $(Get-Date -Format 'yyyy-MM-dd') | 已接受 |

## 添加新 ADR

使用模板创建新 ADR：
\`\`\`bash
cp docs/templates/adr-template.md docs/adr/XXX-title.md
\`\`\`

然后更新本文件添加新 ADR 的索引。
"@

if (-not (Test-Path $adrIndexPath)) {
    Set-Content -Path $adrIndexPath -Value $adrIndexContent -Encoding UTF8
}
Write-ColorOutput "  ✅ ADR 索引创建完成" $Green

# 3. 创建 ADR 模板
$adrTemplatePath = Join-Path $templatesDir "adr-template.md"
$adrTemplateContent = @"
# ADR-XXX: [标题]

## 状态
提议 | 已接受 | 已废弃 | 已替换

## 背景
[描述导致需要做决策的背景]

## 决策
[描述做出的架构决策]

## 结果

### 正面
- [列出正面影响]

### 负面
- [列出负面影响]

### 权衡
- [描述做出的权衡]
"@

if (-not (Test-Path $adrTemplatePath)) {
    Set-Content -Path $adrTemplatePath -Value $adrTemplateContent -Encoding UTF8
}
Write-ColorOutput "  ✅ ADR 模板创建完成" $Green

# 4. 创建 CONTEXT.md（如果不存在）
if (-not $SkipContextGen) {
    Write-ColorOutput "`n生成 CONTEXT.md..." $Yellow
    
    $contextPath = Join-Path $projectRoot "CONTEXT.md"
    
    # 检查是否已存在
    if (Test-Path $contextPath) {
        Write-ColorOutput "  ⚠️  CONTEXT.md 已存在" $Yellow
        $overwrite = Read-Host "  是否覆盖? (y/N)"
        if ($overwrite -ne 'y' -and $overwrite -ne 'Y') {
            Write-ColorOutput "  跳过 CONTEXT.md 生成" $Yellow
        }
        else {
            goto :GenerateContext
        }
    }
    else {
        :GenerateContext
        
        # 读取项目信息
        $projectName = (Get-Item $projectRoot).Name
        $description = "Azure DevOps MCP Server - 开发管理平台"
        
        # 读取 .gitnexus 目录（如果存在）
        $gitnexusExists = Test-Path (Join-Path $projectRoot ".gitnexus")
        
        # 生成 CONTEXT.md
        $contextContent = @"
# 项目上下文

## 项目信息
- **项目名称**: $projectName
- **描述**: $description
- **创建日期**: $(Get-Date -Format 'yyyy-MM-dd')
- **技术栈**: .NET Core, MCP, Azure DevOps API

## 项目目标
本项目旨在通过 MCP (Model Context Protocol) 实现本地开发环境与 Azure DevOps 的无缝集成。

### 核心功能
1. **任务管理**: 通过 MCP 工具管理 Azure DevOps 工作项
2. **状态同步**: 在本地项目与 Azure DevOps 之间同步任务状态
3. **项目映射**: 建立本地项目与 Azure DevOps 项目的映射关系
4. **Windows 集成**: 支持 Windows 集成认证

## 领域模型

### 任务状态
- `NotImplemented` - 未实现
- `Current` - 当前任务
- `Blocked` - 被阻塞
- `Archived` - 已归档

### 状态转换
\`\`\`
NotImplemented → Current → Archived
                  ↓
              Blocked → Current
\`\`\`

## 技术架构

### 核心模块
- **MCP Server**: 提供 MCP 协议接口
- **Services**: 业务逻辑服务层
- **Tools**: MCP 工具实现
- **Models**: 数据模型

### 外部依赖
$gitnexusDep = "- GitNexus: 代码知识图谱"
if (-not $gitnexusExists) {
    $gitnexusDep = "- GitNexus: 代码知识图谱 (可选)"
}
$gitnexusDep
- Azure DevOps REST API
- Windows 集成认证

## 开发规范
详见 [docs/workflow-specification.md](docs/workflow-specification.md)
"@
        
        Set-Content -Path $contextPath -Value $contextContent -Encoding UTF8
        Write-ColorOutput "  ✅ CONTEXT.md 生成完成" $Green
    }
}
else {
    Write-ColorOutput "`n跳过 CONTEXT.md 生成" $Yellow
}

# 5. 创建文档模板目录
Write-ColorOutput "`n创建文档模板..." $Yellow

# Issue 模板
$issueTemplatePath = Join-Path $templatesDir "issue-template.md"
$issueTemplateContent = @"
# Issue: [问题描述]

## 概述
[简要描述要解决的问题]

## 详细信息
[详细说明问题背景和影响]

## 验收标准
- [ ] 标准 1
- [ ] 标准 2
- [ ] 标准 3

## 相关文档
- 相关链接
"@

if (-not (Test-Path $issueTemplatePath)) {
    Set-Content -Path $issueTemplatePath -Value $issueTemplateContent -Encoding UTF8
}

# README 模板
$readmeTemplatePath = Join-Path $templatesDir "readme-template.md"
$readmeTemplateContent = @"
# 模块名称

## 功能描述
[描述模块的主要功能]

## 使用方法
[提供使用示例]

## API 参考
[列出公共 API]

## 配置说明
[描述配置项]
"@

if (-not (Test-Path $readmeTemplatePath)) {
    Set-Content -Path $readmeTemplatePath -Value $readmeTemplateContent -Encoding UTF8
}

Write-ColorOutput "  ✅ 文档模板创建完成" $Green

# 6. 创建 .gitnexus 目录（如果不存在）
Write-ColorOutput "`n检查 GitNexus 目录..." $Yellow
$gitnexusDir = Join-Path $projectRoot ".gitnexus"
if (-not (Test-Path $gitnexusDir)) {
    New-Item -ItemType Directory -Force -Path $gitnexusDir | Out-Null
    Write-ColorOutput "  ✅ .gitnexus 目录创建完成" $Green
}
else {
    Write-ColorOutput "  ✅ .gitnexus 目录已存在" $Green
}

# 7. 更新 docs/README.md
Write-ColorOutput "`n更新 docs/README.md..." $Yellow

$docsReadmePath = Join-Path $docsDir "README.md"
$docsReadmeContent = @"
# 项目文档

## 目录结构

- **adr/**: 架构决策记录 (Architecture Decision Records)
- **issues/**: 问题跟踪和解决方案
- **templates/**: 文档模板

## 快速开始

1. 查看 [workflow-specification.md](workflow-specification.md) 了解开发流程
2. 查看 [../CONTEXT.md](../CONTEXT.md) 了解项目上下文
3. 查看 adr/ 目录了解架构决策

## 贡献指南

### 创建新 ADR
\`\`\`bash
cp docs/templates/adr-template.md docs/adr/XXX-title.md
\`\`\`

### 记录问题
使用 templates/issue-template.md 模板记录问题。
"@

if (-not (Test-Path $docsReadmePath)) {
    Set-Content -Path $docsReadmePath -Value $docsReadmeContent -Encoding UTF8
}

Write-ColorOutput "  ✅ docs/README.md 更新完成" $Green

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    文档初始化完成" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "✅ 文档初始化完成！" $Green
Write-ColorOutput "`n创建的目录结构:" $Cyan
Write-ColorOutput "  docs/" $Yellow
Write-ColorOutput "  ├── adr/" $Yellow
Write-ColorOutput "  │   └── README.md" $White
Write-ColorOutput "  ├── issues/" $Yellow
Write-ColorOutput "  ├── templates/" $Yellow
Write-ColorOutput "  │   ├── adr-template.md" $White
Write-ColorOutput "  │   ├── issue-template.md" $White
Write-ColorOutput "  │   └── readme-template.md" $White
Write-ColorOutput "  └── README.md" $White
Write-ColorOutput "  .gitnexus/" $Yellow
Write-ColorOutput "  CONTEXT.md" $Yellow

exit 0
