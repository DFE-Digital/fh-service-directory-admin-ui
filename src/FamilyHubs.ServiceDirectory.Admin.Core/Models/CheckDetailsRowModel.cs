namespace FamilyHubs.ServiceDirectory.Admin.Core.Models
{
    public class CheckDetailsRowModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string ChangeLocation { get; set; }

        public CheckDetailsRowModel(string name, string value, string changeLocation)
        {
            Name = name;
            Value = value;
            ChangeLocation = changeLocation;
        }
    }
}
