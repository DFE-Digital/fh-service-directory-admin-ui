//using fh_service_directory_api.core.Interfaces.Entities;
//using LocalAuthorityInformationServices.SharedKernel;

//namespace fh_service_directory_api.core.Events;

//public class OldNewServiceAddedEvent : DomainEventBase
//{
//    public OldNewServiceAddedEvent(IOpenReferralOrganisation OpenReferralOrganisation, IService newServiceAdded)
//    {
//        OpenReferralOrganisation = OpenReferralOrganisation;
//        NewServiceAdded = newServiceAdded;
//    }

//    public Guid Id { get; private set; } = Guid.NewGuid();

//    public IService NewServiceAdded { get; private set; }

//    public IOpenReferralOrganisation OpenReferralOrganisation { get; private set; }
//}