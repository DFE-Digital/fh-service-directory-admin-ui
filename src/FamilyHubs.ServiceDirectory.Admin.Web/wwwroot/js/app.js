function fhgov(){this.init=function(){e(),t()};let e=function(){const e=document.querySelector("[data-conditional-active]");e instanceof HTMLElement&&e.click()},t=function(){const e=document.getElementById("modelLaOrganisationName");e&&accessibleAutocomplete.enhanceSelectElement({defaultValue:e.value,name:"LaOrganisationName",selectElement:document.querySelector("#LaOrganisationName")});const t=document.getElementById("modelVcsOrganisationName");t&&accessibleAutocomplete.enhanceSelectElement({defaultValue:t.value,name:"VcsOrganisationName",selectElement:document.querySelector("#VcsOrganisationName")})}}function setupLanguageAutocompleteWhenAddAnother(e){if(!(e instanceof HTMLElement))return;const t=e.querySelectorAll("select[id^='language-']");console.log("enhancing "+t.length+" language selects");new MutationObserver(((e,n)=>{const o=e.some((e=>"childList"===e.type&&e.addedNodes.length>0)),a=e.some((e=>{if("attributes"===e.type&&"class"===e.attributeName){const t=e.target;return"input"===t.tagName.toLowerCase()&&"text"===t.getAttribute("type")}return!1}));o&&console.log("childListMutation"),a&&console.log("attributesMutation"),(o||a)&&t.forEach((function(e){console.log(e.id);const t=document.getElementById(e.id.replace("-select",""));if(console.log(t),!t)return void console.log("no input found for select");addGovUkClasses(t,e.classList.contains("govuk-select--error"))}))})).observe(e,{childList:!0,subtree:!0,attributes:!0}),t.forEach((function(e){accessibleAutocomplete.enhanceSelectElement({name:"languageName",defaultValue:"",selectElement:e})}))}function addGovUkClasses(e,t){e.classList.contains("govuk-input")||e.classList.add("govuk-input"),t&&!e.classList.contains("govuk-input--error")&&e.classList.add("govuk-input--error")}window.fhgov=new fhgov,document.addEventListener("DOMContentLoaded",(function(){window.fhgov.init()})),setupLanguageAutocompleteWhenAddAnother(null);
//# sourceMappingURL=app.js.map
