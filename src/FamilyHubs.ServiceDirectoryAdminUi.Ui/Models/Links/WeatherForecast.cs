using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models.Links;

public class WeatherForecast : Link
{
    public WeatherForecast(string href, string @class = "") : base(href, @class: @class)
    {
    }

    public override string Render()
    {
        return $"<a href = \"{Href}\" id=\"proposition-name\" class=\"{Class}\">Weather Forcast</a>";
    }
}
