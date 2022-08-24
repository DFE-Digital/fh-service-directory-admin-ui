using AutoMapper;

namespace FamilyHubs.ServiceDirectoryAdminUi.Core.Mappings;

public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
