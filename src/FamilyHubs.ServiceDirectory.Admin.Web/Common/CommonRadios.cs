using FamilyHubs.SharedKernel.Razor.FullPages.Radios;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Common;

//todo: move to components?
public static class CommonRadios
{
    public static Radio[] YesNo => new[]
    {
        new Radio("Yes", true.ToString()),
        new Radio("No", false.ToString())
    };
}