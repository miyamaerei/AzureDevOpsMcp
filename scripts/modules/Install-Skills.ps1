#Requires -Version 5.1
<#
.SYNOPSIS
    安装 mattpocock/skills
.DESCRIPTION
    安装 mattpocock 提供的 AI Skills，用于提升 AI 编程助手的代码质量
.EXAMPLE
    .\Install-Skills.ps1
#>

param(
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
Write-ColorOutput "    安装 mattpocock/skills" $Cyan
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

# 检查是否已有 skills 配置
$projectRoot = $PSScriptRoot | Split-Path -Parent | Split-Path -Parent
$skillsConfigPath = Join-Path $projectRoot ".claude\skills.json"
$skillsConfigExists = Test-Path $skillsConfigPath

if ($skillsConfigExists) {
    Write-ColorOutput "`n检查现有 Skills 配置..." $Yellow
    Write-ColorOutput "  ✅ 已存在 .claude/skills.json" $Green
    
    $reinstall = Read-Host "  是否重新安装 Skills? (y/N)"
    if ($reinstall -ne 'y' -and $reinstall -ne 'Y') {
        Write-ColorOutput "  跳过安装" $Yellow
        exit 0
    }
}

# 创建 .claude 目录
$claudeDir = Join-Path $projectRoot ".claude"
if (-not (Test-Path $claudeDir)) {
    Write-ColorOutput "`n创建 .claude 目录..." $Yellow
    New-Item -ItemType Directory -Force -Path $claudeDir | Out-Null
    Write-ColorOutput "  ✅ 目录创建成功" $Green
}

# 安装 mattpocock/skills
Write-ColorOutput "`n安装 mattpocock/skills..." $Yellow
Write-ColorOutput "  使用 npx skills@latest add mattpocock/skills`n" $White

try {
    Set-Location $projectRoot
    
    # 使用 npx 安装 skills
    npx skills@latest add mattpocock/skills 2>&1 | ForEach-Object { Write-ColorOutput "    $_" $White }
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "`n  ✅ mattpocock/skills 安装成功" $Green
        
        # 列出已安装的 skills
        Write-ColorOutput "`n已安装的 Skills:" $Cyan
        $skillsJsonPath = Join-Path $claudeDir "skills.json"
        if (Test-Path $skillsJsonPath) {
            $skillsContent = Get-Content $skillsJsonPath -Raw | ConvertFrom-Json
            if ($skillsContent.skills) {
                $skillsContent.skills | ForEach-Object {
                    Write-ColorOutput "  - $($_.name)" $Green
                }
            }
        }
    } else {
        throw "npx skills install failed with exit code $LASTEXITCODE"
    }
}
catch {
    Write-ColorOutput "  ❌ Skills 安装失败: $_" $Red
    Write-ColorOutput "`n手动安装方法:" $Yellow
    Write-ColorOutput "  1. 在项目根目录运行: npx skills@latest add mattpocock/skills" $White
    Write-ColorOutput "  2. 或者访问: https://skills.mattpocock.com" $White
    exit 1
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置完成" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "✅ Skills 安装完成！" $Green
Write-ColorOutput "`n可用命令:" $Cyan
Write-ColorOutput "  - /skills:list     查看所有可用 Skills" $Yellow
Write-ColorOutput "  - /skills:install mattpocock/skills    安装新 Skill" $Yellow
Write-ColorOutput "  - /skills:remove   移除 Skill" $Yellow

exit 0
