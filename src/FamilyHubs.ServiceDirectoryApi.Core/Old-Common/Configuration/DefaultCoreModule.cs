using Autofac;
using fh_service_directory_api.core.Common.Interfaces.Services.Domain.Postcode;
using fh_service_directory_api.core.Services.Domain.Postcode;

namespace fh_service_directory_api.core.Common.Configuration;

public class DefaultCoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<IPostcodeLocationClientService>().As<PostcodeLocationClientService>().InstancePerLifetimeScope();
    }
}