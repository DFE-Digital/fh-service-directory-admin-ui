using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Error;

public class TestModel : HeaderPageModel
{
    public void OnGet()
    {
#pragma warning disable S112
        throw new Exception("Fault injected exception");
#pragma warning restore S112
    }
}