using FamilyHubs.SharedKernel;
using FamilyHubs.SharedKernel.Interfaces;
using fh_service_directory_api.core.Interfaces.Entities;

namespace fh_service_directory_api.core.Entities;

public class Accessibility_For_Disabilities : EntityBase<string>, IAccessibility_For_Disabilities, IAggregateRoot
{
    private Accessibility_For_Disabilities() { }
    public Accessibility_For_Disabilities(string id, string accessibility)
    {
        Id = id;
        Accessibility = accessibility;
    }
    public string Accessibility { get; init; } = default!;

}
