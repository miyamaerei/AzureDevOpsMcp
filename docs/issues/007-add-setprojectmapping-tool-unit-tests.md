# 添加 SetProjectMapping 工具单元测试

## Parent

测试补充改进

## What to build

为 `ProjectMappingTool` 添加工具方法的单元测试，验证工具正确调用 `MappingService`。

当前 `ProjectMappingTool` 缺少直接的单元测试覆盖，需要验证：
- 工具方法正确调用 MappingService
- 映射创建/更新逻辑正常工作

## Acceptance criteria

- [ ] 添加 ProjectMappingTool 单元测试
- [ ] 验证 CreateOrUpdateMapping 方法调用
- [ ] 验证 GetMappingByLocalProject 方法调用
- [ ] 验证 ValidateMapping 方法调用

## Blocked by

None - can start immediately