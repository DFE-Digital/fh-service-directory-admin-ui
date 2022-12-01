import { Given, Then, When } from "@badeball/cypress-cucumber-preprocessor";

Given("a user has arrived on the type service page", () => {
    cy.visit(`/OrganisationAdmin/TypeOfService`);
});

Then("the user able to select the options", () => {
    cy.get('[data-testid="activities,clubsandgroups"]').should("exist");
    cy.get('[data-testid="familysupport"]').should("exist");
    cy.get('[data-testid="health"]').should("exist");
    cy.get('[data-testid="pregnancy,birthandearlyyears"]').should("exist");
    cy.get('[data-testid="specialeducationalneedsanddisabilities(send)"]').should("exist");
    cy.get('[data-testid="transport"]').should("exist");
    cy.get('[data-testid="familyhub"]').should("exist");
});


When("user selects the {string} option", (option) => {
    cy.get(`[data-testid="${option}"]`).check();
});

When("user clicks on the back button", () => {
    cy.get('[data-testid="button-back"]').click();
});

When("user clicks on the continue button without selecting a option", () => {
    cy.get('[data-testid="button-continue"]').click();
});

When("user clicks on the continue button without selecting a sub option", () => {
    cy.get('[data-testid="transport"]').check();
    cy.get('[data-testid="button-continue"]').click();
});

Then("the user should presented with error message", () => {
    cy.get('[data-testid="error-message"]').should("exist");
});

Then("the user should presented with error message for sub cateogry", () => {
    cy.get('[data-testid="error-subcategory"]').should("exist");
});

Then("the user should redirect to previous page", () => {
    cy.location('pathname').should('match', new RegExp("/OrganisationAdmin/ServiceName"));
});

When("user selects the {string} option and select sub cateogry", (option) => {
    cy.get(`[data-testid="${option}"]`).check();
    cy.get('[data-testid="communitytransport"]').check();
});

Then("the user select continue button should redirect to how can families use the service page", () => {
    cy.get('[data-testid="button-continue"]').click();
    cy.location('pathname').should('match', new RegExp("/OrganisationAdmin/ServiceDeliveryType"));
});

Then("the user able to select the follow sub options", () => {
    cy.get('[data-testid="activities"]').should("exist");
    cy.get('[data-testid="beforeandafterschoolclubs"]').should("exist");
    cy.get('[data-testid="holidayclubsandschemes"]').should("exist");
    cy.get('[data-testid="music,artsanddance"]').should("exist");
    cy.get('[data-testid="parent,babyandtoddlergroups"]').should("exist");
    cy.get('[data-testid="pre-schoolplaygroup"]').should("exist");
    cy.get('[data-testid="sportsandrecreation"]').should("exist");
});

Then("the user able to select the follow sub options for familysupport", () => {
    cy.get('[data-testid="bullyingandcyberbullying"]').should("exist");
    cy.get('[data-testid="debtandwelfareadvice"]').should("exist");
    cy.get('[data-testid="domesticabuse"]').should("exist");
    cy.get('[data-testid="intensivetargetedfamilysupport"]').should("exist");
    cy.get('[data-testid="money,benefitsandhousing"]').should("exist");
    cy.get('[data-testid="parentingsupport"]').should("exist");
    cy.get('[data-testid="reducingparentalconflict"]').should("exist");
    cy.get('[data-testid="separatingandseparatedparentsupport"]').should("exist");
    cy.get('[data-testid="stoppingsmoking"]').should("exist");
    cy.get('[data-testid="substancemisuse(includingalcoholanddrug)"]').should("exist");
    cy.get('[data-testid="supportwithparenting"]').should("exist");
    cy.get('[data-testid="targetedyouthsupport"]').should("exist");
    cy.get('[data-testid="youthjusticeservices"]').should("exist");
});

Then("the user able to select the follow sub options for health", () => {
    cy.get('[data-testid="hearingandsight"]').should("exist");
    cy.get('[data-testid="mentalhealth,socialandemotionalsupport"]').should("exist");
    cy.get('[data-testid="nutritionandweightmanagement"]').should("exist");
    cy.get('[data-testid="oralhealth"]').should("exist");
    cy.get('[data-testid="publichealthservices"]').should("exist");
});

Then("the user able to select the follow sub options for pregnancy,birthandearlyyears", () => {
    cy.get('[data-testid="birthregistration"]').should("exist");
    cy.get('[data-testid="earlyyearslanguageandlearning"]').should("exist");
    cy.get('[data-testid="healthvisiting"]').should("exist");
    cy.get('[data-testid="infantfeedingsupport(includingbreastfeeding)"]').should("exist");
    cy.get('[data-testid="midwifeandmaternity"]').should("exist");
    cy.get('[data-testid="perinatalmentalhealthsupport(pregnancytooneyearpostbirth)"]').should("exist");
});


Then("the user able to select the follow sub options for specialeducationalneeds", () => {
    cy.get('[data-testid="autisticspectrumdisorder(asd)"]').should("exist");
    cy.get('[data-testid="breaksandrespite"]').should("exist");
    cy.get('[data-testid="earlyyearssupport"]').should("exist");
    cy.get('[data-testid="groupsforparentsandcarersofchildrenwithsend"]').should("exist");
    cy.get('[data-testid="hearingimpairment"]').should("exist");
    cy.get('[data-testid="learningdifficultiesanddisabilities"]').should("exist");
    cy.get('[data-testid="multi-sensoryimpairment"]').should("exist");
    cy.get('[data-testid="otherdifficultiesordisabilities"]').should("exist");
    cy.get('[data-testid="physicaldisabilities"]').should("exist");
    cy.get('[data-testid="social,emotionalandmentalhealthsupport"]').should("exist");
    cy.get('[data-testid="speech,languageandcommunicationneeds"]').should("exist");
    cy.get('[data-testid="visualimpairment"]').should("exist");
});

Then("the user able to select the follow sub options for transport", () => {
    cy.get('[data-testid="communitytransport"]').should("exist");
});