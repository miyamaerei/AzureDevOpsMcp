using AzureDevOpsMcpServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// Windows 用户与 Azure DevOps 用户映射服务接口
/// </summary>
public interface IUserMappingService
{
    /// <summary>
    /// 根据 Windows 用户名获取对应的 Azure DevOps 用户
    /// </summary>
    Task<string?> GetAzureDevOpsUserFromWindowsUserAsync(string windowsUsername);
    
    /// <summary>
    /// 创建或更新用户映射
    /// </summary>
    Task<UserMapping> CreateOrUpdateUserMappingAsync(string windowsUsername, string azureDevOpsUser);
    
    /// <summary>
    /// 获取所有用户映射
    /// </summary>
    Task<IEnumerable<UserMapping>> GetAllUserMappingsAsync();
    
    /// <summary>
    /// 删除用户映射
    /// </summary>
    Task<bool> DeleteUserMappingAsync(string windowsUsername);
}

/// <summary>
/// 用户映射服务实现
/// </summary>
public class UserMappingService : IUserMappingService
{
    private readonly AppDbContext _dbContext;

    public UserMappingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 根据 Windows 用户名获取对应的 Azure DevOps 用户
    /// </summary>
    public async Task<string?> GetAzureDevOpsUserFromWindowsUserAsync(string windowsUsername)
    {
        var mapping = await _dbContext.UserMappings
            .FirstOrDefaultAsync(m => m.WindowsUsername == windowsUsername);
        
        return mapping?.AzureDevOpsUser;
    }

    /// <summary>
    /// 创建或更新用户映射
    /// </summary>
    public async Task<UserMapping> CreateOrUpdateUserMappingAsync(string windowsUsername, string azureDevOpsUser)
    {
        var existingMapping = await _dbContext.UserMappings
            .FirstOrDefaultAsync(m => m.WindowsUsername == windowsUsername);

        if (existingMapping != null)
        {
            existingMapping.AzureDevOpsUser = azureDevOpsUser;
            existingMapping.UpdatedAt = DateTime.UtcNow;
            _dbContext.UserMappings.Update(existingMapping);
        }
        else
        {
            existingMapping = new UserMapping
            {
                Id = Guid.NewGuid(),
                WindowsUsername = windowsUsername,
                AzureDevOpsUser = azureDevOpsUser,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbContext.UserMappings.AddAsync(existingMapping);
        }

        await _dbContext.SaveChangesAsync();
        return existingMapping;
    }

    /// <summary>
    /// 获取所有用户映射
    /// </summary>
    public async Task<IEnumerable<UserMapping>> GetAllUserMappingsAsync()
    {
        return await _dbContext.UserMappings.ToListAsync();
    }

    /// <summary>
    /// 删除用户映射
    /// </summary>
    public async Task<bool> DeleteUserMappingAsync(string windowsUsername)
    {
        var mapping = await _dbContext.UserMappings
            .FirstOrDefaultAsync(m => m.WindowsUsername == windowsUsername);

        if (mapping == null)
        {
            return false;
        }

        _dbContext.UserMappings.Remove(mapping);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}