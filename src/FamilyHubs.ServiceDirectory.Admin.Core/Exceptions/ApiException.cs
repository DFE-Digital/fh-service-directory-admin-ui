namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;

public class ApiException : Exception
{
    public ApiErrorResponse ApiErrorResponse { get; }

    public ApiException(ApiErrorResponse apiErrorResponse) : base(apiErrorResponse.Title)
    {
        ApiErrorResponse = apiErrorResponse;
    }

}