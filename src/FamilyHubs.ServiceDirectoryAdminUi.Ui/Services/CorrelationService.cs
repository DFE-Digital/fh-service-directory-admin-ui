namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Services
{
    public interface ICorrelationService
    {
        public string CorrelationId { get; }
    }

    public class CorrelationService : ICorrelationService
    {
        public CorrelationService()
        {
            CorrelationId = Guid.NewGuid().ToString();
        }

        public string CorrelationId { get; private set; }
    }
}
