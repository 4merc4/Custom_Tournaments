using System;
using System.Collections.Generic;

namespace Custom_Tournaments.Domain.Model;

public partial class Team : Entity
{
   

    public int Ownerid { get; set; }

    public string Name { get; set; } = null!;

    public string? Logourl { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Teammatchresult> Teammatchresults { get; set; } = new List<Teammatchresult>();

    public virtual ICollection<Teammember> Teammembers { get; set; } = new List<Teammember>();

    public virtual ICollection<Tournamentparticipant> Tournamentparticipants { get; set; } = new List<Tournamentparticipant>();
}
