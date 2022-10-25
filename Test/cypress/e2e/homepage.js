import { Given, Then, When } from "@badeball/cypress-cucumber-preprocessor";

Given("a user has arrived on the home page", () => {
    cy.visit(`/OrganisationAdmin/Welcome`);
});

Then("the page URL ends with {string}", url => {
    cy.location('pathname').should('match', new RegExp(`${url}$`));
});

Then("the heading should say {string}", (heading) => {
    cy.get("h1").should("have.text", heading);
});

Then("they see the add service button", () => {
    cy.get('[data-testid="add-service"]').should("exist");
});

Then("they see the manage services button", () => {
    cy.get('[data-testid="manage-services"]').should("exist");
});

