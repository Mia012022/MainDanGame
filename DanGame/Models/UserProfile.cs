using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class UserProfile
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly DateOfbirth { get; set; }

    public string? Bio { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdateAt { get; set; }

    public virtual User User { get; set; } = null!;
}
