# 2. End to End Testing Framework

<!-- This is an optional element. Feel free to remove. -->
## Decision Drivers

* Development environment set up complexity
* Complexity of writing tests
* Speed of test execution
* Ability to run headless
* Ability to integrate into a GitHub action
* Test execution reporting

## Considered Option

* Cypress

Chosen option: "Cypress", because it is open source, well documented, was straightforward to set up and integrates well with GitHub actions. It reports test results well and is extremely fast to run. This outweighs the fact that it is primarily a JavaScript tool used by a .NET team.

## Pros and Cons of the Option

* Good, because the documentation is excellent
* Good, because it is heavily used and supported within the NodeJS community
* Good, because it proved simple to set up by a primarily .NET developer
* Good, because it integrates well and simply with GitHub actions
* Good, because it proved significantly faster to execute tests than SpecFlow
* Good, because it reports test results simply and effectively and will produce a video of the test run
* Good, because easy to config and run in different Environment like DEV,QA,PROD
* Good, because it runs the test without need to of selenium drivers in developers laptop
* Neutral, because it is a command line tool which requires NodeJS installed and that can put of .NET devs who prefer all tooling available in Visual Studio

## Testing

### End To End Testing

### PR Workflow
Work In Progress

### Running Locally
From a command prompt, change to the Test directory and run npm install to install the dependencies. Then run one of the following commands:

npx cypress run to run all Cypress end to end tests in a headless browser
npx cypress open to open the Cypress test runner for fully manual configuration of the test runner
npx cypress open --config baseUrl=https://my-url/ --env username=<USERNAME>,password=<PASSWORD> to open the Cypress test runner specifying a different base url and authentication credentials