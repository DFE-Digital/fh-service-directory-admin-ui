using FamilyHubs.ServiceDirectory.Admin.Core.Models;

namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public interface IRequestDistributedCache
{
    Task<OrganisationViewModel?> GetAsync(string professionalsEmail);
    Task SetAsync(string professionalsEmail, OrganisationViewModel model);
    Task RemoveAsync(string professionalsEmail);
}