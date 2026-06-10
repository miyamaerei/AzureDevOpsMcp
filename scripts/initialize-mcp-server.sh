#!/bin/bash
#
# Azure DevOps MCP Server 初始化脚本 (Linux/macOS)
#
# 使用方式:
#   chmod +x initialize-mcp-server.sh
#   ./initialize-mcp-server.sh [options]
#
# 选项:
#   -m, --mode      传输模式: stdio 或 http (默认: stdio)
#   -p, --port      HTTP 端口号 (默认: 5000)
#   -a, --auth      是否需要认证: true 或 false (默认: true)
#   -i, --interval  同步间隔(分钟) (默认: 5)
#   -s, --autosync  归档时自动同步: true 或 false (默认: true)

set -e

# 默认值
TRANSPORT_MODE="stdio"
HTTP_PORT="5000"
REQUIRE_AUTH="true"
SYNC_INTERVAL="5"
AUTO_SYNC="true"

# 解析参数
while [[ $# -gt 0 ]]; do
    case "$1" in
        -m|--mode)
            TRANSPORT_MODE="$2"
            shift 2
            ;;
        -p|--port)
            HTTP_PORT="$2"
            shift 2
            ;;
        -a|--auth)
            REQUIRE_AUTH="$2"
            shift 2
            ;;
        -i|--interval)
            SYNC_INTERVAL="$2"
            shift 2
            ;;
        -s|--autosync)
            AUTO_SYNC="$2"
            shift 2
            ;;
        -h|--help)
            echo "Azure DevOps MCP Server 初始化脚本"
            echo ""
            echo "选项:"
            echo "  -m, --mode      传输模式: stdio 或 http (默认: stdio)"
            echo "  -p, --port      HTTP 端口号 (默认: 5000)"
            echo "  -a, --auth      是否需要认证: true 或 false (默认: true)"
            echo "  -i, --interval  同步间隔(分钟) (默认: 5)"
            echo "  -s, --autosync  归档时自动同步: true 或 false (默认: true)"
            exit 0
            ;;
        *)
            shift
            ;;
    esac
done

# 颜色定义
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${CYAN}==============================================="
echo -e "    Azure DevOps MCP Server 初始化脚本"
echo -e "===============================================${NC}"

# 设置环境变量
echo -e "\n${YELLOW}设置环境变量...${NC}"

# 传输模式
export MCP_TRANSPORT_MODE="${TRANSPORT_MODE^^}"
echo -e "  ${GREEN}✓${NC} MCP_TRANSPORT_MODE: ${TRANSPORT_MODE^^}"

# HTTP 端口
if [[ "${TRANSPORT_MODE^^}" == "HTTP" ]]; then
    export MCP_HTTP_PORT="${HTTP_PORT}"
    echo -e "  ${GREEN}✓${NC} MCP_HTTP_PORT: ${HTTP_PORT}"
fi

# 认证设置
export MCP_REQUIRE_AUTH="${REQUIRE_AUTH}"
echo -e "  ${GREEN}✓${NC} MCP_REQUIRE_AUTH: ${REQUIRE_AUTH}"

# 同步配置
export TASK_SYNC_INTERVAL_MINUTES="${SYNC_INTERVAL}"
echo -e "  ${GREEN}✓${NC} TASK_SYNC_INTERVAL_MINUTES: ${SYNC_INTERVAL}"

export TASK_SYNC_AUTO_ON_ARCHIVE="${AUTO_SYNC}"
echo -e "  ${GREEN}✓${NC} TASK_SYNC_AUTO_ON_ARCHIVE: ${AUTO_SYNC}"

echo -e "\n${GREEN}环境变量设置完成！${NC}"

# 检查 .NET 版本
echo -e "\n${YELLOW}检查 .NET 运行时...${NC}"
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "  ${GREEN}✓${NC} .NET 版本: ${DOTNET_VERSION}"
else
    echo -e "  ${RED}✗${NC} 错误: 未检测到 .NET 运行时"
    echo -e "    请安装 .NET 10.0 或更高版本"
    exit 1
fi

# 构建项目
echo -e "\n${YELLOW}构建项目...${NC}"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/../src/AzureDevOpsMcpServer" && pwd)"
cd "${PROJECT_PATH}"

if dotnet build --configuration Release; then
    echo -e "  ${GREEN}✓${NC} 构建成功"
else
    echo -e "  ${RED}✗${NC} 构建失败"
    exit 1
fi

# 显示启动信息
echo -e "\n${CYAN}==============================================="
echo -e "    初始化完成！"
echo -e "===============================================${NC}"

echo -e "\n${YELLOW}启动命令：${NC}"
if [[ "${TRANSPORT_MODE^^}" == "HTTP" ]]; then
    echo -e "  cd \"${PROJECT_PATH}\""
    echo -e "  dotnet run"
    echo -e ""
    echo -e "${GREEN}服务将在 http://localhost:${HTTP_PORT} 启动${NC}"
else
    echo -e "  cd \"${PROJECT_PATH}\""
    echo -e "  dotnet run"
    echo -e ""
    echo -e "${GREEN}服务将在 Stdio 模式下启动${NC}"
fi

echo -e "\n${YELLOW}环境变量说明：${NC}"
echo -e "  - MCP_TRANSPORT_MODE: 传输模式 (Stdio/Http)"
echo -e "  - MCP_HTTP_PORT: HTTP 端口 (仅 Http 模式)"
echo -e "  - MCP_REQUIRE_AUTH: 是否需要认证"
echo -e "  - TASK_SYNC_INTERVAL_MINUTES: 同步间隔(分钟)"
echo -e "  - TASK_SYNC_AUTO_ON_ARCHIVE: 归档时自动同步"

echo -e "\n${CYAN}===============================================\n${NC}"