using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Tournamentmatch : Entity
{
    [Display(Name = "Турнір")]
    [Required]
    public int Tournamentid { get; set; }

    [Display(Name = "Раунд")]
    public int Round { get; set; } = 1;

    [Display(Name = "Статус")]
    [StringLength(20)]
    public string Status { get; set; } = "planned";
    // planned / active / finished

    [Display(Name = "Нотатки")]
    public string? Notes { get; set; }

    [Display(Name = "Початок")]
    [DataType(DataType.DateTime)]
    public DateTime? StartedAt { get; set; }

    [Display(Name = "Завершення")]
    [DataType(DataType.DateTime)]
    public DateTime? FinishedAt { get; set; }

    public virtual Tournament Tournament { get; set; } = null!;

    public virtual ICollection<Matchparticipant> Matchparticipants { get; set; } = new List<Matchparticipant>();
}