namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions
{
    public class ValidationError
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
