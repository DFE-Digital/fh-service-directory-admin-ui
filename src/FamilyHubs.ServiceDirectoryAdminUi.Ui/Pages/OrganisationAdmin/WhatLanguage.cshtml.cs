using FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using static FamilyHubs.ServiceDirectoryAdminUi.Ui.Infrastructure.Configuration.PageConfiguration;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Pages.OrganisationAdmin;

public class WhatLanguageModel : PageModel
{
    public string LastPage { get; set; } = default!;
    public string UserFlow { get; set; } = default!;

    private readonly ISessionService _session;

    public string SelectedLanguage { get; set; } = default!;

    public List<SelectListItem> LanguageSelectionList { get; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "Select language", Selected = true },
        new SelectListItem { Value = "Afrikaans", Text = "Afrikaans" },
        new SelectListItem { Value = "Albanian", Text = "Albanian" },
        new SelectListItem { Value = "Arabic", Text = "Arabic" },
        new SelectListItem { Value = "Armenian", Text = "Armenian" },
        new SelectListItem { Value = "Basque", Text = "Basque" },
        new SelectListItem { Value = "Bengali", Text = "Bengali" },
        new SelectListItem { Value = "Bulgarian", Text = "Bulgarian" },
        new SelectListItem { Value = "Catalan", Text = "Catalan" },
        new SelectListItem { Value = "Cambodian", Text = "Cambodian" },
        new SelectListItem { Value = "Chinese (Mandarin)", Text = "Chinese (Mandarin)" },
        new SelectListItem { Value = "Croatian", Text = "Croatian" },
        new SelectListItem { Value = "Czech", Text = "Czech" },
        new SelectListItem { Value = "Danish", Text = "Danish" },
        new SelectListItem { Value = "Dutch", Text = "Dutch" },
        new SelectListItem { Value = "English", Text = "English"},
        new SelectListItem { Value = "Estonian", Text = "Estonian" },
        new SelectListItem { Value = "Fiji", Text = "Fiji" },
        new SelectListItem { Value = "Finnish", Text = "Finnish" },
        new SelectListItem { Value = "French", Text = "French" },
        new SelectListItem { Value = "Georgian", Text = "Georgian" },
        new SelectListItem { Value = "German", Text = "German" },
        new SelectListItem { Value = "Greek", Text = "Greek" },
        new SelectListItem { Value = "Gujarati", Text = "Gujarati" },
        new SelectListItem { Value = "Hebrew", Text = "Hebrew" },
        new SelectListItem { Value = "Hindi", Text = "Hindi" },
        new SelectListItem { Value = "Hungarian", Text = "Hungarian" },
        new SelectListItem { Value = "Icelandic", Text = "Icelandic" },
        new SelectListItem { Value = "Indonesian", Text = "Indonesian" },
        new SelectListItem { Value = "Irish", Text = "Irish" },
        new SelectListItem { Value = "Italian", Text = "Italian" },
        new SelectListItem { Value = "Japanese", Text = "Japanese" },
        new SelectListItem { Value = "Javanese", Text = "Javanese" },
        new SelectListItem { Value = "Korean", Text = "Korean" },
        new SelectListItem { Value = "Latin", Text = "Latin" },
        new SelectListItem { Value = "Latvian", Text = "Latvian" },
        new SelectListItem { Value = "Lithuanian", Text = "Lithuanian" },
        new SelectListItem { Value = "Macedonian", Text = "Macedonian" },
        new SelectListItem { Value = "Malay", Text = "Malay" },
        new SelectListItem { Value = "Malayalam", Text = "Malayalam" },
        new SelectListItem { Value = "Maltese", Text = "Maltese" },
        new SelectListItem { Value = "Maori", Text = "Maori" },
        new SelectListItem { Value = "Marathi", Text = "Marathi" },
        new SelectListItem { Value = "Mongolian", Text = "Mongolian" },
        new SelectListItem { Value = "Nepali", Text = "Nepali" },
        new SelectListItem { Value = "Norwegian", Text = "Norwegian" },
        new SelectListItem { Value = "Persian", Text = "Persian" },
        new SelectListItem { Value = "Polish", Text = "Polish" },
        new SelectListItem { Value = "Portuguese", Text = "Portuguese" },
        new SelectListItem { Value = "Punjabi", Text = "Punjabi" },
        new SelectListItem { Value = "Quechua", Text = "Quechua" },
        new SelectListItem { Value = "Romanian", Text = "Romanian" },
        new SelectListItem { Value = "Russian", Text = "Russian" },
        new SelectListItem { Value = "Samoan", Text = "Samoan" },
        new SelectListItem { Value = "Serbian", Text = "Serbian" },
        new SelectListItem { Value = "Slovak", Text = "Slovak" },
        new SelectListItem { Value = "Slovenian", Text = "Slovenian" },
        new SelectListItem { Value = "Spanish", Text = "Spanish" },
        new SelectListItem { Value = "Swahili", Text = "Swahili" },
        new SelectListItem { Value = "Swedish ", Text = "Swedish " },
        new SelectListItem { Value = "Tamil", Text = "Tamil" },
        new SelectListItem { Value = "Tatar", Text = "Tatar" },
        new SelectListItem { Value = "Telugu", Text = "Telugu" },
        new SelectListItem { Value = "Thai", Text = "Thai" },
        new SelectListItem { Value = "Tibetan", Text = "Tibetan" },
        new SelectListItem { Value = "Tonga", Text = "Tonga" },
        new SelectListItem { Value = "Turkish", Text = "Turkish" },
        new SelectListItem { Value = "Ukrainian", Text = "Ukrainian" },
        new SelectListItem { Value = "Urdu", Text = "Urdu" },
        new SelectListItem { Value = "Uzbek", Text = "Uzbek" },
        new SelectListItem { Value = "Vietnamese", Text = "Vietnamese" },
        new SelectListItem { Value = "Welsh", Text = "Welsh" },
        new SelectListItem { Value = "Xhosa", Text = "Xhosa" },
    };

    [BindProperty]
    public int LanguageNumber { get; set; } = 1;

    [BindProperty]
    public List<string> LanguageCode { get; set; } = default!;

    [BindProperty]
    public bool ValidationValid { get; set; } = true;

    [BindProperty]
    public bool AllLanguagesSelected { get; set; } = true;

    [BindProperty]
    public bool NoDuplicateLanguages { get; set; } = true;

    [BindProperty]
    public int LanguageNotSelectedIndex { get; set; } = -1;

    [BindProperty]
    public List<string> LanguageSelectedByField { get; set; } = default!;

    [BindProperty]
    public List<string> DuplicateFoundByField { get; set; } = default!;

    public WhatLanguageModel(ISessionService sessionService)
    {
        _session = sessionService;
    }

    public void OnGet(string strOrganisationViewModel)
    {
        LastPage = _session.RetrieveLastPageName(HttpContext);
        UserFlow = _session.RetrieveUserFlow(HttpContext);

        var organisationViewModel = _session.RetrieveOrganisationWithService(HttpContext);

        if (organisationViewModel != null && organisationViewModel.Languages != null && organisationViewModel.Languages.Any())
        {
            LanguageCode = organisationViewModel.Languages;
            LanguageNumber = LanguageCode.Count();
        }

    }

    public void OnPostAddAnotherLanguage()
    {
        LanguageCode.Add("Select language");
        LanguageNumber = LanguageCode.Count;
    }

    public void OnPostRemoveLanguage(int id)
    {
        LanguageCode.RemoveAt(id);
        LanguageNumber = LanguageCode.Count;
    }

    public IActionResult OnPostNextPage()
    {
        if (LanguageCode == null || LanguageCode.Count == 0)
        {
            ValidationValid = false;
            return Page();
        }
        
        if (!ModelState.IsValid)
        {
            ValidationValid = false;
            return Page();
        }

        var organisationViewModel = _session?.RetrieveOrganisationWithService(HttpContext);
        if (organisationViewModel == null)
        {
            return Page();
        }

        organisationViewModel.Languages = new List<string>(LanguageCode);

        for (int i = 0; i < organisationViewModel.Languages.Count; i++)
        {
            if (organisationViewModel.Languages[i] == null)
            {
                LanguageNotSelectedIndex = i;
                LanguageNumber = organisationViewModel.Languages.Count;
                ValidationValid = false;
                AllLanguagesSelected = false;
                return Page();
            }
        }

        for (int i = 0; i < organisationViewModel.Languages.Count; i++)
        {
            for (int ii = 0; ii < organisationViewModel.Languages.Count; ii++)
            {
                if (organisationViewModel.Languages[i] == organisationViewModel.Languages[ii] && i != ii)
                {
                    LanguageNumber = organisationViewModel.Languages.Count;
                    ValidationValid = false;
                    NoDuplicateLanguages = false;
                    LanguageNotSelectedIndex = ii;
                    return Page();
                }
            }
        }

        _session?.StoreOrganisationWithService(HttpContext, organisationViewModel);

        if (_session?.RetrieveLastPageName(HttpContext) == CheckServiceDetailsPageName)
        {
            return RedirectToPage($"/OrganisationAdmin/{CheckServiceDetailsPageName}");
        }
        return RedirectToPage("/OrganisationAdmin/PayForService");

    }
}
