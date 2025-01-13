using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Dtos.Tasks;

public record class TaskUpdateDetails([Required][MaxLength(50)] string Title, [Required][MaxLength(32768)] string Description, DateTime DueDate, [Required][property: JsonConverter(typeof(JsonStringEnumConverter))] Enums.TaskStatus Status, [Required][Range(-1, 10)] int PriorityLevel);
