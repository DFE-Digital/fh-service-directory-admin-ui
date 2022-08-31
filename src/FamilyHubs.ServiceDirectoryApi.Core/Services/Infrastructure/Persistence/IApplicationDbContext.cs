//using Microsoft.EntityFrameworkCore;

//namespace fh_service_directory_api.core.Services.Infrastructure.Persistence;

//public interface IApplicationDbContext
//{
//    DbSet<OpenReferralOrganisation> OpenReferralOrganisations { get; }

//    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
//}

//DbSet<Tenant> Tenants { get; }
//DbSet<Address> Addresses { get; }
//DbSet<Classification> Classifications { get; }
//DbSet<Contact> Contacts { get; }
//DbSet<Location> Locations { get; }
//DbSet<OpenReferralOrganisation> OpenReferralOrganisations { get; }
//DbSet<OpenReferralOrganisationType> OpenReferralOrganisationTypes { get; }
//DbSet<Service> Services { get; }
//Task<int> SaveChangesAsync(CancellationToken cancellationToken);

//#region Open Referral Entities
//public DbSet<Accessibility_For_Disabilities> Accessibility_For_Disabilities { get; }
//public DbSet<OpenReferralContact> OpenReferralContacts { get; }
//public DbSet<OpenReferralCost_Option> OpenReferralCost_Options { get; }
//public DbSet<OpenReferralEligibility> OpenReferralEligibilities { get; }
//public DbSet<OpenReferralFunding> OpenReferralFundings { get; }
//public DbSet<OpenReferralHoliday_Schedule> OpenReferralHoliday_Schedules { get; }
//public DbSet<OpenReferralLanguage> OpenReferralLanguages { get; }
//public DbSet<OpenReferralLinktaxonomycollection> OpenReferralLinktaxonomycollections { get; }
//public DbSet<OpenReferralLocation> OpenReferralLocations { get; }
//public DbSet<OpenReferralOpenReferralOrganisation> OpenReferralOpenReferralOrganisations { get; }
//public DbSet<OpenReferralParent> OpenReferralParents { get; }
//public DbSet<OpenReferralPhone> OpenReferralPhones { get; }
//public DbSet<OpenReferralPhysical_Address> OpenReferralPhysical_Addresses { get; }
//public DbSet<OpenReferralRegular_Schedule> OpenReferralRegular_Schedules { get; }
//public DbSet<OpenReferralReview> OpenReferralReviews { get; }
//public DbSet<OpenReferralService> OpenReferralServices { get; }
//public DbSet<OpenReferralService_Area> OpenReferralService_Areas { get; }
//public DbSet<OpenReferralService_Taxonomy> OpenReferralService_Taxonomies { get; }
//public DbSet<OpenReferralServiceAtLocation> OpenReferralServiceAtLocations { get; }
//public DbSet<OpenReferralTaxonomy> OpenReferralTaxonomies { get; }