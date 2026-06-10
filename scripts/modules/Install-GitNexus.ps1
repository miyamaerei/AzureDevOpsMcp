#Requires -Version 5.1
<#
.SYNOPSIS
    安装 GitNexus 并运行代码索引
.DESCRIPTION
    安装 GitNexus 代码知识图谱引擎并运行初始代码索引
.EXAMPLE
    .\Install-GitNexus.ps1
#>

param(
    [switch]$SkipAnalyze,
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

function Test-GitNexusInstalled {
    $null -ne (Get-Command gitnexus -ErrorAction SilentlyContinue)
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    安装 GitNexus" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

# 检查 Node.js
Write-ColorOutput "检查 Node.js..." $Yellow
$nodeVersion = node --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "  ❌ Node.js 未安装" $Red
    Write-ColorOutput "    请先运行 Check-Deps.ps1 检查依赖" $Yellow
    exit 1
}
Write-ColorOutput "  ✅ Node.js $nodeVersion" $Green

# 检查是否已安装
Write-ColorOutput "`n检查 GitNexus 安装状态..." $Yellow
if (Test-GitNexusInstalled) {
    $gitnexusVersion = gitnexus --version 2>&1
    Write-ColorOutput "  ✅ GitNexus 已安装: $gitnexusVersion" $Green
    
    $reinstall = Read-Host "  是否重新安装? (y/N)"
    if ($reinstall -ne 'y' -and $reinstall -ne 'Y') {
        Write-ColorOutput "  跳过安装" $Yellow
        
        if (-not $SkipAnalyze) {
            goto :RunAnalyze
        }
        exit 0
    }
}

# 安装 GitNexus
Write-ColorOutput "`n安装 GitNexus..." $Yellow
try {
    npm install -g gitnexus 2>&1 | ForEach-Object { Write-ColorOutput "  $_" $White }
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "  ✅ GitNexus 安装成功" $Green
        
        # 验证安装
        $installedVersion = gitnexus --version 2>&1
        Write-ColorOutput "  安装版本: $installedVersion" $Green
    } else {
        throw "npm install failed with exit code $LASTEXITCODE"
    }
}
catch {
    Write-ColorOutput "  ❌ GitNexus 安装失败: $_" $Red
    exit 1
}

# 运行代码索引
:RunAnalyze
if (-not $SkipAnalyze) {
    Write-ColorOutput "`n==============================================" $Cyan
    Write-ColorOutput "    运行 GitNexus 代码索引" $Cyan
    Write-ColorOutput "==============================================`n" $Cyan
    
    $projectRoot = $PSScriptRoot | Split-Path -Parent | Split-Path -Parent
    
    Write-ColorOutput "正在索引代码库..." $Yellow
    Write-ColorOutput "  项目路径: $projectRoot`n" $White
    
    try {
        Set-Location $projectRoot
        
        Write-ColorOutput "  运行 gitnexus analyze..." $Yellow
        gitnexus analyze 2>&1 | ForEach-Object { Write-ColorOutput "    $_" $White }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "`n  ✅ 代码索引完成" $Green
        } else {
            Write-ColorOutput "`n  ⚠️  代码索引完成但有警告" $Yellow
        }
    }
    catch {
        Write-ColorOutput "  ⚠️  代码索引失败: $_" $Yellow
        Write-ColorOutput "    可以稍后手动运行: gitnexus analyze" $Yellow
    }
}
else {
    Write-ColorOutput "`n跳过代码索引 (--SkipAnalyze)" $Yellow
}

Write-ColorOutput "`n✅ GitNexus 安装完成！" $Green
Write-ColorOutput "`n后续步骤:" $Cyan
Write-ColorOutput "  1. 使用 gitnexus context() 查看代码依赖关系" $Yellow
Write-ColorOutput "  2. 使用 gitnexus impact() 分析代码修改影响范围" $Yellow

exit 0
