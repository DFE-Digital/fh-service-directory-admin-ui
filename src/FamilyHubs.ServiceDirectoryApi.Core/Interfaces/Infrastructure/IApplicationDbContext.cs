using fh_service_directory_api.core.Entities;
using Microsoft.EntityFrameworkCore;

namespace fh_service_directory_api.core.Interfaces.Infrastructure
{
    public interface IApplicationDbContext
    {
        DbSet<Accessibility_For_Disabilities> Accessibility_For_Disabilities { get; }
        DbSet<OpenReferralContact> OpenReferralContacts { get; }
        DbSet<OpenReferralCost_Option> OpenReferralCost_Options { get; }
        DbSet<OpenReferralEligibility> OpenReferralEligibilities { get; }
        DbSet<OpenReferralFunding> OpenReferralFundings { get; }
        DbSet<OpenReferralHoliday_Schedule> OpenReferralHoliday_Schedules { get; }
        DbSet<OpenReferralLanguage> OpenReferralLanguages { get; }
        DbSet<OpenReferralLinktaxonomycollection> OpenReferralLinktaxonomycollections { get; }
        DbSet<OpenReferralLocation> OpenReferralLocations { get; }
        DbSet<OpenReferralOrganisation> OpenReferralOrganisations { get; }
        DbSet<OpenReferralParent> OpenReferralParents { get; }
        DbSet<OpenReferralPhone> OpenReferralPhones { get; }
        DbSet<OpenReferralPhysical_Address> OpenReferralPhysical_Addresses { get; }
        DbSet<OpenReferralRegular_Schedule> OpenReferralRegular_Schedules { get; }
        DbSet<OpenReferralReview> OpenReferralReviews { get; }
        DbSet<OpenReferralService_Area> OpenReferralService_Areas { get; }
        DbSet<OpenReferralService_Taxonomy> OpenReferralService_Taxonomies { get; }
        DbSet<OpenReferralServiceAtLocation> OpenReferralServiceAtLocations { get; }
        DbSet<OpenReferralServiceDelivery> OpenReferralServiceDeliveries { get; }
        DbSet<OpenReferralService> OpenReferralServices { get; }
        DbSet<OpenReferralTaxonomy> OpenReferralTaxonomies { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}