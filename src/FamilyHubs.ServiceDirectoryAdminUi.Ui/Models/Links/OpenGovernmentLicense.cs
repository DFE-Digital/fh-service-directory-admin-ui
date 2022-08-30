using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Links;

public class OpenGovernmentLicense : Link
{
    public OpenGovernmentLicense(string href, string @class = "") : base(href, @class: @class)
    {
    }

    public override string Render()
    {
        return $"<a href = \"{Href}\" rel=\"license\" class=\"{Class}\">Open Government Licence</a>";
    }
}
