//using FluentValidation;

//namespace fh_service_directory_api.core.Commands.Create;


//public class CreateOpenReferralOrganisationValidator : AbstractValidator<Create>
//{
//    public CreateOpenReferralOrganisationValidator()
//    {
//        RuleFor(v => v.OpenReferralOrganisation.Name)
//            .MinimumLength(1)
//            .MaximumLength(50)
//            .NotNull()
//            .NotEmpty();

//        RuleFor(v => v.OpenReferralOrganisation.Description)
//            .MaximumLength(500);
//    }
//}
