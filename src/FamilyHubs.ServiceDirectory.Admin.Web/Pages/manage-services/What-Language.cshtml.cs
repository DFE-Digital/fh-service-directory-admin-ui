using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public class What_LanguageModel : ServicePageModel
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public static SelectListItem[] StaticLanguageOptions { get; set; } =
    {
        new() { Value = "All", Text = "All languages", Selected = true, Disabled = true },
        new() { Value = "Afrikaans", Text = "Afrikaans" },
        new() { Value = "Albanian", Text = "Albanian" },
        new() { Value = "Arabic", Text = "Arabic" },
        new() { Value = "Armenian", Text = "Armenian" },
        new() { Value = "Basque", Text = "Basque" },
        new() { Value = "Bengali", Text = "Bengali" },
        new() { Value = "Bulgarian", Text = "Bulgarian" },
        new() { Value = "Catalan", Text = "Catalan" },
        new() { Value = "Cambodian", Text = "Cambodian" },
        new() { Value = "Chinese (Mandarin)", Text = "Chinese (Mandarin)" },
        new() { Value = "Croatian", Text = "Croatian" },
        new() { Value = "Czech", Text = "Czech" },
        new() { Value = "Danish", Text = "Danish" },
        new() { Value = "Dutch", Text = "Dutch" },
        new() { Value = "English", Text = "English"},
        new() { Value = "Estonian", Text = "Estonian" },
        new() { Value = "Fiji", Text = "Fiji" },
        new() { Value = "Finnish", Text = "Finnish" },
        new() { Value = "French", Text = "French" },
        new() { Value = "Georgian", Text = "Georgian" },
        new() { Value = "German", Text = "German" },
        new() { Value = "Greek", Text = "Greek" },
        new() { Value = "Gujarati", Text = "Gujarati" },
        new() { Value = "Hebrew", Text = "Hebrew" },
        new() { Value = "Hindi", Text = "Hindi" },
        new() { Value = "Hungarian", Text = "Hungarian" },
        new() { Value = "Icelandic", Text = "Icelandic" },
        new() { Value = "Indonesian", Text = "Indonesian" },
        new() { Value = "Irish", Text = "Irish" },
        new() { Value = "Italian", Text = "Italian" },
        new() { Value = "Japanese", Text = "Japanese" },
        new() { Value = "Javanese", Text = "Javanese" },
        new() { Value = "Korean", Text = "Korean" },
        new() { Value = "Latin", Text = "Latin" },
        new() { Value = "Latvian", Text = "Latvian" },
        new() { Value = "Lithuanian", Text = "Lithuanian" },
        new() { Value = "Macedonian", Text = "Macedonian" },
        new() { Value = "Malay", Text = "Malay" },
        new() { Value = "Malayalam", Text = "Malayalam" },
        new() { Value = "Maltese", Text = "Maltese" },
        new() { Value = "Maori", Text = "Maori" },
        new() { Value = "Marathi", Text = "Marathi" },
        new() { Value = "Mongolian", Text = "Mongolian" },
        new() { Value = "Nepali", Text = "Nepali" },
        new() { Value = "Norwegian", Text = "Norwegian" },
        new() { Value = "Persian", Text = "Persian" },
        new() { Value = "Polish", Text = "Polish" },
        new() { Value = "Portuguese", Text = "Portuguese" },
        new() { Value = "Punjabi", Text = "Punjabi" },
        new() { Value = "Quechua", Text = "Quechua" },
        new() { Value = "Romanian", Text = "Romanian" },
        new() { Value = "Russian", Text = "Russian" },
        new() { Value = "Samoan", Text = "Samoan" },
        new() { Value = "Serbian", Text = "Serbian" },
        new() { Value = "Slovak", Text = "Slovak" },
        new() { Value = "Slovenian", Text = "Slovenian" },
        new() { Value = "Somali", Text = "Somali" },
        new() { Value = "Spanish", Text = "Spanish" },
        new() { Value = "Swahili", Text = "Swahili" },
        new() { Value = "Swedish ", Text = "Swedish " },
        new() { Value = "Tamil", Text = "Tamil" },
        new() { Value = "Tatar", Text = "Tatar" },
        new() { Value = "Telugu", Text = "Telugu" },
        new() { Value = "Thai", Text = "Thai" },
        new() { Value = "Tibetan", Text = "Tibetan" },
        new() { Value = "Tonga", Text = "Tonga" },
        new() { Value = "Turkish", Text = "Turkish" },
        new() { Value = "Ukrainian", Text = "Ukrainian" },
        new() { Value = "Urdu", Text = "Urdu" },
        new() { Value = "Uzbek", Text = "Uzbek" },
        new() { Value = "Vietnamese", Text = "Vietnamese" },
        new() { Value = "Welsh", Text = "Welsh" },
        new() { Value = "Xhosa", Text = "Xhosa" },
    };

    public IEnumerable<SelectListItem> LanguageOptions => StaticLanguageOptions;

    public IEnumerable<string> Languages { get; set; }

    public What_LanguageModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.What_Language, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        if (Errors.HasErrors)
        {
            return;
        }

        Languages = StaticLanguageOptions.Take(1).Select(o => o.Value);

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: if edit flow, get service in base
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
                if (service.Languages.Any())
                {
                    Languages = service.Languages.Select(l => l.Name);
                }
                break;

            default:
                break;
        }
    }

    protected override Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: error checks
        //todo: do we want to split the calls in base to have OnPostErrorChecksAsync and OnPostUpdateAsync? (or something)

        var languageValues = Request.Form["Language"];
        foreach (string? lang in languageValues)
        {
            
        }

        //if (Language == null)
        //{
        //    throw new InvalidOperationException("Language is null");
        //}

        //string[] selectedLanguages = Language.Split(',');

        switch (Flow)
        {
            case JourneyFlow.Edit:
                break;

            default:
                break;
        }

        return Task.FromResult(NextPage());
    }
}