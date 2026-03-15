using System;
using System.Collections.Generic;

namespace Custom_Tournaments.Domain.Model;

public partial class Teammember : Entity
{
    

    public int Teamid { get; set; }

    public int Userid { get; set; }

    public string? Role { get; set; }

    public DateTime? Joinedat { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
