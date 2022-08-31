namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Models;

public interface ILinkCollection
{
    IReadOnlyList<Link> Links { get; }
    void AddOrUpdateLink<T>(T link) where T : Link;
    void RemoveLink<T>() where T : Link;
}
