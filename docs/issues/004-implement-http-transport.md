# Issue #4: 实现 HTTP 传输模式

## 父级

N/A - 新功能

## 需要构建的内容

将 MCP Server 从 Stdio 传输模式改为 HTTP 传输模式，支持远程调用。

### PRD 要求

根据 PRD：
- MCP Server 采用 HTTP 传输模式，支持远程调用
- 端点：https://platform.company.com/mcp
- 支持 HTTPS

### 实现方案

使用 `Microsoft.Extensions.AI.Server` 包配置 HTTP 传输：

```csharp
// Program.cs
builder.Services
    .AddMcpServer()
    .WithHttpServerTransport()  // 使用 HTTP 传输
    .WithTools<WorkItemTool>()
    .WithTools<ProjectRepositoryTool>();

// 配置 HTTPS 和端点
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});
```

### 认证集成

HTTP 模式需要与 Windows 认证集成：
- 使用 Negotiate/NTLM 认证
- 或使用 API Key 认证

### 配置

```json
{
  "AzureDevOps": {
    "HttpPort": 5000,
    "UseHttps": true,
    "RequireAuthentication": true
  }
}
```

## 验收标准

- [ ] 添加 Microsoft.Extensions.AI.Server 包
- [ ] 配置 HTTP 传输模式
- [ ] 配置 HTTPS 支持
- [ ] 集成 Windows 认证
- [ ] 测试远程调用
- [ ] 更新文档说明部署方式

## 阻塞

Issue #3: 实现 Windows 集成认证（建议先完成认证）

## 优先级

高

## 来源

根据 PRD 实现决策: "MCP Server 服务端 PAT：采用 HTTP 传输模式，支持远程调用"
