using AzureDevOpsMcpServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 管理本地项目到 Azure Repo 的映射。
/// </summary>
public class RepositoryMappingService
{
    private readonly AppDbContext _dbContext;

    public RepositoryMappingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RepositoryMapping?> GetRepositoryMappingByLocalProjectAsync(string windowsUsername, string localProjectName)
    {
        var mappings = await _dbContext.RepositoryMappings.ToListAsync();
        return mappings.FirstOrDefault(rm =>
            rm.WindowsUsername.Equals(windowsUsername, StringComparison.OrdinalIgnoreCase) &&
            rm.LocalProjectName.Equals(localProjectName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<RepositoryMapping?> GetRepositoryMappingByWorkingDirectoryAsync(string windowsUsername, string workingDirectory)
    {
        var mappings = await _dbContext.RepositoryMappings.ToListAsync();
        return mappings.FirstOrDefault(rm =>
            rm.WindowsUsername.Equals(windowsUsername, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(rm.WorkingDirectory) &&
            rm.WorkingDirectory.Equals(workingDirectory, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<RepositoryMapping?> GetDefaultRepositoryMappingAsync(string windowsUsername)
    {
        return await _dbContext.RepositoryMappings.FirstOrDefaultAsync(rm =>
            rm.WindowsUsername == windowsUsername && rm.IsDefault);
    }

    public async Task<IEnumerable<RepositoryMapping>> GetAllRepositoryMappingsAsync(string windowsUsername)
    {
        return await _dbContext.RepositoryMappings
            .Where(rm => rm.WindowsUsername == windowsUsername)
            .ToListAsync();
    }

    public async Task<RepositoryMapping> CreateOrUpdateRepositoryMappingAsync(
        string windowsUsername,
        string azureDevOpsUser,
        string localProjectName,
        string? workingDirectory,
        string azureDevOpsProjectId,
        string azureDevOpsProjectName,
        string repositoryId,
        string repositoryName,
        string remoteUrl,
        string organization,
        bool isDefault = false,
        string? machineName = null)
    {
        if (isDefault)
        {
            var defaultMappings = await _dbContext.RepositoryMappings
                .Where(rm => rm.WindowsUsername == windowsUsername && rm.IsDefault && !rm.LocalProjectName.Equals(localProjectName))
                .ToListAsync();

            foreach (var mapping in defaultMappings)
            {
                mapping.IsDefault = false;
                mapping.UpdatedAt = DateTime.UtcNow;
            }
        }

        var existingMapping = await GetRepositoryMappingByLocalProjectAsync(windowsUsername, localProjectName);
        if (existingMapping != null)
        {
            existingMapping.AzureDevOpsUser = azureDevOpsUser;
            existingMapping.MachineName = machineName ?? Environment.MachineName;
            existingMapping.WorkingDirectory = workingDirectory;
            existingMapping.AzureDevOpsProjectId = azureDevOpsProjectId;
            existingMapping.AzureDevOpsProjectName = azureDevOpsProjectName;
            existingMapping.RepositoryId = repositoryId;
            existingMapping.RepositoryName = repositoryName;
            existingMapping.RemoteUrl = remoteUrl;
            existingMapping.Organization = organization;
            existingMapping.IsDefault = isDefault;
            existingMapping.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existingMapping = new RepositoryMapping
            {
                Id = Guid.NewGuid(),
                WindowsUsername = windowsUsername,
                AzureDevOpsUser = azureDevOpsUser,
                MachineName = machineName ?? Environment.MachineName,
                LocalProjectName = localProjectName,
                WorkingDirectory = workingDirectory,
                AzureDevOpsProjectId = azureDevOpsProjectId,
                AzureDevOpsProjectName = azureDevOpsProjectName,
                RepositoryId = repositoryId,
                RepositoryName = repositoryName,
                RemoteUrl = remoteUrl,
                Organization = organization,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.RepositoryMappings.Add(existingMapping);
        }

        await _dbContext.SaveChangesAsync();
        return existingMapping;
    }
}
