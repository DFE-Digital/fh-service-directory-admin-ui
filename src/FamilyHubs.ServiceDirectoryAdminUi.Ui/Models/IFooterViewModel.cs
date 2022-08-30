using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

public interface IFooterViewModel : ILinkCollection, ILinkHelper
{
    bool UseLegacyStyles { get; }
}
