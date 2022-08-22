namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;

public static class RequestExtensions
{
    public static string GetRequestUrlRoot(this HttpRequest request)
    {
        var url = $"{request.Scheme}://{request.Host}";
        return url;
    }
}
