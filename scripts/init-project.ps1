#Requires -Version 5.1
<#
.SYNOPSIS
    项目初始化主脚本
.DESCRIPTION
    交互式向导，引导用户完成项目初始化全过程
.EXAMPLE
    .\init-project.ps1
.EXAMPLE
    # 跳过某些步骤
    .\init-project.ps1 -SkipGitNexus -SkipSkills
.EXAMPLE
    # 仅检查依赖
    .\init-project.ps1 -Step deps
#>

param(
    [switch]$SkipGitNexus,
    [switch]$SkipSkills,
    [switch]$SkipMcp,
    [switch]$SkipProjectMapping,
    [switch]$SkipDocs,
    [string]$Step = "",
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

function Write-Banner {
    Write-ColorOutput "`n" $Cyan
    Write-ColorOutput "╔══════════════════════════════════════════════════════╗" $Cyan
    Write-ColorOutput "║                                                      ║" $Cyan
    Write-ColorOutput "║            开发管理平台 - 项目初始化                  ║" $Cyan
    Write-ColorOutput "║                                                      ║" $Cyan
    Write-ColorOutput "╚══════════════════════════════════════════════════════╝" $Cyan
    Write-ColorOutput "`n" $Cyan
}

function Write-StepHeader {
    param([int]$Step, [string]$Title)
    Write-ColorOutput "`n┌─────────────────────────────────────────────────────┐" $Cyan
    Write-ColorOutput "│  步骤 $Step : $Title" $Cyan
    Write-ColorOutput "└─────────────────────────────────────────────────────┘`n" $Cyan
}

function Invoke-ScriptModule {
    param(
        [string]$ScriptName,
        [hashtable]$Params = @{}
    )
    
    $scriptPath = Join-Path $PSScriptRoot "modules\$ScriptName"
    
    if (-not (Test-Path $scriptPath)) {
        Write-ColorOutput "  ❌ 脚本未找到: $scriptPath" $Red
        return $false
    }
    
    try {
        $paramString = $Params.Keys | ForEach-Object { 
            if ($Params[$_] -is [switch]) { 
                if ($Params[$_]) { "-$_" } 
            } 
            else { "-$_ `"$($Params[$_])`"" }
        }
        
        Write-ColorOutput "  执行: .\modules\$ScriptName $paramString" $White
        
        $process = Start-Process -FilePath "pwsh" -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $scriptPath, $paramString -NoNewWindow -Wait -PassThru
        
        if ($process.ExitCode -eq 0) {
            Write-ColorOutput "  ✅ 完成`n" $Green
            return $true
        }
        else {
            Write-ColorOutput "  ⚠️  完成但有警告`n" $Yellow
            return $false
        }
    }
    catch {
        Write-ColorOutput "  ❌ 执行失败: $_`n" $Red
        return $false
    }
}

# 主脚本
$scriptRoot = $PSScriptRoot

Write-Banner

Write-ColorOutput "欢迎使用开发管理平台项目初始化向导！`n" $Cyan
Write-ColorOutput "此脚本将帮助您完成以下初始化步骤:`n" $White
Write-ColorOutput "  1. 检查系统依赖 (Node.js, Git, .NET)" $Yellow
Write-ColorOutput "  2. 安装 GitNexus 代码知识图谱" $Yellow
Write-ColorOutput "  3. 安装 mattpocock/skills" $Yellow
Write-ColorOutput "  4. 配置 MCP 连接" $Yellow
Write-ColorOutput "  5. 配置项目映射" $Yellow
Write-ColorOutput "  6. 初始化项目文档" $Yellow

$confirm = Read-Host "`n按 Enter 开始初始化 (或输入 'q' 退出)"
if ($confirm -eq 'q' -or $confirm -eq 'Q') {
    Write-ColorOutput "`n已取消初始化`n" $Yellow
    exit 0
}

# 步骤计数器
$stepNum = 0
$successCount = 0

# ============================================
# 步骤 1: 检查系统依赖
# ============================================
if ([string]::IsNullOrEmpty($Step) -or $Step -eq "deps") {
    $stepNum++
    Write-StepHeader $stepNum "检查系统依赖"
    
    if (Invoke-ScriptModule "Check-Deps.ps1") {
        $successCount++
    }
    
    if (-not $Silent) {
        $continue = Read-Host "按 Enter 继续 (或输入 'q' 退出)"
        if ($continue -eq 'q' -or $continue -eq 'Q') {
            goto :End
        }
    }
}

# ============================================
# 步骤 2: 安装 GitNexus
# ============================================
if ((-not $SkipGitNexus) -and ([string]::IsNullOrEmpty($Step) -or $Step -eq "gitnexus")) {
    $stepNum++
    Write-StepHeader $stepNum "安装 GitNexus"
    
    if (Invoke-ScriptModule "Install-GitNexus.ps1" @{ SkipAnalyze = $true }) {
        $successCount++
    }
    
    if (-not $Silent) {
        $continue = Read-Host "按 Enter 继续 (或输入 'q' 退出)"
        if ($continue -eq 'q' -or $continue -eq 'Q') {
            goto :End
        }
    }
}

# ============================================
# 步骤 3: 安装 Skills
# ============================================
if ((-not $SkipSkills) -and ([string]::IsNullOrEmpty($Step) -or $Step -eq "skills")) {
    $stepNum++
    Write-StepHeader $stepNum "安装 mattpocock/skills"
    
    if (Invoke-ScriptModule "Install-Skills.ps1") {
        $successCount++
    }
    
    if (-not $Silent) {
        $continue = Read-Host "按 Enter 继续 (或输入 'q' 退出)"
        if ($continue -eq 'q' -or $continue -eq 'Q') {
            goto :End
        }
    }
}

# ============================================
# 步骤 4: 配置 MCP
# ============================================
if ((-not $SkipMcp) -and ([string]::IsNullOrEmpty($Step) -or $Step -eq "mcp")) {
    $stepNum++
    Write-StepHeader $stepNum "配置 MCP 连接"
    
    if (Invoke-ScriptModule "Config-Mcp.ps1" @{ SkipPatSetup = $true }) {
        $successCount++
    }
    
    if (-not $Silent) {
        $continue = Read-Host "按 Enter 继续 (或输入 'q' 退出)"
        if ($continue -eq 'q' -or $continue -eq 'Q') {
            goto :End
        }
    }
}

# ============================================
# 步骤 5: 配置项目映射
# ============================================
if ((-not $SkipProjectMapping) -and ([string]::IsNullOrEmpty($Step) -or $Step -eq "mapping")) {
    $stepNum++
    Write-StepHeader $stepNum "配置项目映射"
    
    if (Invoke-ScriptModule "Config-Project.ps1") {
        $successCount++
    }
    
    if (-not $Silent) {
        $continue = Read-Host "按 Enter 继续 (或输入 'q' 退出)"
        if ($continue -eq 'q' -or $continue -eq 'Q') {
            goto :End
        }
    }
}

# ============================================
# 步骤 6: 初始化文档
# ============================================
if ((-not $SkipDocs) -and ([string]::IsNullOrEmpty($Step) -or $Step -eq "docs")) {
    $stepNum++
    Write-StepHeader $stepNum "初始化项目文档"
    
    if (Invoke-ScriptModule "Setup-Docs.ps1") {
        $successCount++
    }
}

# ============================================
# 结束
# ============================================
:End

Write-ColorOutput "`n" $Cyan
Write-ColorOutput "╔══════════════════════════════════════════════════════╗" $Cyan
Write-ColorOutput "║                    初始化完成                        ║" $Cyan
Write-ColorOutput "╚══════════════════════════════════════════════════════╝" $Cyan

Write-ColorOutput "`n初始化摘要:" $Cyan
Write-ColorOutput "  完成步骤: $successCount / $stepNum" $(if ($successCount -eq $stepNum) { $Green } else { $Yellow })

Write-ColorOutput "`n后续步骤:" $Cyan
Write-ColorOutput "  1. 配置 Azure DevOps PAT Token (环境变量 AZURE_DEVOPS_PAT)" $Yellow
Write-ColorOutput "  2. 重启 Claude Code / Cursor 以加载 MCP 配置" $Yellow
Write-ColorOutput "  3. 运行 gitnexus analyze 进行代码索引" $Yellow
Write-ColorOutput "  4. 查看 CONTEXT.md 了解项目上下文" $Yellow

Write-ColorOutput "`n分步执行命令:" $Cyan
Write-ColorOutput "  # 仅检查依赖" $White
Write-ColorOutput "  .\scripts\init-project.ps1 -Step deps" $White
Write-ColorOutput "`n  # 仅配置项目映射" $White
Write-ColorOutput "  .\scripts\modules\Config-Project.ps1" $White

Write-ColorOutput "`n✅ 项目初始化向导结束！`n" $Green

exit 0
