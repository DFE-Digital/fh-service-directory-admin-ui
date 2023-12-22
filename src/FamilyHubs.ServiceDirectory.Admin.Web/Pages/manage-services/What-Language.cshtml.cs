using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Factories;
using FamilyHubs.ServiceDirectory.Shared.ReferenceData;
using FamilyHubs.SharedKernel.Razor.AddAnother;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

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

    public const string NoLanguageValue = "";
    public const string NoLanguageText = "";
    public const string InvalidNameValue = "--";

    public static SelectListItem[] LanguageOptions { get; set; }

    static What_LanguageModel()
    {
        LanguageOptions = Languages.CodeToName
            .OrderBy(kv => kv.Value)
            .Select(kv => new SelectListItem(kv.Value, kv.Key))
            .Prepend(new SelectListItem(NoLanguageText, NoLanguageValue, true, true))
            .ToArray();
    }

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
                    if (name == NoLanguageText)
                    {
                        return new SelectListItem(NoLanguageText, NoLanguageValue);
                    }

                    bool nameFound = Languages.NameToCode.TryGetValue(name, out string? code);
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

        // default to no language selected
        UserLanguageOptions = LanguageOptions.Take(1);

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
                            bool codeFound = Languages.CodeToName.TryGetValue(l.Code, out string? name);
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
                        bool codeFound = Languages.CodeToName.TryGetValue(l, out string? name);
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

        //todo: language to languageCode

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
                //todo: if javascript is disabled, we *could* keep a count the number of empty language inputs, or have a different name for each select with each having a hidden field
                // but it's a lot of effort for probably little or no users
                languageCodes = languageCodes.Append(NoLanguageValue);
            }
            else if (button.StartsWith("remove"))
            {
                int indexToRemove = int.Parse(button.Substring("remove-".Length));
                languageCodes = languageCodes.Where((_, i) => i != indexToRemove);

                if (!languageCodes.Any())
                {
                    languageCodes = languageCodes.Append(NoLanguageValue);
                }
            }

            viewModel.Languages = languageCodes
                .Select(c => c == NoLanguageValue ? NoLanguageValue : Languages.CodeToName[c]);

            return RedirectToSelf(viewModel);
        }

        viewModel.ErrorIndexes = AddAnotherAutocompleteErrorChecker.Create(
            Request.Form, "language", "languageName", LanguageOptions.Skip(1));

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
            if (!viewModel.Languages.Any())
            {
                // handle the case where javascript is disabled and the user has a single empty select
                viewModel.Languages = viewModel.Languages.Append(NoLanguageText);
            }
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

        service.Languages = languageCodes.Select(LanguageDtoFactory.Create).ToList();

        await _serviceDirectoryClient.UpdateService(service, cancellationToken);
    }
}