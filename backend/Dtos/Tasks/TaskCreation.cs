using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Dtos;

public record class TaskCreation([Required][MaxLength(50)] string Title, [Required][MaxLength(32768)] string Description, DateTime DueDate, [Required][property: JsonConverter(typeof(JsonStringEnumConverter))] Enums.TaskStatus Status, [Required][Range(-1, 10)] int PriorityLevel);