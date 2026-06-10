# Project Init Skill

## Purpose

引导用户完成新项目的初始化配置。

## Description

该技能提供交互式向导，帮助用户配置新项目的开发环境，包括：
1. 检查系统依赖
2. 安装 GitNexus 和 Skills
3. 配置 MCP 连接
4. 配置项目与 Azure DevOps 的映射关系
5. 初始化文档结构

## Configuration

```yaml
name: project-init
version: 1.0.0
description: Project initialization skill
author: Development Platform Team
```

## Tools Used

- `SetProjectMapping()` - 配置项目映射
- `SetUserMapping()` - 配置用户映射（可选）
- `GetProjects()` - 获取 Azure DevOps 项目列表

## Dependencies

- GitNexus - 代码分析工具
- mattpocock/skills - 工程化技能

## Execution Steps

1. **欢迎信息**: 显示初始化欢迎信息
2. **依赖检查**: 检查 Node.js、Git 等依赖
3. **安装组件**: 安装 GitNexus 和 Skills
4. **配置 MCP**: 配置 MCP Server 连接
5. **项目映射**: 配置本地项目与 Azure DevOps 项目映射
6. **用户映射**: 配置 Windows 用户与 Azure DevOps 用户映射（可选）
7. **文档初始化**: 创建 CONTEXT.md 和 docs/adr/ 目录
8. **完成报告**: 显示初始化完成信息

## Usage

```
/project-init
```

## Output

- 初始化完成报告
- 配置摘要
- 后续操作指引

## Examples

```
/project-init

> 欢迎使用项目初始化向导！
> 
> 🔍 检查系统依赖...
> ✅ Node.js v20.10.0
> ✅ Git v2.45.0
> 
> 📦 安装 GitNexus...
> ✅ GitNexus 安装成功
> 
> 📦 安装 Skills...
> ✅ Skills 安装成功
> 
> 🔧 配置 MCP 连接...
> ✅ MCP 连接配置完成
> 
> 🗺️ 配置项目映射...
> 请输入 Azure DevOps 项目 ID: abc123
> 请输入本地项目名称: MyProject
> ✅ 项目映射配置完成
> 
> 📄 初始化文档...
> ✅ CONTEXT.md 创建完成
> ✅ docs/adr/ 目录创建完成
> 
> 🎉 项目初始化完成！
> 
> 下一步：
> 1. 使用 GetAssignedTasks() 获取任务
> 2. 使用 /grill-with-docs 分析需求
> 3. 使用 /tdd 进行开发
```

## Notes

- 需要管理员权限安装组件
- 需要 Azure DevOps 项目 ID
- 建议在空目录中运行