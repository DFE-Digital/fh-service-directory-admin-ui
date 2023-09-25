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
        addFilterEventListerners();
    };

    this.showAlert = function (message) {
        alert(message);
    }

    let displayFilters = function () {
        document.getElementById("filters-component").style.display = 'block';
        document.getElementById("buttonShowFilters").classList.add("govuk-!-display-none");
        document.getElementById("buttonHideFilters").classList.remove("govuk-!-display-none");
    }
    let hideFilters = function () {
        document.getElementById("filters-component").style.display = '';
        document.getElementById("buttonShowFilters").classList.remove("govuk-!-display-none");
        document.getElementById("buttonHideFilters").classList.add("govuk-!-display-none");
    }

    let addFilterEventListerners = function () {
        const showFilterButton = document.getElementById('buttonShowFilters') as HTMLButtonElement;
        if (showFilterButton) {
            showFilterButton.addEventListener('click', displayFilters);
        }

        const hideFilterButton = document.getElementById('buttonHideFilters') as HTMLButtonElement;
        if (hideFilterButton) {
            hideFilterButton.addEventListener('click', hideFilters);
        }
    }

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