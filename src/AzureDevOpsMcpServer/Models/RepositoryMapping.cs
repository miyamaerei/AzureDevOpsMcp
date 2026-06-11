using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// 本地项目、Azure DevOps Project 和 Azure Repo 的映射关系。
/// </summary>
public class RepositoryMapping
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Windows 用户名。仓库本地路径和默认映射必须按用户隔离。
    /// </summary>
    public string WindowsUsername { get; set; } = string.Empty;

    /// <summary>
    /// Azure DevOps 用户标识。
    /// </summary>
    public string AzureDevOpsUser { get; set; } = string.Empty;

    /// <summary>
    /// 当前机器名，用于区分同一用户在多台机器上的本地路径。
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// 本地项目名称。
    /// </summary>
    public string LocalProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 本地工作目录路径。
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Azure DevOps 项目 ID。
    /// </summary>
    public string AzureDevOpsProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Azure DevOps 项目名称。
    /// </summary>
    public string AzureDevOpsProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Azure DevOps 仓库 ID。GitHub 仓库映射可为空。
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// 仓库提供方，例如 GitHub 或 AzureRepos。
    /// </summary>
    public string RepositoryProvider { get; set; } = "AzureRepos";

    /// <summary>
    /// 仓库 Owner。GitHub 场景为 owner/organization。
    /// </summary>
    public string RepositoryOwner { get; set; } = string.Empty;

    /// <summary>
    /// Azure Repo 名称或 GitHub repo 名称。
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// 仓库远程地址。
    /// </summary>
    public string RemoteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Azure DevOps 组织名称。
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认仓库映射。
    /// </summary>
    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
