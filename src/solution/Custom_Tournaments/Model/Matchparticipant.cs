using System;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Matchparticipant : Entity
{
    [Display(Name = "Матч")]
    [Required]
    public int Matchid { get; set; }

    [Display(Name = "Команда")]
    [Required]
    public int Teamid { get; set; }

    [Display(Name = "Рахунок")]
    [StringLength(50)]
    public string? Score { get; set; }

    [Display(Name = "Місце")]
    public int? Place { get; set; }

    [Display(Name = "Очки")]
    public int Points { get; set; } = 0;

    [Display(Name = "Вибула з матчу")]
    public bool IsEliminated { get; set; } = false;

    public DateTime EliminatedAt { get; set; }

    [Display(Name = "Переможець")]
    public bool IsWinner { get; set; } = false;

    public virtual Tournamentmatch Match { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}