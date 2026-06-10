<#
.SYNOPSIS
Initialize Azure DevOps MCP Server environment
#>

param (
    [ValidateSet("Stdio", "Http")]
    [string]$TransportMode = "Stdio",
    [int]$HttpPort = 5000,
    [bool]$RequireAuth = $true,
    [int]$SyncIntervalMinutes = 5,
    [bool]$AutoSyncOnArchive = $true
)

$ErrorActionPreference = "Stop"

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
    $originalColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Host $Message
    $Host.UI.RawUI.ForegroundColor = $originalColor
}

function Test-Elevation {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Elevation)) {
    Write-ColorOutput "Warning: It is recommended to run this script as Administrator" $Yellow
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    Azure DevOps MCP Server Initialization" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "Setting environment variables..." $Yellow

[Environment]::SetEnvironmentVariable("MCP_TRANSPORT_MODE", $TransportMode, "Process")
Write-ColorOutput "  - MCP_TRANSPORT_MODE: $TransportMode" $Green

if ($TransportMode -eq "Http") {
    [Environment]::SetEnvironmentVariable("MCP_HTTP_PORT", $HttpPort.ToString(), "Process")
    Write-ColorOutput "  - MCP_HTTP_PORT: $HttpPort" $Green
}

[Environment]::SetEnvironmentVariable("MCP_REQUIRE_AUTH", $RequireAuth.ToString().ToLower(), "Process")
Write-ColorOutput "  - MCP_REQUIRE_AUTH: $RequireAuth" $Green

[Environment]::SetEnvironmentVariable("TASK_SYNC_INTERVAL_MINUTES", $SyncIntervalMinutes.ToString(), "Process")
Write-ColorOutput "  - TASK_SYNC_INTERVAL_MINUTES: $SyncIntervalMinutes" $Green

[Environment]::SetEnvironmentVariable("TASK_SYNC_AUTO_ON_ARCHIVE", $AutoSyncOnArchive.ToString().ToLower(), "Process")
Write-ColorOutput "  - TASK_SYNC_AUTO_ON_ARCHIVE: $AutoSyncOnArchive" $Green

Write-ColorOutput "`nEnvironment variables set successfully!" $Green

Write-ColorOutput "`nChecking .NET runtime..." $Yellow
try {
    $dotnetVersion = & dotnet --version
    if (-not $?) {
        throw "Failed to detect .NET runtime"
    }
    Write-ColorOutput "  - .NET Version: $dotnetVersion" $Green
}
catch {
    Write-ColorOutput "  - Error: $_" $Red
    Write-ColorOutput "Please install .NET 10.0 or later" $Red
    exit 1
}

Write-ColorOutput "`nBuilding project..." $Yellow
$projectPath = Join-Path $PSScriptRoot ".." "src" "AzureDevOpsMcpServer"
Set-Location $projectPath

try {
    & dotnet build --configuration Release
    if (-not $?) {
        throw "Build failed"
    }
    Write-ColorOutput "  - Build successful" $Green
}
catch {
    Write-ColorOutput "  - Build failed: $_" $Red
    exit 1
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    Initialization Complete!" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

Write-ColorOutput "Startup Command:" $Yellow
if ($TransportMode -eq "Http") {
    Write-ColorOutput "  cd `"$projectPath`"" $White
    Write-ColorOutput "  dotnet run`n" $White
    Write-ColorOutput "Service will start at http://localhost:$HttpPort" $Green
} else {
    Write-ColorOutput "  cd `"$projectPath`"" $White
    Write-ColorOutput "  dotnet run`n" $White
    Write-ColorOutput "Service will start in Stdio mode" $Green
}

Write-ColorOutput "Environment Variables:" $Yellow
Write-ColorOutput "  - MCP_TRANSPORT_MODE: Transport mode (Stdio/Http)" $White
Write-ColorOutput "  - MCP_HTTP_PORT: HTTP port (Http mode only)" $White
Write-ColorOutput "  - MCP_REQUIRE_AUTH: Require authentication" $White
Write-ColorOutput "  - TASK_SYNC_INTERVAL_MINUTES: Sync interval in minutes" $White
Write-ColorOutput "  - TASK_SYNC_AUTO_ON_ARCHIVE: Auto sync on archive" $White

Write-ColorOutput "`n==============================================`n" $Cyan