#!/bin/bash

# 安装 GitNexus 并运行代码索引
# Description: 安装 GitNexus 代码知识图谱引擎并运行初始代码索引
# Usage: ./install-gitnexus.sh [--skip-analyze] [--silent]

set -e

# Parse arguments
SKIP_ANALYZE=false
SILENT=false

while [[ "$#" -gt 0 ]]; do
    case $1 in
        --skip-analyze) SKIP_ANALYZE=true ;;
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

print_color "\n==============================================" "$CYAN"
print_color "    安装 GitNexus" "$CYAN"
print_color "==============================================\n" "$CYAN"

# 检查 Node.js
print_color "检查 Node.js..." "$YELLOW"
if ! command_exists "node"; then
    print_color "  ❌ Node.js 未安装" "$RED"
    print_color "    请先运行 check-deps.sh 检查依赖" "$YELLOW"
    exit 1
fi

node_version=$(node --version)
print_color "  ✅ Node.js $node_version" "$GREEN"

# 检查是否已安装
print_color "\n检查 GitNexus 安装状态..." "$YELLOW"
if command_exists "gitnexus"; then
    gitnexus_version=$(gitnexus --version 2>&1 || echo "unknown")
    print_color "  ✅ GitNexus 已安装: $gitnexus_version" "$GREEN"
    
    read -p "  是否重新安装? (y/N): " reinstall
    if [[ ! "$reinstall" =~ ^[Yy]$ ]]; then
        print_color "  跳过安装" "$YELLOW"
        
        if [ "$SKIP_ANALYZE" = false ]; then
            # Run analyze
            print_color "\n==============================================" "$CYAN"
            print_color "    运行 GitNexus 代码索引" "$CYAN"
            print_color "==============================================\n" "$CYAN"
            
            project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
            
            print_color "正在索引代码库..." "$YELLOW"
            print_color "  项目路径: $project_root\n" "$NC"
            
            cd "$project_root"
            
            print_color "  运行 gitnexus analyze..." "$YELLOW"
            if gitnexus analyze 2>&1; then
                print_color "\n  ✅ 代码索引完成" "$GREEN"
            else
                print_color "\n  ⚠️  代码索引完成但有警告" "$YELLOW"
            fi
        fi
        
        exit 0
    fi
fi

# 安装 GitNexus
print_color "\n安装 GitNexus..." "$YELLOW"
if npm install -g gitnexus 2>&1; then
    print_color "  ✅ GitNexus 安装成功" "$GREEN"
    
    # 验证安装
    installed_version=$(gitnexus --version 2>&1 || echo "unknown")
    print_color "  安装版本: $installed_version" "$GREEN"
else
    print_color "  ❌ GitNexus 安装失败" "$RED"
    exit 1
fi

# 运行代码索引
if [ "$SKIP_ANALYZE" = false ]; then
    print_color "\n==============================================" "$CYAN"
    print_color "    运行 GitNexus 代码索引" "$CYAN"
    print_color "==============================================\n" "$CYAN"
    
    project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
    
    print_color "正在索引代码库..." "$YELLOW"
    print_color "  项目路径: $project_root\n" "$NC"
    
    cd "$project_root"
    
    print_color "  运行 gitnexus analyze..." "$YELLOW"
    if gitnexus analyze 2>&1; then
        print_color "\n  ✅ 代码索引完成" "$GREEN"
    else
        print_color "\n  ⚠️  代码索引完成但有警告" "$YELLOW"
    fi
else
    print_color "\n跳过代码索引 (--skip-analyze)" "$YELLOW"
fi

print_color "\n✅ GitNexus 安装完成！" "$GREEN"
print_color "\n后续步骤:" "$CYAN"
print_color "  1. 使用 gitnexus context() 查看代码依赖关系" "$YELLOW"
print_color "  2. 使用 gitnexus impact() 分析代码修改影响范围" "$YELLOW"

exit 0
