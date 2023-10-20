using System.Net;

namespace FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;

// ignore Sonar's "Update this implementation of 'ISerializable' to confirm to the recommended serialization pattern" (https://rules.sonarsource.com/csharp/RSPEC-3925)
// .Net Core itself doesn't implement serialization on most exceptions, see https://github.com/dotnet/runtime/issues/21433#issue-225189643
#pragma warning disable S3925
public class IdamsClientServiceException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? ReasonPhrase { get; }
    public Uri? RequestUri { get; }
    public string? ErrorResponse { get; }

    public IdamsClientServiceException(HttpResponseMessage httpResponseMessage, string errorResponse)
        : base(GenerateMessage(httpResponseMessage, errorResponse))
    {
        StatusCode = httpResponseMessage.StatusCode;
        ReasonPhrase = httpResponseMessage.ReasonPhrase;
        //todo: when is RequestMessage null?
        RequestUri = httpResponseMessage.RequestMessage!.RequestUri;
        ErrorResponse = errorResponse;
    }

    private static string GenerateMessage(HttpResponseMessage httpResponseMessage, string errorResponse)
    {
        //todo: when is RequestMessage null?
        return $@"Request '{httpResponseMessage.RequestMessage?.RequestUri}'
                    returned {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}
                    Response: {errorResponse}";
    }
}