using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models.ServiceJourney;

public class MiniJourneyServiceModel<TUserInput>
    where TUserInput : class?
{
    public MiniJourneyServiceModel()
    {
        // parameterless constructor required for deserialization
        HowUse = Array.Empty<AttendingType>();
        Locations = new();
    }

    public MiniJourneyServiceModel(ServiceModel<TUserInput> model)
    {
        HasTimeDetails = model.HasTimeDetails;
        TimeDescription = model.TimeDescription;
        Times = model.Times;
        HowUse = model.HowUse;
        CurrentLocation = model.CurrentLocation;
        Locations = model.Locations;
        Updated = model.Updated;
    }

    public void RestoreTo(ServiceModel<TUserInput> model)
    {
        model.HasTimeDetails = HasTimeDetails;
        model.TimeDescription = TimeDescription;
        model.Times = Times;
        model.HowUse = HowUse;
        model.CurrentLocation = CurrentLocation;
        model.Locations = Locations;
        model.Updated = Updated;
    }

    public bool? HasTimeDetails { get; set; }
    public string? TimeDescription { get; set; }
    public IEnumerable<string>? Times { get; set; }
    public AttendingType[] HowUse { get; set; }
    public ServiceLocationModel? CurrentLocation { get; set; }
    public List<ServiceLocationModel> Locations { get; set; }
    public bool Updated { get; set; }
}