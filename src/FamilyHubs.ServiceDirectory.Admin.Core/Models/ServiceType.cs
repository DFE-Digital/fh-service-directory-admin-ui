
namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;

//todo: when adding, this will come as a query param
// when editing, we'll set it in start-edit-service
// we call it ServiceTypeArg because ServiceType is already taken
//todo: tidy up OrganisationType/ServiceType/ServiceTypeArg
public enum ServiceTypeArg
{
    La,
    Vcs
}