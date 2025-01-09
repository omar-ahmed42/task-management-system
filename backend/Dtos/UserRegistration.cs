using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public record UserRegistration([Required(AllowEmptyStrings = false)][MaxLength(50)] string FirstName, [Required(AllowEmptyStrings = false)] [MaxLength(50)] string LastName, [EmailAddress] string Email, string Password, [Required] DateOnly BirthDate);
