// Login
Cypress.Commands.add('Login', (userEmail) => {
    cy.visit('https://signin.integration.account.gov.uk/cookies', {
        auth: {
            username: 'integration-user',
            password: 'winter2021',
        },
    });

    //  Load Home Page
    cy.visit('/');

    //  Click Start Now to be directed to authenticated page
    cy.get('a.govuk-button--start').click();
    cy.get('#sign-in-button').click();

    cy.get('#email').type(userEmail);
    cy.get('form button.govuk-button').click();

    cy.get('#password').type('43@$mellyC4t3');
    cy.get('form button.govuk-button').click();
})
