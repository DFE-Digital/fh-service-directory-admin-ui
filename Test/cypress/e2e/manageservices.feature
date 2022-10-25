Feature: Manage Services Tests
  Scenario: manage service heading is 'Manage your services'
    Given a user has arrived on the manage your services page
    Then the heading should say 'Manage your services'

  Scenario: manage service has a service name with 'Aid for Children with Tracheostomies'
    Given a user has arrived on the manage your services page
    Then the aidforchildren with tracheostmies service name exists

  Scenario: manage service has a service name with view button
    Given a user has arrived on the manage your services page
    Then the aidforchildren with tracheostmies service view button exists

   Scenario: manage service has a service name with delete button
    Given a user has arrived on the manage your services page
    Then the aidforchildren with tracheostmies service view delete exists