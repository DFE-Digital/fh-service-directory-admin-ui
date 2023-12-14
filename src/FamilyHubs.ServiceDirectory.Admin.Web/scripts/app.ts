export { };

declare const accessibleAutocomplete: any;
declare global {
    interface Window {
        fhgov: any;
    }
}

function fhgov() {
    this.init = function () {
        restoreConditionalInputs();
        enhanceAccessibleAutocomplete();
    };

    let restoreConditionalInputs = function () {
        const element = document.querySelector("[data-conditional-active]");
        if (element instanceof HTMLElement) {
            element.click()
        }

    }

    let enhanceAccessibleAutocomplete = function () {

        //WhichLocalAuthority.cshtml && AddOrganisationWhichLocalAuthority.cshtml
        const modelLaOrganisationName = document.getElementById('modelLaOrganisationName') as HTMLInputElement;
        if (modelLaOrganisationName) {
            accessibleAutocomplete.enhanceSelectElement({
                defaultValue: modelLaOrganisationName.value,
                name: 'LaOrganisationName',
                selectElement: document.querySelector('#LaOrganisationName')
            });
        }

        //WhichVcsOrganisation.cshtml
        const modelVcsOrganisationName = document.getElementById('modelVcsOrganisationName') as HTMLInputElement;
        if (modelVcsOrganisationName) {
            accessibleAutocomplete.enhanceSelectElement({
                defaultValue: modelVcsOrganisationName.value,
                name: 'VcsOrganisationName',
                selectElement: document.querySelector('#VcsOrganisationName')
            })
        }

        //What=Language.cshtml

        //todo: this will have to be re-called when we add a new language
        // we might have to call this first, before we render it, otherwise might get a flash
        //todo: prefix with screen name for safety, or only run code on the correct page.
        // perhaps could have code per page name and only execute code for the current page (if there is any) - allows us to work around no inline scripts
        //todo: type this
        const languageSelects = document.querySelectorAll("[id^='language-']"); // [id$='\\d+']");
        //todo: use a null '' value for all languages?
        languageSelects.forEach(function (select) {
            accessibleAutocomplete.enhanceSelectElement({
                //defaultValue: select.value,
                //todo: does it default to name in html?
                //name: select.name,
                defaultValue: '',
                selectElement: select
            })
        });
    }
}

window.fhgov = new fhgov();

document.addEventListener('DOMContentLoaded', function () {
    window.fhgov.init();
});