# 分支管理策略

## 1. 概述

本文档定义了项目的分支管理策略，旨在确保代码版本控制的有序性和可追溯性，支持高效的团队协作和持续集成。

## 2. 分支模型

### 2.1 整体架构

```
main (生产分支)
    │
    └── develop (开发分支)
            │
            ├── feature/* (功能分支)
            ├── bugfix/* (Bug 修复分支)
            ├── release/* (发布分支)
            └── hotfix/* (紧急修复分支)
```

### 2.2 分支类型说明

| 分支类型 | 命名规范 | 说明 | 生命周期 |
|----------|----------|------|----------|
| **main** | `main` | 生产环境代码，稳定可部署 | 永久 |
| **develop** | `develop` | 开发集成分支，包含最新功能 | 永久 |
| **feature** | `feature/xxx` | 功能开发分支 | 临时 |
| **bugfix** | `bugfix/xxx` | Bug 修复分支 | 临时 |
| **release** | `release/x.y.z` | 发布准备分支 | 临时 |
| **hotfix** | `hotfix/xxx` | 紧急修复分支 | 临时 |

## 3. 分支详细说明

### 3.1 main 分支

- **用途**: 存放可直接部署到生产环境的稳定代码
- **保护**: 设置分支保护规则，禁止直接推送
- **更新方式**: 仅通过 Pull Request 合并

### 3.2 develop 分支

- **用途**: 集成所有开发中的功能
- **来源**: 从 `main` 分支创建
- **更新方式**: 通过 Pull Request 合并 feature/bugfix 分支
- **稳定性**: 应保持可构建、可测试的状态

### 3.3 feature 分支

- **用途**: 开发新功能
- **来源**: 从 `develop` 分支创建
- **命名**: `feature/<功能名称>`，如 `feature/task-status-transition`
- **生命周期**: 功能开发完成后合并到 `develop`，然后删除

### 3.4 bugfix 分支

- **用途**: 修复开发环境中的 Bug
- **来源**: 从 `develop` 分支创建
- **命名**: `bugfix/<问题描述>`，如 `bugfix/status-mapping-error`
- **生命周期**: Bug 修复完成后合并到 `develop`，然后删除

### 3.5 release 分支

- **用途**: 准备新版本发布
- **来源**: 从 `develop` 分支创建
- **命名**: `release/<版本号>`，如 `release/1.0.0`
- **生命周期**: 发布完成后合并到 `main` 和 `develop`，然后删除

### 3.6 hotfix 分支

- **用途**: 修复生产环境中的紧急问题
- **来源**: 从 `main` 分支创建
- **命名**: `hotfix/<问题描述>`，如 `hotfix/critical-api-error`
- **生命周期**: 修复完成后合并到 `main` 和 `develop`，然后删除

## 4. 分支操作流程

### 4.1 功能开发流程

```
1. 创建 feature 分支
   git checkout -b feature/my-feature develop

2. 开发功能并提交
   git add .
   git commit -m "feat: 实现功能"

3. 推送分支到远程
   git push origin feature/my-feature

4. 创建 Pull Request 到 develop

5. 代码审查通过后合并

6. 删除本地和远程分支
   git branch -d feature/my-feature
   git push origin --delete feature/my-feature
```

### 4.2 Bug 修复流程

```
1. 创建 bugfix 分支
   git checkout -b bugfix/my-bugfix develop

2. 修复 Bug 并提交
   git add .
   git commit -m "fix: 修复 Bug"

3. 推送分支到远程
   git push origin bugfix/my-bugfix

4. 创建 Pull Request 到 develop

5. 代码审查通过后合并

6. 删除分支
```

### 4.3 发布流程

```
1. 创建 release 分支
   git checkout -b release/1.0.0 develop

2. 更新版本号和发布说明

3. 进行最终测试和验证

4. 合并到 main 分支
   git checkout main
   git merge --no-ff release/1.0.0
   git tag -a v1.0.0 -m "Version 1.0.0"

5. 合并到 develop 分支
   git checkout develop
   git merge --no-ff release/1.0.0

6. 删除 release 分支
   git branch -d release/1.0.0
   git push origin --delete release/1.0.0
```

### 4.4 紧急修复流程

```
1. 创建 hotfix 分支
   git checkout -b hotfix/my-hotfix main

2. 修复问题并提交
   git add .
   git commit -m "fix: 紧急修复"

3. 合并到 main 分支
   git checkout main
   git merge --no-ff hotfix/my-hotfix
   git tag -a v1.0.1 -m "Version 1.0.1"

4. 合并到 develop 分支
   git checkout develop
   git merge --no-ff hotfix/my-hotfix

5. 删除 hotfix 分支
   git branch -d hotfix/my-hotfix
   git push origin --delete hotfix/my-hotfix
```

## 5. 分支保护规则

### 5.1 main 分支

- [ ] 禁止直接推送
- [ ] 要求至少 1 个审查者批准
- [ ] 要求所有测试通过
- [ ] 禁止强制推送
- [ ] 启用分支保护

### 5.2 develop 分支

- [ ] 禁止直接推送
- [ ] 要求至少 1 个审查者批准
- [ ] 要求所有测试通过

### 5.3 其他分支

- [ ] 建议启用审查要求
- [ ] 建议启用测试检查

## 6. Pull Request 规范

### 6.1 PR 标题格式

```
<类型>(<范围>): <简要描述>
```

### 6.2 PR 描述要求

- 描述变更的目的和内容
- 列出主要修改的文件
- 提供测试说明
- 引用相关 Issue
- 说明是否有破坏性变更

### 6.3 PR 检查清单

- [ ] 代码符合编码规范
- [ ] 所有测试通过
- [ ] 代码审查通过
- [ ] 没有未解决的冲突
- [ ] 提交消息符合规范

## 7. 版本号规范

### 7.1 版本格式

```
v<主版本号>.<次版本号>.<修订号>
```

### 7.2 版本号规则

| 类型 | 变更内容 | 版本号变化 | 示例 |
|------|----------|------------|------|
| **主版本** | 破坏性变更、重大重构 | 主版本号 + 1 | v2.0.0 |
| **次版本** | 新增功能、向后兼容 | 次版本号 + 1 | v1.1.0 |
| **修订号** | Bug 修复、小改进 | 修订号 + 1 | v1.0.1 |

## 8. 标签管理

### 8.1 标签命名

```
v<版本号>
```

### 8.2 标签创建

```bash
git tag -a v1.0.0 -m "Version 1.0.0"
git push origin v1.0.0
```

### 8.3 标签用途

- 标记发布版本
- 用于部署回滚
- 便于版本追溯

## 9. 代码审查规范

### 9.1 审查者要求

- 至少 1 名审查者
- 审查者应熟悉相关代码模块
- 审查时间应在 PR 创建后 24 小时内完成

### 9.2 审查要点

- 代码正确性
- 代码质量（可读性、可维护性）
- 安全性
- 测试覆盖
- 符合编码规范

## 10. 冲突解决

### 10.1 分支冲突

当分支存在冲突时：

1. 从目标分支拉取最新代码
2. 在本地解决冲突
3. 测试修复后的代码
4. 推送更新

```bash
git checkout feature/my-feature
git fetch origin
git merge origin/develop
# 解决冲突
git add .
git commit -m "Merge develop, resolve conflicts"
git push origin feature/my-feature
```

## 11. 工具支持

推荐使用以下工具辅助分支管理：

- **GitHub/GitLab**: 提供分支保护和 PR 功能
- **GitFlow**: 分支模型管理工具
- **Dependabot**: 依赖更新管理

## 12. 版本控制

本文档版本：v1.0  
最后更新：2026-06-10