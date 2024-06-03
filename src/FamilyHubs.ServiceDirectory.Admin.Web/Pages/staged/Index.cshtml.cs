﻿using System.Web;
using FamilyHubs.ServiceDirectory.Admin.Core.ApiClient;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Models;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.staged;

/*
 * system prompt:
 *
   review the user content for suitability to be shown an a GOV.UK public site.
   reply with a json object only - do not add any pre or post amble.
   the json object should be in the following format:
   {
   "ReadingLevel": 9,
   "InappropriateLanguage": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Swear words",
         "Content": "bloody stupid idiots",
       },
       { 
         "Reason": "Inappropriate slang",
         "Content": "OMFG this is fun",
       }
   ]
   },
   "Security": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "SQL injection",
         "Content": "' OR '1'='1'",
       },
       { 
         "Reason": "Cross-site scripting (XSS)",
         "Content": "<script>alert("not allowed")</script>",
       },
       { 
         "Reason": "Cross-site request forgery (CSRF)",
         "Content": "<img src='http://malicious.com/csrf'/>",
       },
       { 
         "Reason": "Sensitive data exposure",
         "Content": "The password for the admin account is 'password123'",
       }
   ]
   },
   "PoliticisedSentiment": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Negative sentiment towards conservative party",
         "Content": "We help people the Tories couldn't care less about",
       }
   ]
   },
   "PII": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Name of person not part of running the service",
         "Content": "we've helped famous alcoholics like Oliver Reed",
       }
   ]
   },
   "GrammarAndSpelling": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Spelling",
         "Content": "eggselent",
       }
   ]
   },
   "StyleViolations": {
     "Flag": true,
     "Instances": [
       { 
         "Reason": "Abbreviations and acronyms",
         "Content": "as featured on the B.B.C.",
       }
   ]
   }
}
   
   The ReadingLevel integer should be the reading age required to read and comprehend the content. Consider sentence complexity, vocabulary, content depth, paragraph length, and topic relevance.
   
InappropriateLanguage should flag whether the content contains inappropriate language.
If the flag is true, then the Instances array should contain objects with a Reason property and a Content property.
The Reason property should be a string describing why the content is inappropriate, and the Content property should be the text that is inappropriate.

Security is similar to InappropriateLanguage, but should flag whether the content contains security vulnerabilities.

PoliticisedSentiment is similar to InappropriateLanguage, but should flag whether the content contains politicised sentiment.

PII is similar to InappropriateLanguage, but should flag whether the content potentially contains personally identifiable information.
It is allowed to contain the name of a person if they are part of running the service, but not if they are a user of the service.

GrammarAndSpelling is similar to InappropriateLanguage, but should flag whether the content contains grammar or spelling mistakes.
The reason for a spelling mistake should be 'Spelling'. Grammatical mistakes should be flagged with the name of the type of grammatical mistake.
Example grammatical mistakes include:
    Its vs. It’s
    There vs. Their
    Your vs. You’re
    Affect vs. Effect
    Then vs. Than
    Lose vs. Loose
    Less vs. Fewer
    Farther vs. Further
    Complement vs. Compliment
    Principal vs. Principle
    Stationary vs. Stationery
    Elicit vs. Illicit
    Discreet vs. Discrete
    i.e. vs. e.g.
    Ensure vs. Insure
    Enquire vs. Inquire
    Practice vs. Practise
    Licence vs. License
    Adviser vs. Advisor
    Affect vs. Effect
    Compliment vs. Complement
    Principal vs. Principle
    Stationary vs. Stationery
    Elicit vs. Illicit
    Discreet vs. Discrete
    i.e. vs. e.g.
    Ensure vs. Insure
    Enquire vs. Inquire
    Practice vs. Practise
    Licence vs. License
    Adviser vs. Advisor
    Affect vs. Effect
    Compliment vs. Complement
    Principal vs. Principle
    Stationary vs. Stationery
    Elicit vs. Illicit
    Discreet vs. Discrete
    i.e. vs. e.g.
    Ensure vs. Insure
    Enquire vs. Inquire
    Practice vs. Practise
    Licence vs. License
    Adviser vs. Advisor
    Affect vs. Effect
    Compliment vs. Complement
    Principal vs. Principle
    Stationary vs. Stationery
    Elicit vs. Illicit
    Discreet vs. Discrete
    i.e. vs. e.g.
    Ensure vs. Insure
    Enquire vs. Inquire
    Practice vs. Practise
    Licence vs. License
    Adviser vs. Advisor
    Affect vs. Effect
    Compliment vs. Complement
    Principal vs. Principle
    Stationary vs. Stationery
    Elicit vs. Illicit
    Discreet vs. Discrete
    i.e. vs. e.g.
    Ensure vs. Insure
    Enquire vs. Inquire
    Practice vs. Practise
    Licence vs. License

StyleViolations is similar to InappropriateLanguage, but should flag whether the content contains style violations.
Each instance should quote the name in the Reason instance field.
The style rules are:

Name: A*, A*s
Rule: The top grade in A levels. Use the symbol * not the word ‘star’. No apostrophe in the plural.

Name: A level
Rule: No hyphen. Lower case level.

Name: Abbreviations and acronyms
Rule: The first time you use an abbreviation or acronym explain it in full on each page unless it’s well known, like UK, DVLA, US, EU, VAT and MP. This includes government departments or schemes. Then refer to it by initials, and use acronym Markdown so the full explanation is available as hover text.
   Do not use full stops in abbreviations: BBC, not B.B.C.

Name: the academies programme
Rule: Lower case.


further style rules:
todo: will either have to use a model with a big enough context, or just pick out what seem the most important rules to apply



   academy
   
   Only use upper case when referring to the name of an academy, like Mossbourne Community Academy. See also Titles.
   academy converters
   
   Lower case.
   academy order
   
   Lower case.
   academy trust
   
   Lower case.
   Access to Work
   
   Upper case when referring directly to the actual programme, otherwise use lower case.
   accountancy service provider
   
   Upper case when referring to the business area covered by Money Laundering Regulations. Do not use the acronym.
   Accounts Office
   
   Upper case.
   Activation PIN
   
   Upper case. Activation PIN has been changed to Activation Code on outgoing correspondence from the Government Gateway. Until all hard-coded instances of Activation PIN have been removed from the Online Services pages, use ‘Activation Code (also known as Activation PIN)’.
   act, act of Parliament
   
   Lower case. Only use upper case when using the full title: Planning and Compulsory Purchase Act 2004, for example.
   Active voice
   
   Use the active rather than passive voice. This will help us write concise, clear content.
   Addresses in the UK
   
   Start each part of the address on a new line. You should:
   
       write the town and postcode on separate lines
       not use commas at the end of each line
       write the country on the line after the postcode, not before
       only include a country if there is a reasonable chance that the user will be writing to the address from a different country
   
   For example:
   
   HM Revenue and Customs - Child Benefit Office
   PO Box 1
   Newcastle Upon Tyne
   NE88 1AA
   United Kingdom
   Addressing the user
   
   Address the user as ‘you’ where possible and avoid using gendered pronouns like ‘he’ and ‘she’. Content on the site often makes a direct appeal to citizens and businesses to get involved or take action: ‘You can contact HMRC by phone and email’ or ‘Pay your car tax’, for example.
   Adoption Register
   
   Upper case when referring to the national Adoption Register.
   
   Lower case in subsequent mentions that do not use the full term: the register.
   adviser
   
   For example, special adviser. Not advisor, but advisory is the correct adjective.
   ages
   
   Do not use hyphens in ages unless to avoid confusion, although it’s always best to write in a way that avoids ambiguity. For example, ‘a class of 15 16-year-old students took the A level course’ can be written as ‘15 students aged 16 took the A level course’. Use ‘aged 4 to 16 years’, not ‘4-16 years’.
   
   Avoid using ‘the over 50s’ or ‘under-18s’. Instead, make it clear who’s included: ‘aged 50 years and over’ and ‘aged 17 and under’.
   agile
   
   Upper case when referring to the Agile Manifesto and principles and processes, otherwise use lower case.
   allow list
   
   Use allow list as the noun and allow as the verb. Do not use white list or whitelist.
   al-Qa’ida
   
   Not al-Qaeda or al-Qaida.
   alternative provision
   
   Lower case.
   American and UK English
   
   Use UK English spelling and grammar. For example, use ‘organise’ not ‘organize’, ‘modelling’ not ‘modeling’, and ‘fill in a form’, not ‘fill out a form’.
   
   American proper nouns, like 4th Mechanized Brigade or Pearl Harbor, take American English spelling.
   Ampersand
   
   Use and rather than &, unless it’s a department’s logo image or a company’s name as it appears on the Companies House register.
   animal health
   
   Lower case.
   antisocial
   
   No hyphen.
   applied general qualifications
   
   Lower case.
   apprenticeship programme
   
   Lower case.
   A-road
   
   Hyphenated.
   armed forces
   
   Lower case.
   arm’s length body
   
   Apostrophe, no hyphen.
   assembly ministers
   
   Lower case.
   artificial intelligence
   
   Write first as artificial intelligence (AI) then AI throughout.
   Attendance Allowance
   
   Upper case.
   Bacs (Bankers Automated Clearing System)
   
   Acronym should come first as it’s more widely known than the full name. Please note that the acronym has changed to Bacs.
   backend
   
   Used in a technical context, not “back-end” or “back end”.
   Bank details
   
   When adding bank details:
   
       do not use a table - use bullet points and a lead-in line instead
       use spaces rather than hyphens in sort codes - 60 70 80 (not 60-70-80)
       avoid using spaces in account numbers unless they are very long (like an International Bank Account Number)
   
   For example:
   
   Transfer the fee to the following account within 5 working days of emailing your form:
   
       sort code - 80 26 50
       account number - 10014069
       account name - The Public Trustee
   
   Banned words
   
   See Words to avoid
   baseline
   
   One word, lower case.
   Behavioural Insights team
   
   Upper case if it’s a specific, named team. Always lower case for team and generic names like research team, youth offending team.
   Bereavement Payment
   
   Upper case.
   Blind Person’s Allowance
   
   Upper case.
   block list
   
   Use block list as the noun and block as the verb. Do not use black list or blacklist.
   blog post
   
   Use 2 words when referring to an article published on a blog. A ‘blog’ is the site on which a blog post is published.
   board
   
   Always lower case unless it’s part of a proper title: so upper case for the Judicial Executive Board, but lower case for the DFT’s management board.
   bold
   
   Only use bold to indicate interface elements in text that are explicitly telling the user what to do, for example:
   
       Select Start.
       Enter your information then select Done.
   
   Use inverted commas when referring to interface elements in non-instructional contexts, for example: “The ‘Done’ button will always be at the bottom of the page.”
   
   Use bold sparingly - using too much will make it difficult for users to know which parts of your content they need to pay the most attention to.
   
   Do not use bold in other situations, for example to emphasise text.
   
   To emphasise words or phrases, you can:
   
       front-load sentences
       use headings
       use bullets
   
   Brackets
   
   Use (round brackets).
   
   Do not use round brackets to refer to something that could either be singular or plural, like ‘Check which document(s) you need to send to DVLA.’
   
   Always use the plural instead, as this will cover each possibility: ‘Check which documents you need to send to DVLA.’
   
   Use [square brackets] for explanatory notes in reported speech or for placeholder text:
   
   “Thank you [Foreign Minister] Mr Smith.”
   
   “Witnessed by [signature of witness].”
   Brexit
   
   You can use the term ‘Brexit’ to provide historical context, but it’s better to use specific dates where possible. For example, use:
   
       ‘31 December 2020’ rather than ‘Brexit’ or ‘when the UK left the EU’
       ‘before 31 December 2020’ rather than ‘during the transition period’
       ‘after 1 January 2021’ rather than ‘after the transition period’
   
   Britain
   
   See Great Britain
   British citizen
   
   One of 6 types of British nationalities. See British people.
   British national
   
   See British people.
   British people
   
   Reference British nationals by their activity where possible, for example British tourists, British farmers. If you’re talking about them in the general sense, use British people.
   
   Do not use British nationals unless you need to refer to them in a legal context, for example in eligibility criteria. Do not use British citizen unless you’re referring to people with that particular type of British nationality.
   BTEC National Diploma
   
   Upper case.
   Bullet points and steps
   
   You can use bullets to make text easier to read. Make sure that:
   
       you always use a lead-in line
       you use more than one bullet
       the bullets make sense running on from the lead-in line
       you use lower case at the start of the bullet
       you do not use more than one sentence per bullet - use commas or dashes to expand on an item
       you do not put ‘or’ or ‘and’ after the bullets
       you do not make the whole bullet a link if it’s a long phrase
       you do not put a semicolon at the end of a bullet
       there is no full stop after the last bullet
   
   Bullets should normally form a complete sentence following from the lead text. But it’s sometimes necessary to add a short phrase to clarify whether all or some of the points apply. For example, ‘You can only register a pension scheme that is one of the following:’
   
   The number and type of examples in a list may lead the user to believe the list is exhaustive. This can be dealt with by:
   
       checking if there are other conditions (or if the list is actually complete)
       listing the conditions which apply to the most users and removing the rest
       consider broader terms in the list which capture more scenarios (and could make the list exhaustive)
       creating a journey to specialist content to cover the remaining conditions
   
   Steps
   
   Use numbered steps instead of bullet points to guide a user through a process. You do not need a lead-in line and you can use links and downloads (with appropriate Markdown) in steps. Steps end in a full stop because each should be a complete sentence.
   business continuity management
   
   Lower case.
   business plan
   
   Lower case. Do not use upper case even in the title of a business plan publication.
   business statement
   
   Lower case.
   C of E
   
   For Church of England when referring to school names.
   cabinet
   
   The cabinet is lower case.
   Capital Gains Tax
   
   Upper case.
   Capitalisation
   
   DO NOT USE BLOCK CAPITALS FOR LARGE AMOUNTS OF TEXT AS IT’S QUITE HARD TO READ.
   
   Always use sentence case, even in page titles and service names. The exceptions to this are proper nouns, including:
   
       departments (specific government departments - see below)
       the Civil Service, with lower case for ‘the’
       specific job titles
       titles like Mr, Mrs, Dr, the Duke of Cambridge (the duke at second mention); Pope Francis, but the pope
       Rt Hon (no full stops)
       buildings
       place names
       brand names
       faculties, departments, institutes and schools
       names of groups, directorates and organisations: Knowledge and Innovation Group
       Parliament, the House
       titles of specific acts or bills: Housing Reform Bill (but use ‘the act’ or ‘the bill’ after the first time you use the full act or bill title)
       names of specific, named government schemes known to people outside government: Right to Buy, King’s Awards for Enterprise
       specific select committees: Public Administration Select Committee
       header cells in tables: Annual profits
       titles of books (and within single quotes), for example, ‘The Study Skills Handbook’
       World War 1 and World War 2 (note caps and numbers)
   
   Do not capitalise:
   
       government - see government
       minister, never Minister, unless part of a specific job title, like Minister for the Cabinet Office
       department or ministry - never Department or Ministry, unless referring to a specific one: Ministry of Justice, for example
       white paper, green paper, command paper, House of Commons paper
       budget, autumn statement, spring statement, unless referring to and using the full name of a specific statement - for example, “2016 Budget”
       sections or schedules within specific named acts, regulations or orders
       director general (no hyphen), deputy director, director, unless in a specific job title
       group and directorate, unless referring to a specific group or directorate: the Commercial Directorate, for example
       departmental board, executive board, the board
       policy themes like sustainable communities, promoting economic growth, local enterprise zones
       general mention of select committees (but do cap specific ones - see above)
       the military
   
   Capitals for government departments
   
   Use the following conventions for government departments. A department using an ampersand in its logo image is fine but use ‘and’ when writing in full text.
   
       Attorney General’s Office (AGO)
       Cabinet Office (CO)
       Department for Business and Trade (DBT)
       Department for Culture, Media and Sport (DCMS)
       Department for Education (DfE)
       Department for Energy Security and Net Zero (DESNZ)
       Department for Environment, Food and Rural Affairs (Defra)
       Department for Levelling Up, Housing and Communities (DLUHC)
       Department for Science, Innovation and Technology (DSIT)
       Department for Transport (DfT)
       Department for Work and Pensions (DWP)
       Department of Health and Social Care (DHSC)
       Foreign, Commonwealth and Development Office (FCDO)
       HM Treasury (HMT)
       Home Office (HO)
       Ministry of Defence (MOD)
       Ministry of Justice (MOJ)
   
   care worker
   
   Two words. Lower case.
   chair of governors
   
   Lower case.
   chairman, chairwoman, chairperson
   
   Lower case in text. Upper case in titles: Spencer Tracy, Chairman, GDS.
   Change notes
   
   See change notes in the content design manual.
   changelog
   
   Not “change log”.
   CHAPS (Clearing House Automated Payment System)
   
   The acronym should come first as it’s more widely known than the full name.
   checkbox
   
   Not “check box”.
   chemical, biological, radiological and nuclear (CBRN) materials
   
   Lower case. Use upper case for the acronym.
   chief constable
   
   Lower case except where it’s a title with the holder’s name, like Chief Constable Andrew Trotter.
   Child Benefit
   
   Upper case.
   Child Tax Credit
   
   Upper case, but generic references to tax credits are lower case.
   childcare
   
   Lower case.
   Childcare Grant
   
   Upper case.
   childminder, childminding
   
   One word.
   Children in Need
   
   Upper case for the BBC fundraising event, lower case for children in need census.
   Civil Contingencies Secretariat
   
   Upper case because it’s the name of an organisation.
   Civil Service
   
   Upper case.
   civil servants
   
   Lower case.
   classwork
   
   One word.
   click
   
   Don’t use “click” when talking about user interfaces because not all users click. Use “select”.
   
   You can use “right-click” if the user needs to right-click to open up a list of options to progress through the user journey.
   coalition
   
   Lower case in all instances, including ‘the coalition’.
   CO2
   
   Use capital letters and a regular 2.
   coastguard
   
   Lower case.
   code of practice
   
   Lower case.
   command paper
   
   Lower case.
   commercial software
   
   Not “third-party software”. Also use “commercial” for types of software, for example “commercial word processor”.
   Community Care Grant
   
   Upper case.
   community resilience
   
   Lower case.
   community, voluntary and foundation schools
   
   Lower case.
   competence order
   
   Lower case unless used in the full title, like the National Assembly for Wales (Legislative Competence) (Social Welfare) Order 2008.
   Components that control other components
   
   In technical writing, use:
   
       primary for a component that controls other components
       secondary for a component that’s controlled by the primary component
   
   Do not use master or slave.
   conduct of business rules
   
   Lower case.
   Construction Industry Scheme
   
   Use upper case when referring to the actual Construction Industry Scheme (CIS, not the CIS).
   Construction Industry Scheme Online/CIS Online
   
   Upper case.
   consultation responses
   
   Lower case.
   continuous improvement
   
   Lower case.
   contractions
   
   Avoid negative contractions like can’t and don’t. Many users find them harder to read, or misread them as the opposite of what they say. Use cannot, instead of can’t.
   
   Avoid should’ve, could’ve, would’ve, they’ve too. These can also be hard to read.
   co-operation
   
   Hyphenated.
   core standards
   
   Lower case.
   Corporation Tax
   
   Upper case.
   Corporation Tax for Agents online service
   
   Upper case.
   Corporation Tax Online
   
   Use upper case Online if referring to the actual service, not if you’re describing using the service: ‘you can pay your Corporation Tax online or at the Post Office.’
   COTS
   
   Meaning “commercial-off-the-shelf software”. Not “cots” or “Cots”. Explain the acronym at first use.
   council
   
   Use lower case when writing about local councils in general. Use capitals for the official name of a local council. For example ‘Reading Borough Council’, ‘Warwick District Council’ and ‘Swanage Town Council’.
   Council Tax
   
   Upper case.
   countries and territories
   
   When referring to a country or territory, use the names listed in the country register or territory register.
   County Court
   
   Upper case as it represents a single court system.
   coursework
   
   One word.
   COVID-19
   
   Upper case.
   
   Do not use:
   
       ‘Covid-19’ with only the first letter capitalised
       ‘covid-19’ lower case
       ‘coronavirus’ as ‘COVID-19’ is the specific condition
   
   credit unions
   
   Lower case.
   critical national infrastructure
   
   Lower case.
   critical worker
   
   Lower case.
   
   Used to define workers critical to an emergency response whose children get prioritised for school attendance. It is not the same as an ‘essential worker’.
   
   Use ‘critical worker’ only in relation to educational provision.
   
   Do not use ‘keyworker’.
   cross-curricular learning
   
   Hyphenated.
   crown servants
   
   Lower case.
   curriculums
   
   Not curricula.
   customs duty
   
   Lower case.
   customs union
   
   Lower case. Only use upper case when part of the title of a specific customs union: the European Union Customs Union, for example.
   cyber bullying
   
   Two words. Lower case.
   data
   
   Treat as a singular noun: The data is stored on a secure server.
   data centre
   
   Not “datacentre”.
   data set
   
   Not “dataset”.
   data store
   
   Not “datastore”.
   Dates
   
       use upper case for months: January, February
       do not use a comma between the month and year: 4 June 2017
       when space is an issue - in tables or publication titles, for example - you can use truncated months: Jan, Feb
       we use ‘to’ in date ranges - not hyphens, en rules or em dashes. For example:
           tax year 2011 to 2012
           Monday to Friday, 9am to 5pm (put different days on a new line, do not separate with a comma)
           10 November to 21 December
       do not use quarter for dates, use the months: ‘department expenses, Jan to Mar 2013’
       when referring to today (as in a news article) include the date: ‘The minister announced today (14 June 2012) that…’
   
   Read more about dates.
   Daycare Trust
   
   Two words. Upper case.
   dedicated schools grant
   
   Lower case.
   defence
   
   Lower case even when referring to the defence team at the MOD.
   defence team
   
   Lower case.
   department
   
   Lower case except when in the title: the Department of Health and Social Care.
   devolved administrations
   
   Lower case.
   DevOps
   
   Similarly, use “WebOps”.
   diploma
   
   Lower case unless part of a title like Edexcel L2 Diploma in IT.​
   Direct Debit
   
   Upper case.
   Direct Debit Instruction
   
   Upper case.
   director
   
   Lower case in text. Upper case in titles: Spencer Tracy, Director, GDS.
   director general
   
   Lower case. No hyphen.
   Disability Living Allowance
   
   Upper case.
   disabled people
   
   Not ‘the disabled’ or ‘people with disabilities’.
   
   Read more about words to use and avoid when writing about disability.
   dispensation
   
   Lower case.
   Discretionary Housing Payment
   
   Upper case.
   Duty Deferment Electronic Statements (DDES)
   
   Upper case.
   early career teacher (ECT)
   
   Lower case.
   early years
   
   Lower case.
   early years foundation stage (EYFS)
   
   Lower case.
   early years professional status
   
   Lower case.
   early years teacher
   
   Lower case.
   early years teacher status
   
   Lower case.
   the Earth
   
   Upper case for the Earth, Planet Earth and Earth sciences, with lower case for ‘the’.
   East End (London)
   
   Upper case.
   EBacc
   
   A performance measure linked to GCSEs. Upper case E and B.
   EC Sales List (ESL)
   
   The acronym is ESL, not ECSL.
   eco-schools
   
   Hyphenated.
   education, health and care plan
   
   Lower case.
   eg, etc and ie
   
   eg can sometimes be read aloud as ‘egg’ by screen reading software. Instead use ‘for example’ or ‘such as’ or ‘like’ or ‘including’ - whichever works best in the specific context.
   
   etc can usually be avoided. Try using ‘for example’ or ‘such as’ or ‘like’ or ‘including’. Never use etc at the end of a list starting with these words.
   
   ie - used to clarify a sentence - is not always well understood. Try (re)writing sentences to avoid the need to use it. If that is not possible, use an alternative such as ‘meaning’ or ‘that is’.
   email
   
   One word.
   Email addresses
   
   Write email addresses in full, in lower case and as active links. Do not include any other words in the link text.
   emergency plan
   
   Lower case.
   Employment and Support Allowance (New Style or income-related)
   
   Upper case.
   
   Use ‘New Style Employment and Support Allowance (ESA)’ the first time the benefit name is used. From then on, you can use ‘New Style ESA’. 
   
   Use ‘income-related Employment and Support Allowance (ESA)’ the first time the benefit name is used. From then on, you can use the abbreviation as long as you put ‘income-related’ first, for example ‘income-related ESA’.
   
   You can use ‘Employment and Support Allowance (ESA)’ and the acronym ‘ESA’ if you need to refer to both benefits at the same time.
   endpoint
   
   Not “end point” in the context of APIs.
   enrol
   
   Lower case.
   enrolling
   
   Lower case.
   enrolment
   
   Lower case.
   ethnic minorities
   
   When writing about ethnicity, refer to ethnic minority groups individually, rather than as a single group. Where it’s absolutely necessary to group people from different ethnic minority backgrounds, use ‘ethnic minorities’ or ‘people from ethnic minority backgrounds.’
   
   Do not use the terms BAME (black, Asian and minority ethnic) and BME (black and minority ethnic). These terms emphasise certain ethnic minority groups (Asian and black) and exclude others (mixed, other and white ethnic minority groups).
   European Commission
   
   Leave unabbreviated to distinguish from the European Community. Write out in full at first mention, then call it the Commission.
   European Economic Area (EEA)
   
   Avoid using as it is not widely understood. Say ‘the EU, Norway, Iceland and Liechtenstein’.
   
   When rules covering the EEA also cover Switzerland, say ‘the EU, Switzerland, Norway, Iceland and Liechtenstein’.
   European Union vs European Community
   
   Use EU when you mean EU member states: EU countries, EU businesses, EU consumers, goods exported from the EU, EU VAT numbers.
   
   EC should be used when it’s EC directives, EC Sales List.
   euros, the euro
   
   Lower case, if referring to the currency.
   etc
   
   See eg, etc and ie
   Excel spreadsheet
   
   Upper case because Excel is a brand name.
   executive director
   
   Lower case in text. Upper case in titles: Spencer Tracy, Executive Director, GDS.
   Extended Project Qualification
   
   Upper case.
   extra-curricular
   
   Hyphenated
   FAQs (frequently asked questions)
   
   Do not use FAQs on GOV.UK. If you write content by starting with user needs, you will not need to use FAQs.
   
   Read more about FAQs.
   finance and procurement
   
   Lower case.
   fine
   
   Use ‘fine’ instead of ‘financial penalty’.
   
   For example, “You’ll pay a £50 fine.”
   
   For other types of sanction, say what will happen to the user - you’ll get points on your licence, go to court and so on. Only say ‘civil penalty’ if there’s evidence users are searching for the term.
   
   Describe what the user might need to do, rather than what government calls a thing.
   fire and rescue service
   
   Lower case.
   fixed-period exclusions
   
   Hyphenated.
   foot and mouth disease
   
   Lower case.
   foundation degrees
   
   Lower case.
   foundation schools
   
   Lower case.
   foundation stage / foundation subjects
   
   Lower case.
   foundation trust
   
   Lower case unless the full name of the foundation trust is being used: Salisbury NHS Foundation Trust.
   free school
   
   Lower case.
   the free schools programme
   
   Lower case.
   free school meals
   
   Lower case.
   Freedom of Information
   
   You can make a Freedom of Information (FOI) request, but not a request under the FOI Act.
   frontend
   
   Not “front-end” or “front end”.
   Full Payment Submission
   
   Upper case.
   funding agreement
   
   Lower case.
   further education (FE)
   
   Lower case.
   GCSE, GCSEs
   
   No full stops between the initials. No apostrophe in the plural.
   Gender
   
   Make sure text is gender neutral wherever possible, such as ‘them’, ‘their’ or ‘they’.
   
   If you do need to refer to gender, use ‘women’ and ‘men’ rather than ‘males’ and ‘females’. For example, ‘33% of our senior leaders are women’.
   general election
   
   Lower case, but upper case if referring to a specific election. For example, the 2019 General Election.
   Geography and regions
   
   Use lower case for north, south, east and west, except when they’re part of a name or recognised region.
   
   So, the south-west (compass direction), but the South West (administrative region).
   
   Use lower case for the north, the south of England, the south-west, north-east Scotland, south Wales, the west, western Europe, the far east, south-east Asia.
   
   Use upper case for East End, West End (London), East Midlands, West Midlands, Middle East, Central America, South America.
   
   Always write out the full name of the area the first time you use it. You can use a capital for a shortened version of a specific area or region if it’s commonly known by that name, like the Pole for the North Pole.
   GHz
   
   Not “Ghz”.
   governing body
   
   Singular noun.
   
   The governing body is meeting today. It will decide who to appoint.
   government
   
   Lower case unless it’s a full title. For example: ‘UK government’, but ‘His Majesty’s Government of the United Kingdom of Great Britain and Northern Ireland’.
   
   Also ‘Welsh Government’, as it’s the full title.
   government offices
   
   Lower case.
   government procurement card
   
   Lower case.
   governor
   
   Lower case.
   GOV.UK
   
   All upper case.
   GOV.UK One Login
   
   Title case. Always use the full name, GOV.UK One Login. Not ‘One Login’, ‘login’ or acronyms.
   
   Do not refer to GOV.UK One Login as an account. This helps avoid confusion with other government accounts.
   
   For signing in to a service use ‘Sign in with GOV.UK One Login’.
   
   For signing in to your GOV.UK One Login, use ‘Sign in to your GOV.UK One Login’.
   
   Use ‘sign in details’ not ‘GOV.UK One Login details’ to refer to the information you sign in with, for example your email address and password.
   
   See also sign in or log in
   grammar school
   
   Lower case unless part of a school name: The Manchester Grammar School.
   Great Britain
   
   Refers only to England, Scotland and Wales and does not include Northern Ireland.
   
   Use ‘Great Britain (England, Scotland and Wales)’ in the first instance. Where possible, you should also make a specific point of saying that Northern Ireland is not included.
   
   For example ‘These rules apply to Great Britain (England, Scotland and Wales). This does not include Northern Ireland.’
   
   Use ‘Great Britain’ in subsequent mentions on the page.
   Britain
   
   Use UK and United Kingdom in preference to Britain and British (UK business, UK foreign policy, ambassador and high commissioner). But British embassy, not UK embassy.
   Green Deal
   
   Upper case because it’s the name of a programme, but note that it’s Green Deal programme, Green Deal team, Green Deal assessment.
   green paper
   
   Lower case.
   Group
   
   Upper case for names of groups, directorates and organisations: Knowledge and Innovation Group.
   
   Lower case when a group has a very generic title like working group or research team.
   Guardian’s Allowance
   
   Upper case.
   guidance
   
   Lower case: national recovery guidance.
   Gypsies
   
   Upper case because Gypsies are legally recognised as an ethnic group.
   harbour authority
   
   Lower case unless part of a proper noun: Cardiff Harbour Authority.
   harbour master
   
   Lower case.
   hazardous waste registration
   
   Lower case.
   headteacher
   
   One word. You can use head if the context is clear.
   health protection unit
   
   Lower case unless it’s the title of an organisation: North East and Central London Health Protection Unit.
   helpdesk
   
   Not “help desk”.
   high-attaining pupils
   
   Hyphenated.
   higher education (HE)
   
   Lower case.
   Holocaust
   
   Upper case.
   home-school agreement
   
   Hyphenated.
   homepage
   
   Lower case.
   HTTPS
   
   Upper case. No need to explain the acronym if it’s used in content for a technical audience.
   human resources
   
   Lower case.
   Hurricane
   
   Upper case for named hurricanes: Hurricane Katrina, Hurricane Sandy.
   Hyphenation
   
   Hyphenate:
   
       re- words starting with e, like re-evaluate
       co-ordinate
       co-operate
   
   Do not hyphenate:
   
       reuse
       reinvent
       reorder
       reopen
       email
   
   Do not use a hyphen unless it’s confusing without it, for example, a little used-car is different from a little-used car. You can also refer to The Guardian style guide for advice on hyphenation.
   
   Use ‘to’ for time and date ranges, not hyphens.
   IaaS
   
   Stands for “Infrastructure as a Service”. Explain the acronym at first use.
   ID
   
   In technical writing, don’t write ‘identification’ or ‘identifier’, unless it’s part of a standard abbreviation. For example, ‘unique identifier (UID)’.
   ie
   
   See eg, etc and ie
   Import Control System
   
   Upper case.
   implementation period
   
   Always lower case.
   inclusion statement
   
   Lower case.
   Income Support
   
   All names of benefits are upper case.
   Income Tax
   
   Names of taxes are upper case, except input tax.
   independent schools adjudicator
   
   Lower case.
   individual education plan
   
   Lower case.
   individual schools budget
   
   Lower case.
   initial teacher training
   
   Lower case.
   input tax
   
   Lower case.
   inset day
   
   Lower case.
   instrument of government
   
   Lower case.
   International Baccalaureate
   
   Upper case.
   internet
   
   Lower case.
   Intrastat Supplementary Declaration
   
   Upper case.
   IP
   
   When used in the technical context (for example ‘internet protocol’), there’s no need to explain the acronym.
   Italics
   
   Do not use italics. Use ‘single quotation marks’ if referring to a document, scheme or initiative.
   Job titles
   
   Specific job titles and ministers’ role titles are upper case: Minister for Housing, Home Secretary.
   
   Generic job titles and ministers’ role titles are lower case: director, minister.
   
   See also Shadow job titles
   Jobseeker’s Allowance (New Style or income-based)
   
   Upper case. Always use the apostrophe before the ‘s’.
   
   Use ‘New Style Jobseeker’s Allowance (JSA)’ the first time the benefit name is used. From then on, you can use ‘New Style JSA’.
   
   Use ‘income-based Jobseeker’s Allowance (JSA)’ the first time the benefit name is used. From then on, you can use the abbreviation as long as you put ‘income-based’ first, for example ‘income-based JSA’.
   
   You can use ‘Jobseeker’s Allowance (JSA)’ and the acronym ‘JSA’ if you need to refer to both benefits at the same time.
   kanban
   
   Upper case when referring to The Kanban Method, otherwise lower case.
   key stage
   
   Lower case and numeral: key stage 4.
   the King
   
   Upper case K, lower case t.
   law
   
   Lower case even when it’s ‘the law’.
   legal aid
   
   Lower case.
   Legal content
   
   Legal content can still be written in plain English. It’s important that users understand content and that we present complicated information simply.
   
   If you have to publish legal jargon, it will be a publication so write a plain English summary.
   
   Where evidence shows there’s a clear user need for including a legal term (like bona vacantia), always explain it in plain English.
   
   Read more about writing legal content
   legislative competence order
   
   Upper case if used as the full title: the National Assembly for Wales (Legislative Competence) (Social Welfare) Order 2008.
   
   Lower case otherwise: the legislative competence orders (LCOs) are approved, rejected or withdrawn.
   liaison officers
   
   Lower case.
   life cycle
   
   Not “lifecycle” or “life-cycle”.
   Links
   
   Front-load your link text with the relevant terms and make them active and specific. Always link to online services first. Offer offline alternatives afterwards, when possible.
   
   Learn more about links.
   Lists
   
   Lists should be bulleted to make them easier to read. See bullets and steps.
   
   Very long lists can be written as a paragraph with a lead-in sentence if it looks better: ‘The following countries are in the EU: Spain, France, Italy…’
   
   In an alphanumeric list:
   
       put entries that start with numbers before entries that start with letters
       order the numbers numerically in the correct order for the whole number
   
   local authority
   
   Lower case. Do not use LA.
   
   When referring to local government, use local council instead of local authority where possible. See also council.
   Local Authority Trading Standards Services
   
   Upper case as long as it’s a specific named organisation, not trading standards services in general.
   local council
   
   Lower case.
   
   When referring to local government, use local council instead of local authority where possible. See also council.
   log book
   
   Two words.
   log in
   
   See sign in or log in and GOV.UK One Login.
   looked-after children
   
   Hyphenated.
   lottery
   
   Always use the National Lottery if that’s what you mean.
   lunchtime
   
   One word.
   Machine Games Duty (MGD)
   
   Upper case.
   Machine Games Duty for Agents online service
   
   Upper case.
   mainstream schools
   
   Lower case.
   maintained schools, maintained nursery schools
   
   Lower case.
   mark scheme, mark sheet
   
   Lower case.
   Maths content
   
   Use a minus sign for negative numbers: –6
   
   Ratios have no space either side of the colon: 5:12
   
   One space each side of symbols: +, –, ×, ÷ and = (so: 2 + 2 = 4)
   
   Use the minus sign for subtraction. Use the correct symbol for the multiplication sign (×), not the letter x.
   
   Write out and hyphenate fractions: two-thirds, three-quarters.
   
   Write out decimal fractions as numerals. Use the same number format for a sequence: 0.75 and 0.45
   MD5
   
   Used in a technical context there’s no need to explain the acronym.
   Measurements
   
   Use numerals and spell out measurements at first mention.
   
   Do not use a space between the numeral and abbreviated measurement: 3,500kg not 3,500 kg.
   
   Abbreviating kilograms to kg is fine - you do not need to spell it out.
   
   Use ‘grams’ (not ‘grammes’). For example: micrograms, milligrams.
   
   If the measurement is more than one word, like kilometres per hour, then spell it out the first time it’s used with the abbreviation. From then on, abbreviate. If it’s only mentioned once, do not abbreviate.
   
   Use Celsius for temperature: 37°C
   member states of the EU
   
   Lower case.
   memorandum of understanding
   
   Lower case.
   metadata
   
   Not “meta data”.
   metaphors
   
   See words to avoid
   MHz
   
   Not “Mhz”.
   Middle East
   
   Upper case.
   middle-deemed primary school, middle-deemed secondary school
   
   Hyphenated.
   Midlands
   
   Upper case.
   migrate
   
   When talking about software, not “migrate over”.
   Mileage Allowance Payments
   
   Upper case.
   military
   
   Lower case.
   Millions
   
   Always use million in money (and billion): £138 million.
   
   Use millions in phrases: millions of people.
   
   But do not use £0.xx million for amounts less than £1 million.
   
   Do not abbreviate million to m.
   minister
   
   Use upper case for the full title, like Minister for Overseas Development, or when used with a name, as a title, like Health Minister Norman Lamb.
   
   When used without the name, shortened titles are lower case: The health minister welcomed the research team.
   MIT License
   
   Note the spelling.
   mixed-age class
   
   Hyphenated.
   mixed-sex schools
   
   Hyphenated.
   MLA
   
   Do not use Member of the Legislative Assembly (Northern Ireland), just MLA.
   modern foreign languages
   
   Lower case.
   money
   
   Use the £ symbol: £75
   
   Do not use decimals unless pence are included: £75.50 but not £75.00
   
   Do not use ‘£0.xx million’ for amounts less than £1 million.
   
   Write out pence in full: calls will cost 4 pence per minute from a landline.
   
   Currencies are lower case.
   money laundering
   
   Lower case when referring to the activity not the regulation.
   Months
   
   See Dates.
   MP
   
   Do not use Member of Parliament, just MP.
   MS
   
   Do not use Member of the Senedd (Wales), just MS.
   MSP
   
   Do not use Member of the Scottish Parliament, just MSP.
   multi-academy trust
   
   Hyphenated.
   multidisciplinary
   
   One word.
   multi-ethnic
   
   Hyphenated.
   multi-year funding
   
   Hyphenated.
   multilingual
   
   One word.
   N/A
   
   Separate with a slash. Only use in tables.
   national curriculum
   
   Lower case.
   national curriculum tests
   
   Do not call them SATs.
   National Insurance card
   
   Upper case.
   National Insurance contributions
   
   Upper case.
   National Insurance number
   
   Upper case. Not NINO.
   National Living Wage
   
   Upper case.
   National Minimum Wage
   
   Upper case.
   national occupational standards
   
   Lower case.
   national pupil database
   
   Lower case.
   national scholarship fund
   
   Lower case.
   NATO (North Atlantic Treaty Organisation)
   
   Upper case.
   .NET
   
   For the programming language, not “.net” or “.Net”.
   New Computerised Transit System (NCTS)
   
   Upper case.
   New Export System (NES)
   
   Upper case.
   newly qualified teacher
   
   Use ‘early career teacher (ECT)’ instead.
   non-executive director
   
   Lower case in text, upper case in titles: Spencer Tracy, Non-executive Director, GDS.
   the north, the north of England
   
   Lower case.
   north-east, north-west
   
   Lower case, hyphenated.
   Northern Ireland Assembly
   
   Upper case.
   Northern Ireland Civil Service
   
   Upper case.
   Northern Ireland Executive
   
   Upper case.
   north Wales
   
   Not a specific region of the UK.
   Nuclear Decommissioning Authority
   
   Upper case.
   Numbers
   
   Use ‘one’ unless you’re talking about a step, a point in a list or another situation where using the numeral makes more sense: ‘in point 1 of the design instructions’, for example. Or this:
   
   You’ll be shown 14 clips that feature everyday road scenes.
   
   There will be:
   
       1 developing hazard in 13 clips
       2 developing hazards in the other clip
   
   Write all other numbers in numerals (including 2 to 9) except where it’s part of a common expression like ‘one or two of them’ where numerals would look strange.
   
   If a number starts a sentence, write it out in full (Thirty-four, for example) except where it starts a title or subheading.
   
   For numerals over 999 - insert a comma for clarity: 9,000
   
   Spell out common fractions like one-half.
   
   Use a % sign for percentages: 50%
   
   Use a 0 where there’s no digit before the decimal point.
   
   Use ‘500 to 900’ and not ‘500-900’ (except in tables).
   
   Use MB for anything over 1MB: 4MB not 4096KB.
   
   Use KB for anything under 1MB: 569KB not 0.55MB.
   
   Keep it as accurate as possible and up to 2 decimal places: 4.03MB.
   
   Addresses: use ‘to’ in address ranges: 49 to 53 Cherry Street.
   Ordinal numbers
   
   Spell out first to ninth. After that use 10th, 11th and so on.
   
   In tables, use numerals throughout.
   nursery school
   
   Lower case.
   occupational pension
   
   Lower case. This term covers both company and public sector pension schemes. Only use this term if explaining tax rules that are specific to occupational pension schemes.
   Ofsted judgements
   
   Lower case and not in inverted commas: Westminster School was judged outstanding in its latest Ofsted inspection.
   
   There are 4 Ofsted grades:
   
       outstanding (or grade 1)
       good (or grade 2)
       requires improvement (or grade 3)
       inadequate (or grade 4)
   
   one-year-on
   
   If used adjectivally, hyphenate and use one rather than 1.
   online
   
   One word.
   online services
   
   Lower case if the service name starts with a verb - write the sentence so the user knows what action they can take. For example: You can visit someone in prison by booking online.
   
   Only use upper case if the name of the service you’re referring to contains named thing. For example: You can apply for Marriage Allowance.
   open source software
   
   Not “Open Source software” or “OS software”.
   opposition
   
   Lower case even for the opposition and opposition leader.
   or
   
   Do not use slashes instead of “or”. For example, “Do this 3/4 times”.
   order
   
   Lower case unless used as the full title: Standing Order 22
   Organisations
   
   Use the singular verb form when referring to organisations by name. Use ‘they’ when replacing an organisation name with a pronoun.
   
   For example: ‘HMPO is the sole issuer of UK passports. They will send your new passport within 3 weeks’
   
   The definite article can be used when referring to the organisation by its full name, but should not be used with the organisation’s acronym: ‘You should contact the Driver and Vehicle Standards Agency if…’ but ‘You should contact DVSA if…’
   
   You should only use ‘we’ if it’s clear which organisation you’re referring to.
   
   Read more about when to use ‘we’ in content.
   
   Use local council, instead of local authority, where possible. See also council.
   overseas-trained teacher
   
   Lower case. Hyphenated.
   PaaS
   
   Stands for “Platform as a Service”. Explain the acronym at first use.
   Paper B
   
   In national curriculum tests.
   Parliament
   
   Upper case.
   Parliamentary committees
   
   Parliamentary is upper case and committees is in lower case.
   Parliamentary report
   
   Parliamentary is upper case and report is in lower case.
   Patent Box
   
   When referring to the product/relief/regime, then say the Patent Box. Occasionally the definite article will be dropped, for example in calculations, where we use ‘Patent Box deduction’ and when using phrases like ‘Answers to your Patent Box questions’.
   pathfinder
   
   Lower case.
   payroll
   
   Lower case.
   PAYE/CIS for Agents online service
   
   Upper case.
   PAYE Coding Notice
   
   Upper case.
   PAYE Online for employers
   
   This can be abbreviated to PAYE Online within the PAYE Online for employers area of the website.
   PAYE Settlement Agreements (PSAs)
   
   Upper case.
   PDF
   
   Upper case. No need to explain the acronym.
   penalty
   
   See the entry for ‘fine’.
   pension provider
   
   Lower case. Not pension payer.
   Pension Schemes for administrators
   
   Lower case on administrators.
   Pension Schemes for practitioners
   
   Lower case on practitioners.
   Per cent
   
   Use per cent not percent. Percentage is one word. Always use % with a number.
   performance management
   
   Lower case.
   performance tables
   
   Lower case.
   performance-related pay
   
   Hyphenated.
   Personal Independence Payment
   
   Upper case
   physical education or PE
   
   You can write in full or use the initials.
   plain English
   
   Lower case plain and upper case English unless in a title: the Plain English Campaign.
   
   All content on GOV.UK should be written in plain English. You should also make sure you use language your audience will understand - check which words you should avoid.
   Planet Earth
   
   Upper case.
   police
   
   Lower case, even when referring to ‘the police’.
   police service
   
   Lower case. You can use police force when referring to a regional police body.
   policy note
   
   Lower case.
   policy statement
   
   Lower case.
   PowerPoint presentation
   
   Upper case because PowerPoint is a brand name.
   pre-school
   
   Hyphenated.
   Primary Care Trust (PCT)
   
   Upper case because it’s the name of an organisation.
   Prime Minister
   
   Use Prime Minister Rishi Sunak and the Prime Minister.
   priority school building programme
   
   Lower case.
   Private Member’s Bill
   
   Upper case.
   probate/grant of probate
   
   Lower case.
   probation trust
   
   Lower case unless in a title: Hampshire Probation Trust.
   Proforma
   
   Do not use proforma - say what it is in plain English: a template or form, for example. Be specific about what to do with it.
   programme
   
   Lower case: Troubled Families programme, Sure Start programme.
   Progress 8 measure
   
   Upper case P, lower case m.
   public health
   
   Lower case.
   public sector
   
   Lower case.
   pull request
   
   Lowercase, the same as GitHub does in its documentation. GitLab uses the term “merge request”.
   pupil premium
   
   Lower case.
   pupil referral unit
   
   Lower case.
   qualified teacher status
   
   Lower case.
   Quotes and speech marks
   
   In long passages of speech, open quotes for every new paragraph, but close quotes only at the end of the final paragraph.
   Single quotes
   
   Use single quotes:
   
       in headlines
       for unusual terms - only for the first mention
       when referring to words
       when referring to publications
       when referring to notifications such as emails or alerts
   
   For example: Download the publication ‘Understanding Capital Gains Tax’ (PDF, 360KB).
   Double quotes
   
   Use double quotes in body text for direct quotations.
   Block quotes
   
   Use the block quote Markdown for quotes longer than a few sentences.
   Real Time Information and RTI
   
   This is an HMRC programme and should only appear either with initial capitals or as an acronym when referring to the programme itself.
   
   When describing customer processes, use common language phrases like ‘send your payroll information to HMRC’ or ‘operate your payroll in real time’. Do not say ‘send your payroll under RTI’ or use the acronym, for example ‘in RTI’ or ‘under RTI’.
   
   When using real time information in any other sense, it should be lower case.
   Rebated Oils Enquiry Service
   
   Upper case.
   recovery structures
   
   Lower case.
   Reduced Earnings Allowance
   
   Upper case.
   References
   
   References should be easy to understand by anyone, not just specialists.
   
   They should follow the style guide. When writing a reference:
   
       do not use italics
       use single quote marks around titles
       write out abbreviations in full: page not p, Nutrition Journal not Nutr J.
       use plain English, for example use ‘and others’ not ‘et al’
       do not use full stops after initials or at the end of the reference
   
   If the reference is available online, make the title a link and include the date you accessed the online version:
   
   Corallo AN and others. ‘A systematic review of medical practice variation in OECD countries’ Health Policy 2014: volume 114, pages 5-14 (viewed on 18 November 2014)
   reform plan
   
   Lower case.
   regional resilience team
   
   Lower case.
   Registered Dealers in Controlled Oils (RDCO)
   
   Upper case.
   regulations
   
   Upper case in the full title: Licensing of Animal Dealers (Scotland) Regulations 2009. (No comma before the date.) Lower case when referring to them: the licensing of animal dealers regulations.
   religious education
   
   Lower case.
   resilience
   
   Lower case.
   resilience plans
   
   Lower case.
   RESTful
   
   In the context of APIs, not “restful” or “Restful”.
   risk assessment
   
   Lower case.
   risk management
   
   Lower case.
   the Royal Household
   
   Upper case when referring to the departments that, collectively, support the British Royal Family.
   Rt Hon
   
   No full stops.
   SaaS
   
   Stands for “Software as a Service”. Explain the acronym at first use.
   same-sex schools
   
   Hyphenated.
   sat nav
   
   Two words, lower case.
   SATs
   
   See national curriculum tests.
   School Admissions Code
   
   Upper case. After the first mention you can refer to it in lower case: the admissions code or the code.
   school and college performance tables
   
   Lower case.
   school improvement plan
   
   Lower case.
   school subjects
   
   Lower case for all except languages and initialisations.
   schools workforce
   
   No apostrophe as it’s an attributive noun.
   schoolwork
   
   One word.
   science and technical advice cell
   
   Lower case.
   Scientific names
   
   Capitalise the first letter of the first part of the scientific name. Do not use italics.
   Scottish Government
   
   Upper case.
   Scottish Parliament
   
   Upper case.
   Scrum
   
   Upper case when referring to the framework and method for developing products, otherwise use lower case.
   seasons
   
   spring, summer, autumn, winter are lower case.
   Secretary of State for XXX
   
   The Secretary of State for XXX is upper case whether or not it’s used with the holder’s name because there is only one. Use common sense to capitalise shortened versions of the SoS titles such as Health Secretary. The rule for ministers is different because there is more than one.
   section 2
   
   As in part of an act or a strategy.
   sector resilience plans
   
   Lower case.
   Security classifications
   
   Official, Secret, Top Secret
   
   Upper case when referring to government security classifications, otherwise lower case.
   
   If it’s not clear from the context, you may need to clarify that it’s a classification not a general description: ‘information classified as Official’ rather than ‘Official information’.
   self-assessment
   
   This compound noun should be hyphenated, unless it’s an HMRC title.
   Self Assessment for Agents online service
   
   Upper case.
   Self Assessment Online
   
   Upper case.
   Self Assessment Online for partnerships
   
   Upper case.
   Self Assessment Online for trusts
   
   Upper case.
   Self Assessment tax return
   
   See tax returns.
   self-driving vehicle
   
   Hyphenated. Use self-driving vehicle not automated vehicle.
   self-employment
   
   Hyphenate this noun.
   semicolons
   
   Do not use semicolons as they are often mis-read. Long sentences using semicolons should be broken up into separate sentences instead.
   Senedd Cymru (Welsh Parliament)
   
   Upper case. Write in full the first time you use it, then use the Senedd.
   
   This is the parliament and should not be confused with the Welsh Government.
   Sentence length
   
   Do not use long sentences. Check sentences with more than 25 words to see if you can split them to make them clearer.
   
   Read more about short sentences.
   serious case review
   
   Lower case when written in full.
   service children
   
   Recognised term for children whose parents serve in the armed forces.
   services
   
   In military contexts this should be lower case, even when referring to the armed forces services or the services.
   
   It can be upper or lower case for other contexts (for example, Pension Service, HM Courts and Tribunals Service, customer service).
   settlor
   
   A settler of trusts.
   Shadow job titles
   
   The Shadow Secretary of State for XXX is upper case whether or not it’s used with the holder’s name because there is only one. Use common sense to capitalise shortened versions of the Secretary of State titles: the Shadow Health Secretary.
   
   See also Job titles
   Shadow Cabinet
   
   Upper case.
   sign in or log in
   
   Use sign in rather than log in (verb) for calls-to-action where users enter their details to access a service.
   
   Do not use login as a noun - say what the user actually needs to enter (like username, password, National Insurance number). You can use it as a noun if it’s part of a name such as GOV.UK One Login or NHS login.
   16 to 19 Bursary Fund
   
   Upper case. After the first mention you can refer to it in lower case: the fund.
   sixth former
   
   Not hyphenated.
   sixth-form college
   
   Hyphenated. Lower case.
   SMEs
   
   This acronym means small and medium-sized enterprises. Use SME for the singular.
   south, the south of England
   
   Lower case.
   south-east, south-west
   
   Lower case, hyphenated.
   spaces
   
   One space after a full stop, not 2.
   special educational needs/special educational needs and disabilities (SEN/D)
   
   Lower case, but use upper case for the acronym.
   Special Educational Needs Code of Practice
   
   Upper case. When not using the full title in subsequent mentions, refer to it in lower case: the code of practice or the code.
   special measures
   
   Lower case.
   Speech marks
   
   See ‘Quotes and speech marks’
   Spending Review
   
   Upper case for the 5-year view of the government’s spending plans. Lower case in other contexts: we are conducting a spending review.
   Stamp Taxes for Agents online service
   
   Upper case.
   Stamp Taxes Online
   
   Upper case.
   standards of conduct
   
   Lower case.
   standing order
   
   Lower case unless used as the full title: Standing Order 22.
   State Pension
   
   Upper case.
   statement of SEND
   
   Lower case.
   statistical first release
   
   Lower case.
   Statistics
   
   Read Style.ONS to find out how to write about statistics. This has been produced by the Office for National Statistics for all members of the Government Statistical Service.
   
   Upper case National Statistics for the official statistics quality mark. Lower case for anything else, including statistics that are national in scope.
   Statutory Adoption Pay
   
   Upper case.
   Statutory Maternity Pay
   
   Upper case.
   Statutory Sick Pay
   
   Upper case.
   steps
   
   See Bullet points and steps
   strategic national framework on XXX
   
   Lower case.
   strategic partners
   
   Not a title.
   strategy
   
   Lower case. Do not capitalise a named strategy: national health and welfare strategy.
   studio school
   
   Lower case.
   study programme
   
   Lower case.
   subdomain
   
   Not “sub domain” or “sub-domain”.
   Summaries
   
   Summaries should:
   
       be 160 characters or less
       end with a full stop
       not repeat the title or body text
       be clear and specific
   
   summary of consultation responses
   
   All lower case.
   summer school
   
   Lower case.
   Sure Start programme
   
   Upper case because it’s the name of a programme, but programme is lower case.
   T Level
   
   No hyphen. Upper case for ‘Level’.
   tax credits
   
   Lower case and plural. Working Tax Credit and Child Tax Credit are specific benefits, so are upper case and singular.
   tax returns
   
   Upper case when referring to proper titles for the first time: Company Tax Return, Partnership Tax Return, Employer Annual Return.
   
   Use Self Assessment tax return at first mention, as it’s not a proper title.
   
   After that refer to them in full, or if it’s clear what you’re referring to, simply as a return. General references to tax returns are lower case.
   
   When referring to the legal requirement we use deliver or file the return. Online, we say submit the return. For Self Assessment (paper or online) use send or file the return. Send is better.
   the teachers’ standards
   
   Lower case.
   teaching school
   
   Lower case.
   team
   
   Lower case: youth offending team, Behavioural Insights team.
   teamwork
   
   Lower case. One word.
   tech levels
   
   Lower case. The name given to the occupational qualifications endorsed by employers and trade associations.
   technical level qualifications
   
   Lower case.
   TechBacc
   
   A performance measure of level 3 vocational qualifications.
   technical terms
   
   Use technical terms where you need to. They’re not jargon. You just need to explain what they mean the first time you use them.
   
   Read more about writing for specialists.
   Telephone numbers
   
   Use Telephone: 011 111 111 or Mobile: - not Mob:.
   
   Use spaces between city and local exchange. Here are the different formats to use:
   
   01273 800 900
   
   020 7450 4000
   
   0800 890 567
   
   07771 900 900
   
   077718 300 300
   
   +44 (0)20 7450 4000
   
   +39 1 33 45 70 90
   
   When a number is memorable, group the numbers into easily remembered units: 0800 80 70 60.
   Temperature
   
   Use Celsius: 37°C
   threshold assessment
   
   Lower case.
   Times
   
       use ‘to’ in time ranges, not hyphens, en rules or em dashes: 10am to 11am (not 10-11am)
       5:30pm (not 1730hrs)
       midnight (not 00:00)
       midday (not 12 noon, noon or 12pm)
       6 hours 30 minutes
   
   Midnight is the first minute of the day, not the last. You should consider using “11:59pm” to avoid confusion about a single, specific time.
   
   For example, “You must register by 11:59pm on Tuesday 14 June.” can only be read one way, but “You must register by midnight on Tuesday 14 June” can be read in two ways (the end of Monday 13, or end of Tuesday 14).
   Tied Oils Enquiry Service
   
   Upper case.
   Titles
   
   Page titles should:
   
       be 65 characters or less
       be unique, clear and descriptive
       be front-loaded and optimised for search
       use a colon to break up longer titles
       not contain dashes or slashes
       not have a full stop at the end
       not be questions
       not use acronyms unless they are well-known, like EU
   
   Trade marks
   
   Avoid using trademarked names where possible - so tablet not iPAD.
   
   Trade mark is 2 words but trademarked is one word.
   Trading Standards
   
   Upper case.
   training schools
   
   Lower case.
   transition period
   
   The period of time between 1 February and 31 December 2020 during which the UK and EU are negotiating their future relationship. Not ‘transition phase’, ‘implementation phase’ or ‘implementation period’.
   Travellers
   
   Upper case because Irish Travellers are legally recognised as an ethnic group. New age travellers is lower case.
   Trust or Company Service Provider
   
   When used to refer to the business area covered by Money Laundering Regulations.
   trust school
   
   Lower case.
   Twitter account
   
   Upper case. Twitter is a trademarked name.
   two-factor authentication
   
   Shorten as “2FA”. Do not confuse with “multi-factor authentication”.
   UK government
   
   Never HM government.
   umbrella trust
   
   Lower case.
   underachiever
   
   One word.
   underperforming
   
   One word.
   under-declared
   
   Hyphenated.
   union (the)
   
   If using “the union” to refer to the United Kingdom, use lower case.
   unique pupil number
   
   Lower case.
   Universal Credit
   
   Upper case.
   university technical college
   
   Lower case.
   URL
   
   Upper case. No need to explain the acronym.
   user ID
   
   Lower case ‘user’.
   USA
   
   Upper case. Not ‘US’.
   username
   
   Not “user name”.
   VAT for Agents online service
   VAT EC Sales List (ECSL)
   VAT EU Refunds
   VAT EU Refunds for Agents online service
   VAT on e-Services
   VAT Online
   VAT online services
   
   Used when referring to all the online services for VAT.
   VAT-registered
   
   Hyphenated when used as a compound adjective: VAT-registered business.
   VAT registration number
   
   Lower case, except when it refers to a field within a form.
   VAT Registration Online
   
   Upper case.
   VAT registration threshold
   
   Lower case.
   VAT Return
   
   Always use VAT Return unless it’s very clear from the context which return you’re referring to (as in ‘How to submit your return’ within a guide on VAT Returns).
   VAT Reverse Charge Sales List (RCSL)
   
   Upper case.
   voluntary-aided schools, voluntary-controlled schools
   
   Hyphenated. Lower case.
   VPN
   
   Upper case. No need to explain the acronym. When describing a VPN that is always on, write it like this: ‘always-on’ VPN. Note the single quotes and hyphen.
   walkaround
   
   When it’s the daily check that lorry and bus drivers do, it’s one word - a vehicle walkaround.
   webchat
   
   One word. Not ‘web chat’.
   webpage
   
   One word.
   web server
   
   Not “webserver”.
   Welsh Government
   
   Title case because it’s the full, official title.
   Welsh exotic animal disease contingency plan
   
   Lower case. This is not a proper title.
   Welsh Parliament
   
   See Senedd Cymru (Welsh Parliament).
   the west, western Europe
   
   Lower case.
   West End (London)
   
   Upper case.
   WhatsApp
   
   Use ‘WhatsApp’ with an upper case A. Do not use ‘Whatsapp’.
   white paper
   
   Lower case.
   Widowed Parent’s Allowance
   
   Upper case.
   wifi
   
   Lower case, no hyphen.
   Withdrawal Agreement
   
   Use ‘Withdrawal Agreement’ if you’re referring to the legal document.
   
   Do not refer to the withdrawal agreement to let users know if:
   
       they fall into a particular group
       a rule applies to them
   
   Instead, refer to things which allow a user to understand which group they fall into - for example, if they were living in an EU country before 1 January 2021.
   
   Do not link to further information about the withdrawal agreement from guidance content.
   Word document
   
   Upper case, because it’s a brand name.
   Words to avoid
   
   Plain English is mandatory for all of GOV.UK so avoid using these words:
   
       agenda (unless it’s for a meeting), use ‘plan’ instead
       advance, use ‘improve’ or something more specific
       collaborate, use ‘work with’
       combat (unless military), use ‘solve’, ‘fix’ or something more specific
       commit/pledge, use ‘plan to x’, or ‘we’re going to x’ where ‘x’ is a specific verb
       counter, use ‘prevent’ or try to rephrase a solution to a problem
       deliver, use ‘make’, ‘create’, ‘provide’ or a more specific term (pizzas, post and services are delivered - not abstract concepts like improvements)
       deploy (unless it’s military or software), use ‘use’ or if putting something somewhere use ‘build’, ‘create’ or ‘put into place’
       dialogue, use ‘spoke to’ or ‘discussion’
       disincentivise, use ‘discourage’ or ‘deter’
       empower, use ‘allow’ or ‘give permission’
       facilitate, say something specific about how you’re helping - for example, use ‘run’ if talking about a workshop
       focus, use ‘work on’ or ‘concentrate on’
       foster (unless it’s children), use ‘encourage’ or ‘help’
       impact (unless talking about a collision), use ‘have an effect on’ or ‘influence’
       incentivise, use ‘encourage’ or ‘motivate’
       initiate, use ‘start’ or ‘begin’
       key (unless it unlocks something), usually not needed but can use ‘important’ or ‘significant’
       land (unless you’re talking about aircraft), depending on context, use ‘get’ or ‘achieve’
       leverage (unless in the financial sense), use ‘influence’ or ‘use’
       liaise, use ‘work with’ or ‘work alongside’
       overarching, usually superfluous but can use ‘encompassing’
       progress, use ‘work on’ or ‘develop’ or ‘make progress’
       promote (unless talking about an ad campaign or career advancement), use ‘recommend’ or ‘support’
       robust (unless talking about a sturdy object), depending on context, use ‘well thought out’ or ‘comprehensive’
       slim down (unless talking about one’s waistline), use ‘make smaller’ or ‘reduce the size’
       streamline, use ‘simplify’ or ‘remove unnecessary administration’
       strengthening (unless it’s strengthening bridges or other structures), depending on context, use ‘increasing funding’ or ‘concentrating on’ or ‘adding more staff’
       tackle (unless talking about fishing tackle or a physical tackle, like in rugby), use ‘stop’, ‘solve’ or ‘deal with’
       transform, describe what you’re doing to change the thing
       utilise, use ‘use’
   
   Avoid using metaphors - they do not say what you actually mean and lead to slower comprehension of your content. For example:
   
       drive, use ‘create’, ‘cause’ or ‘encourage’ instead (you can only drive vehicles, not schemes or people)
       drive out (unless it’s cattle), use ‘stop’, ‘avoid’ or ‘prevent’
       going/moving forward, use ‘from now on’ or ‘in the future’ (it’s unlikely we are giving travel directions)
       in order to, usually not needed - do not use it
       one-stop shop, use ‘website’ (we are government, not a retail outlet)
       ring fencing, use ‘separate’ or when talking about budgets use ‘money that will be spent on x’
   
   With all of these words you can generally replace them by breaking the term into what you’re actually doing. Be open and specific.
   
   Read more about plain English and words to avoid.
   Working Tax Credit
   
   Upper case, but generic references to tax credits are lower case.
   World War 1, World War 2
   
   Upper case and numbers.
   written ministerial statement, written statement
   
   Lower case.
   year 1, year 2
   
   Lower case.
   zero-hours contract
   
   Not “zero-hour contract” or “zero hours contract”.
    
 
 *
 *
 */

public record RowData(long Id, string Name);

public class Row : IRow<RowData>
{
    public RowData Item { get; }

    public Row(RowData data)
    {
        Item = data;
    }

    //public IEnumerable<ICell> Cells
    //{
    //    get
    //    {
    //        yield return new Cell(Item.Name);
    //        yield return new Cell(GetPassCell(false));
    //        yield return new Cell(GetPassCell(null));
    //        yield return new Cell(GetPassCell(false));
    //        yield return new Cell(GetPassCell(true));
    //        yield return new Cell(GetPassCell(true));
    //        //todo: actions approve/edit/delete/rerun analysis (either as separate action or do after edit?)
    //        //todo: separate delete into with/without prejudice, with prejudice marks same service resubmitted as auto-rejected
    //        // without prejudice means will show up again in the list if re-ingested
    //        yield return new Cell($"<a href=\"/manage-services/start-edit-service?serviceId={Item.Id}\">Approve</a>");
    //    }
    //}

    public IEnumerable<ICell> Cells
    {
        get
        {
            yield return new Cell($"<a href=\"/staged/service-details?serviceId={Item.Id}\">{Item.Name}</a>");
            yield return new Cell(string.Join(" ", GetTag("Render", "red"), GetTag("Content", "red")));
            //todo: actions approve/edit/delete/rerun analysis (either as separate action or do after edit?)
            //todo: separate delete into with/without prejudice, with prejudice marks same service resubmitted as auto-rejected
            // without prejudice means will show up again in the list if re-ingested
            yield return new Cell($"<a href=\"/manage-services/start-edit-service?serviceId={Item.Id}\">Approve</a>");
        }
    }

    private string GetPassCell(bool? pass)
    {
        return pass switch
        {
            true => "<strong class=\"govuk-tag govuk-tag--green\">Pass</strong>",
            false => "<strong class=\"govuk-tag govuk-tag--red\">Fail</strong>",
            null => "<strong class=\"govuk-tag govuk-tag--orange\">Not run</strong>",
        };
    }

    //todo: use enums for both params
    private string GetTag(string text, string colour)
    {
        return $"<strong class=\"govuk-tag govuk-tag--{colour}\">{text}</strong>";
    }
}

[Authorize(Roles = RoleGroups.AdminRole)]
public class IndexModel : HeaderPageModel, IDashboard<RowData>
{
    public const string PagePath = "/manage-services";

    public string? Title { get; set; }
    public string? OrganisationTypeContent { get; set; }
    public bool FilterApplied { get; set; }
    public string? CurrentServiceNameSearch { get; set; }
    public ServiceTypeArg? ServiceType { get; set; }

    //private enum Column
    //{
    //    Services,
    //    SecurityAnalysis,
    //    FindRender,
    //    ContentAnalysis,
    //    ReadingLevel,
    //    Locations,
    //    Approvals,
    //    ActionLinks
    //}

    private enum Column
    {
        Services,
        Issues,
        ActionLinks
    }

    //todo: we could pick up services from
    // staging database
    // same database, different schema name
    // inactive (and not archived) from same db
    // have a new table to store meta-data about services/locations

    //todo: service is no good without approved locations - best way to handle?

    //todo: need way to approve all in batch

    private const int PageSize = 10;
    private static ColumnImmutable[] _columnImmutables =
    {
        //todo: either going to have to combine some of these, or have a global ok or not and a details page
        //todo: instead show all flagged areas with a tag, e.g.
        // Service Name, Issues
        // "Autistic Cinema", "Political, PII"

        //new("Services", Column.Services.ToString()),
        ////todo: run security static analysis, or ask llm to check for potential security issues, such as sql injection, etc
        //new("Security Analysis", Column.SecurityAnalysis.ToString()),
        //// green tick icons or red cross icons with visually hidden text or just PASS/FAIL badge
        ////todo: link to page that shows the results of the auto render tests
        //// broken down into search result page/ details page/ anywhere else service details are rendered : dashboard?
        //// could have iframes showing the rendered pages (or parts of)
        ////todo: if security fails, then don't try and render the service
        //new("Find Render", Column.FindRender.ToString()),
        ////todo: link to page that shows the results of the auto analysis tests (pass to llm to check content for political bias (especially during elections), inappropriate language (e.g. spelling), PII, grammar,  
        //new("Content Analysis", Column.ContentAnalysis.ToString()),
        ////todo: GDS recommends reading level suitable for typical 9 year old. get llm to assign a reading age level
        //new("Reading Level", Column.ReadingLevel.ToString()),
        //// all approved? can't approve service until locations are approved
        //new("Locations", Column.Locations.ToString()),
        ////todo: check external links are reachable, and don't have inappropriate content
        //new("External links", Column.ReadingLevel.ToString()),
        ////todo: check style according to these pages...
        ////https://www.gov.uk/guidance/style-guide/a-to-z-of-gov-uk-style
        ////https://www.gov.uk/guidance/content-design/writing-for-gov-uk
        //new("Style", Column.ReadingLevel.ToString()),
        ////todo: multiple approvers? dfe/la/vcs (if vcs?)
        //new("Approvals", Column.Approvals.ToString()),
        // actions would be approve/view (or get to that through render page?)/manage locations/archive
        //new("<span class=\"govuk-visually-hidden\">Actions</span>", ColumnType: ColumnType.AlignedRight)

        //todo: upload id/date?

        new("Services", Column.Services.ToString()),
        new("Issues", Column.Issues.ToString()),
        new("<span class=\"govuk-visually-hidden\">Actions</span>", ColumnType: ColumnType.AlignedRight)
    };

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<RowData>> _rows = Enumerable.Empty<IRow<RowData>>();

    IEnumerable<IColumnHeader> IDashboard<RowData>.ColumnHeaders => _columnHeaders;
    public IEnumerable<IRow<RowData>> Rows => _rows;

    string? IDashboard<RowData>.TableClass => "app-services-dash";

    public IPagination Pagination { get; set; } = ILinkPagination.DontShow;
    private readonly IServiceDirectoryClient _serviceDirectoryClient;
    private readonly IAiClient _aiClient;

    public IndexModel(
        IServiceDirectoryClient serviceDirectoryClient,
        IAiClient aiClient)
    {
        _serviceDirectoryClient = serviceDirectoryClient;
        _aiClient = aiClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken,
        string? serviceType,
        string? columnName,
        SortOrder sort,
        int currentPage = 1,
        string? serviceNameSearch = null)
    {
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.Services;
            sort = SortOrder.ascending;
        }

        FilterApplied = serviceNameSearch != null;

        ServiceType = GetServiceTypeArg(serviceType);

        var user = HttpContext.GetFamilyHubsUser();

        long? organisationId;
        switch (user.Role)
        {
            case RoleTypes.DfeAdmin:
                Title = "Services";
                organisationId = null;
                OrganisationTypeContent = " for Local Authorities and VCS organisations";
                break;

            case RoleTypes.LaManager or RoleTypes.LaDualRole or RoleTypes.VcsManager or RoleTypes.VcsDualRole:
                organisationId = long.Parse(user.OrganisationId);

                // don't assume that user has come through the welcome page by expecting the org in the cache
                var organisation = await _serviceDirectoryClient.GetOrganisationById(organisationId.Value, cancellationToken);

                Title = $"{organisation.Name} services";

                if (user.Role is RoleTypes.LaManager or RoleTypes.LaDualRole)
                {
                    OrganisationTypeContent = " in your Local Authority";
                }
                else
                {
                    OrganisationTypeContent = " in your VCS organisation";
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown role: {user.Role}");
        }

        //todo: PaginatedList is in many places, there should be only one
        var services = await _serviceDirectoryClient.GetServiceSummaries(
            organisationId, serviceNameSearch, currentPage, PageSize, sort, cancellationToken);

        //todo: don't add serviceType if null
        string filterQueryParams = $"serviceNameSearch={HttpUtility.UrlEncode(serviceNameSearch)}&serviceType={ServiceType}";

        //todo: have combined factory that creates columns and pagination? (there's quite a bit of commonality)
        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, PagePath, column.ToString(), sort, filterQueryParams)
            .CreateAll();
        _rows = GetRows(services);

        Pagination = new LargeSetLinkPagination<Column>(PagePath, services.TotalPages, currentPage, column, sort, filterQueryParams);

        CurrentServiceNameSearch = serviceNameSearch;
    }

    public IActionResult OnPost(
        CancellationToken cancellationToken,
        string? serviceType,
        string? columnName,
        SortOrder sort,
        string? serviceNameSearch,
        bool? clearFilter)
    {
        if (clearFilter == true)
        {
            serviceNameSearch = null;
        }

        return RedirectToPage($"{PagePath}/Index", new
        {
            columnName,
            sort,
            serviceNameSearch,
            serviceType
        });
    }

    private ServiceTypeArg? GetServiceTypeArg(string? serviceType)
    {
        if (!Enum.TryParse<ServiceTypeArg>(serviceType, out var serviceTypeEnum))
        {
            // it's only really needed for the dfe admin, but we'll require it for consistency (and for when we allow LAs to add VCS services)
            //throw new InvalidOperationException("ServiceType must be passed as a query parameter");
            return null;
        }
        return serviceTypeEnum;
    }

    private IEnumerable<Row> GetRows(PaginatedList<ServiceNameDto> services)
    {
        return services.Items.Select(s => new Row(new RowData(s.Id, s.Name)));
    }
}
