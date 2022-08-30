using Autofac;
using fh_service_directory_api.core.Entities;
using fh_service_directory_api.core.Interfaces.Entities;

namespace fh_service_directory_api.core;

public class DefaultCoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //builder.RegisterType<EntityBase<string>>()
        //    .As<IEntityBase<string>>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralContact>()
            .As<IOpenReferralContact>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralCost_Option>()
            .As<IOpenReferralCost_Option>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralEligibility>()
            .As<IOpenReferralEligibility>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralFunding>()
            .As<IOpenReferralFunding>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralHoliday_Schedule>()
            .As<IOpenReferralHoliday_Schedule>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralLanguage>()
            .As<IOpenReferralLanguage>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralLinktaxonomycollection>()
            .As<IOpenReferralLinktaxonomycollection>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralLocation>()
            .As<IOpenReferralLocation>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralOrganisation>()
            .As<IOpenReferralOrganisation>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralParent>()
            .As<IOpenReferralParent>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralPhone>()
            .As<IOpenReferralPhone>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralPhysical_Address>()
            .As<IOpenReferralPhysical_Address>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralRegular_Schedule>()
            .As<IOpenReferralRegular_Schedule>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralReview>()
            .As<IOpenReferralReview>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralService>()
            .As<IOpenReferralService>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralService_Area>()
            .As<IOpenReferralService_Area>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralService_Taxonomy>()
            .As<IOpenReferralService_Taxonomy>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralServiceAtLocation>()
            .As<IOpenReferralServiceAtLocation>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralServiceDelivery>()
            .As<IOpenReferralServiceDelivery>().InstancePerLifetimeScope();
        builder.RegisterType<OpenReferralTaxonomy>()
            .As<IOpenReferralTaxonomy>().InstancePerLifetimeScope();
        builder.RegisterType<Accessibility_For_Disabilities>()
            .As<IAccessibility_For_Disabilities>().InstancePerLifetimeScope();


    }
}
