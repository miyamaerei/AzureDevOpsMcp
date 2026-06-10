#!/bin/bash

# 安装 mattpocock/skills
# Description: 安装 mattpocock 提供的 AI Skills，用于提升 AI 编程助手的代码质量
# Usage: ./install-skills.sh [--silent]

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

print_color "\n==============================================" "$CYAN"
print_color "    安装 mattpocock/skills" "$CYAN"
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

# 检查是否已有 skills 配置
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
skills_config_path="$project_root/.claude/skills.json"

if [ -f "$skills_config_path" ]; then
    print_color "\n检查现有 Skills 配置..." "$YELLOW"
    print_color "  ✅ 已存在 .claude/skills.json" "$GREEN"
    
    read -p "  是否重新安装 Skills? (y/N): " reinstall
    if [[ ! "$reinstall" =~ ^[Yy]$ ]]; then
        print_color "  跳过安装" "$YELLOW"
        exit 0
    fi
fi

# 创建 .claude 目录
claude_dir="$project_root/.claude"
if [ ! -d "$claude_dir" ]; then
    print_color "\n创建 .claude 目录..." "$YELLOW"
    mkdir -p "$claude_dir"
    print_color "  ✅ 目录创建成功" "$GREEN"
fi

# 安装 mattpocock/skills
print_color "\n安装 mattpocock/skills..." "$YELLOW"
print_color "  使用 npx skills@latest add mattpocock/skills\n" "$NC"

cd "$project_root"

# 使用 npx 安装 skills
if npx skills@latest add mattpocock/skills 2>&1; then
    print_color "\n  ✅ mattpocock/skills 安装成功" "$GREEN"
    
    # 列出已安装的 skills
    print_color "\n已安装的 Skills:" "$CYAN"
    skills_json_path="$claude_dir/skills.json"
    if [ -f "$skills_json_path" ]; then
        # 简单提取 skills 名称
        if command_exists "jq"; then
            jq -r '.skills[].name' "$skills_json_path" 2>/dev/null | while read -r skill_name; do
                print_color "  - $skill_name" "$GREEN"
            done
        else
            print_color "  (安装 jq 工具可查看详细列表)" "$YELLOW"
        fi
    fi
else
    print_color "  ❌ Skills 安装失败" "$RED"
    print_color "\n手动安装方法:" "$YELLOW"
    print_color "  1. 在项目根目录运行: npx skills@latest add mattpocock/skills" "$NC"
    print_color "  2. 或者访问: https://skills.mattpocock.com" "$NC"
    exit 1
fi

print_color "\n==============================================" "$CYAN"
print_color "    配置完成" "$CYAN"
print_color "==============================================\n" "$CYAN"

print_color "✅ Skills 安装完成！" "$GREEN"
print_color "\n可用命令:" "$CYAN"
print_color "  - /skills:list     查看所有可用 Skills" "$YELLOW"
print_color "  - /skills:install mattpocock/skills    安装新 Skill" "$YELLOW"
print_color "  - /skills:remove   移除 Skill" "$YELLOW"

exit 0
