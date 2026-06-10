#Requires -Version 5.1
<#
.SYNOPSIS
    配置项目与 Azure DevOps 项目的映射关系
.DESCRIPTION
    交互式配置本地项目与 Azure DevOps 项目的映射
.EXAMPLE
    .\Config-Project.ps1
#>

param(
    [string]$LocalProjectName = "",
    [string]$AzureProjectId = "",
    [string]$AzureProjectName = "",
    [string]$AzureOrgUrl = "",
    [string]$WorkingDir = "",
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
Write-ColorOutput "    配置项目映射" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$projectRoot = $PSScriptRoot | Split-Path -Parent | Split-Path -Parent

# 1. 确定本地项目名称
if ([string]::IsNullOrEmpty($LocalProjectName)) {
    Write-ColorOutput "输入本地项目名称:" $Yellow
    Write-ColorOutput "  (默认: 当前目录名称)" $White
    $inputProjectName = Read-Host "  "
    $LocalProjectName = if ([string]::IsNullOrEmpty($inputProjectName)) { (Get-Item $projectRoot).Name } else { $inputProjectName }
}

Write-ColorOutput "本地项目名称: $LocalProjectName" $Green

# 2. 确定工作目录
if ([string]::IsNullOrEmpty($WorkingDir)) {
    $WorkingDir = $projectRoot
    $confirmDir = Read-Host "  工作目录: $WorkingDir (按 Enter 确认或输入新路径)"
    if (-not [string]::IsNullOrEmpty($confirmDir)) {
        $WorkingDir = $confirmDir
    }
}

Write-ColorOutput "工作目录: $WorkingDir" $Green

# 3. 获取 Azure DevOps 组织 URL
if ([string]::IsNullOrEmpty($AzureOrgUrl)) {
    Write-ColorOutput "`n输入 Azure DevOps 组织 URL:" $Yellow
    Write-ColorOutput "  格式: https://dev.azure.com/{organization}" $White
    $AzureOrgUrl = Read-Host "  "
}

if ([string]::IsNullOrEmpty($AzureOrgUrl)) {
    Write-ColorOutput "  ❌ Azure DevOps 组织 URL 不能为空" $Red
    exit 1
}

Write-ColorOutput "Azure DevOps 组织: $AzureOrgUrl" $Green

# 4. 获取 Azure DevOps 项目 ID
if ([string]::IsNullOrEmpty($AzureProjectId)) {
    Write-ColorOutput "`n输入 Azure DevOps 项目 ID:" $Yellow
    Write-ColorOutput "  (可以在项目设置中找到)" $White
    $AzureProjectId = Read-Host "  "
}

if ([string]::IsNullOrEmpty($AzureProjectId)) {
    Write-ColorOutput "  ❌ Azure DevOps 项目 ID 不能为空" $Red
    exit 1
}

Write-ColorOutput "Azure DevOps 项目 ID: $AzureProjectId" $Green

# 5. 获取 Azure DevOps 项目名称
if ([string]::IsNullOrEmpty($AzureProjectName)) {
    $AzureProjectName = $AzureProjectId
    $confirmName = Read-Host "`nAzure DevOps 项目名称 (默认: $AzureProjectName)"
    if (-not [string]::IsNullOrEmpty($confirmName)) {
        $AzureProjectName = $confirmName
    }
}

Write-ColorOutput "Azure DevOps 项目名称: $AzureProjectName" $Green

# 6. 询问是否设置为默认项目
$isDefault = $false
$setDefault = Read-Host "`n是否设置为默认项目? (Y/n)"
if ($setDefault -ne 'n' -and $setDefault -ne 'N') {
    $isDefault = $true
}

$defaultColor = $Yellow
if ($isDefault) {
    $defaultColor = $Green
}
Write-ColorOutput "默认项目: $isDefault" $defaultColor

# 7. 创建映射配置
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    创建项目映射配置" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

# 创建 config 目录
$configDir = Join-Path $projectRoot "config"
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Force -Path $configDir | Out-Null
}

# 读取或创建映射文件
$mappingFilePath = Join-Path $configDir "project-mapping.json"
if (Test-Path $mappingFilePath) {
    Write-ColorOutput "更新现有映射配置..." $Yellow
    $existingMappings = Get-Content $mappingFilePath -Raw | ConvertFrom-Json
    $mappings = @($existingMappings)
}
else {
    Write-ColorOutput "创建新映射配置..." $Yellow
    $mappings = @()
}

# 如果设置为默认，先取消其他默认
if ($isDefault) {
    $mappings = @($mappings | Where-Object { $_.isDefault -ne $true })
}

# 添加新映射
$newMapping = @{
    localProjectName = $LocalProjectName
    azureDevOpsProjectId = $AzureProjectId
    azureDevOpsProjectName = $AzureProjectName
    azureDevOpsOrganizationUrl = $AzureOrgUrl
    workingDirectory = $WorkingDir
    isDefault = $isDefault
    createdAt = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
}

$mappings += $newMapping

# 保存配置
$mappingConfig = @{
    version = "1.0"
    mappings = $mappings
    lastUpdated = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json -Depth 5

Write-ColorOutput "写入配置到: config\project-mapping.json`n" $Yellow
Set-Content -Path $mappingFilePath -Value $mappingConfig -Encoding UTF8
Write-ColorOutput "✅ 项目映射配置完成" $Green

# 8. 创建用户配置（存储用户 ID）
$userConfigPath = Join-Path $configDir "user-config.json"
$userConfigTemplate = @{
    azureDevOps = @{
        organizationUrl = $AzureOrgUrl
        defaultProjectId = $AzureProjectId
        userId = ""  # 用户需要在首次运行时配置
    }
    projectDefaults = @{
        localProjectName = $LocalProjectName
    }
} | ConvertTo-Json -Depth 5

Write-ColorOutput "`n创建用户配置文件..." $Yellow
Set-Content -Path $userConfigPath -Value $userConfigTemplate -Encoding UTF8
Write-ColorOutput "✅ 用户配置文件创建完成" $Green

# 9. 提示配置用户 ID
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置用户信息" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "⚠️  请配置您的 Azure DevOps 用户 ID" $Yellow
Write-ColorOutput "  可以在 Azure DevOps 中访问: https://aex.dev.azure.com/me" $White
Write-ColorOutput "  或运行: az ad signed-in-user show" $White

$userId = Read-Host "  输入您的 Azure DevOps 用户 ID (或按 Enter 稍后配置)"
if (-not [string]::IsNullOrEmpty($userId)) {
    $userConfig = Get-Content $userConfigPath -Raw | ConvertFrom-Json
    $userConfig.azureDevOps.userId = $userId
    $updatedConfig = $userConfig | ConvertTo-Json -Depth 5
    Set-Content -Path $userConfigPath -Value $updatedConfig -Encoding UTF8
    Write-ColorOutput "✅ 用户 ID 已保存" $Green
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    配置完成" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "✅ 项目映射配置完成！" $Green
Write-ColorOutput "`n配置摘要:" $Cyan
Write-ColorOutput "  本地项目: $LocalProjectName" $Yellow
Write-ColorOutput "  Azure 项目: $AzureProjectName" $Yellow
Write-ColorOutput "  组织: $AzureOrgUrl" $Yellow
$defaultColor = $Yellow
if ($isDefault) {
    $defaultColor = $Green
}
Write-ColorOutput "  默认项目: $isDefault" $defaultColor

Write-ColorOutput "`n后续步骤:" $Cyan
Write-ColorOutput "  1. 运行 GetAssignedTasks 查看分配给您的任务" $Yellow
Write-ColorOutput "  2. 使用 UpdateTaskStatus 更新任务状态" $Yellow
Write-ColorOutput "  3. 使用 QueryWorkItems 查询工作项" $Yellow

exit 0
