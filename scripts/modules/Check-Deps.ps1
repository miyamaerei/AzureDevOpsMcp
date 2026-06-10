#Requires -Version 5.1
<#
.SYNOPSIS
    Check system dependencies
.DESCRIPTION
    Check if Node.js, Git, and other dependencies are installed
.EXAMPLE
    .\Check-Deps.ps1
#>

param(
    [switch]$Silent
)

$ErrorActionPreference = "Stop"

# Color definitions
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

function Test-CommandExists {
    param([string]$Command)
    $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Get-CommandVersion {
    param([string]$Command, [string]$VersionRegex)
    try {
        $output = & $Command --version 2>&1
        if ($output -match $VersionRegex) {
            return $matches[1]
        }
        return $output.ToString().Trim()
    }
    catch {
        return $null
    }
}

Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    System Dependency Check" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$allPassed = $true
$deps = @()

# Check Node.js
Write-ColorOutput "Checking Node.js..." $Yellow
if (Test-CommandExists "node") {
    $nodeVersion = Get-CommandVersion "node" "v?(\d+\.\d+\.\d+)"
    if ($nodeVersion) {
        $versionParts = $nodeVersion -split '\.'
        $majorVersion = [int]$versionParts[0]
        if ($majorVersion -ge 18) {
            Write-ColorOutput "  [OK] Node.js v$nodeVersion" $Green
            $deps += @{Name="Node.js"; Version=$nodeVersion; Status="OK"}
        } else {
            Write-ColorOutput "  [FAIL] Node.js v$nodeVersion (requires >= 18.0.0)" $Red
            $allPassed = $false
            $deps += @{Name="Node.js"; Version=$nodeVersion; Status="FAIL"}
        }
    } else {
        Write-ColorOutput "  [WARN] Node.js installed but version not detected" $Yellow
        $deps += @{Name="Node.js"; Version="Unknown"; Status="WARN"}
    }
} else {
    Write-ColorOutput "  [FAIL] Node.js not installed" $Red
    Write-ColorOutput "    Please install from https://nodejs.org (Node.js >= 18.0.0)" $Yellow
    $allPassed = $false
    $deps += @{Name="Node.js"; Version="Not installed"; Status="FAIL"}
}

# Check Git
Write-ColorOutput "`nChecking Git..." $Yellow
if (Test-CommandExists "git") {
    $gitVersion = Get-CommandVersion "git" "git version ([\d\.]+)"
    if ($gitVersion) {
        Write-ColorOutput "  [OK] Git v$gitVersion" $Green
        $deps += @{Name="Git"; Version=$gitVersion; Status="OK"}
    } else {
        Write-ColorOutput "  [OK] Git installed" $Green
        $deps += @{Name="Git"; Version="Unknown"; Status="OK"}
    }
} else {
    Write-ColorOutput "  [FAIL] Git not installed" $Red
    Write-ColorOutput "    Please install from https://git-scm.com" $Yellow
    $allPassed = $false
    $deps += @{Name="Git"; Version="Not installed"; Status="FAIL"}
}

# Check npm
Write-ColorOutput "`nChecking npm..." $Yellow
if (Test-CommandExists "npm") {
    $npmVersion = Get-CommandVersion "npm" "(\d+\.\d+\.\d+)"
    if ($npmVersion) {
        Write-ColorOutput "  [OK] npm v$npmVersion" $Green
        $deps += @{Name="npm"; Version=$npmVersion; Status="OK"}
    } else {
        Write-ColorOutput "  [OK] npm installed" $Green
        $deps += @{Name="npm"; Version="Unknown"; Status="OK"}
    }
} else {
    Write-ColorOutput "  [WARN] npm not installed (part of Node.js)" $Yellow
    $deps += @{Name="npm"; Version="Unknown"; Status="WARN"}
}

# Check .NET
Write-ColorOutput "`nChecking .NET..." $Yellow
if (Test-CommandExists "dotnet") {
    $dotnetVersion = Get-CommandVersion "dotnet" "(\d+\.\d+\.\d+)"
    if ($dotnetVersion) {
        Write-ColorOutput "  [OK] .NET v$dotnetVersion" $Green
        $deps += @{Name=".NET"; Version=$dotnetVersion; Status="OK"}
    } else {
        Write-ColorOutput "  [OK] .NET SDK installed" $Green
        $deps += @{Name=".NET"; Version="Unknown"; Status="OK"}
    }
} else {
    Write-ColorOutput "  [WARN] .NET SDK not installed (required for MCP Server)" $Yellow
    $deps += @{Name=".NET"; Version="Not installed"; Status="WARN"}
}

# Summary
Write-ColorOutput "`n==============================================" $Cyan
Write-ColorOutput "    Dependency Check Summary" $Cyan
Write-ColorOutput "==============================================`n" $Cyan

$deps | ForEach-Object {
    # Determine status text
    $statusText = "[FAIL]"
    if ($_.Status -eq "OK") { 
        $statusText = "[OK]" 
    } elseif ($_.Status -eq "WARN") { 
        $statusText = "[WARN]" 
    }
    
    # Determine color
    $statusColor = $Red
    if ($_.Status -eq "OK") { 
        $statusColor = $Green 
    } elseif ($_.Status -eq "WARN") { 
        $statusColor = $Yellow 
    }
    
    Write-ColorOutput "$statusText $($_.Name): $($_.Version)" $statusColor
}

if ($allPassed) {
    Write-ColorOutput "`n[OK] All required dependencies are installed!" $Green
    exit 0
} else {
    Write-ColorOutput "`n[FAIL] Some dependencies are not installed or version not met" $Red
    exit 1
}
