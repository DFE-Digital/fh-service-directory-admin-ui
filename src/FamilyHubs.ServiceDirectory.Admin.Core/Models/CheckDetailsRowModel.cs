namespace FamilyHubs.ServiceDirectory.Admin.Core.Models
{
    public class CheckDetailsRowModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }
        public string ChangeLocation { get; set; } = string.Empty;
        public string ChangeAnchorText { get; set; } = "Change";

        public CheckDetailsRowModel(string name, string value, string id)
        {
            Name = name;
            Value = value;
            Id = id;
        }

        public CheckDetailsRowModel(string name, string value, string changeLocation, string id)
        {
            Name = name;
            Value = value;
            ChangeLocation = changeLocation;
            Id = id;
        }

        public CheckDetailsRowModel(string name, string value, string changeLocation, string changeAnchorText, string id)
        {
            Name = name;
            Value = value;
            ChangeLocation = changeLocation;
            ChangeAnchorText = changeAnchorText;
            Id = id;
        }
    }
}
