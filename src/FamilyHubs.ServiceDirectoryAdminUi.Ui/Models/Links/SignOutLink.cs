namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Links;

public class SignOutLink : Link
{
    private readonly string _userName;
    public SignOutLink(string userName, string href, string @class = "") : base(href, @class: @class)
    {
        _userName = userName;
    }

    public override string Render()
    {
        return $"<a href = \"{Href}\" id=\"sign-out-link\" class=\"{Class}\">Hello {_userName} Sign Out</a>";
    }
}
