namespace FamilyHubs.ServiceDirectory.Admin.Web.Areas.AccountAdmin.Pages
{
    public class CheckDetailsRow
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string ChangeLocation { get; set; }

        public CheckDetailsRow(string name, string value, string changeLocation)
        {
            Name = name;
            Value = value;
            ChangeLocation = changeLocation;
        }
    }
}
