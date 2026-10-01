using Microsoft.AspNetCore.Identity;

namespace homeWork_63.Models;

public class User : IdentityUser
{
    public DateTime BirthDate { get; set; }
    public string Avatar { get; set; } = "/images/default-avatar.png";
    public List<Message> Messages { get; set; } = new();
}