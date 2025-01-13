using backend.Dtos;
using backend.Dtos.Tasks;
using Riok.Mapperly.Abstractions;

namespace backend.Mappers;

[Mapper]
public static partial class TaskMapper
{
    public static partial Entities.Task ToTask(TaskCreation task);
    public static partial TaskResponse ToTaskResponse(Entities.Task task);
}
