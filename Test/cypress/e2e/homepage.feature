Feature: Home Page Page Tests
  Scenario: home page url is '/OrganisationAdmin/Welcome'
    Given a user has arrived on the home page 
    Then the page URL ends with '/OrganisationAdmin/Welcome'

   Scenario: home page heading is 'Bristol City Council'
    Given a user has arrived on the home page
    Then the heading should say 'Bristol City Council'

   Scenario: home page has a add service button 
    Given a user has arrived on the home page
    Then they see the add service button

   Scenario: home page has a add service button 
    Given a user has arrived on the home page
    Then they see the manage services button

