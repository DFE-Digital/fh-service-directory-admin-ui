Feature: Check Service Details Tests
Scenario: check service detials has a heading with 'Aid for Children with Tracheostomies'
    Given a user has arrived on the check service details page
    Then the heading should say 'Aid for Children with Tracheostomies'

Scenario: check service detials has name section
    Given a user has arrived on the check service details page
    Then the checkdetails page as name section with 'Aid for Children with Tracheostomies'

Scenario: check service details page redirect to service name 
    Given a user has arrived on the check service details page
    When user clicks on name change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/ServiceName'

Scenario: check service details page redirect to type of service
    Given a user has arrived on the check service details page
    When user clicks on typeofservice change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/TypeOfService'


Scenario: check service details page redirect to service delivery type
    Given a user has arrived on the check service details page
    When user clicks on servicedeliverytype change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/ServiceDeliveryType'

Scenario: check service details page redirect to who for
    Given a user has arrived on the check service details page
    When user clicks on whofor change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/WhoFor'

Scenario: check service details page redirect to pay for service
    Given a user has arrived on the check service details page
    When user clicks on payforservice change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/PayForService'

Scenario: check service details page redirect to contact details
    Given a user has arrived on the check service details page
    When user clicks on contactdetails change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/ContactDetails'

Scenario: check service details page redirect to service description
    Given a user has arrived on the check service details page
    When user clicks on servicedescription change button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/ServiceDescription'

Scenario: check service details page redirect to confirm details
    Given a user has arrived on the check service details page
    When user clicks on button confirm details
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/DetailsSaved'

Scenario: save details page redirect to home page
    Given a user has arrived on the check service details page
    When user clicks on button confirm details
    Then user clicks on home page button
    Then the user clicks on the change button it should redirect to '/OrganisationAdmin/Welcome'

