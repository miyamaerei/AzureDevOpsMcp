using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// 任务状态流转规则测试（基于 PRD 定义）
/// </summary>
public class TaskStatusTransitionTests
{
    private readonly TaskStatusTransitionService _transitionService;

    public TaskStatusTransitionTests()
    {
        _transitionService = new TaskStatusTransitionService();
    }

    #region 从 Azure DevOps 拉取后的状态转换

    [Fact]
    public void ValidateTransition_FromAzureDevOpsNew_ToCurrent_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.NotImplemented, Models.TaskStatus.Current);
        Assert.True(result);
    }

    #endregion

    #region 当前任务(Current)的状态转换

    [Fact]
    public void ValidateTransition_CurrentToBlocked_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Current, Models.TaskStatus.Blocked);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_CurrentToNotImplemented_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Current, Models.TaskStatus.NotImplemented);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_CurrentToArchived_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Current, Models.TaskStatus.Archived);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_CurrentToCurrent_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Current, Models.TaskStatus.Current);
        Assert.True(result);
    }

    #endregion

    #region 阻塞中(Blocked)的状态转换

    [Fact]
    public void ValidateTransition_BlockedToCurrent_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Blocked, Models.TaskStatus.Current);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_BlockedToNotImplemented_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Blocked, Models.TaskStatus.NotImplemented);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_BlockedToArchived_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Blocked, Models.TaskStatus.Archived);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_BlockedToBlocked_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Blocked, Models.TaskStatus.Blocked);
        Assert.True(result);
    }

    #endregion

    #region 未实现(NotImplemented)的状态转换

    [Fact]
    public void ValidateTransition_NotImplementedToCurrent_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.NotImplemented, Models.TaskStatus.Current);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_NotImplementedToBlocked_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.NotImplemented, Models.TaskStatus.Blocked);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_NotImplementedToArchived_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.NotImplemented, Models.TaskStatus.Archived);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_NotImplementedToNotImplemented_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.NotImplemented, Models.TaskStatus.NotImplemented);
        Assert.True(result);
    }

    #endregion

    #region 归档(Archived)的状态转换

    [Fact]
    public void ValidateTransition_ArchivedToCurrent_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Archived, Models.TaskStatus.Current);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_ArchivedToNotImplemented_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Archived, Models.TaskStatus.NotImplemented);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_ArchivedToBlocked_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Archived, Models.TaskStatus.Blocked);
        Assert.True(result);
    }

    [Fact]
    public void ValidateTransition_ArchivedToArchived_IsValid()
    {
        bool result = _transitionService.CanTransition(Models.TaskStatus.Archived, Models.TaskStatus.Archived);
        Assert.True(result);
    }

    #endregion

    #region 获取状态流转描述

    [Fact]
    public void GetTransitionDescription_CurrentToBlocked_ReturnsDescription()
    {
        string description = _transitionService.GetTransitionDescription(Models.TaskStatus.Current, Models.TaskStatus.Blocked);
        Assert.Equal("任务因外部依赖或问题被阻塞", description);
    }

    [Fact]
    public void GetTransitionDescription_CurrentToArchived_ReturnsDescription()
    {
        string description = _transitionService.GetTransitionDescription(Models.TaskStatus.Current, Models.TaskStatus.Archived);
        Assert.Equal("任务完成并归档，将自动同步到 Azure DevOps", description);
    }

    [Fact]
    public void GetTransitionDescription_NotImplementedToCurrent_ReturnsDescription()
    {
        string description = _transitionService.GetTransitionDescription(Models.TaskStatus.NotImplemented, Models.TaskStatus.Current);
        Assert.Equal("开始任务开发", description);
    }

    #endregion

    #region 获取允许的目标状态

    [Fact]
    public void GetAllowedTransitions_Current_ReturnsCorrectStates()
    {
        var allowed = _transitionService.GetAllowedTransitions(Models.TaskStatus.Current);
        Assert.Contains(Models.TaskStatus.Blocked, allowed);
        Assert.Contains(Models.TaskStatus.NotImplemented, allowed);
        Assert.Contains(Models.TaskStatus.Archived, allowed);
        Assert.Contains(Models.TaskStatus.Current, allowed);
    }

    [Fact]
    public void GetAllowedTransitions_Archived_ReturnsCorrectStates()
    {
        var allowed = _transitionService.GetAllowedTransitions(Models.TaskStatus.Archived);
        Assert.Contains(Models.TaskStatus.Current, allowed);
        Assert.Contains(Models.TaskStatus.NotImplemented, allowed);
        Assert.Contains(Models.TaskStatus.Blocked, allowed);
        Assert.Contains(Models.TaskStatus.Archived, allowed);
    }

    #endregion
}