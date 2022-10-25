describe('| e2e-AddServ_001 | Add Service - Org - Online - NotforChildren - English - Not Paid - emailContact |', function () {
    it('Org - Online - NotforChildren - English - Not Paid - emailContact  ', function () {
        cy.visit('/OrganisationAdmin/Welcome')
        // select add service
        cy.welcomePage('add')
        // give service name
        cy.addService('TestService001')
        // select required service(s)
        cy.selectSupport('organisation')
        //type of service
        cy.serviceDeliveryType('online')
        // who is it for ?
        cy.whoFor('No')
        // what language 
        cy.whatLanguage('English')
        //pay for service
        cy.payForService('No')
        //contact details
        cy.contactDetails('Email', 'email', 'test@email.co.uk')
        //more details
        cy.moreDetails('Test details')
        // check details
        cy.checkDetails('TestService001')
        // service added 
        cy.serviceAdded()
    })
})