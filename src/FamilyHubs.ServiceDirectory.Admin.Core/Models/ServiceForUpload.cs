using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

public class ServiceForUpload
{
    public ServiceDto? Service { get; set; }
    public List<int> RelatedRows { get; init; } = new();
    public string ServiceUniqueIdentifier { get; init; } = default!;
    public bool IsNewService { get; set; }
}