#!/bin/bash

# Check system dependencies
# Description: Check if Node.js, Git, and other dependencies are installed
# Usage: ./check-deps.sh [--silent]

set -e

# Parse arguments
SILENT=false
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --silent) SILENT=true ;;
        *) echo "Unknown parameter: $1"; exit 1 ;;
    esac
    shift
done

# Color definitions
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_color() {
    local message="$1"
    local color="$2"
    
    if [ "$SILENT" = false ]; then
        echo -e "${color}${message}${NC}"
    fi
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to get command version
get_version() {
    local cmd="$1"
    local regex="$2"
    
    if command_exists "$cmd"; then
        local version_output
        version_output=$("$cmd" --version 2>&1 || echo "")
        
        if [ -n "$regex" ]; then
            echo "$version_output" | grep -oE "$regex" | head -n1
        else
            echo "$version_output" | tr -d '\n'
        fi
    fi
}

print_color "\n==============================================" "$CYAN"
print_color "    System Dependency Check" "$CYAN"
print_color "==============================================\n" "$CYAN"

all_passed=true
declare -a deps

# Check Node.js
print_color "Checking Node.js..." "$YELLOW"
if command_exists "node"; then
    node_version=$(get_version "node" "[0-9]+\.[0-9]+\.[0-9]+")
    if [ -n "$node_version" ]; then
        major_version=$(echo "$node_version" | cut -d. -f1)
        if [ "$major_version" -ge 18 ]; then
            print_color "  [OK] Node.js v$node_version" "$GREEN"
            deps+=("Node.js:$node_version:OK")
        else
            print_color "  [FAIL] Node.js v$node_version (requires >= 18.0.0)" "$RED"
            all_passed=false
            deps+=("Node.js:$node_version:FAIL")
        fi
    else
        print_color "  [WARN] Node.js installed but version not detected" "$YELLOW"
        deps+=("Node.js:Unknown:WARN")
    fi
else
    print_color "  [FAIL] Node.js not installed" "$RED"
    print_color "    Please install from https://nodejs.org (Node.js >= 18.0.0)" "$YELLOW"
    all_passed=false
    deps+=("Node.js:Not installed:FAIL")
fi

# Check Git
print_color "\nChecking Git..." "$YELLOW"
if command_exists "git"; then
    git_version=$(get_version "git" "[0-9]+\.[0-9]+\.[0-9]+")
    if [ -n "$git_version" ]; then
        print_color "  [OK] Git v$git_version" "$GREEN"
        deps+=("Git:$git_version:OK")
    else
        print_color "  [OK] Git installed" "$GREEN"
        deps+=("Git:Unknown:OK")
    fi
else
    print_color "  [FAIL] Git not installed" "$RED"
    print_color "    Please install from https://git-scm.com" "$YELLOW"
    all_passed=false
    deps+=("Git:Not installed:FAIL")
fi

# Check npm
print_color "\nChecking npm..." "$YELLOW"
if command_exists "npm"; then
    npm_version=$(get_version "npm" "[0-9]+\.[0-9]+\.[0-9]+")
    if [ -n "$npm_version" ]; then
        print_color "  [OK] npm v$npm_version" "$GREEN"
        deps+=("npm:$npm_version:OK")
    else
        print_color "  [OK] npm installed" "$GREEN"
        deps+=("npm:Unknown:OK")
    fi
else
    print_color "  [WARN] npm not installed (part of Node.js)" "$YELLOW"
    deps+=("npm:Unknown:WARN")
fi

# Check .NET
print_color "\nChecking .NET..." "$YELLOW"
if command_exists "dotnet"; then
    dotnet_version=$(get_version "dotnet" "[0-9]+\.[0-9]+\.[0-9]+")
    if [ -n "$dotnet_version" ]; then
        print_color "  [OK] .NET v$dotnet_version" "$GREEN"
        deps+=(".NET:$dotnet_version:OK")
    else
        print_color "  [OK] .NET SDK installed" "$GREEN"
        deps+=(".NET:Unknown:OK")
    fi
else
    print_color "  [WARN] .NET SDK not installed (required for MCP Server)" "$YELLOW"
    deps+=(".NET:Not installed:warn")
fi

# Summary
print_color "\n==============================================" "$CYAN"
print_color "    Dependency Check Summary" "$CYAN"
print_color "==============================================\n" "$CYAN"

for dep in "${deps[@]}"; do
    IFS=':' read -r name version status <<< "$dep"
    
    status_text="[FAIL]"
    status_color="$RED"
    
    if [ "$status" = "OK" ]; then
        status_text="[OK]"
        status_color="$GREEN"
    elif [ "$status" = "WARN" ]; then
        status_text="[WARN]"
        status_color="$YELLOW"
    fi
    
    print_color "$status_text $name: $version" "$status_color"
done

if [ "$all_passed" = true ]; then
    print_color "\n[OK] All required dependencies are installed!" "$GREEN"
    exit 0
else
    print_color "\n[FAIL] Some dependencies are not installed or version not met" "$RED"
    exit 1
fi
