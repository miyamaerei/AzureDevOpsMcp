#!/bin/bash
#===============================================================================
# config-project.sh - 配置项目与 Azure DevOps 项目的映射关系
#===============================================================================
# 描述：交互式配置本地项目与 Azure DevOps 项目的映射
# 用法：./config-project.sh [--local-project "项目名"] [--azure-org "组织URL"] 
#                           [--azure-project-id "项目ID"] [--azure-project-name "项目名"]
#                           [--working-dir "工作目录"] [--silent]
#===============================================================================

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 默认值
LOCAL_PROJECT_NAME=""
AZURE_ORG_URL=""
AZURE_PROJECT_ID=""
AZURE_PROJECT_NAME=""
WORKING_DIR=""
SILENT=false

#===============================================================================
# 辅助函数
#===============================================================================

# 打印带颜色的消息
print_color() {
    local color=$1
    local message=$2
    if [ "$SILENT" = false ]; then
        echo -e "${color}${message}${NC}"
    fi
}

# 打印带颜色的输出到 stdout
write_color() {
    local color=$1
    local message=$2
    if [ "$SILENT" = false ]; then
        echo -e "${color}${message}${NC}"
    fi
}

# 解析命令行参数
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --local-project)
                LOCAL_PROJECT_NAME="$2"
                shift 2
                ;;
            --azure-org)
                AZURE_ORG_URL="$2"
                shift 2
                ;;
            --azure-project-id)
                AZURE_PROJECT_ID="$2"
                shift 2
                ;;
            --azure-project-name)
                AZURE_PROJECT_NAME="$2"
                shift 2
                ;;
            --working-dir)
                WORKING_DIR="$2"
                shift 2
                ;;
            --silent)
                SILENT=true
                shift
                ;;
            *)
                echo "未知参数: $1"
                exit 1
                ;;
        esac
    done
}

# 获取项目根目录
get_project_root() {
    local script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    echo "$(cd "$script_dir/../.." && pwd)"
}

# 交互式获取输入
get_input() {
    local prompt="$1"
    local default="$2"
    local result
    
    if [ "$SILENT" = true ]; then
        echo "$default"
        return
    fi
    
    if [ -n "$default" ]; then
        read -p "$prompt (默认: $default): " result
        echo "${result:-$default}"
    else
        read -p "$prompt: " result
        echo "$result"
    fi
}

#===============================================================================
# 主函数
#===============================================================================

main() {
    # 解析参数
    parse_args "$@"
    
    # 打印标题
    print_color "$CYAN" "\n=============================================="
    print_color "$CYAN" "    配置项目映射"
    print_color "$CYAN" "==============================================\n"
    
    # 获取项目根目录
    local project_root
    project_root=$(get_project_root)
    
    # 1. 确定本地项目名称
    if [ -z "$LOCAL_PROJECT_NAME" ]; then
        print_color "$YELLOW" "输入本地项目名称:"
        print_color "$WHITE" "  (默认: 当前目录名称)"
        LOCAL_PROJECT_NAME=$(get_input "  " "$(basename "$project_root")")
    fi
    print_color "$GREEN" "本地项目名称: $LOCAL_PROJECT_NAME"
    
    # 2. 确定工作目录
    if [ -z "$WORKING_DIR" ]; then
        WORKING_DIR="$project_root"
        local confirm_dir
        confirm_dir=$(get_input "  工作目录" "$WORKING_DIR")
        if [ -n "$confirm_dir" ]; then
            WORKING_DIR="$confirm_dir"
        fi
    fi
    print_color "$GREEN" "工作目录: $WORKING_DIR"
    
    # 3. 获取 Azure DevOps 组织 URL
    if [ -z "$AZURE_ORG_URL" ]; then
        print_color "$YELLOW" "\n输入 Azure DevOps 组织 URL:"
        print_color "$WHITE" "  格式: https://dev.azure.com/{organization}"
        AZURE_ORG_URL=$(get_input "  " "")
    fi
    
    if [ -z "$AZURE_ORG_URL" ]; then
        print_color "$RED" "  ❌ Azure DevOps 组织 URL 不能为空"
        exit 1
    fi
    print_color "$GREEN" "Azure DevOps 组织: $AZURE_ORG_URL"
    
    # 4. 获取 Azure DevOps 项目 ID
    if [ -z "$AZURE_PROJECT_ID" ]; then
        print_color "$YELLOW" "\n输入 Azure DevOps 项目 ID:"
        print_color "$WHITE" "  (可以在项目设置中找到)"
        AZURE_PROJECT_ID=$(get_input "  " "")
    fi
    
    if [ -z "$AZURE_PROJECT_ID" ]; then
        print_color "$RED" "  ❌ Azure DevOps 项目 ID 不能为空"
        exit 1
    fi
    print_color "$GREEN" "Azure DevOps 项目 ID: $AZURE_PROJECT_ID"
    
    # 5. 获取 Azure DevOps 项目名称
    if [ -z "$AZURE_PROJECT_NAME" ]; then
        AZURE_PROJECT_NAME="$AZURE_PROJECT_ID"
        local confirm_name
        confirm_name=$(get_input "Azure DevOps 项目名称" "$AZURE_PROJECT_NAME")
        if [ -n "$confirm_name" ]; then
            AZURE_PROJECT_NAME="$confirm_name"
        fi
    fi
    print_color "$GREEN" "Azure DevOps 项目名称: $AZURE_PROJECT_NAME"
    
    # 6. 询问是否设置为默认项目
    local is_default=false
    local set_default
    if [ "$SILENT" = true ]; then
        set_default="n"
    else
        read -p "是否设置为默认项目? (Y/n): " set_default
    fi
    
    if [ "$set_default" != "n" ] && [ "$set_default" != "N" ]; then
        is_default=true
    fi
    
    print_color "$GREEN" "默认项目: $is_default"
    
    # 7. 创建映射配置
    print_color "$CYAN" "\n=============================================="
    print_color "$CYAN" "    创建项目映射配置"
    print_color "$CYAN" "==============================================\n"
    
    # 创建 config 目录
    local config_dir="$project_root/config"
    if [ ! -d "$config_dir" ]; then
        mkdir -p "$config_dir"
        print_color "$YELLOW" "创建配置目录: config/"
    fi
    
    # 读取或创建映射文件
    local mapping_file="$config_dir/project-mapping.json"
    local mappings="[]"
    local timestamp
    timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    
    if [ -f "$mapping_file" ]; then
        print_color "$YELLOW" "更新现有映射配置..."
        # 保留现有映射，添加新的
        mappings=$(cat "$mapping_file" | jq '.mappings')
    else
        print_color "$YELLOW" "创建新映射配置..."
    fi
    
    # 如果设置为默认，先取消其他默认
    if [ "$is_default" = true ]; then
        mappings=$(echo "$mappings" | jq --argjson default false 'map(if has("isDefault") then .isDefault = $default else . end)')
    fi
    
    # 创建新映射
    local new_mapping
    new_mapping=$(cat <<EOF
{
    "localProjectName": "$LOCAL_PROJECT_NAME",
    "azureDevOpsProjectId": "$AZURE_PROJECT_ID",
    "azureDevOpsProjectName": "$AZURE_PROJECT_NAME",
    "azureDevOpsOrganizationUrl": "$AZURE_ORG_URL",
    "workingDirectory": "$WORKING_DIR",
    "isDefault": $is_default,
    "createdAt": "$timestamp"
}
EOF
)
    
    # 添加新映射到数组
    mappings=$(echo "$mappings" | jq ". + [$new_mapping]")
    
    # 保存配置
    local mapping_config
    mapping_config=$(cat <<EOF
{
    "version": "1.0",
    "mappings": $mappings,
    "lastUpdated": "$timestamp"
}
EOF
)
    
    print_color "$YELLOW" "写入配置到: config/project-mapping.json\n"
    echo "$mapping_config" > "$mapping_file"
    print_color "$GREEN" "✅ 项目映射配置完成"
    
    # 8. 创建用户配置
    local user_config_path="$config_dir/user-config.json"
    local user_config
    user_config=$(cat <<EOF
{
    "azureDevOps": {
        "organizationUrl": "$AZURE_ORG_URL",
        "defaultProjectId": "$AZURE_PROJECT_ID",
        "userId": ""
    },
    "projectDefaults": {
        "localProjectName": "$LOCAL_PROJECT_NAME"
    }
}
EOF
)
    
    print_color "$YELLOW" "\n创建用户配置文件..."
    echo "$user_config" > "$user_config_path"
    print_color "$GREEN" "✅ 用户配置文件创建完成"
    
    # 9. 提示配置用户 ID
    print_color "$CYAN" "\n=============================================="
    print_color "$CYAN" "    配置用户信息"
    print_color "$CYAN" "==============================================\n"
    
    print_color "$YELLOW" "⚠️  请配置您的 Azure DevOps 用户 ID"
    print_color "$WHITE" "  可以在 Azure DevOps 中访问: https://aex.dev.azure.com/me"
    
    local user_id
    user_id=$(get_input "  输入您的 Azure DevOps 用户 ID" "")
    
    if [ -n "$user_id" ]; then
        user_config=$(cat "$user_config_path")
        user_config=$(echo "$user_config" | jq ".azureDevOps.userId = \"$user_id\"")
        echo "$user_config" > "$user_config_path"
        print_color "$GREEN" "✅ 用户 ID 已保存"
    fi
    
    # 打印完成摘要
    print_color "$CYAN" "\n=============================================="
    print_color "$CYAN" "    配置完成"
    print_color "$CYAN" "==============================================\n"
    
    print_color "$GREEN" "✅ 项目映射配置完成！"
    print_color "$CYAN" "\n配置摘要:"
    print_color "$YELLOW" "  本地项目: $LOCAL_PROJECT_NAME"
    print_color "$YELLOW" "  Azure 项目: $AZURE_PROJECT_NAME"
    print_color "$YELLOW" "  组织: $AZURE_ORG_URL"
    print_color "$GREEN" "  默认项目: $is_default"
    
    print_color "$CYAN" "\n后续步骤:"
    print_color "$YELLOW" "  1. 运行 GetAssignedTasks 查看分配给您的任务"
    print_color "$YELLOW" "  2. 使用 UpdateTaskStatus 更新任务状态"
    print_color "$YELLOW" "  3. 使用 QueryWorkItems 查询工作项"
    
    exit 0
}

# 执行主函数
main "$@"
