
* if the add/edit journey gets longer, it might be better to have rule based navigation (at least for the mini-journeys). Something like...

```
public record MiniJourneyRule(
    ServiceJourneyChangeFlow? ChangeFlow,
    ServiceJourneyPage? FirstPage = null,
    ServiceJourneyPage? LastPage = null)
//todo: one predicate or first/last?
//todo: either just hardcode this, or have one for back and one for next?
//ServiceJourneyPage? OverridePage = ServiceJourneyPage.Service_Detail)
{
    private static ServiceJourneyPage OverridePage = ServiceJourneyPage.Service_Detail;

    public ServiceJourneyPage? CheckOverrideNextPage(
        ServiceJourneyChangeFlow? changeFlow,
        ServiceJourneyPage currentPage) // nextpage too?
    {
        //todo: better would be the calculated next page, ratther than currentpage
        return changeFlow == ChangeFlow && currentPage >= LastPage
            ? OverridePage : null;
    }

    public ServiceJourneyPage? CheckOverrideBackPage(
        ServiceJourneyChangeFlow? changeFlow,
        ServiceJourneyPage currentPage) // nextpage too?
    {
        //todo: better would be the calculated next page, ratther than currentpage
        return changeFlow == ChangeFlow && currentPage <= FirstPage
            ? OverridePage : null;
    }
}

// declarative mini-journey rules, rather than hand coding
private static readonly MiniJourneyRule[] MiniJourneyRules =
{
    new(ServiceJourneyChangeFlow.SinglePage),
    new(ServiceJourneyChangeFlow.LocalAuthority, ServiceJourneyPage.Local_Authority, ServiceJourneyPage.Vcs_Organisation),
    //todo: these will need nextpage/ previouspage after core calc
    // convert 2 above to use nextpage/ previouspage (or could support current and next/previous)
    //new(ServiceJourneyChangeFlow.Location),
    //new(ServiceJourneyChangeFlow.HowUse),
};


```