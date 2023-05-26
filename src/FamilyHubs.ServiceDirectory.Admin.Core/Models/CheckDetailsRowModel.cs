namespace FamilyHubs.ServiceDirectory.Admin.Core.Models
{
    public class CheckDetailsRowModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string ChangeLocation { get; set; }
        public string Id { get; set; }

        public CheckDetailsRowModel(string name, string value, string changeLocation, string id)
        {
            Name = name;
            Value = value;
            ChangeLocation = changeLocation;
            Id = id;
        }
    }
}
