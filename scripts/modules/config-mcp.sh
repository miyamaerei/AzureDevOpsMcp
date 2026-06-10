#!/bin/bash

# 配置 MCP 连接
# Description: 配置 MCP Server 连接信息，包括 Claude Code 和 Cursor 的 MCP 配置
# Usage: ./config-mcp.sh [--mcp-url URL] [--pat-token TOKEN] [--skip-pat] [--silent]

set -e

# Parse arguments
MCP_SERVER_URL="http://localhost:5000"
PAT_TOKEN=""
SKIP_PAT=false
SILENT=false

while [[ "$#" -gt 0 ]]; do
    case $1 in
        --mcp-url) MCP_SERVER_URL="$2"; shift ;;
        --pat-token) PAT_TOKEN="$2"; shift ;;
        --skip-pat) SKIP_PAT=true ;;
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

print_color "\n==============================================" "$CYAN"
print_color "    配置 MCP 连接" "$CYAN"
print_color "==============================================\n" "$CYAN"

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

# 1. 配置 MCP Server URL
if [ -z "$MCP_SERVER_URL" ] || [ "$MCP_SERVER_URL" = "http://localhost:5000" ]; then
    print_color "输入 MCP Server URL:" "$YELLOW"
    print_color "  (默认: http://localhost:5000)" "$NC"
    read -p "  " mcp_url_input
    MCP_SERVER_URL=${mcp_url_input:-"http://localhost:5000"}
fi

print_color "MCP Server URL: $MCP_SERVER_URL" "$GREEN"

# 2. 配置 PAT Token
if [ "$SKIP_PAT" = true ]; then
    print_color "\n跳过 PAT Token 配置" "$YELLOW"
    PAT_TOKEN=""
elif [ -z "$PAT_TOKEN" ]; then
    print_color "\n配置 Azure DevOps PAT Token:" "$YELLOW"
    print_color "  ⚠️  注意: Token 需要具有 Work Items (read/write) 和 Projects (read) 权限" "$RED"
    
    # 尝试读取环境变量
    if [ -n "$AZURE_DEVOPS_PAT" ]; then
        read -p "  发现环境变量 AZURE_DEVOPS_PAT，是否使用? (Y/n): " use_env
        if [[ ! "$use_env" =~ ^[Nn]$ ]]; then
            PAT_TOKEN="$AZURE_DEVOPS_PAT"
            print_color "  ✅ 使用环境变量中的 PAT Token" "$GREEN"
        fi
    fi
    
    if [ -z "$PAT_TOKEN" ]; then
        read -s -p "  输入 PAT Token (输入后不可见): " PAT_TOKEN
        echo ""
        
        # 保存到环境变量
        read -p "  是否保存到环境变量 AZURE_DEVOPS_PAT? (y/N): " save_choice
        if [[ "$save_choice" =~ ^[Yy]$ ]]; then
            echo "export AZURE_DEVOPS_PAT=\"$PAT_TOKEN\"" >> ~/.bashrc
            print_color "  ✅ 已保存到 ~/.bashrc" "$GREEN"
        fi
    fi
else
    print_color "\n✅ 使用提供的 PAT Token" "$GREEN"
fi

# 3. 创建 Claude Code 配置
print_color "\n==============================================" "$CYAN"
print_color "    配置 Claude Code MCP" "$CYAN"
print_color "==============================================\n" "$CYAN"

claude_settings_path="$project_root/.claude/settings.json"
claude_settings_dir=$(dirname "$claude_settings_path")

# 创建 .claude 目录
mkdir -p "$claude_settings_dir"

# Claude Code MCP 配置
cat > "$claude_settings_path" <<EOF
{
  "mcpServers": {
    "azure-devops": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-azure-devops",
        "--accessToken",
        "$PAT_TOKEN"
      ],
      "env": {
        "AZURE_DEVOPS_ORGANIZATION": "your-org"
      }
    },
    "local-mcp": {
      "url": "$MCP_SERVER_URL/mcp",
      "description": "本地 Azure DevOps MCP Server"
    }
  }
}
EOF

print_color "写入配置到: .claude/settings.json\n" "$YELLOW"
print_color "✅ Claude Code MCP 配置完成" "$GREEN"

# 4. 创建 Cursor MCP 配置
print_color "\n==============================================" "$CYAN"
print_color "    配置 Cursor MCP" "$CYAN"
print_color "==============================================\n" "$CYAN"

cursor_mcp_path="$project_root/.cursor/mcp.json"
cursor_mcp_dir=$(dirname "$cursor_mcp_path")

# 创建 .cursor 目录
mkdir -p "$cursor_mcp_dir"

# Cursor MCP 配置
cat > "$cursor_mcp_path" <<EOF
{
  "mcpServers": {
    "azure-devops": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-azure-devops",
        "--accessToken",
        "$PAT_TOKEN"
      ],
      "env": {
        "AZURE_DEVOPS_ORGANIZATION": "your-org"
      }
    },
    "local-mcp": {
      "url": "$MCP_SERVER_URL/mcp",
      "description": "本地 Azure DevOps MCP Server"
    }
  }
}
EOF

print_color "写入配置到: .cursor/mcp.json\n" "$YELLOW"
print_color "✅ Cursor MCP 配置完成" "$GREEN"

# 5. 创建本地 MCP Server 配置
print_color "\n==============================================" "$CYAN"
print_color "    配置本地 MCP Server" "$CYAN"
print_color "==============================================\n" "$CYAN"

mcp_config_path="$project_root/config/mcp-config.json"
mcp_config_dir=$(dirname "$mcp_config_path")

# 创建 config 目录
mkdir -p "$mcp_config_dir"

# MCP Server 配置
cat > "$mcp_config_path" <<EOF
{
  "server": {
    "url": "$MCP_SERVER_URL",
    "healthEndpoint": "$MCP_SERVER_URL/health"
  },
  "auth": {
    "type": "pat",
    "tokenEnvVar": "AZURE_DEVOPS_PAT"
  },
  "windowsIntegration": {
    "enabled": true,
    "authenticationMethod": "integrated"
  }
}
EOF

print_color "写入配置到: config/mcp-config.json\n" "$YELLOW"
print_color "✅ 本地 MCP Server 配置完成" "$GREEN"

# 6. 创建 .gitignore
print_color "\n==============================================" "$CYAN"
print_color "    更新 .gitignore" "$CYAN"
print_color "==============================================\n" "$CYAN"

gitignore_path="$project_root/.gitignore"

if [ -f "$gitignore_path" ]; then
    if ! grep -q '.gitnexus/' "$gitignore_path"; then
        print_color "添加 .gitnexus/ 到 .gitignore..." "$YELLOW"
        echo -e "\n# GitNexus\n.gitnexus/" >> "$gitignore_path"
    fi
else
    print_color "创建 .gitignore..." "$YELLOW"
    cat > "$gitignore_path" <<EOF
# GitNexus
.gitnexus/

# MCP Config (local overrides)
config/local-*.json

# Azure DevOps PAT
.env
EOF
fi

print_color "✅ .gitignore 更新完成" "$GREEN"

print_color "\n==============================================" "$CYAN"
print_color "    配置完成" "$CYAN"
print_color "==============================================\n" "$CYAN"

print_color "✅ MCP 配置完成！" "$GREEN"
print_color "\n后续步骤:" "$CYAN"
print_color "  1. 重启 Claude Code / Cursor 以加载新配置" "$YELLOW"
print_color "  2. 配置 Azure DevOps 组织名称" "$YELLOW"
print_color "  3. 运行 init-project.sh 配置项目映射" "$YELLOW"

exit 0
