# Configure PAT Skill

## Purpose

帮助用户配置 Azure DevOps PAT（Personal Access Token）。

## Description

该技能引导用户创建和配置 Azure DevOps PAT 令牌，包括：
1. 指导用户在 Azure DevOps 中生成 PAT
2. 配置环境变量
3. 验证 PAT 有效性

## Configuration

```yaml
name: configure-pat
version: 1.0.0
description: Azure DevOps PAT configuration skill
author: Development Platform Team
```

## Tools Used

- Azure DevOps API - 验证 PAT 有效性

## Dependencies

- Azure DevOps 账号
- 网络访问 Azure DevOps

## Execution Steps

1. **说明 PAT 用途**: 解释 PAT 的作用和权限要求
2. **引导生成 PAT**: 指导用户在 Azure DevOps 中生成 PAT
3. **配置环境变量**: 设置 AZURE_DEVOPS_PAT 环境变量
4. **验证 PAT**: 测试 PAT 是否有效

## Usage

```
/configure-pat
```

## Output

- PAT 配置状态
- 验证结果
- 权限检查报告

## Examples

```
/configure-pat

> 配置 Azure DevOps PAT
> 
> 📋 PAT 权限要求：
> - Work Items: Read & Write
> - Projects and Teams: Read
> 
> 🔗 生成链接：https://dev.azure.com/{organization}/_usersSettings/tokens
> 
> 请输入 PAT 令牌：********************
> 
> 🔧 配置环境变量...
> ✅ AZURE_DEVOPS_PAT 已设置
> 
> 🧪 验证 PAT...
> ✅ PAT 验证成功
> ✅ 权限检查通过
> 
> PAT 配置完成！
```

## Notes

- PAT 需要 Work Items 读写权限
- PAT 应妥善保管，不要提交到版本控制
- 建议定期轮换 PAT