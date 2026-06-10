# Issue #3: 实现 Windows 集成认证

## 父级

N/A - 新功能

## 需要构建的内容

实现 Windows 集成认证，使 MCP Server 能够识别调用者的 Windows 用户身份。

### PRD 要求

根据 PRD：
- MCP Server 使用 Windows 集成认证识别调用者身份
- 开发者无需手动配置凭证
- 通过 Windows 用户名自动映射到 Azure DevOps 用户

### 实现方案

#### 方案 A: 使用 IIS/ASP.NET Core 托管

```csharp
// Program.cs
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization();
```

#### 方案 B: 使用 HTTP + Negotiate 认证

修改传输层支持 HTTP + Windows Negotiate 认证。

### 用户名映射

需要实现 Windows 用户名到 Azure DevOps 用户的映射：

```csharp
public interface IUserMappingService
{
    Task<string?> GetAzureDevOpsUserFromWindowsUserAsync(string windowsUsername);
    Task SetUserMappingAsync(string windowsUsername, string azureDevOpsUser);
}
```

存储映射关系到 SQLite 数据库。

### 数据模型

```csharp
public class UserMapping
{
    public Guid Id { get; set; }
    public string WindowsUsername { get; set; }      // 如: DOMAIN\username
    public string AzureDevOpsUser { get; set; }      // 如: user@company.com
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## 验收标准

- [x] 创建 UserMapping 数据模型
- [x] 创建 UserMappingService
- [x] 实现用户名映射功能
- [ ] 修改传输层支持 Windows 认证（需要 Issue #4 HTTP 传输）
- [x] 在调用 MCP 工具时注入当前用户信息
- [x] 编写测试
- [ ] 更新文档

## 阻塞

Issue #4: 实现 HTTP 传输模式（Windows 认证需要 HTTP 传输才能完整实现）

## 优先级

高

## 来源

根据 PRD 用户故事 #10: "我希望配置 MCP Server 使用 Windows 用户认证，以便识别调用者身份"

## 完成状态

### 已完成
- UserMapping 数据模型 (`src/AzureDevOpsMcpServer/Models/UserMapping.cs`)
- UserMappingService 实现 (`src/AzureDevOpsMcpServer/Services/UserMappingService.cs`)
- UserContext 服务 (`src/AzureDevOpsMcpServer/Services/UserContext.cs`)
- UserMappingTool MCP 工具 (`src/AzureDevOpsMcpServer/Tools/UserMappingTool.cs`)
- 测试文件:
  - `tests/AzureDevOpsMcpServer.Tests/UserMappingTests.cs` (7 个测试)
  - `tests/AzureDevOpsMcpServer.Tests/UserContextTests.cs` (5 个测试)

### 待完成
- Issue #4 HTTP 传输模式（需要配置 ASP.NET Core 的 Negotiate 认证才能完整支持 Windows 集成认证）

### 使用方式
```csharp
// 获取当前用户的 Azure DevOps 用户名
var azureDevOpsUser = await userContext.GetCurrentAzureDevOpsUserAsync();

// 设置用户映射
await userMappingService.CreateOrUpdateUserMappingAsync("DOMAIN\\user", "user@company.com");

// MCP 工具调用
var currentUser = await userMappingTool.GetCurrentAzureDevOpsUser();
```
