using System.ComponentModel.DataAnnotations;

namespace Lexicon.DTOs;

public record RegisterRequest(
    [Required] [EmailAddress] string Email,
    [Required] [MaxLength(100)] string UserName,
    [Required] [MinLength(6)] string Password);