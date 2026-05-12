using System.ComponentModel.DataAnnotations;

namespace Lexicon.DTOs;

public record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password);