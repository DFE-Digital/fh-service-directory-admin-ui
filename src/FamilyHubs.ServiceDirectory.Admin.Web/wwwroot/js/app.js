function fhgov(){this.init=function(){e(),t()};let e=function(){const e=document.querySelector("[data-conditional-active]");e instanceof HTMLElement&&e.click()},t=function(){const e=document.getElementById("modelLaOrganisationName");e&&accessibleAutocomplete.enhanceSelectElement({defaultValue:e.value,name:"LaOrganisationName",selectElement:document.querySelector("#LaOrganisationName")});const t=document.getElementById("modelVcsOrganisationName");t&&accessibleAutocomplete.enhanceSelectElement({defaultValue:t.value,name:"VcsOrganisationName",selectElement:document.querySelector("#VcsOrganisationName")})}}function setupLanguageAutocompleteWhenAddAnother(e){if(!(e instanceof HTMLElement))return;e.querySelectorAll("select[id^='language-']").forEach((function(e){accessibleAutocomplete.enhanceSelectElement({name:"languageName",defaultValue:"",selectElement:e}),console.log(e.id);const t=document.getElementById(e.id.replace("-select",""));if(console.log(t),!t)return;t.classList.contains("govuk-input")||t.classList.add("govuk-input");const n=e.classList.contains("govuk-select--error");addGovUkClasses(t,n);new MutationObserver(((e,o)=>{for(let o of e)"attributes"===o.type&&"class"===o.attributeName&&addGovUkClasses(t,n)})).observe(t,{attributes:!0})}))}function addGovUkClasses(e,t){e.classList.contains("govuk-input")||e.classList.add("govuk-input"),t&&!e.classList.contains("govuk-input--error")&&e.classList.add("govuk-input--error")}window.fhgov=new fhgov,document.addEventListener("DOMContentLoaded",(function(){window.fhgov.init()})),setupLanguageAutocompleteWhenAddAnother(null);
//# sourceMappingURL=app.js.map
