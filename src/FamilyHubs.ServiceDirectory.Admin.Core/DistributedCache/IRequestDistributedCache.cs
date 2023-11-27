
namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public interface IRequestDistributedCache
{
    Task<T?> GetAsync<T>(string emailAddress);
    Task SetAsync<T>(string emailAddress, T model);
}