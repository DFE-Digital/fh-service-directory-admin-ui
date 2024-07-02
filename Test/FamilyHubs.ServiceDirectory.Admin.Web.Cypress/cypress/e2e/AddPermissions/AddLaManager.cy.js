describe('Add La Manager', () => {
  it('Add La Manager', () => {
    const uniqueUser = Date.now();

    cy.Login('familyhubs.dfeadmin@education.gov.uk');
    cy.get('#add-permission').click();

    // Page - TypeOfRole
    cy.get('[data-testid="role-for-organisation-type-la"]').click();
    cy.get('form button.govuk-button').click();
    
    // Page - TypeOfUserLa
    cy.get('[data-testid="LaManager"]').click();
    cy.get('form button.govuk-button').click();

    // Page - WhichLocalAuthority
    cy.get('#LaOrganisationName').type('B');
    cy.contains('Bristol County Council').click();
    cy.get('#buttonContinue').click();

    // Page - UserEmail
    const email = `user${uniqueUser}@somedomain.com`;
    cy.get('#emailAddress').type(email);
    cy.get('#buttonContinue').click();

    // Page - UserName
    const userName = `User ${uniqueUser}`;
    cy.get('#fullName').type(userName);
    cy.get('#buttonContinue').click();

    // Page - AddLaManager
    cy.get('#WhoFor').contains('Someone who works for a local authority');
    cy.get('#TypeOfPermission').contains('Add and manage services, family hubs and accounts');
    cy.get('#LocalAuthority').contains('Bristol County Council');
    cy.get('#EmailAddress').contains(email);
    cy.get('#FullName').contains(userName);
  })
})