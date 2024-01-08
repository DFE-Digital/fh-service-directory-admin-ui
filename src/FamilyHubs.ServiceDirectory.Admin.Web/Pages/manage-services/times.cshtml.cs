using Azure.Storage.Blobs.Models;
using FamilyHubs.ServiceDirectory.Admin.Core.DistributedCache;
using FamilyHubs.ServiceDirectory.Admin.Core.Models;
using FamilyHubs.ServiceDirectory.Admin.Web.Pages.Shared;

namespace FamilyHubs.ServiceDirectory.Admin.Web.Pages.manage_services;

public enum AmPm
{
    Am,
    Pm
}

//todo: separate for name/desc & values?
//todo: doesn't belong with page. have in components?
public class TimeModel
{
    public TimeModel(string name, string? description = null, AmPm defaultAmPm = AmPm.Am)
    {
        Name = name;
        Description = description;
        DefaultAmPm = defaultAmPm;
        //Hour = 12;
        //Minute = 0;
        //AmPm = AmPm.Pm;
    }

    public string? Description { get; set; }
    public string Name { get; set; }
    public AmPm DefaultAmPm { get; set; }

    public int Hour { get; set; }
    public int Minute { get; set; }
    public AmPm AmPm { get; set; }
}

public class timesModel : ServicePageModel
{
    public TimeModel WeekdaysStarts { get; set; }
    public TimeModel WeekdaysFinishes { get; set; }

    public TimeModel WeekendsStarts { get; set; }
    public TimeModel WeekendsFinishes { get; set; }

    public timesModel(IRequestDistributedCache connectionRequestCache)
        : base(ServiceJourneyPage.Times, connectionRequestCache)
    {
    }

    protected override async Task OnGetWithModelAsync(CancellationToken cancellationToken)
    {
        WeekdaysStarts = new TimeModel("weekdays-starts", "Starts", AmPm.Am);
        WeekdaysFinishes = new TimeModel("weekdays-finishes", "Finishes", AmPm.Pm);
        WeekendsStarts = new TimeModel("weekends-starts", "Starts", AmPm.Am);
        WeekendsFinishes = new TimeModel("weekends-finishes", "Finishes", AmPm.Pm);
    }
}