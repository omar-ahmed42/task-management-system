using System.Text.Json.Serialization;

namespace backend.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus
{
    ToDo = 0,
    InProgress = 1,
    Completed = 2,
    Stall = 3

}
