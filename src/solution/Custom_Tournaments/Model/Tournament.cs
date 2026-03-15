using System;
using System.Collections.Generic;

namespace Custom_Tournaments.Domain.Model;

public partial class Tournament : Entity
{
    

    public int Organizerid { get; set; }

    public string Title { get; set; } = null!;

    public decimal? Prizepool { get; set; }

    public string? Rules { get; set; }

    public bool? Isprivate { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual User Organizer { get; set; } = null!;

    public virtual ICollection<Tournamentparticipant> Tournamentparticipants { get; set; } = new List<Tournamentparticipant>();
}
