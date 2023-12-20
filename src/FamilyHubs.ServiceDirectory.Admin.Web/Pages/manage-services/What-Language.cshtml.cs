using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Factories;
using FamilyHubs.SharedKernel.Razor.AddAnother;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: sometimes when add another and js enabled, the new text box contains the value of the previous one
//todo: when add languages, then remove languages, then add again, the indexes aren't ordinal, so subsequent removes don't work properly
//todo: update connect to use the code to search
//todo: update Connect, so that the language names match

public class WhatLanguageViewModel
{
    public IEnumerable<string> Languages { get; set; } = Enumerable.Empty<string>();
    public bool TranslationServices { get; set; }
    public bool BritishSignLanguage { get; set; }
    public AddAnotherAutocompleteErrorChecker? ErrorIndexes { get; set; }
}

public class What_LanguageModel : ServicePageModel<WhatLanguageViewModel>
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public const string AllLanguagesValue = "";
    public const string InvalidNameValue = "--";

    public static SelectListItem[] StaticLanguageOptions { get; set; }

    static What_LanguageModel()
    {
        // list taken from https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes#References

        StaticLanguageOptions = LanguageDtoFactory.CodeToName
            .OrderBy(kv => kv.Value)
            .Select(kv => new SelectListItem(kv.Value, kv.Key))
            .Prepend(new SelectListItem("All languages", AllLanguagesValue, true, true))
            .ToArray();
    }

    public IEnumerable<SelectListItem> LanguageOptions => StaticLanguageOptions;
    public IEnumerable<SelectListItem> UserLanguageOptions { get; set; } = Enumerable.Empty<SelectListItem>();

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
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: move error handling to method
        // base could call GetHandleErrors if HasErrors is true

        if (ServiceModel?.UserInput != null)
        {
            // we have redirected to self with user input, so either the browser has javascript disabled, or there are errors

            UserLanguageOptions = ServiceModel.UserInput.Languages
                .Select(name =>
                {
                    if (name == "")
                    {
                        //todo: no magic string (or just leave as blank?)
                        return new SelectListItem("All languages", AllLanguagesValue);
                    }

                    bool nameFound = LanguageDtoFactory.NameToCode.TryGetValue(name, out string? code);
                    return new SelectListItem(name, nameFound ? code : InvalidNameValue);
                });

            TranslationServices = ServiceModel.UserInput.TranslationServices;
            BritishSignLanguage = ServiceModel.UserInput.BritishSignLanguage;

            if (Errors.HasErrors)
            {
                if (ServiceModel?.UserInput?.ErrorIndexes == null)
                {
                    throw new InvalidOperationException("ServiceModel?.UserInput?.ErrorIndexes is null");
                }

                ErrorToSelectIndex = new Dictionary<int, int>();

                if (Errors.HasTriggeredError((int)ErrorId.What_Language__EnterLanguages))
                {
                    ErrorToSelectIndex.Add((int)ErrorId.What_Language__EnterLanguages,
                        ServiceModel.UserInput.ErrorIndexes.FirstEmptyIndex!.Value);
                }

                if (Errors.HasTriggeredError((int)ErrorId.What_Language__EnterSupportedLanguage))
                {
                    ErrorToSelectIndex.Add((int)ErrorId.What_Language__EnterSupportedLanguage,
                        ServiceModel.UserInput.ErrorIndexes.FirstInvalidNameIndex!.Value);
                }

                if (Errors.HasTriggeredError((int)ErrorId.What_Language__SelectLanguageOnce))
                {
                    ErrorToSelectIndex.Add((int)ErrorId.What_Language__SelectLanguageOnce,
                        ServiceModel.UserInput.ErrorIndexes.FirstDuplicateLanguageIndex!.Value);
                }
            }
            return;
        }

        // default to 'All' languages
        UserLanguageOptions = StaticLanguageOptions.Take(1);

        switch (Flow)
        {
            case JourneyFlow.Edit:
                //todo: if edit flow, get service in base
                var service = await _serviceDirectoryClient.GetServiceById(ServiceId!.Value, cancellationToken);
                if (service.Languages.Any())
                {
                    UserLanguageOptions = service.Languages
                        .Select(l =>
                        {
                            bool codeFound = LanguageDtoFactory.CodeToName.TryGetValue(l.Code, out string? name);
                            return new SelectListItem(name, codeFound ? l.Code : InvalidNameValue);
                        });
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
                if (ServiceModel!.LanguageCodes != null)
                {
                    UserLanguageOptions = ServiceModel!.LanguageCodes.Select(l =>
                    {
                        //todo: put into method
                        bool codeFound = LanguageDtoFactory.CodeToName.TryGetValue(l, out string? name);
                        return new SelectListItem(name, codeFound ? l : InvalidNameValue);
                    });
                }
                TranslationServices = ServiceModel.TranslationServices ?? false;
                BritishSignLanguage = ServiceModel.BritishSignLanguage ?? false;
                break;
        }

        UserLanguageOptions = UserLanguageOptions.OrderBy(sli => sli.Text);
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: do we want to split the calls in base to have OnPostErrorChecksAsync and OnPostUpdateAsync? (or something)

        IEnumerable<string> languageCodes = Request.Form["language"];
        var viewModel = new WhatLanguageViewModel
        {
            Languages = Request.Form["languageName"],
            TranslationServices = TranslationServices,
            BritishSignLanguage = BritishSignLanguage
        };

        // handle add/remove buttons first. if there are any validation errors, we'll ignore then until they click continue
        string? button = Request.Form["button"].FirstOrDefault();

        if (button != null)
        {
            // to get here, the user must have javascript disabled
            // the form contains the select values in "language" and there are no "languageName" values as the inputs weren't created

            if (button is "add")
            {
                // if javascript is disabled, we *could* keep a count the number of empty language inputs and have csv empty values in the hidden field
                // but it'd be quite a bit of effort for an unlikely scenario
                //todo: so document that when javascript is disabled, the user needs to add a language first before they can add another

                languageCodes = languageCodes.Append("");
            }
            else if (button.StartsWith("remove"))
            {
                int indexToRemove = int.Parse(button.Substring("remove-".Length));
                languageCodes = languageCodes.Where((_, i) => i != indexToRemove);

                if (!languageCodes.Any())
                {
                    languageCodes = languageCodes.Append("");
                }
            }

            viewModel.Languages = languageCodes
                .Select(c => c == "" ? "" : LanguageDtoFactory.CodeToName[c]);

            return RedirectToSelf(viewModel);
        }

        viewModel.ErrorIndexes = AddAnotherAutocompleteErrorChecker.Create(
            Request.Form, "language", "languageName", StaticLanguageOptions.Skip(1));

        var errorIds = new List<ErrorId>();
        if (viewModel.ErrorIndexes.FirstEmptyIndex != null)
        {
            errorIds.Add(ErrorId.What_Language__EnterLanguages);
        }
        if (viewModel.ErrorIndexes.FirstInvalidNameIndex != null)
        {
            errorIds.Add(ErrorId.What_Language__EnterSupportedLanguage);
        }
        if (viewModel.ErrorIndexes.FirstDuplicateLanguageIndex != null)
        {
            errorIds.Add(ErrorId.What_Language__SelectLanguageOnce);
        }

        if (errorIds.Count > 0)
        {
            return RedirectToSelf(viewModel, errorIds.ToArray());
        }

        switch (Flow)
        {
            case JourneyFlow.Edit:
                await UpdateLanguages(viewModel, languageCodes, cancellationToken);
                break;

            default:
                ServiceModel!.LanguageCodes = languageCodes;
                ServiceModel.TranslationServices = TranslationServices;
                ServiceModel.BritishSignLanguage = BritishSignLanguage;
                break;
        }

        return NextPage();
    }

    //todo: Update called when in edit mode and no errors? could call get and update in base?
    private async Task UpdateLanguages(
        WhatLanguageViewModel viewModel,
        IEnumerable<string> languageCodes,
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
        //todo: order by name here?
        service.Languages = languageCodes.Select(LanguageDtoFactory.Create).ToList();

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}