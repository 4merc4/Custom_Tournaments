using System;
using System.Collections.Generic;

namespace Custom_Tournaments.Domain.Model;

public partial class Teammatchresult : Entity
{
    

    public int Teamid { get; set; }

    public string Gamename { get; set; } = null!;

    public string? Opponentname { get; set; }

    public string Result { get; set; } = null!;

    public string? Score { get; set; }

    public string? Duration { get; set; }

    public string? Gamestats { get; set; }

    public DateTime? Playedat { get; set; }

    public virtual Team Team { get; set; } = null!;
}
