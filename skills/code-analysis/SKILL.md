# Code Analysis Skill

## Purpose

提供代码分析能力，帮助用户理解代码结构和影响范围。

## Description

该技能封装 GitNexus 的代码分析能力，提供：
1. 代码影响分析
2. 依赖关系查看
3. 代码复杂度评估
4. 重构建议

## Configuration

```yaml
name: code-analysis
version: 1.0.0
description: Code analysis skill based on GitNexus
author: Development Platform Team
```

## Tools Used

- `gitnexus impact()` - 分析代码影响范围
- `gitnexus context()` - 查看代码上下文
- `gitnexus search()` - 搜索代码

## Dependencies

- GitNexus - 需要提前安装和索引

## Execution Steps

1. **选择分析类型**: 影响分析、上下文查看或代码搜索
2. **输入参数**: 根据选择提供参数
3. **执行分析**: 调用 GitNexus 进行分析
4. **展示结果**: 显示分析结果和建议

## Usage

```
/code-analysis [type] [target]
```

### Parameters

| 参数 | 类型 | 说明 |
|------|------|------|
| type | string | 分析类型：impact、context、search |
| target | string | 目标文件或函数名（可选） |

## Output

- 分析结果摘要
- 影响范围报告
- 代码依赖关系图
- 重构建议

## Examples

```
/code-analysis impact src/services/auth.ts

> 分析 src/services/auth.ts 的影响范围...
> 
> 直接依赖：
> - src/utils/jwt.ts
> - src/models/User.ts
> 
> 被引用位置：
> - src/controllers/auth.controller.ts
> - src/middleware/auth.middleware.ts
> - src/tests/auth.test.ts
> 
> 影响评估：高
> 建议：修改前进行完整测试

/code-analysis context validateUser

> 查看 validateUser 函数的上下文...
> 
> 定义位置：src/services/auth.ts:42
> 
> 函数签名：
> function validateUser(email: string, password: string): Promise<User>
> 
> 调用链：
> auth.controller.ts → login() → validateUser()
> 
> 依赖函数：
> - hashPassword()
> - findUserByEmail()
> 
> 被测试覆盖：是
> 测试文件：src/tests/auth.test.ts
```

## Notes

- 需要提前运行 `gitnexus analyze` 建立索引
- 分析结果取决于代码库索引的完整性