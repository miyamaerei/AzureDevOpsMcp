# 最佳实践指南

本文档总结了使用 AI 辅助开发管理平台的最佳实践，帮助团队提高开发效率和代码质量。

## 目录

1. [项目初始化最佳实践](#项目初始化最佳实践)
2. [任务管理最佳实践](#任务管理最佳实践)
3. [代码开发最佳实践](#代码开发最佳实践)
4. [代码审查最佳实践](#代码审查最佳实践)
5. [文档编写最佳实践](#文档编写最佳实践)
6. [团队协作最佳实践](#团队协作最佳实践)

---

## 项目初始化最佳实践

### 1. 使用一键初始化脚本

**推荐做法**：
```bash
# Linux/Mac
curl -sSL https://platform.company.com/init-project.sh | bash

# Windows PowerShell
Invoke-Expression (Invoke-WebRequest -Uri https://platform.company.com/init-project.ps1 -UseBasicParsing).Content
```

**避免**：
- 手动配置开发环境
- 跳过依赖检查步骤
- 使用不同版本的依赖工具

### 2. 配置项目映射

**推荐做法**：
- 在初始化时配置本地项目与 Azure DevOps 项目的映射
- 设置默认项目以便快速拉取任务
- 确保项目映射准确无误

**示例**：
```
本地项目: my-feature-project
Azure 项目: MyFeatureProject
组织: https://dev.azure.com/mycompany
默认项目: Yes
```

### 3. 验证初始化结果

**检查清单**：
- ✅ GitNexus 已安装并运行代码索引
- ✅ Matt Pocock skills 已安装
- ✅ MCP 连接已配置
- ✅ 项目映射已设置
- ✅ CONTEXT.md 已生成
- ✅ docs/adr/ 目录已创建

---

## 任务管理最佳实践

### 1. 任务状态管理

**四状态模型**：
- **当前任务**：正在开发中的任务
- **阻塞中**：因外部依赖无法继续的任务
- **未实现**：暂时搁置的任务
- **归档**：已完成并验证的任务

**状态流转规则**：
```
Azure DevOps 拉取 → 当前任务
当前任务 → 阻塞中（遇到外部依赖）
当前任务 → 未实现（需要搁置）
阻塞中 → 当前任务（依赖已解决）
未实现 → 当前任务（重新开始）
当前任务 → 归档（完成并验证）
归档 → Azure DevOps（自动同步状态）
```

### 2. 任务拉取策略

**推荐做法**：
- 每天开始工作时拉取指派给自己的任务
- 按项目过滤任务，专注于当前项目
- 使用 `/task-workflow` 技能引导开发流程

**示例**：
```bash
# 拉取所有指派给我的任务
GetAssignedTasks(userId: "me")

# 拉取特定项目的任务
GetAssignedTasks(userId: "me", projectId: "MyProject")
```

### 3. 任务状态同步

**自动同步**：
- 任务完成时自动同步到 Azure DevOps
- 使用 `UpdateTaskStatus` 工具更新状态
- 确保本地状态与 Azure DevOps 保持一致

**手动同步**：
```bash
# 更新任务状态
UpdateTaskStatus(taskId: "12345", status: "Archived")
```

---

## 代码开发最佳实践

### 1. 使用 TDD 开发流程

**推荐做法**：
- 使用 `/tdd` 技能进行测试驱动开发
- 遵循 Red-Green-Refactor 循环
- 先写测试，再写实现，最后重构

**TDD 流程**：
```
1. Red: 编写失败的测试
2. Green: 编写最小实现使测试通过
3. Refactor: 重构代码，保持测试通过
4. 重复以上步骤
```

### 2. 代码影响分析

**推荐做法**：
- 提交前使用 GitNexus 分析代码修改的影响范围
- 使用 `impact()` 工具查看受影响的代码
- 使用 `context()` 工具查看代码依赖关系

**示例**：
```bash
# 分析修改的影响范围
gitnexus impact()

# 查看代码依赖关系
gitnexus context()
```

### 3. 代码质量保障

**检查清单**：
- ✅ 所有测试通过
- ✅ 代码覆盖率达标（≥ 80%）
- ✅ 代码复杂度合理
- ✅ 无明显的代码异味
- ✅ 文档已更新

---

## 代码审查最佳实践

### 1. 使用结构化审查流程

**推荐做法**：
- 使用 `/code-review` 技能进行代码审查
- 关注代码质量、正确性和最佳实践
- 提供建设性的反馈意见

### 2. 审查重点

**关注点**：
- **功能正确性**：代码是否实现了预期功能？
- **代码质量**：代码是否清晰、可维护？
- **性能**：是否存在性能问题？
- **安全性**：是否存在安全风险？
- **测试覆盖**：是否有足够的测试？

### 3. 提供有价值的反馈

**推荐做法**：
- 具体指出问题所在
- 提供改进建议
- 解释为什么需要改进
- 给出代码示例

---

## 文档编写最佳实践

### 1. 维护 CONTEXT.md

**推荐做法**：
- 在项目根目录维护 CONTEXT.md
- 定义领域语言和术语
- 记录重要的架构决策
- 保持文档更新

**CONTEXT.md 结构**：
```markdown
# 项目名称

## 领域语言
- 术语1: 定义
- 术语2: 定义

## 架构决策
- 决策1: 原因和影响
- 决策2: 原因和影响

## 模块说明
- 模块1: 职责和接口
- 模块2: 职责和接口
```

### 2. 记录架构决策

**推荐做法**：
- 在 `docs/adr/` 目录记录架构决策
- 使用 ADR (Architecture Decision Record) 格式
- 记录决策背景、考虑的方案和最终选择

**ADR 模板**：
```markdown
# ADR-XXX: 决策标题

## 状态
已接受 / 已废弃 / 已取代

## 背景
描述决策的背景和问题

## 决策
描述做出的决策

## 后果
描述决策的影响和后果
```

### 3. 使用 `/grill-with-docs` 技能

**推荐做法**：
- 在需求分析阶段使用此技能
- 建立共享语言和术语
- 更新 CONTEXT.md 和 ADR 文档
- 确保团队对需求的理解一致

---

## 团队协作最佳实践

### 1. 使用标准开发流程

**推荐流程**：
```
1. 初始化项目 → 配置开发环境
2. 配置项目映射 → 连接 Azure DevOps
3. 拉取任务 → GetAssignedTasks
4. 需求分析 → /grill-with-docs
5. TDD 开发 → /tdd
6. 代码分析 → GitNexus impact()
7. 代码审查 → /code-review
8. 提交代码 → 遵循提交规范
9. 同步状态 → UpdateTaskStatus
```

### 2. 遵循分支管理策略

**推荐策略**：
- `main`: 主分支，始终保持可发布状态
- `develop`: 开发分支，集成各功能分支
- `feature/*`: 功能分支，开发新功能
- `bugfix/*`: 修复分支，修复 bug
- `release/*`: 发布分支，准备发布

**分支命名规范**：
```
feature/任务ID-功能描述
bugfix/任务ID-修复描述
release/版本号
```

### 3. 遵循提交规范

**提交消息格式**：
```
<type>(<scope>): <subject>

<body>

<footer>
```

**类型 (type)**：
- `feat`: 新功能
- `fix`: 修复 bug
- `docs`: 文档更新
- `style`: 代码格式调整
- `refactor`: 重构
- `test`: 测试相关
- `chore`: 构建/工具相关

**示例**：
```
feat(auth): 添加 Windows 集成认证支持

实现了基于 Windows 用户身份的自动认证功能，
开发者无需手动配置凭证即可访问 MCP Server。

Closes #123
```

### 4. 使用共享技能

**推荐技能**：
- `/task-workflow`: 任务开发工作流
- `/grill-with-docs`: 需求分析和文档更新
- `/tdd`: 测试驱动开发
- `/diagnose`: 结构化调试
- `/improve-codebase-architecture`: 架构优化
- `/code-review`: 代码审查

---

## 常见问题与解决方案

### 问题 1: 初始化脚本执行失败

**可能原因**：
- Node.js 版本过低（需要 ≥ 18.0.0）
- Git 未安装
- 网络连接问题

**解决方案**：
```bash
# 检查依赖
./scripts/modules/check-deps.sh

# 手动安装缺失的依赖
# Node.js: https://nodejs.org
# Git: https://git-scm.com
```

### 问题 2: MCP 连接失败

**可能原因**：
- MCP Server 未启动
- 配置文件错误
- 认证失败

**解决方案**：
```bash
# 检查 MCP Server 状态
curl http://localhost:5000/health

# 重新配置 MCP 连接
./scripts/modules/config-mcp.sh
```

### 问题 3: GitNexus 代码索引失败

**可能原因**：
- 项目代码量过大
- 文件权限问题
- 内存不足

**解决方案**：
```bash
# 清理缓存重新索引
rm -rf .gitnexus
gitnexus analyze

# 增加内存限制
NODE_OPTIONS="--max-old-space-size=4096" gitnexus analyze
```

### 问题 4: 任务状态同步失败

**可能原因**：
- Azure DevOps API 不可用
- PAT 令牌过期
- 网络连接问题

**解决方案**：
```bash
# 检查 Azure DevOps 连接
curl https://dev.azure.com/{organization}/_apis/projects

# 更新 PAT 令牌
./scripts/modules/config-mcp.sh
```

---

## 总结

遵循这些最佳实践可以帮助团队：

1. **提高效率**：标准化流程减少重复劳动
2. **保证质量**：TDD 和代码审查确保代码质量
3. **促进协作**：共享语言和文档改善沟通
4. **降低风险**：代码影响分析避免破坏性修改
5. **持续改进**：定期回顾和优化流程

记住：**最佳实践是指导原则，不是死板规则**。根据项目实际情况灵活应用，持续改进。
