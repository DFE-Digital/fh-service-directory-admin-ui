Feature: Add Service Tests
 Scenario: add service heading is 'What is the name of the service?'
    Given a user has arrived on the add services page
    Then the heading should say 'What is the name of the service?'

  Scenario: add service has a no error message on page load
     Given a user has arrived on the add services page
    Then the error message doesn't exisits on page load
  
  Scenario: add service page  throw valid error
     Given a user has arrived on the add services page
     When the user clicks continue button without entering the service name
     Then the error message shown as 'You must enter a service name'

  Scenario: add service back button redirect to home page
     Given a user has arrived on the add services page
     When the user clicks back button 
     Then the user should redirect to the home page

  Scenario: add service enter a service name and continue
     Given a user has arrived on the add services page
     When the user enters the service name 
     Then the user click on continue page should redirect to typeofservice page

  Scenario: add service select type of service and continue
     Given a user has arrived on the add services page
     When the user enters the service name 
     Then the user click on continue page should redirect to typeofservice page
     Then the user check the organisation has type of service and click continue should redirect to servicedeliverytype
