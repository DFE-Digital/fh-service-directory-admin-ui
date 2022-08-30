//using fh_service_directory_api.core.Interfaces.Entities;
//using MediatR;

//namespace fh_service_directory_api.core.Commands.Create;

//public class CreateOpenReferralOrganisationCommand : IRequest<string>, ICreateOpenReferralOrganisationCommand
//{
//    public IOpenReferralOrganisation OpenReferralOrganisation { get; init; }

//    public CreateOpenReferralOrganisationCommand(IOpenReferralOrganisation OpenReferralOrganisation)
//    {
//        OpenReferralOrganisation = OpenReferralOrganisation;
//    }

//}

//public class CreateOpenReferralOrganisationCommandHandler : IRequestHandler<CreateOpenReferralOrganisationCommand, string>
//{
//    private readonly DBContext _context;

//    public CreateOrOpenReferralOrganisationCommandHandler(ILAHubDbContext context)
//    {
//        _context = context;
//    }
//    public async Task<string> Handle(Create request, CancellationToken cancellationToken)
//    {
//        var entity = request.OrOpenReferralOrganisation;

//        try
//        {
//            entity.AddDomainEvent(new OpenReferralOrganisationCreatedEvent(entity));

//            _context.OpenReferralOrOpenReferralOrganisations.Add(entity);

//            await _context.SaveChangesAsync(cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            throw new Exception(ex.Message, ex);
//        }

//        if (entity is not null)
//            return entity.Id;
//        else
//            return string.Empty;
//    }
//}
