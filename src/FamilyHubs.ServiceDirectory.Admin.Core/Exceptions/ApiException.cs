using FamilyHubs.SharedKernel.Exceptions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;

public class ApiException : Exception
{
    public ApiExceptionResponse<ValidationError> ApiErrorResponse { get; }

    public ApiException(ApiExceptionResponse<ValidationError> apiErrorResponse) : base(apiErrorResponse.Title)
    {
        ApiErrorResponse = apiErrorResponse;
    }

}