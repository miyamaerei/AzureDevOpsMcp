#!/bin/bash
#===============================================================================
# setup-docs.sh - 初始化项目文档
#===============================================================================
# 描述：创建必要的文档目录和初始文件，包括 ADR、CONTEXT.md 等
# 用法：./setup-docs.sh [--skip-context] [--silent]
#===============================================================================

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 默认值
SKIP_CONTEXT=false
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

# 获取项目根目录
get_project_root() {
    local script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    echo "$(cd "$script_dir/../.." && pwd)"
}

# 解析命令行参数
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --skip-context)
                SKIP_CONTEXT=true
                shift
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

#===============================================================================
# 主函数
#===============================================================================

main() {
    # 解析参数
    parse_args "$@"
    
    # 打印标题
    print_color "$CYAN" "\n=============================================="
    print_color "$CYAN" "    初始化项目文档"
    print_color "$CYAN" "==============================================\n"
    
    # 获取项目根目录
    local project_root
    project_root=$(get_project_root)
    
    # 1. 创建 docs 目录结构
    print_color "$YELLOW" "创建文档目录结构..."
    
    local docs_dir="$project_root/docs"
    local adr_dir="$docs_dir/adr"
    local issues_dir="$docs_dir/issues"
    local templates_dir="$docs_dir/templates"
    
    # 创建目录
    mkdir -p "$adr_dir"
    mkdir -p "$issues_dir"
    mkdir -p "$templates_dir"
    
    print_color "$GREEN" "  ✅ 文档目录结构创建完成"
    
    # 2. 创建 ADR 目录
    print_color "$YELLOW" "\n创建 ADR (Architecture Decision Records)..."
    
    local adr_index_path="$adr_dir/README.md"
    local today
    today=$(date -u +"%Y-%m-%d")
    
    # 检查文件是否存在
    if [ ! -f "$adr_index_path" ]; then
        cat > "$adr_index_path" <<EOF
# Architecture Decision Records

## 什么是 ADR?

ADR (Architecture Decision Records) 是记录重要架构决策的文档。每个 ADR 包含：
- **背景**: 做出决策的原因
- **决策**: 具体的架构选择
- **结果**: 决策的后果

## ADR 列表

| ID | 标题 | 日期 | 状态 |
|----|------|------|------|
| 000 | ADR 模板 | $today | 已接受 |

## 添加新 ADR

使用模板创建新 ADR：
\`\`\`bash
cp docs/templates/adr-template.md docs/adr/XXX-title.md
\`\`\`

然后更新本文件添加新 ADR 的索引。
EOF
    fi
    print_color "$GREEN" "  ✅ ADR 索引创建完成"
    
    # 3. 创建 ADR 模板
    local adr_template_path="$templates_dir/adr-template.md"
    if [ ! -f "$adr_template_path" ]; then
        cat > "$adr_template_path" <<'EOF'
# ADR-XXX: [标题]

## 状态
提议 | 已接受 | 已废弃 | 已替换

## 背景
[描述导致需要做决策的背景]

## 决策
[描述做出的架构决策]

## 结果

### 正面
- [列出正面影响]

### 负面
- [列出负面影响]

### 权衡
- [描述做出的权衡]
EOF
    fi
    print_color "$GREEN" "  ✅ ADR 模板创建完成"
    
    # 4. 创建 CONTEXT.md（如果不存在）
    if [ "$SKIP_CONTEXT" = false ]; then
        print_color "$YELLOW" "\n生成 CONTEXT.md..."
        
        local context_path="$project_root/CONTEXT.md"
        
        # 检查是否已存在
        if [ -f "$context_path" ]; then
            # 检查是否由 mattpocock/skills 创建（通常包含特定格式）
            if grep -q "行为驱动开发\|BDD\|Given-When-Then" "$context_path" 2>/dev/null; then
                print_color "$YELLOW" "  ⚠️  CONTEXT.md 已存在（由 mattpocock/skills 创建）"
                print_color "$YELLOW" "  建议手动编辑 CONTEXT.md 添加项目特定内容"
            else
                print_color "$YELLOW" "  ⚠️  CONTEXT.md 已存在"
                if [ "$SILENT" = false ]; then
                    read -p "  是否覆盖? (y/N): " overwrite
                    if [ "$overwrite" != "y" ] && [ "$overwrite" != "Y" ]; then
                        print_color "$YELLOW" "  跳过 CONTEXT.md 生成"
                    else
                        generate_context
                    fi
                fi
            fi
        else
            generate_context
        fi
    else
        print_color "$YELLOW" "\n跳过 CONTEXT.md 生成"
    fi
    
    # 5. 创建文档模板
    print_color "$YELLOW" "\n创建文档模板..."
    
    # Issue 模板
    local issue_template_path="$templates_dir/issue-template.md"
    if [ ! -f "$issue_template_path" ]; then
        cat > "$issue_template_path" <<'EOF'
# Issue: [问题描述]

## 概述
[简要描述要解决的问题]

## 详细信息
[详细说明问题背景和影响]

## 验收标准
- [ ] 标准 1
- [ ] 标准 2
- [ ] 标准 3

## 相关文档
- 相关链接
EOF
    fi
    
    # README 模板
    local readme_template_path="$templates_dir/readme-template.md"
    if [ ! -f "$readme_template_path" ]; then
        cat > "$readme_template_path" <<'EOF'
# 模块名称

## 功能描述
[描述模块的主要功能]

## 使用方法
[提供使用示例]

## API 参考
[列出公共 API]

## 配置说明
[描述配置项]
EOF
    fi
    
    print_color "$GREEN" "  ✅ 文档模板创建完成"
    
    # 6. 创建 .gitnexus 目录（如果不存在）
    print_color "$YELLOW" "\n检查 GitNexus 目录..."
    local gitnexus_dir="$project_root/.gitnexus"
    if [ ! -d "$gitnexus_dir" ]; then
        mkdir -p "$gitnexus_dir"
        print_color "$GREEN" "  ✅ .gitnexus 目录创建完成"
    else
        print_color "$GREEN" "  ✅ .gitnexus 目录已存在"
    fi
    
    # 7. 更新 docs/README.md
    print_color "$YELLOW" "\n更新 docs/README.md..."
    
    local docs_readme_path="$docs_dir/README.md"
    if [ ! -f "$docs_readme_path" ]; then
        cat > "$docs_readme_path" <<'EOF'
# 项目文档

## 目录结构

- **adr/**: 架构决策记录 (Architecture Decision Records)
- **issues/**: 问题跟踪和解决方案
- **templates/**: 文档模板

## 快速开始

1. 查看 [workflow-specification.md](workflow-specification.md) 了解开发流程
2. 查看 [../CONTEXT.md](../CONTEXT.md) 了解项目上下文
3. 查看 adr/ 目录了解架构决策

## 贡献指南

### 创建新 ADR
\`\`\`bash
cp docs/templates/adr-template.md docs/adr/XXX-title.md
\`\`\`

### 记录问题
使用 templates/issue-template.md 模板记录问题。
EOF
    fi
    
    print_color "$GREEN" "  ✅ docs/README.md 更新完成"
    
    # 打印完成摘要
    print_color "$CYAN" "\n=============================================="
    print_color "$CYAN" "    文档初始化完成"
    print_color "$CYAN" "==============================================\n"
    
    print_color "$GREEN" "✅ 文档初始化完成！"
    print_color "$CYAN" "\n创建的目录结构:"
    print_color "$YELLOW" "  docs/"
    print_color "$YELLOW" "  ├── adr/"
    print_color "$WHITE" "  │   └── README.md"
    print_color "$YELLOW" "  ├── issues/"
    print_color "$YELLOW" "  ├── templates/"
    print_color "$WHITE" "  │   ├── adr-template.md"
    print_color "$WHITE" "  │   ├── issue-template.md"
    print_color "$WHITE" "  │   └── readme-template.md"
    print_color "$WHITE" "  └── README.md"
    print_color "$YELLOW" "  .gitnexus/"
    print_color "$YELLOW" "  CONTEXT.md"
    
    exit 0
}

# 生成 CONTEXT.md 的函数
generate_context() {
    local project_root
    project_root=$(get_project_root)
    
    local project_name
    project_name=$(basename "$project_root")
    local description="Azure DevOps MCP Server - 开发管理平台"
    local today
    today=$(date -u +"%Y-%m-%d")
    
    # 检查 .gitnexus 是否存在
    local gitnexus_dep="- GitNexus: 代码知识图谱 (可选)"
    if [ -d "$project_root/.gitnexus" ]; then
        gitnexus_dep="- GitNexus: 代码知识图谱"
    fi
    
    cat > "$project_root/CONTEXT.md" <<EOF
# 项目上下文

## 项目信息
- **项目名称**: $project_name
- **描述**: $description
- **创建日期**: $today
- **技术栈**: .NET Core, MCP, Azure DevOps API

## 项目目标
本项目旨在通过 MCP (Model Context Protocol) 实现本地开发环境与 Azure DevOps 的无缝集成。

### 核心功能
1. **任务管理**: 通过 MCP 工具管理 Azure DevOps 工作项
2. **状态同步**: 在本地项目与 Azure DevOps 之间同步任务状态
3. **项目映射**: 建立本地项目与 Azure DevOps 项目的映射关系
4. **Windows 集成**: 支持 Windows 集成认证

## 领域模型

### 任务状态
- \`NotImplemented\` - 未实现
- \`Current\` - 当前任务
- \`Blocked\` - 被阻塞
- \`Archived\` - 已归档

### 状态转换
\`\`\`
NotImplemented → Current → Archived
                  ↓
              Blocked → Current
\`\`\`

## 技术架构

### 核心模块
- **MCP Server**: 提供 MCP 协议接口
- **Services**: 业务逻辑服务层
- **Tools**: MCP 工具实现
- **Models**: 数据模型

### 外部依赖
$gitnexus_dep
- Azure DevOps REST API
- Windows 集成认证

## 开发规范
详见 [docs/workflow-specification.md](docs/workflow-specification.md)
EOF
    
    print_color "$GREEN" "  ✅ CONTEXT.md 生成完成"
}

# 执行主函数
main "$@"
