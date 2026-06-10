# Configure Project Skill

## Purpose

帮助用户配置本地项目与 Azure DevOps 项目的映射关系。

## Description

该技能引导用户完成项目映射配置，包括：
1. 获取 Azure DevOps 项目列表
2. 创建或更新项目映射
3. 设置默认项目
4. 验证映射配置

## Configuration

```yaml
name: configure-project
version: 1.0.0
description: Project mapping configuration skill
author: Development Platform Team
```

## Tools Used

- `GetProjects()` - 获取 Azure DevOps 项目列表
- `SetProjectMapping()` - 创建/更新项目映射
- `GetDefaultMapping()` - 获取默认映射
- `SetDefaultMapping()` - 设置默认映射

## Dependencies

- MCP Server 连接正常
- 用户已通过认证

## Execution Steps

1. **显示当前映射**: 展示已配置的项目映射
2. **获取 Azure 项目**: 调用 GetProjects() 获取项目列表
3. **选择项目**: 用户选择要映射的 Azure DevOps 项目
4. **配置映射**: 创建或更新项目映射
5. **设置默认**: 选择是否设为默认映射
6. **验证配置**: 测试映射是否生效

## Usage

```
/configure-project
```

## Output

- 项目映射列表
- 配置结果
- 验证状态

## Examples

```
/configure-project

> 当前项目映射：
> ┌─────────────┬─────────────────┬─────────┐
> │ 本地项目     │ Azure 项目      │ 默认    │
> ├─────────────┼─────────────────┼─────────┤
> │ MyProject   │ MyProject (abc) │ 是      │
> └─────────────┴─────────────────┴─────────┘
> 
> 可用的 Azure DevOps 项目：
> 1. MyProject (ID: abc123)
> 2. AnotherProject (ID: def456)
> 
> 请选择要映射的项目编号：1
> 
> 请输入本地项目名称：MyProject
> 
> 是否设为默认映射？(Y/N): Y
> 
> ✅ 项目映射配置完成！
> 
> 🧪 验证映射...
> ✅ 映射验证成功
```

## Notes

- 需要配置至少一个项目映射才能拉取任务
- 默认映射用于省略 projectId 参数时
- 可以配置多个项目映射