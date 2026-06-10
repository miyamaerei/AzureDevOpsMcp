#!/bin/bash

# 项目初始化主脚本
# Description: 交互式向导，引导用户完成项目初始化全过程
# Usage: ./init-project.sh [--skip-gitnexus] [--skip-skills] [--skip-mcp] [--skip-project-mapping] [--skip-docs] [--step STEP] [--silent]

set -e

# Parse arguments
SKIP_GITNEXUS=false
SKIP_SKILLS=false
SKIP_MCP=false
SKIP_PROJECT_MAPPING=false
SKIP_DOCS=false
STEP=""
SILENT=false

while [[ "$#" -gt 0 ]]; do
    case $1 in
        --skip-gitnexus) SKIP_GITNEXUS=true ;;
        --skip-skills) SKIP_SKILLS=true ;;
        --skip-mcp) SKIP_MCP=true ;;
        --skip-project-mapping) SKIP_PROJECT_MAPPING=true ;;
        --skip-docs) SKIP_DOCS=true ;;
        --step) STEP="$2"; shift ;;
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

# Function to print banner
print_banner() {
    print_color "\n" "$CYAN"
    print_color "╔════════════════════════════════════════════════════════════╗" "$CYAN"
    print_color "║                                                            ║" "$CYAN"
    print_color "║        AI 辅助开发管理平台 - 项目初始化                   ║" "$CYAN"
    print_color "║                                                            ║" "$CYAN"
    print_color "╚════════════════════════════════════════════════════════════╝" "$CYAN"
    print_color "\n" "$CYAN"
}

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MODULES_DIR="$SCRIPT_DIR/modules"

print_banner

# Step 1: Check dependencies
if [ -z "$STEP" ] || [ "$STEP" = "deps" ]; then
    print_color "步骤 1/6: 检查系统依赖" "$CYAN"
    print_color "========================================\n" "$CYAN"
    
    if [ -f "$MODULES_DIR/check-deps.sh" ]; then
        bash "$MODULES_DIR/check-deps.sh" || {
            print_color "\n❌ 依赖检查失败，请先安装缺失的依赖" "$RED"
            exit 1
        }
    else
        print_color "⚠️  check-deps.sh 不存在，跳过依赖检查" "$YELLOW"
    fi
fi

# Step 2: Install GitNexus
if [ "$SKIP_GITNEXUS" = false ] && ([ -z "$STEP" ] || [ "$STEP" = "gitnexus" ]); then
    print_color "\n步骤 2/6: 安装 GitNexus" "$CYAN"
    print_color "========================================\n" "$CYAN"
    
    if [ -f "$MODULES_DIR/install-gitnexus.sh" ]; then
        bash "$MODULES_DIR/install-gitnexus.sh" || {
            print_color "\n⚠️  GitNexus 安装失败，但将继续初始化" "$YELLOW"
        }
    else
        print_color "⚠️  install-gitnexus.sh 不存在，跳过 GitNexus 安装" "$YELLOW"
    fi
fi

# Step 3: Install Skills
if [ "$SKIP_SKILLS" = false ] && ([ -z "$STEP" ] || [ "$STEP" = "skills" ]); then
    print_color "\n步骤 3/6: 安装 Matt Pocock Skills" "$CYAN"
    print_color "========================================\n" "$CYAN"
    
    if [ -f "$MODULES_DIR/install-skills.sh" ]; then
        bash "$MODULES_DIR/install-skills.sh" || {
            print_color "\n⚠️  Skills 安装失败，但将继续初始化" "$YELLOW"
        }
    else
        print_color "⚠️  install-skills.sh 不存在，跳过 Skills 安装" "$YELLOW"
    fi
fi

# Step 4: Configure MCP
if [ "$SKIP_MCP" = false ] && ([ -z "$STEP" ] || [ "$STEP" = "mcp" ]); then
    print_color "\n步骤 4/6: 配置 MCP 连接" "$CYAN"
    print_color "========================================\n" "$CYAN"
    
    if [ -f "$MODULES_DIR/config-mcp.sh" ]; then
        bash "$MODULES_DIR/config-mcp.sh" || {
            print_color "\n⚠️  MCP 配置失败，但将继续初始化" "$YELLOW"
        }
    else
        print_color "⚠️  config-mcp.sh 不存在，跳过 MCP 配置" "$YELLOW"
    fi
fi

# Step 5: Configure Project Mapping
if [ "$SKIP_PROJECT_MAPPING" = false ] && ([ -z "$STEP" ] || [ "$STEP" = "project" ]); then
    print_color "\n步骤 5/6: 配置项目映射" "$CYAN"
    print_color "========================================\n" "$CYAN"
    
    if [ -f "$MODULES_DIR/config-project.sh" ]; then
        bash "$MODULES_DIR/config-project.sh" || {
            print_color "\n⚠️  项目映射配置失败，但将继续初始化" "$YELLOW"
        }
    else
        print_color "⚠️  config-project.sh 不存在，跳过项目映射配置" "$YELLOW"
    fi
fi

# Step 6: Setup Docs
if [ "$SKIP_DOCS" = false ] && ([ -z "$STEP" ] || [ "$STEP" = "docs" ]); then
    print_color "\n步骤 6/6: 初始化文档" "$CYAN"
    print_color "========================================\n" "$CYAN"
    
    if [ -f "$MODULES_DIR/setup-docs.sh" ]; then
        bash "$MODULES_DIR/setup-docs.sh" || {
            print_color "\n⚠️  文档初始化失败，但将继续" "$YELLOW"
        }
    else
        print_color "⚠️  setup-docs.sh 不存在，跳过文档初始化" "$YELLOW"
    fi
fi

# Final summary
print_color "\n╔════════════════════════════════════════════════════════════╗" "$CYAN"
print_color "║                    初始化完成                             ║" "$CYAN"
print_color "╚════════════════════════════════════════════════════════════╝" "$CYAN"

print_color "\n✅ 项目初始化完成！" "$GREEN"
print_color "\n后续步骤:" "$CYAN"
print_color "  1. 重启 Claude Code / Cursor 以加载新配置" "$YELLOW"
print_color "  2. 查看 CONTEXT.md 了解项目领域语言" "$YELLOW"
print_color "  3. 使用 /task-workflow 开始任务开发" "$YELLOW"
print_color "  4. 查看 docs/best-practices.md 了解最佳实践" "$YELLOW"

print_color "\n可用命令:" "$CYAN"
print_color "  - GetAssignedTasks()      拉取指派的任务" "$YELLOW"
print_color "  - UpdateTaskStatus()      更新任务状态" "$YELLOW"
print_color "  - /task-workflow          任务开发工作流" "$YELLOW"
print_color "  - /grill-with-docs        需求分析和文档更新" "$YELLOW"
print_color "  - /tdd                    测试驱动开发" "$YELLOW"

exit 0
