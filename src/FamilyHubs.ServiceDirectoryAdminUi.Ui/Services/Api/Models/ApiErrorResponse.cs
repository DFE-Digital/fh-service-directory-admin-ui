namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api.Models
{
    public class ApiErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public List<ValidationErrors> Errors { get; set; } = new List<ValidationErrors>();
    }

    public class ValidationErrors
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
