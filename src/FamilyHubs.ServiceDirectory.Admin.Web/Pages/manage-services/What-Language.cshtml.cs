using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: store Code as well as language name? (The ISO 639-1 or ISO 639-3 code for the language.)
//todo: where did the list of languages come from? do we need to support all ISO 639-1/3 languages? probably just use 639-1
// prototype allows entering more languages that are in our list (e.g. in Find)
//todo: if we have free text languages:
// what happens when javascript is disabled? do they just have to spell it right
// what happens when they enter a language that isn't in the list (e.g. misspelling Chinese Mandarin, rather than Chinese (Mandarin) )
// do we accept all ISO languages, or just the ones in our list
//todo: we'll have to change the existing languages in the db to use the new format (i.e. names match and add codes)
//todo: add code to language (and use that in the Connect search)
//todo: update Connect, so that the language names match

public class WhatLanguageViewModel
{
    public IEnumerable<string> Languages { get; set; }
    public bool TranslationServices { get; set; }
    public bool BritishSignLanguage { get; set; }
}

public class What_LanguageModel : ServicePageModel<WhatLanguageViewModel>
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public const string AllLanguagesValue = "All";
    
    public static SelectListItem[] StaticLanguageOptions { get; set; } =
    {
        new() { Value = AllLanguagesValue, Text = "All languages", Selected = true, Disabled = true },
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

    [BindProperty]
    public bool TranslationServices { get; set; }
    [BindProperty]
    public bool BritishSignLanguage { get; set; }

    public Dictionary<int, int>? ErrorToSelectIndex { get; set; }
    
    public What_LanguageModel(
        IRequestDistributedCache connectionRequestCache,
        IServiceDirectoryClient serviceDirectoryClient)
        : base(ServiceJourneyPage.What_Language, connectionRequestCache)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        Languages = Enumerable.Empty<string>();
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: move error handling to method
        // base could call GetHandleErrors if HasErrors is true
        if (Errors.HasErrors)
        {
            //todo: have viewmodel as property and bind - will it ignore languages?
            var viewModel = ServiceModel?.UserInput ?? throw new InvalidOperationException("ServiceModel.UserInput is null");
            Languages = viewModel.Languages;
            TranslationServices = viewModel.TranslationServices;
            BritishSignLanguage = viewModel.BritishSignLanguage;

            ErrorToSelectIndex = new Dictionary<int, int>();

            if (Errors.HasTriggeredError((int)ErrorId.What_Language__SelectLanguageOnce))
            {
                int? duplicateLanguageIndex = viewModel.Languages.Select((language, index) => new { Language = language, Index = index })
                    .GroupBy(x => x.Language)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Skip(1).First().Index)
                    .FirstOrDefault();
                if (duplicateLanguageIndex != null)
                {
                    ErrorToSelectIndex.Add((int)ErrorId.What_Language__SelectLanguageOnce, duplicateLanguageIndex.Value);
                }
            }

            if (Errors.HasTriggeredError((int)ErrorId.What_Language__EnterLanguages))
            {
                int? firstEmptySelectIndex = viewModel.Languages
                    .Select((language, index) => new { Language = language, Index = index })
                    .FirstOrDefault(l => l.Language == AllLanguagesValue)?.Index ?? 0;

                if (firstEmptySelectIndex != null)
                {
                    ErrorToSelectIndex.Add((int)ErrorId.What_Language__EnterLanguages, firstEmptySelectIndex.Value);
                }
            }
            return;
        }

        // default to 'All' languages
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

                // how we store these flags will change soon (they'll be stored as attributes)
                service.InterpretationServices?.Split(',').ToList().ForEach(s =>
                {
                    switch (s)
                    {
                        case "translation":
                            TranslationServices = true;
                            break;
                        case "bsl":
                            BritishSignLanguage = true;
                            break;
                    }
                });
                break;

            default:
                if (ServiceModel!.Languages != null)
                {
                    Languages = ServiceModel!.Languages;
                }
                TranslationServices = ServiceModel.TranslationServices ?? false;
                BritishSignLanguage = ServiceModel.BritishSignLanguage ?? false;
                break;
        }
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(
        CancellationToken cancellationToken)
    {
        //todo: do we want to split the calls in base to have OnPostErrorChecksAsync and OnPostUpdateAsync? (or something)

        var languageValues = Request.Form["Language"];
        
        var viewModel = new WhatLanguageViewModel
        {
            Languages = languageValues,
            TranslationServices = TranslationServices,
            BritishSignLanguage = BritishSignLanguage
        };
        
        //todo: new selects aren't defaulted to 'All' languages (which is what we want), so this doesn't work
        if (languageValues.Count == 0 || languageValues.Any(l => l == AllLanguagesValue))
        {
            return RedirectToSelf(viewModel, ErrorId.What_Language__EnterLanguages);
        }

        if (languageValues.Count > languageValues.Distinct().Count())
        {
            return RedirectToSelf(viewModel, ErrorId.What_Language__SelectLanguageOnce);
        }

        viewModel.Languages = viewModel.Languages.OrderBy(l => l);
        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateLanguages(viewModel, cancellationToken);
                break;

            default:
                ServiceModel!.Languages = languageValues;
                ServiceModel.TranslationServices = TranslationServices;
                ServiceModel.BritishSignLanguage = BritishSignLanguage;
                break;
        }

        return NextPage();
    }

    //todo: Update called when in edit mode and no errors? could call get and update in base?
    private async Task UpdateLanguages(
        WhatLanguageViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);

        var interpretationServices = new List<string>();
        if (viewModel.TranslationServices)
        {
            interpretationServices.Add("translation");
        }
        if (viewModel.BritishSignLanguage)
        {
            interpretationServices.Add("bsl");
        }

        service.InterpretationServices = string.Join(',', interpretationServices);
        
        //todo: check for null language?
        // will this delete the existing languages?
        service.Languages = viewModel.Languages.Select(l => new LanguageDto { Name = l }).ToList();

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}