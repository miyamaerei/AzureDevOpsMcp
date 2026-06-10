#Requires -Version 5.1
<#
.SYNOPSIS
    配置 MCP 连接
.DESCRIPTION
    配置 MCP Server 连接信息，包括 Claude Code 和 Cursor 的 MCP 配置
.EXAMPLE
    .\Config-Mcp.ps1
#>

param(
    [string]$McpServerUrl = "http://localhost:5000",
    [string]$PatToken = "",
    [switch]$SkipPatSetup,
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
Write-ColorOutput "    配置 MCP 连接" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$projectRoot = $PSScriptRoot | Split-Path -Parent | Split-Path -Parent

# 1. 配置 MCP Server URL
if ([string]::IsNullOrEmpty($McpServerUrl)) {
    Write-ColorOutput "输入 MCP Server URL:" $Yellow
    Write-ColorOutput "  (默认: http://localhost:5000)" $White
    $mcpUrlInput = Read-Host "  "
    $McpServerUrl = if ([string]::IsNullOrEmpty($mcpUrlInput)) { "http://localhost:5000" } else { $mcpUrlInput }
}

Write-ColorOutput "MCP Server URL: $McpServerUrl" $Green

# 2. 配置 PAT Token
if ($SkipPatSetup) {
    Write-ColorOutput "`n跳过 PAT Token 配置" $Yellow
    $PatToken = $null
}
elseif ([string]::IsNullOrEmpty($PatToken)) {
    Write-ColorOutput "`n配置 Azure DevOps PAT Token:" $Yellow
    Write-ColorOutput "  ⚠️  注意: Token 需要具有 Work Items (read/write) 和 Projects (read) 权限" $Red
    
    # 尝试读取环境变量
    $envPat = $env:AZURE_DEVOPS_PAT
    if (-not [string]::IsNullOrEmpty($envPat)) {
        $useEnv = Read-Host "  发现环境变量 AZURE_DEVOPS_PAT，是否使用? (Y/n)"
        if ($useEnv -ne 'n' -and $useEnv -ne 'N') {
            $PatToken = $envPat
            Write-ColorOutput "  ✅ 使用环境变量中的 PAT Token" $Green
        }
    }
    
    if ([string]::IsNullOrEmpty($PatToken)) {
        $securePat = Read-Host "  输入 PAT Token (输入后不可见)" -AsSecureString
        $PatToken = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePat))
        
        # 保存到环境变量
        $saveChoice = Read-Host "  是否保存到环境变量 AZURE_DEVOPS_PAT? (y/N)"
        if ($saveChoice -eq 'y' -or $saveChoice -eq 'Y') {
            [System.Environment]::SetEnvironmentVariable("AZURE_DEVOPS_PAT", $PatToken, "User")
            Write-ColorOutput "  ✅ 已保存到用户环境变量" $Green
        }
    }
}
else {
    Write-ColorOutput "`n✅ 使用提供的 PAT Token" $Green
}

# 3. 创建 Claude Code 配置
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置 Claude Code MCP" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$claudeSettingsPath = Join-Path $projectRoot ".claude\settings.json"
$claudeSettingsDir = Split-Path $claudeSettingsPath

# 创建 .claude 目录
if (-not (Test-Path $claudeSettingsDir)) {
    New-Item -ItemType Directory -Force -Path $claudeSettingsDir | Out-Null
}

# Claude Code MCP 配置
$claudeMcpConfig = @{
    mcpServers = @{
        azure-devops = @{
            command = "npx"
            args = @(
                "-y"
                "@modelcontextprotocol/server-azure-devops"
                "--accessToken"
                $PatToken
            )
            env = @{
                AZURE_DEVOPS_ORGANIZATION = "your-org"
            }
        }
        local-mcp = @{
            url = "$McpServerUrl/mcp"
            description = "本地 Azure DevOps MCP Server"
        }
    }
} | ConvertTo-Json -Depth 5

Write-ColorOutput "写入配置到: .claude\settings.json`n" $Yellow
Set-Content -Path $claudeSettingsPath -Value $claudeMcpConfig -Encoding UTF8
Write-ColorOutput "✅ Claude Code MCP 配置完成" $Green

# 4. 创建 Cursor MCP 配置
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置 Cursor MCP" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$cursorMcpPath = Join-Path $projectRoot ".cursor\mcp.json"
$cursorMcpDir = Split-Path $cursorMcpPath

# 创建 .cursor 目录
if (-not (Test-Path $cursorMcpDir)) {
    New-Item -ItemType Directory -Force -Path $cursorMcpDir | Out-Null
}

# Cursor MCP 配置
$cursorMcpConfig = @{
    mcpServers = @{
        azure-devops = @{
            command = "npx"
            args = @(
                "-y"
                "@modelcontextprotocol/server-azure-devops"
                "--accessToken"
                $PatToken
            )
            env = @{
                AZURE_DEVOPS_ORGANIZATION = "your-org"
            }
        }
        local-mcp = @{
            url = "$McpServerUrl/mcp"
            description = "本地 Azure DevOps MCP Server"
        }
    }
} | ConvertTo-Json -Depth 5

Write-ColorOutput "写入配置到: .cursor\mcp.json`n" $Yellow
Set-Content -Path $cursorMcpPath -Value $cursorMcpConfig -Encoding UTF8
Write-ColorOutput "✅ Cursor MCP 配置完成" $Green

# 5. 创建本地 MCP Server 配置
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置本地 MCP Server" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$mcpConfigPath = Join-Path $projectRoot "config\mcp-config.json"
$mcpConfigDir = Split-Path $mcpConfigPath

# 创建 config 目录
if (-not (Test-Path $mcpConfigDir)) {
    New-Item -ItemType Directory -Force -Path $mcpConfigDir | Out-Null
}

# MCP Server 配置
$mcpServerConfig = @{
    server = @{
        url = $McpServerUrl
        healthEndpoint = "$McpServerUrl/health"
    }
    auth = @{
        type = "pat"
        tokenEnvVar = "AZURE_DEVOPS_PAT"
    }
    windowsIntegration = @{
        enabled = $true
        authenticationMethod = "integrated"
    }
} | ConvertTo-Json -Depth 5

Write-ColorOutput "写入配置到: config\mcp-config.json`n" $Yellow
Set-Content -Path $mcpConfigPath -Value $mcpServerConfig -Encoding UTF8
Write-ColorOutput "✅ 本地 MCP Server 配置完成" $Green

# 6. 创建 .gitignore
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    更新 .gitignore" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$gitignorePath = Join-Path $projectRoot ".gitignore"
$gitignoreContent = @"
# GitNexus
.gitnexus/

# MCP Config (local overrides)
config/local-*.json

# Azure DevOps PAT
.env
"@

if (Test-Path $gitignorePath) {
    $existingContent = Get-Content $gitignorePath -Raw
    if ($existingContent -notmatch '.gitnexus/') {
        Write-ColorOutput "添加 .gitnexus/ 到 .gitignore..." $Yellow
        Add-Content -Path $gitignorePath -Value "`n# GitNexus`n.gitnexus/"
    }
}
else {
    Write-ColorOutput "创建 .gitignore..." $Yellow
    Set-Content -Path $gitignorePath -Value $gitignoreContent -Encoding UTF8
}
Write-ColorOutput "✅ .gitignore 更新完成" $Green

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置完成" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "✅ MCP 配置完成！" $Green
Write-ColorOutput "`n后续步骤:" $Cyan
Write-ColorOutput "  1. 重启 Claude Code / Cursor 以加载新配置" $Yellow
Write-ColorOutput "  2. 配置 Azure DevOps 组织名称" $Yellow
Write-ColorOutput "  3. 运行 init-project.ps1 配置项目映射" $Yellow

exit 0
