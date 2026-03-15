using System;
using System.Collections.Generic;

namespace Custom_Tournaments.Domain.Model;

public partial class Tournamentparticipant : Entity
{
    

    public int Tournamentid { get; set; }

    public int Teamid { get; set; }

    public DateTime? Joinedat { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual Tournament Tournament { get; set; } = null!;
}
