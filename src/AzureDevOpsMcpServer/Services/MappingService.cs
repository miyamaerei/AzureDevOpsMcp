using AzureDevOpsMcpServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureDevOpsMcpServer.Services;

public class MappingService
{
    private readonly AppDbContext _dbContext;

    public MappingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 根据本地项目名称获取映射
    /// </summary>
    public async Task<ProjectMapping?> GetMappingByLocalProjectAsync(string localProjectName)
    {
        return await _dbContext.ProjectMappings
            .FirstOrDefaultAsync(pm => pm.LocalProjectName == localProjectName);
    }

    /// <summary>
    /// 根据工作目录获取映射
    /// </summary>
    public async Task<ProjectMapping?> GetMappingByWorkingDirectoryAsync(string workingDirectory)
    {
        return await _dbContext.ProjectMappings
            .FirstOrDefaultAsync(pm => pm.WorkingDirectory == workingDirectory);
    }

    /// <summary>
    /// 根据 Azure DevOps 项目 ID 获取映射
    /// </summary>
    public async Task<ProjectMapping?> GetMappingByAzureProjectIdAsync(string azureProjectId)
    {
        return await _dbContext.ProjectMappings
            .FirstOrDefaultAsync(pm => pm.AzureDevOpsProjectId == azureProjectId);
    }

    /// <summary>
    /// 获取所有映射
    /// </summary>
    public async Task<IEnumerable<ProjectMapping>> GetAllMappingsAsync()
    {
        return await _dbContext.ProjectMappings.ToListAsync();
    }

    /// <summary>
    /// 获取默认映射
    /// </summary>
    public async Task<ProjectMapping?> GetDefaultMappingAsync()
    {
        return await _dbContext.ProjectMappings
            .FirstOrDefaultAsync(pm => pm.IsDefault);
    }

    /// <summary>
    /// 验证映射是否存在且有效
    /// </summary>
    public async Task<bool> ValidateMappingAsync(string localProjectName)
    {
        var mapping = await GetMappingByLocalProjectAsync(localProjectName);
        return mapping != null && !string.IsNullOrEmpty(mapping.AzureDevOpsProjectId);
    }

    /// <summary>
    /// 创建或更新项目映射
    /// </summary>
    public async Task<ProjectMapping> CreateOrUpdateMappingAsync(
        string localProjectName, 
        string azureProjectId, 
        string azureProjectName, 
        string organization,
        string? workingDirectory = null,
        bool isDefault = false)
    {
        if (isDefault)
        {
            var defaultMappings = await _dbContext.ProjectMappings
                .Where(pm => pm.IsDefault && !pm.LocalProjectName.Equals(localProjectName))
                .ToListAsync();

            foreach (var mapping in defaultMappings)
            {
                mapping.IsDefault = false;
                mapping.UpdatedAt = DateTime.UtcNow;
            }
        }

        var existingMapping = await GetMappingByLocalProjectAsync(localProjectName);
        
        if (existingMapping != null)
        {
            existingMapping.AzureDevOpsProjectId = azureProjectId;
            existingMapping.AzureDevOpsProjectName = azureProjectName;
            existingMapping.Organization = organization;
            existingMapping.WorkingDirectory = workingDirectory;
            existingMapping.IsDefault = isDefault;
            existingMapping.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existingMapping = new ProjectMapping
            {
                Id = Guid.NewGuid(),
                LocalProjectName = localProjectName,
                AzureDevOpsProjectId = azureProjectId,
                AzureDevOpsProjectName = azureProjectName,
                Organization = organization,
                WorkingDirectory = workingDirectory,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.ProjectMappings.Add(existingMapping);
        }
        
        await _dbContext.SaveChangesAsync();
        return existingMapping;
    }

    /// <summary>
    /// 删除项目映射
    /// </summary>
    public async Task<bool> DeleteMappingAsync(string localProjectName)
    {
        var mapping = await GetMappingByLocalProjectAsync(localProjectName);
        if (mapping == null) return false;
        
        _dbContext.ProjectMappings.Remove(mapping);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}