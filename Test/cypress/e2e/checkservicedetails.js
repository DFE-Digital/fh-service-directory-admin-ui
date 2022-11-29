import { Given, Then, When } from "@badeball/cypress-cucumber-preprocessor";

Given("a user has arrived on the check service details page", () => {
    cy.visit(`/OrganisationAdmin/SignIn`);
    cy.wait(10)
    cy.get('[data-testid="email-id"]').type('BtlVCSAdmin@email.com')
    cy.get('[data-testid="password-id"]').type('Pass123$')
    cy.get('[data-testid="button-continue"]').click();
    cy.get('[data-testid="manage-services"]').click();
    cy.get('[data-testid="aidforchildrenwithtracheostomies-view"]').click();
});

Then("the heading should say {string}", (heading) => {
    cy.get("h1").should("contain.text", heading);
});

Then("the checkdetails page as name section with {string}", (heading) => {
    cy.get('[data-testid="service-name"]').should("contain.text", heading);
});

When("user clicks on name change button", () => {
    cy.get('[data-testid="name-change"]').click();
});

When("user clicks on typeofservice change button", () => {
    cy.get('[data-testid="change-typeofservice"]').click();
});

When("user clicks on servicedeliverytype change button", () => {
    cy.get('[data-testid="change-servicedeliverytype"]').click();
});

When("user clicks on whofor change button", () => {
    cy.get('[data-testid="change-whofor"]').click();
});

When("user clicks on whatlanguage change button", () => {
    cy.get('[data-testid="change-whatlanguage"]').click();
});

When("user clicks on payforservice change button", () => {
    cy.get('[data-testid="change-payforservice"]').click();
});

When("user clicks on contactdetails change button", () => {
    cy.get('[data-testid="change-contactdetails"]').click();
});
 
When("user clicks on servicedescription change button", () => {
    cy.get('[data-testid="change-servicedescription"]').click();
});

When("user clicks on button confirm details", () => {
    cy.get('[data-testid="button-save"]').click();
});

When("user clicks on home page button", () => {
    cy.get('[data-testid="homepage-button"]').click();
});

Then("the user clicks on the change button it should redirect to {string}", (url) => {
    cy.location('pathname').should('match', new RegExp(url));
});

