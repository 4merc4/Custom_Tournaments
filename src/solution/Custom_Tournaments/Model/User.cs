using System;
using System.Collections.Generic;

namespace Custom_Tournaments.Domain.Model;

public partial class User : Entity
{
    
    public string? IdentityUserId { get; set; }

    
    public string Username { get; set; } = null!;

    
    public string Email { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Teammember> Teammembers { get; set; } = new List<Teammember>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}
