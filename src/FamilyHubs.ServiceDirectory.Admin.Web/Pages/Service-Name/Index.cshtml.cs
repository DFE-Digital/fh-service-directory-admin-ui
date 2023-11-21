using System.Collections.Immutable;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Errors;
using FamilyHubs.SharedKernel.Razor.FullPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.Service_Name;

//todo: use AdminRole from updated shared kernel (need to update azure.identity first)
//todo: reinstate before finishing
//[Authorize(Roles = $"{RoleTypes.DfeAdmin},{RoleTypes.LaManager},{RoleTypes.LaDualRole},{RoleTypes.VcsManager},{RoleTypes.VcsDualRole}")]
public class IndexModel : PageModel, ISingleTextboxPageModel
{
    public string? HeadingText { get; set; }
    public string? HintText { get; set; }
    public string TextBoxLabel { get; set; } = "What is the service name?";
    public IErrorState Errors { get; set; } = ErrorState.Empty;

    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    [BindProperty]
    public string? TextBoxValue { get; set; }

    public IndexModel(IServiceDirectoryClient serviceDirectoryClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    public async Task OnGetAsync(long? serviceId)
    {
        //todo: this will work in 2 modes: as part of creating a service, where it'll have to support fetching a current service from the cache

        if (serviceId == null)
        {
            throw new ArgumentNullException(nameof(serviceId));
        }

        var service = await _serviceDirectoryClient.GetServiceById(serviceId.Value);

        TextBoxValue = service.Name;
    }

    public void OnPost()
    {
        if (string.IsNullOrWhiteSpace(TextBoxValue))
        {
            Errors = ErrorState.Create(PossibleErrors, new[] { ErrorId.AnswerMissing });
        }
    }

    public enum ErrorId
    {
        AnswerMissing,
        AnswerTooLong
    }

    public static readonly ImmutableDictionary<int, SharedKernel.Razor.Errors.Error> PossibleErrors =
        ImmutableDictionary.Create<int, SharedKernel.Razor.Errors.Error>()
            .Add(ErrorId.AnswerMissing, ISingleTextboxPageModel.TextBoxId, "Guru meditation required")
            .Add(ErrorId.AnswerTooLong, ISingleTextboxPageModel.TextBoxId, "The answer is too long");
}