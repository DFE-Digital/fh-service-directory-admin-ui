import { Given, Then, When } from "@badeball/cypress-cucumber-preprocessor";

Given("a user has arrived on the add services page", () => {
    cy.visit(`/OrganisationAdmin/Welcome`);
    cy.get('[data-testid="add-service"]').click();
});

Then("the heading should say {string}", (heading) => {
    cy.get("h1").should("contain.text", heading);
});

Then("the error message doesn't exisits on page load", () => {
    cy.get('[data-testid="enter-service-name-error"]').should("not.exist");
    cy.get('[data-testid="ame-enter-validation"]').should("not.exist");
});

When("the user clicks continue button without entering the service name", () => {
    cy.get('[data-testid="name-continue-button"]').click();
});

Then("the error message shown as {string}", (error) => {
    cy.get('[data-testid="enter-service-name-error"]').should("exist");
    cy.get('[data-testid="enter-service-name-error"]').should("contain.text", error);
    cy.get('[data-testid="name-enter-validation"]').should("exist");
});

When("the user clicks back button", () => {
    cy.get('[data-testid="back-button"]').click();
});

Then("the user should redirect to the home page", () => {
    cy.location('pathname').should('match', new RegExp("/OrganisationAdmin/Welcome"));
});