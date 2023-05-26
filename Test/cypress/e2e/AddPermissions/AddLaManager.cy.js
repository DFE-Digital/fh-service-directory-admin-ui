describe('Add La Manager', () => {

  afterEach(function() {
    if (this.currentTest.state === 'failed') {
      Cypress.runner.stop()
    }
  });

  it('Add La Manager', () => {

    var uniqueUser = Date.now();

    cy.Login('dfeAdmin.user@stub.com');
    cy.get('#add-permission').click();

    // Page - TypeOfRole
    cy.get('#role-for-organisation-type-La').click();
    cy.get('#buttonContinue').click();
    
    // Page - TypeOfUserLa
    cy.get('#la-admin').click();
    cy.get('#buttonContinue').click();

    // Page - WhichLocalAuthority
    cy.get('#LaOrganisationName').type('B');
    cy.contains('Bristol County Council').click;
    cy.get('#buttonContinue').click();

    // Page - UserEmail
    var email = `user${uniqueUser}@somedomain.com`;
    cy.get('#emailAddress').type(email);
    cy.get('#buttonContinue').click();

    // Page - UserName
    var userName = `User ${uniqueUser}`;
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