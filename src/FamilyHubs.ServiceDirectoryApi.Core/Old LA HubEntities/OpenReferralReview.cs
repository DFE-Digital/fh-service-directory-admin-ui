//using FamilyHubs.SharedKernel;
//using FamilyHubs.SharedKernel.Interfaces;
//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

//public class OpenReferralReview : EntityBase<string>, IOpenReferralReview, IAggregateRoot
//{
//    private OpenReferralReview() { }


//    public OpenReferralReview(string id, string title, string? description, DateTime date, string? score, string? url, string? widget
//        //,OpenReferralService service
//        )
//    {
//        Id = id;
//        //Service = service;
//        Title = title;
//        Description = description;
//        Date = date;
//        Score = score;
//        Url = url;
//        Widget = widget;
//    }

//    //public OpenReferralService Service { get; set; } = default!;
//    public string Title { get; private set; } = default!;
//    public string? Description { get; private set; }
//    public DateTime Date { get; private set; }
//    public string? Score { get; private set; }
//    public string? Url { get; private set; }
//    public string? Widget { get; private set; }

//    public void Update(OpenReferralReview openReferralReview)
//    {
//        Title = openReferralReview.Title;
//        Description = openReferralReview.Description;
//        Date = openReferralReview.Date;
//        Score = openReferralReview.Score;
//        Url = openReferralReview.Url;
//        Widget = openReferralReview.Widget;
//    }

//}
