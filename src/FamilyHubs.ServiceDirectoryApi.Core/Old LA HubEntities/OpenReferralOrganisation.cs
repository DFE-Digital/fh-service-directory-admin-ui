//using FamilyHubs.SharedKernel;
//using FamilyHubs.SharedKernel.Interfaces;
//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

//public class OpenReferralOrganisation : EntityBase<string>, IOpenReferralOrganisation, IAggregateRoot
//{
//    private OpenReferralOrganisation() { }

//    public OpenReferralOrganisation(
//        string id,
//        string name,
//        string? description,
//        string? logo,
//        string? uri,
//        string? url,
//        ICollection<OpenReferralReview>? reviews,
//        ICollection<OpenReferralService>? services
//    // TODO: Lock down the access to the collections
//    //IEnumerable<IOpenReferralReview>? reviews = default,
//    //IEnumerable<IOpenReferralService>? services = default
//    )
//    {
//        Id = id;
//        Name = name;
//        Description = description ?? string.Empty;
//        Logo = logo ?? string.Empty;
//        Uri = uri ?? string.Empty;
//        Url = url ?? string.Empty;
//        Reviews = reviews;
//        Services = services;
//        //_reviews = (IList<IOpenReferralReview>)(reviews ?? new List<IOpenReferralReview>());
//        //_services = (IList<IOpenReferralService>)(services ?? new List<IOpenReferralService>());
//    }

//    public string Name { get; private set; }
//    public string? Description { get; private set; }
//    public string? Logo { get; private set; }
//    public string? Uri { get; private set; }
//    public string? Url { get; private set; }
//    public virtual ICollection<OpenReferralReview>? Reviews { get; set; } = default!;
//    public virtual ICollection<OpenReferralService>? Services { get; set; } = default!;
//    // public IEnumerable<IOpenReferralReview> Reviews => _reviews;
//    // public IEnumerable<IOpenReferralService> Services => _services;

//    public void Update(IOpenReferralOrganisation openReferralOpenReferralOrganisation)
//    {
//        Name = openReferralOpenReferralOrganisation.Name;
//        Description = openReferralOpenReferralOrganisation.Description;
//        Logo = openReferralOpenReferralOrganisation.Logo;
//        Uri = openReferralOpenReferralOrganisation.Uri;
//        Url = openReferralOpenReferralOrganisation.Url;
//    }

//    private readonly IList<IOpenReferralReview> _reviews = new List<IOpenReferralReview>();

//    private readonly IList<IOpenReferralService> _services = new List<IOpenReferralService>();

//}

