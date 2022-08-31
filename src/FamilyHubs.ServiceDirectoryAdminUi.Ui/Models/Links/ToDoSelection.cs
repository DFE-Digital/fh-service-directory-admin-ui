using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Links;

public class ToDoSelection : Link
{
    public ToDoSelection(string href, string @class = "") : base(href, @class: @class)
    {
    }

    public override string Render()
    {
        return $"<a href = \"{Href}\" id=\"proposition-name\" class=\"{Class}\">To Do</a>";
    }
}
