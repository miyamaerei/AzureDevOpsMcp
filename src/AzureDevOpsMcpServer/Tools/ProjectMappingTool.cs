using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

public class ProjectMappingTool
{
    private readonly MappingService _mappingService;

    public ProjectMappingTool(MappingService mappingService)
    {
        _mappingService = mappingService;
    }

    [McpServerTool]
    [Description("配置本地项目与Azure DevOps项目的映射关系")]
    public async Task<ProjectMapping> SetProjectMapping(
        [Description("本地项目名称")] string localProject,
        [Description("Azure DevOps项目ID")] string azureProjectId,
        [Description("Azure DevOps项目名称")] string azureProjectName,
        [Description("组织名称")] string organization)
    {
        return await _mappingService.CreateOrUpdateMappingAsync(localProject, azureProjectId, azureProjectName, organization);
    }

    [McpServerTool]
    [Description("获取项目映射信息")]
    public async Task<ProjectMapping?> GetProjectMapping(
        [Description("本地项目名称")] string localProject)
    {
        return await _mappingService.GetMappingByLocalProjectAsync(localProject);
    }

    [McpServerTool]
    [Description("获取所有项目映射")]
    public async Task<IEnumerable<ProjectMapping>> GetAllProjectMappings()
    {
        return await _mappingService.GetAllMappingsAsync();
    }

    [McpServerTool]
    [Description("删除项目映射")]
    public async Task<bool> DeleteProjectMapping(
        [Description("本地项目名称")] string localProject)
    {
        return await _mappingService.DeleteMappingAsync(localProject);
    }
}