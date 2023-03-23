using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.DataUpload.Models
{
    public class ServiceForUpload
    {
        public bool IsValid { get; set; } = true;
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public ServiceDto? Service { get; set; }
        public List<int> RelatedRows { get; set; } = new List<int>();
        public string ServiceUniqueIdentifier { get; set; } = default!;
    }
}
