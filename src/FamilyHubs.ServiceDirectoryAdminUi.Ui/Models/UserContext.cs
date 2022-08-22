using System.Security.Principal;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

public class UserContext : IUserContext
{
    public string HashedAccountId { get; set; } = default!;
    public IPrincipal User { get; set; } = default!;
}
