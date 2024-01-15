﻿export { };

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
    }
}

window.fhgov = new fhgov();

document.addEventListener('DOMContentLoaded', function () {
    window.fhgov.init();
});

function setupLanguageAutocompleteWhenAddAnother(element: HTMLElement) {

    if (!(element instanceof HTMLElement)) {
        return;
    }

    const languageSelects = element.querySelectorAll("select[id^='language-']") as NodeListOf<HTMLSelectElement>;

    languageSelects.forEach(function (select) {
        accessibleAutocomplete.enhanceSelectElement({
            name: 'languageName',
            defaultValue: '',
            selectElement: select
        });
    });

    // work around accessible-autocomplete not handling errors or using standard govuk styling classes
    // there's a discussion here about it...
    // https://github.com/alphagov/accessible-autocomplete/issues/428
    // but we've had to implement our own (hacky) solution by using MutationObserver
    // and adding extra classes (with custom css) to the input element.

    // I was going to either package up this code into an exported function to ease reuse and maintanence,
    // or fork the accessible-autocomplete preact component, 
    // but someone is adding official support today (2024-01-12) so we should be able to remove this soon!
    // https://github.com/alphagov/accessible-autocomplete/pull/602

    //todo: fix aria-describedBy on the input too
    // see https://github.com/alphagov/accessible-autocomplete/issues/589

    const domObserver = new MutationObserver((mutationsList, observer) => {
        const childListMutation = mutationsList.some(mutation => mutation.type === 'childList' && mutation.addedNodes.length > 0);
        const attributesMutation = mutationsList.some(mutation => {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const targetElement = mutation.target as HTMLElement;
                return targetElement.tagName.toLowerCase() === 'input' && targetElement.getAttribute('type') === 'text';
            }
            return false;
        });

        if (childListMutation) {
            console.log('childListMutation');
        }

        if (attributesMutation) {
            console.log('attributesMutation');
        }

        if (childListMutation || attributesMutation) {
            /*todo: create list of input ids outside of observer? */
            languageSelects.forEach(function (select) {
                console.log(select.id);
                const input = document.getElementById(select.id.replace('-select', '')) as HTMLInputElement;
                console.log(input);

                if (!input) {
                    console.log('no input found for select')
                    return;
                }

                const errorState = select.classList.contains('govuk-select--error');

                addGovUkClasses(input, errorState);
            });
        }
    });

    domObserver.observe(element, { childList: true, subtree: true, attributes: true });
}

function addGovUkClasses(input: HTMLInputElement, errorState: boolean) {
    if (!input.classList.contains('govuk-input')) {
        input.classList.add('govuk-input');
    }

    if (errorState && !input.classList.contains('govuk-input--error')) {
        input.classList.add('govuk-input--error');
    }
}

//todo: this is a hack - we want setupLanguageAutocompleteWhenAddAnother to be in the generated js file.
// if we export it, it includes the export keyword in the generated js file
// (but we use export in the other ts files, without the js containing export!)
// so as a workaround we call it where it no-ops
setupLanguageAutocompleteWhenAddAnother(null);
//});