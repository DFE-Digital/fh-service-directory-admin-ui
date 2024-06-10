using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
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

    public Dictionary<int, int>? ErrorIdToFirstSelectIndex { get; set; }
    public Dictionary<int, SharedKernel.Razor.ErrorNext.Error>? SelectIndexToError { get; set; }

    public What_LanguageModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.What_Language, connectionRequestCache)
    {
    }

    protected override void OnGetWithError()
    {
        SetFormData();

        if (ServiceModel?.UserInput?.ErrorIndexes == null)
        {
            throw new InvalidOperationException("ServiceModel?.UserInput?.ErrorIndexes is null");
        }

        ErrorIdToFirstSelectIndex = new Dictionary<int, int>();
        SelectIndexToError = new Dictionary<int, SharedKernel.Razor.ErrorNext.Error>();

        var errorIndexes = ServiceModel.UserInput.ErrorIndexes;

        AddToErrorLookups(ErrorId.What_Language__EnterLanguages, errorIndexes.EmptyIndexes);
        AddToErrorLookups(ErrorId.What_Language__EnterSupportedLanguage, errorIndexes.InvalidIndexes);
        AddDuplicatesToErrorLookups(ErrorId.What_Language__SelectLanguageOnce, errorIndexes.DuplicateIndexes);
    }

    protected override void OnGetWithModel()
    {
        // we've redirected to self with user input and no errors, so javascript must be disabled
        if (ServiceModel?.UserInput != null)
        {
            SetFormData();
        }

        // default to no language selected
        UserLanguageOptions = LanguageOptions.Take(1);

        if (ServiceModel!.LanguageCodes?.Any() == true)
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

        UserLanguageOptions = UserLanguageOptions.OrderBy(sli => sli.Text);
    }

    private void SetFormData()
    {
        UserLanguageOptions = ServiceModel!.UserInput!.Languages
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
    }

    private void AddToErrorLookups(ErrorId errorId, IEnumerable<int> indexes)
    {
        var error = Errors.GetErrorIfTriggered((int)errorId);
        if (error == null)
        {
            return;
        }

        ErrorIdToFirstSelectIndex!.Add(error.Id, indexes.First());
        foreach (int index in indexes)
        {
            SelectIndexToError!.Add(index, error);
        }
    }

    private void AddDuplicatesToErrorLookups(ErrorId errorId, IEnumerable<IEnumerable<int>> setIndexes)
    {
        var error = Errors.GetErrorIfTriggered((int)errorId);
        if (error == null)
        {
            return;
        }

        ErrorIdToFirstSelectIndex!.Add(error.Id,
            setIndexes.SelectMany(si => si.Skip(1).Take(1)).Min());

        foreach (var indexes in setIndexes)
        {
            foreach (int index in indexes.Skip(1))
            {
                SelectIndexToError!.Add(index, error);
            }
        }
    }

    protected override IActionResult OnPostWithModel()
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

        //todo: find all instances, rather than just first?
        viewModel.ErrorIndexes = AddAnotherAutocompleteErrorChecker.Create(
            Request.Form, "language", "languageName", LanguageOptions.Skip(1));

        var errorIds = new List<ErrorId>();
        if (viewModel.ErrorIndexes.EmptyIndexes.Any())
        {
            errorIds.Add(ErrorId.What_Language__EnterLanguages);
        }
        if (viewModel.ErrorIndexes.InvalidIndexes.Any())
        {
            errorIds.Add(ErrorId.What_Language__EnterSupportedLanguage);
        }
        if (viewModel.ErrorIndexes.DuplicateIndexes.Any())
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

        ServiceModel!.Updated = ServiceModel.Updated || HaveLanguagesBeenUpdated(languageCodes);

        ServiceModel!.LanguageCodes = languageCodes;
        ServiceModel.TranslationServices = TranslationServices;
        ServiceModel.BritishSignLanguage = BritishSignLanguage;

        return NextPage();
    }

    // updated only *needs* to be set if in edit flow. do we want to check?
    private bool HaveLanguagesBeenUpdated(IEnumerable<string> languageCodes)
    {
        bool languagesAreEqual = ServiceModel!.LanguageCodes != null && 
                        ServiceModel.LanguageCodes
                            .OrderBy(x => x)
                            .SequenceEqual(languageCodes.OrderBy(x => x));

        return !languagesAreEqual
               || ServiceModel.TranslationServices != TranslationServices
               || ServiceModel.BritishSignLanguage != BritishSignLanguage;
    }
}