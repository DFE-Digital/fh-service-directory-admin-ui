using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Factories;
using FamilyHubs.SharedKernel.Razor.AddAnother;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

//todo: construct select list from single source
//todo: update connect to use the code to search
//todo: update Connect, so that the language names match

public class WhatLanguageViewModel
{
    public IEnumerable<string> Languages { get; set; }
    public bool TranslationServices { get; set; }
    public bool BritishSignLanguage { get; set; }
    public AddAnotherAutocompleteErrorChecker? ErrorIndexes { get; set; }
}

public class What_LanguageModel : ServicePageModel<WhatLanguageViewModel>
{
    private readonly IServiceDirectoryClient _serviceDirectoryClient;

    public const string AllLanguagesValue = "";
    public const string InvalidNameValue = "--";

    // list taken from https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes#References
    //todo: build up from LanguageDtoFactory or common list, so that there's one source of languages
    public static SelectListItem[] StaticLanguageOptions { get; set; } =
    {
        new() { Value = AllLanguagesValue, Text = "All languages", Selected = true, Disabled = true },
        new() { Value = "ab", Text = "Abkhazian" },
        new() { Value = "aa", Text = "Afar" },
        new() { Value = "af", Text = "Afrikaans" },
        new() { Value = "ak", Text = "Akan" },
        new() { Value = "sq", Text = "Albanian" },
        new() { Value = "am", Text = "Amharic" },
        new() { Value = "ar", Text = "Arabic" },
        new() { Value = "an", Text = "Aragonese" },
        new() { Value = "hy", Text = "Armenian" },
        new() { Value = "as", Text = "Assamese" },
        new() { Value = "av", Text = "Avaric" },
        new() { Value = "ae", Text = "Avestan" },
        new() { Value = "ay", Text = "Aymara" },
        new() { Value = "az", Text = "Azerbaijani" },
        new() { Value = "bm", Text = "Bambara" },
        new() { Value = "ba", Text = "Bashkir" },
        new() { Value = "eu", Text = "Basque" },
        new() { Value = "be", Text = "Belarusian" },
        new() { Value = "bn", Text = "Bengali" },
        new() { Value = "bi", Text = "Bislama" },
        new() { Value = "bs", Text = "Bosnian" },
        new() { Value = "br", Text = "Breton" },
        new() { Value = "bg", Text = "Bulgarian" },
        new() { Value = "my", Text = "Burmese" },
        new() { Value = "ca", Text = "Catalan; Valencian" },
        new() { Value = "km", Text = "Central Khmer" },
        new() { Value = "ch", Text = "Chamorro" },
        new() { Value = "ce", Text = "Chechen" },
        new() { Value = "ny", Text = "Chichewa; Chewa; Nyanja" },
        new() { Value = "zh", Text = "Chinese" },
        new() { Value = "cu", Text = "Church Slavic; Old Slavonic; Old Church Slavonic" },
        new() { Value = "cv", Text = "Chuvash" },
        new() { Value = "kw", Text = "Cornish" },
        new() { Value = "co", Text = "Corsican" },
        new() { Value = "cr", Text = "Cree" },
        new() { Value = "hr", Text = "Croatian" },
        new() { Value = "cs", Text = "Czech" },
        new() { Value = "da", Text = "Danish" },
        new() { Value = "dv", Text = "Divehi; Dhivehi; Maldivian" },
        new() { Value = "nl", Text = "Dutch; Flemish" },
        new() { Value = "dz", Text = "Dzongkha" },
        new() { Value = "en", Text = "English" },
        new() { Value = "eo", Text = "Esperanto" },
        new() { Value = "et", Text = "Estonian" },
        new() { Value = "ee", Text = "Ewe" },
        new() { Value = "fo", Text = "Faroese" },
        new() { Value = "fj", Text = "Fijian" },
        new() { Value = "fi", Text = "Finnish" },
        new() { Value = "fr", Text = "French" },
        new() { Value = "ff", Text = "Fulah" },
        new() { Value = "gd", Text = "Gaelic; Scottish Gaelic" },
        new() { Value = "gl", Text = "Galician" },
        new() { Value = "lg", Text = "Ganda" },
        new() { Value = "ka", Text = "Georgian" },
        new() { Value = "de", Text = "German" },
        new() { Value = "el", Text = "Greek" }, // Greek, Modern (1453-)
        new() { Value = "gn", Text = "Guarani" },
        new() { Value = "gu", Text = "Gujarati" },
        new() { Value = "ht", Text = "Haitian; Haitian Creole" },
        new() { Value = "ha", Text = "Hausa" },
        new() { Value = "he", Text = "Hebrew" },
        new() { Value = "hz", Text = "Herero" },
        new() { Value = "hi", Text = "Hindi" },
        new() { Value = "ho", Text = "Hiri Motu" },
        new() { Value = "hu", Text = "Hungarian" },
        new() { Value = "is", Text = "Icelandic" },
        new() { Value = "io", Text = "Ido" },
        new() { Value = "ig", Text = "Igbo" },
        new() { Value = "id", Text = "Indonesian" },
        new() { Value = "ia", Text = "Interlingua" }, // Interlingua (International Auxiliary Language Association)
        new() { Value = "ie", Text = "Interlingue; Occidental" },
        new() { Value = "iu", Text = "Inuktitut" },
        new() { Value = "ik", Text = "Inupiaq" },
        new() { Value = "ga", Text = "Irish" },
        new() { Value = "it", Text = "Italian" },
        new() { Value = "ja", Text = "Japanese" },
        new() { Value = "jv", Text = "Javanese" },
        new() { Value = "kl", Text = "Kalaallisut; Greenlandic" },
        new() { Value = "kn", Text = "Kannada" },
        new() { Value = "kr", Text = "Kanuri" },
        new() { Value = "ks", Text = "Kashmiri" },
        new() { Value = "kk", Text = "Kazakh" },
        new() { Value = "ki", Text = "Kikuyu; Gikuyu" },
        new() { Value = "rw", Text = "Kinyarwanda" },
        new() { Value = "ky", Text = "Kirghiz; Kyrgyz" },
        new() { Value = "kv", Text = "Komi" },
        new() { Value = "kg", Text = "Kongo" },
        new() { Value = "ko", Text = "Korean" },
        new() { Value = "kj", Text = "Kuanyama; Kwanyama" },
        new() { Value = "ku", Text = "Kurdish" },
        new() { Value = "lo", Text = "Lao" },
        new() { Value = "la", Text = "Latin" },
        new() { Value = "lv", Text = "Latvian" },
        new() { Value = "li", Text = "Limburgan; Limburger; Limburgish" },
        new() { Value = "ln", Text = "Lingala" },
        new() { Value = "lt", Text = "Lithuanian" },
        new() { Value = "lu", Text = "Luba-Katanga" },
        new() { Value = "lb", Text = "Luxembourgish; Letzeburgesch" },
        new() { Value = "mk", Text = "Macedonian" },
        new() { Value = "mg", Text = "Malagasy" },
        new() { Value = "ms", Text = "Malay" },
        new() { Value = "ml", Text = "Malayalam" },
        new() { Value = "mt", Text = "Maltese" },
        new() { Value = "gv", Text = "Manx" },
        new() { Value = "mi", Text = "Maori" },
        new() { Value = "mr", Text = "Marathi" },
        new() { Value = "mh", Text = "Marshallese" },
        new() { Value = "mn", Text = "Mongolian" },
        new() { Value = "na", Text = "Nauru" },
        new() { Value = "nv", Text = "Navajo; Navaho" },
        new() { Value = "ng", Text = "Ndonga" },
        new() { Value = "ne", Text = "Nepali" },
        new() { Value = "nd", Text = "North Ndebele" },
        new() { Value = "se", Text = "Northern Sami" },
        new() { Value = "no", Text = "Norwegian" },
        new() { Value = "nb", Text = "Norwegian Bokmål" },
        new() { Value = "nn", Text = "Norwegian Nynorsk" },
        new() { Value = "oc", Text = "Occitan" },
        new() { Value = "oj", Text = "Ojibwa" },
        new() { Value = "or", Text = "Oriya" },
        new() { Value = "om", Text = "Oromo" },
        new() { Value = "os", Text = "Ossetian; Ossetic" },
        new() { Value = "pi", Text = "Pali" },
        new() { Value = "ps", Text = "Pashto; Pushto" },
        new() { Value = "fa", Text = "Persian" },
        new() { Value = "pl", Text = "Polish" },
        new() { Value = "pt", Text = "Portuguese" },
        new() { Value = "pa", Text = "Punjabi; Panjabi" },
        new() { Value = "qu", Text = "Quechua" },
        new() { Value = "ro", Text = "Romanian; Moldavian; Moldovan" },
        new() { Value = "rm", Text = "Romansh" },
        new() { Value = "rn", Text = "Rundi" },
        new() { Value = "ru", Text = "Russian" },
        new() { Value = "sm", Text = "Samoan" },
        new() { Value = "sg", Text = "Sango" },
        new() { Value = "sa", Text = "Sanskrit" },
        new() { Value = "sc", Text = "Sardinian" },
        new() { Value = "sr", Text = "Serbian" },
        new() { Value = "sn", Text = "Shona" },
        new() { Value = "ii", Text = "Sichuan Yi; Nuosu" },
        new() { Value = "sd", Text = "Sindhi" },
        new() { Value = "si", Text = "Sinhala; Sinhalese" },
        new() { Value = "sk", Text = "Slovak" },
        new() { Value = "sl", Text = "Slovenian" },
        new() { Value = "so", Text = "Somali" },
        new() { Value = "nr", Text = "South Ndebele" },
        new() { Value = "st", Text = "Southern Sotho" },
        new() { Value = "es", Text = "Spanish; Castilian" },
        new() { Value = "su", Text = "Sundanese" },
        new() { Value = "sw", Text = "Swahili" },
        new() { Value = "ss", Text = "Swati" },
        new() { Value = "sv", Text = "Swedish" },
        new() { Value = "tl", Text = "Tagalog" },
        new() { Value = "ty", Text = "Tahitian" },
        new() { Value = "tg", Text = "Tajik" },
        new() { Value = "ta", Text = "Tamil" },
        new() { Value = "tt", Text = "Tatar" },
        new() { Value = "te", Text = "Telugu" },
        new() { Value = "th", Text = "Thai" },
        new() { Value = "bo", Text = "Tibetan" },
        new() { Value = "ti", Text = "Tigrinya" },
        new() { Value = "to", Text = "Tonga (Tonga Islands)" },
        new() { Value = "ts", Text = "Tsonga" },
        new() { Value = "tn", Text = "Tswana" },
        new() { Value = "tr", Text = "Turkish" },
        new() { Value = "tk", Text = "Turkmen" },
        new() { Value = "tw", Text = "Twi" },
        new() { Value = "ug", Text = "Uighur; Uyghur" },
        new() { Value = "uk", Text = "Ukrainian" },
        new() { Value = "ur", Text = "Urdu" },
        new() { Value = "uz", Text = "Uzbek" },
        new() { Value = "ve", Text = "Venda" },
        new() { Value = "vi", Text = "Vietnamese" },
        new() { Value = "vo", Text = "Volapük" },
        new() { Value = "wa", Text = "Walloon" },
        new() { Value = "cy", Text = "Welsh" },
        new() { Value = "fy", Text = "Western Frisian" },
        new() { Value = "wo", Text = "Wolof" },
        new() { Value = "xh", Text = "Xhosa" },
        new() { Value = "yi", Text = "Yiddish" },
        new() { Value = "yo", Text = "Yoruba" },
        new() { Value = "za", Text = "Zhuang; Chuang" },
        new() { Value = "zu", Text = "Zulu" }
    };

    public IEnumerable<SelectListItem> LanguageOptions => StaticLanguageOptions;
    public IEnumerable<SelectListItem> UserLanguageOptions { get; set; }

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

            UserLanguageOptions = ServiceModel.UserInput.Languages.Select(name =>
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
                    UserLanguageOptions = service.Languages.Select(l =>
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
    }

    protected override async Task<IActionResult> OnPostWithModelAsync(CancellationToken cancellationToken)
    {
        //todo: do we want to split the calls in base to have OnPostErrorChecksAsync and OnPostUpdateAsync? (or something)

        //todo: in the DOM is the text input and the hidden select, so we get 2 values for each selection - the code from the select, and the name from input
        // when multiple languages and js enabled, we get the multiple languageName's but only 1 languageCode
        // looks like we'll have to explicitly look for different data when js is disabled

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
            // the form contains the select values in "language" and there are no "languageName" values as the inputs wern't created

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

        //todo: need to order by language names
        languageCodes = languageCodes.OrderBy(l => l);
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