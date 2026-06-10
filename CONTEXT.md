# AI辅助开发管理流程

基于Azure DevOps + MCP + Skills + GitNexus的AI辅助开发管理流程原型。

## 语言

**行为驱动开发 (BDD)**:
一种以用户行为为核心的软件开发方法，通过自然语言描述系统应表现出的行为。
_Avoid_: 测试驱动开发

**Given-When-Then**:
Gherkin语言中描述场景行为流程的三元组：Given（前置条件）、When（触发动作）、Then（预期结果）。

**Three Amigos**:
产品负责人、开发者和测试人员三方协作的工作模式。
_Avoid_: 三方会议

**可执行文档**:
由自动化测试支持的需求文档，场景既是验收标准又是可运行的测试。
_Avoid_: 活文档

**当前任务**:
正在积极开发中的任务，处于"当前任务"状态。
_Avoid_: 进行中任务、进行中

**阻塞中**:
因外部依赖或问题无法继续推进的任务。
_Avoid_: 卡住、停滞

**未实现**:
尚未开始或被搁置的任务。
_Avoid_: 待办、未开始

**归档**:
已完成所有验证步骤的任务，包括CI通过、QA测试通过和Workitem同步完成。等同于"验证通过"状态。
_Avoid_: 完成、关闭

**验证通过**:
与"归档"同义。任务完成所有质量门禁的状态。
_Avoid_: 完成、关闭

**MCP Server**:
基于模型上下文协议(MCP)的服务端实现，提供任务管理和集成能力。

**服务端 PAT**:
MCP Server 用于访问 Azure DevOps API 的个人访问令牌，由管理员在后台配置。
_Avoid_: server PAT

**Pipeline ID**:
Azure DevOps流水线运行的唯一标识符，用于追踪和验证构建结果。

**MCP**:
模型上下文协议，用于在不同系统间同步任务状态和上下文信息。
_Avoid_: Model Context Protocol

**GitNexus**:
代码知识图谱引擎，通过 MCP 协议为 AI Agent 提供代码依赖关系、调用链和影响半径分析能力。

**AutoJob文档**:
任务分类管理文档，包含归档、未实现、当前任务三个分类。

**PAT令牌**:
个人访问令牌，用于Azure DevOps API认证和授权。
_Avoid_: Personal Access Token

**CI流水线**:
持续集成流水线，自动构建、测试和部署代码变更。
_Avoid_: 持续集成、CI/CD

**Workitem**:
Azure DevOps中的工作项，包括任务、Bug、用户故事等。
_Avoid_: 工作项、任务项