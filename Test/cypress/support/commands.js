// Login
Cypress.Commands.add('Login', (userEmail) => {
    
    //  Load Home Page
    cy.visit('/');
    cy.wait(500);

    //  Click Start Now to be directed to authenticated page
    cy.get('#linkStart').click();

    //  Select account to use
    cy.get('a').contains(userEmail).click();
})
