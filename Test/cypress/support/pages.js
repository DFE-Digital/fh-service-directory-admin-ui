import { Given, When, Then } from "@badeball/cypress-cucumber-preprocessor";

Given("a user has arrived on the home page", () => {
    cy.visit(`/OrganisationAdmin/Welcome`);
});