# Task Workflow Skill

## Purpose

引导用户完成完整的任务开发工作流，从拉取任务到同步状态到 Azure DevOps。

## Description

该技能提供标准化的任务开发流程，包含以下步骤：
1. 拉取指派给当前用户的任务
2. 选择要处理的任务
3. 进行需求分析（调用 /grill-with-docs）
4. 进行 TDD 开发（调用 /tdd）
5. 分析代码影响（调用 GitNexus）
6. 提交代码
7. 同步任务状态到 Azure DevOps

## Configuration

```yaml
name: task-workflow
version: 1.0.0
description: Task development workflow skill
author: Development Platform Team
```

## Tools Used

- `GetAssignedTasks()` - 获取指派任务
- `GetTaskDetails()` - 获取任务详情
- `UpdateTaskStatus()` - 更新任务状态
- `SyncTaskToAzureDevOps()` - 同步任务到 Azure DevOps

## Dependencies

- `/grill-with-docs` - 需求分析
- `/tdd` - 测试驱动开发
- `gitnexus` - 代码分析

## Execution Steps

1. **拉取任务**: 调用 `GetAssignedTasks()` 获取指派给当前用户的任务列表
2. **选择任务**: 用户选择要处理的任务
3. **需求分析**: 调用 `/grill-with-docs` 分析需求
4. **TDD 开发**: 调用 `/tdd` 进行测试驱动开发
5. **代码分析**: 调用 `gitnexus impact()` 分析代码影响
6. **代码提交**: 指导用户提交代码
7. **状态同步**: 调用 `UpdateTaskStatus()` 和 `SyncTaskToAzureDevOps()`

## Usage

```
/task-workflow
```

## Output

- 任务开发完成报告
- 代码变更摘要
- Azure DevOps 同步状态

## Examples

```
/task-workflow

> 获取到 3 个指派任务：
> 1. [Task-123] 实现用户登录功能
> 2. [Task-124] 修复数据同步 Bug
> 3. [Task-125] 优化报表查询性能
> 
> 请选择要处理的任务编号：1
> 
> 正在分析任务需求...
> 调用 /grill-with-docs...
> 
> 需求分析完成，开始 TDD 开发...
> 调用 /tdd 实现用户登录功能...
> 
> 代码实现完成，分析影响范围...
> 调用 gitnexus impact()...
> 
> 代码分析完成，请提交代码。
> 
> 代码提交成功，同步任务状态...
> 
> ✅ 任务状态已同步到 Azure DevOps！
```

## Notes

- 需要配置项目映射才能正确拉取任务
- 需要 Windows 集成认证才能识别当前用户
- 任务完成后会自动同步到 Azure DevOps