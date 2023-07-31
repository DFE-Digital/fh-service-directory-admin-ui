using FamilyHubs.ServiceDirectory.Admin.Core.Models;

namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public interface IConnectionRequestDistributedCache
{
    Task<ConnectionRequestModel?> GetAsync(string professionalsEmail);
    Task SetAsync(string professionalsEmail, ConnectionRequestModel model);
    Task RemoveAsync(string professionalsEmail);
}