using System.Text.Json.Serialization;

namespace backend.Enums;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssigneeType
{
    User = 0,
    Team = 1,
}
