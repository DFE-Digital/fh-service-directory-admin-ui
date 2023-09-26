using FamilyHubs.ServiceDirectory.Admin.Core.Models;

namespace FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;

public interface IRequestDistributedCache
{
    Task<OrganisationViewModel?> GetAsync(string emailAddress);
    Task SetAsync(string emailAddress, OrganisationViewModel model);
    Task RemoveAsync(string emailAddress);

    Task<SubjectAccessRequestViewModel?> GetSarAsync(string emailAddress);
    Task SetSarAsync(string emailAddress, SubjectAccessRequestViewModel model);

    Task<string?> GetCurrentPageAsync(string emailAddress);
    Task<string?> GetLastPageAsync(string emailAddress);
    Task SetPageAsync(string emailAddress, string page);
}