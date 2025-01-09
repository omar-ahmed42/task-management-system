using backend.Dtos;
using backend.Entities;
using Riok.Mapperly.Abstractions;

namespace backend.Mappers;

[Mapper]
public partial class UserMapper
{
    public partial User ToUser(UserRegistration user);

}
