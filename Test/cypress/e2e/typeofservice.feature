Feature: Type  Of Service 
Scenario: What support does the service offer page has options
    Given a user has arrived on the type service page
    Then the user able to select the options

 Scenario: user select no option show throw a error
    Given a user has arrived on the type service page
    When user clicks on the continue button without selecting a option
    Then the user should presented with error message

 Scenario: user select doesnt select sub option  throw a error
    Given a user has arrived on the type service page
    When user clicks on the continue button without selecting a sub option
    Then the user should presented with error message for sub cateogry

Scenario: Activities, clubs and groups has sub options
    Given a user has arrived on the type service page
    When user selects the 'activities,clubsandgroups' option
    Then the user able to select the follow sub options

Scenario: familysupport has sub options
    Given a user has arrived on the type service page
    When user selects the 'familysupport' option
    Then the user able to select the follow sub options for familysupport

 Scenario: health has sub options
    Given a user has arrived on the type service page
    When user selects the 'health' option
    Then the user able to select the follow sub options for health

 Scenario: pregnancy,birthandearlyyears has sub options
    Given a user has arrived on the type service page
    When user selects the 'pregnancy,birthandearlyyears' option
    Then the user able to select the follow sub options for pregnancy,birthandearlyyears

 Scenario: specialeducationalneedsanddisabilities(send) has sub options
    Given a user has arrived on the type service page
    When user selects the 'specialeducationalneedsanddisabilities(send)' option
    Then the user able to select the follow sub options for specialeducationalneeds

 Scenario: transport has sub options
    Given a user has arrived on the type service page
    When user selects the 'transport' option
    Then the user able to select the follow sub options for transport

 Scenario: Multiple Options Shows Sub Cateogry
    Given a user has arrived on the type service page
    When user selects the 'transport' option
    When user selects the 'health' option
    Then the user able to select the follow sub options for transport
    Then the user able to select the follow sub options for health

 Scenario: Should redirect to how can families use the service page
    Given a user has arrived on the type service page
    When user selects the 'transport' option and select sub cateogry
    Then the user select continue button should redirect to how can families use the service page

 Scenario: Back button should redirect to service name page
    Given a user has arrived on the type service page
    When user clicks on the back button
    Then the user should redirect to previous page
