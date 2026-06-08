using System.ComponentModel.DataAnnotations;

namespace Lexicon.DTOs;

public class DeleteAccountRequest
{
    [Required]
    public string Password { get; set; } = "";
}
