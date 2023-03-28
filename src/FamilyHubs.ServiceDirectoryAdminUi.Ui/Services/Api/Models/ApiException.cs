namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api.Models
{
    public class ApiException : Exception
    {
        public ApiErrorResponse ApiErrorResponse { get; private set; }

        public ApiException(ApiErrorResponse apiErrorResponse) : base(apiErrorResponse.Title)
        {
            ApiErrorResponse = apiErrorResponse;
        }

    }
}
