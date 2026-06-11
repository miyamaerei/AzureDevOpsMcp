# 添加 GetProjects 按用户过滤测试

## Parent

测试补充改进

## What to build

增强 `GetProjects` 方法的测试，验证按用户过滤逻辑。

当前 `AzureDevOpsServiceTests` 中 `GetProjects` 测试只验证返回所有项目，未验证按用户过滤功能。

## Acceptance criteria

- [ ] 添加按用户过滤的测试用例
- [ ] 验证只返回用户可访问的项目
- [ ] 验证无权限用户返回空列表

## Blocked by

None - can start immediately