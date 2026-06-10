using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存数据
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// 设置缓存数据
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// 获取或创建缓存数据
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// 移除缓存
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    void Clear();
}

/// <summary>
/// 缓存服务实现
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T? Get<T>(string key)
    {
        return _memoryCache.Get<T>(key);
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };

        _memoryCache.Set(key, value, options);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue!;
        }

        var value = await factory();
        Set(key, value, expiration);
        return value;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public bool Exists(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public void Clear()
    {
        // MemoryCache 不支持直接清空，需要通过创建新缓存或使用其他方式
        // 这里使用遍历键的方式移除所有缓存（需要实现一个键的追踪机制）
        // 简单实现：可以在服务中维护一个键的列表
    }
}

/// <summary>
/// 缓存键常量
/// </summary>
public static class CacheKeys
{
    public const string Projects = "projects:{0}";
    public const string Project = "project:{0}";
    public const string Repositories = "repos:{0}";
    public const string Repository = "repo:{0}:{1}";
    public const string Tasks = "tasks:{0}";
    public const string Task = "task:{0}";
    public const string UserMappings = "userMappings";
    public const string ProjectMappings = "projectMappings";

    public static string FormatProjectCacheKey(string? userId = null)
    {
        return string.Format(Projects, userId ?? "all");
    }

    public static string FormatTaskCacheKey(string userId)
    {
        return string.Format(Tasks, userId);
    }
}