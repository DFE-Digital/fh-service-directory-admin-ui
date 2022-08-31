//using fh_service_directory_api.core.Interfaces.Entities;

//namespace fh_service_directory_api.core.Entities;

//public class Contact : EntityBase<string>, IContact, IAggregateRoot
//{
//    public string Name { get; init; } = default!;

//    public string Title { get; init; } = default!;

//    public IEnumerable<IPhone> ContactPhones => _contactPhones; // prevent consumers doing ContactPhones.Add and ContactPhones.Remove etc 

//    private Contact() { }
//    public Contact
//    (
//        string id,
//        string title,
//        string name,
//        IEnumerable<IPhone> contactPhones
//    )
//    {
//        Id = id;
//        Title = title;
//        Name = name;
//        _contactPhones = (ICollection<IPhone>)(contactPhones ?? new List<IPhone>().AsReadOnly());
//    }

//    // Private Properties
//    private readonly ICollection<IPhone> _contactPhones = new List<IPhone>().AsReadOnly();

//}
