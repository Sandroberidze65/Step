using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDTO{

    [Required]
    public string Username { get; set; }
    [Required]
    public string KnownAs { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    public DateOnly? DateOfBirth { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Country { get; set; }
    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string Password { get; set; }
}

