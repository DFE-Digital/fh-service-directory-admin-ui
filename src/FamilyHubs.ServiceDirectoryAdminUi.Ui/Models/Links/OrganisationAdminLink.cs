namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Links;

public class OrganisationAdminLink : Link
{
    public OrganisationAdminLink(string href, string @class = "") : base(href, @class: @class)
    {
    }

    public override string Render()
    {
        return $"<a href = \"{Href}\" id=\"oragisation-admin-link\" class=\"{Class}\">Organisation / Service Admin</a>";
    }
}
