import { Given, Then, When } from "@badeball/cypress-cucumber-preprocessor";

Given("a user has arrived on the manage your services page", () => {
    cy.visit(`/OrganisationAdmin/SignIn`);
    cy.get('[data-testid="email-id"]').type('BtlVCSAdmin@email.com')
    cy.get('[data-testid="password-id"]').type('Pass123$')
    cy.get('[data-testid="button-continue"]').click();
    cy.get('[data-testid="manage-services"]').click();
});

Then("the heading should say {string}", (heading) => {
    cy.get("h1").should("have.text", heading);
});

Then("the aidforchildren with tracheostmies service name exists", () => {
    cy.get('[data-testid="aidforchildrenwithtracheostomies"]').should("exist");
});

Then("the aidforchildren with tracheostmies service view button exists", () => {
    cy.get('[data-testid="aidforchildrenwithtracheostomies-view"]').should("exist");
});

Then("the aidforchildren with tracheostmies service view delete exists", () => {
    cy.get('[data-testid="aidforchildrenwithtracheostomies-delete"]').should("exist");
});