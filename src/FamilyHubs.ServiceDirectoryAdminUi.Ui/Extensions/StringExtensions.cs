namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
    public static class StringExtensions
    {
    public static string[] SplitByLineBreaks(this string value)
        => value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
}

