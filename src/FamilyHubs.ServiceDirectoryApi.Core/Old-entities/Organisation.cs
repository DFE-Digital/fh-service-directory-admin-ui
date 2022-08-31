
//using FamilyHubs.SharedKernel;
//using FamilyHubs.SharedKernel.Interfaces;
//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

///// <summary>
///// From the Open Referral code.
///// The organization record is used to provide basic description and details about each organization delivering or reviewing services.
///// Each service should be linked to the organization responsible for its delivery.
///// One organization may deliver many services.
/////
///// Note that the LGA extension table 'link_taxonomy' enables many taxonomy terms to associated with an organization.
///// These can define the type of organization e.g.
/////     "charity"
/////     "voluntary group"
/////     "local authority"
///// </summary>
//public class OpenReferralOrganisation
//    : EntityBase<string>,
//    IOpenReferralOrganisation,
//    IAggregateRoot
//{
//    #region Properties
//    public string Name { get; private set; } = string.Empty;

//    public string Description { get; private set; } = string.Empty;

//    public string? Logo { get; private set; } = string.Empty;

//    public string? Uri { get; private set; } = string.Empty;

//    public string? Url { get; private set; } = string.Empty;

//    private readonly IList<IReview> _reviews = new List<IReview>();

//    public IEnumerable<IReview> Reviews => _reviews;

//    private readonly IList<IService> _services = new List<IService>();

//    public IEnumerable<IService> Services => _services;
//    #endregion Properties

//    #region Constructors
//    private OpenReferralOrganisation() { }                                          // EF use only, EF needs a parameterless constructor

//    public OpenReferralOrganisation
//    (
//        string name,                                                    // The official or public name of the organization.
//        string description,                                             // A brief summary about the organization. It can contain markup such as HTML or Markdown.
//        string? logo = default,                                         // A URL to an image associated with the organization which can be presented alongside its name.
//        string? uri = default,                                          // Added by Open Referral UK - TODO find out what it's use is.
//        string? url = default,                                          // The URL (website address) of the organization.
//        IEnumerable<IReview>? reviews = default,                        // The review table contains service reviews made by organizations. This is an LGA Extension table. This table provides a structured version of the text information contained in the 'accreditations' field of the 'service' table.
//        IEnumerable<IService>? services = default                       // Services are provided by organizations to a range of different groups. Details on where each service is delivered are contained in the services_at_location table.
//    )
//    {
//        Name = name;
//        Description = description;
//        Logo = logo ?? string.Empty;
//        Uri = uri ?? string.Empty;
//        Url = url ?? string.Empty;
//        _reviews = (IList<IReview>)(reviews ?? new List<IReview>());
//        _services = (IList<IService>)(services ?? new List<IService>());
//    }
//    #endregion Constructors

//    #region Public Methods
//    public void UpdateOpenReferralOrganisation
//    (
//        string name,
//        string description,
//        string? logo = default,
//        string? uri = default,
//        string? url = default
//    )
//    {
//        Name = name;
//        Description = description;
//        Logo = logo ?? string.Empty;
//        Uri = uri ?? string.Empty;
//        Url = url ?? string.Empty;
//    }

//    public IService AddNewService(IService service)
//    {
//        ArgumentNullException.ThrowIfNull(service, nameof(service));

//        _services.Add(service);
//        var newServiceAddedEvent = new NewServiceAddedEvent(this, service);
//        base.RegisterDomainEvent(newServiceAddedEvent);

//        return service;
//    }
//    #endregion Public Methods
//}

