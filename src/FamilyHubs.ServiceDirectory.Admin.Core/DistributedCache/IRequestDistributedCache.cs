﻿
namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public interface IRequestDistributedCache
{
    Task<T?> GetAsync<T>(string emailAddress);
    Task<T> SetAsync<T>(string emailAddress, T model);
    Task RemoveAsync<T>(string emailAddress);
}