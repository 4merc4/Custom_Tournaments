using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Tournamentparticipant : Entity
{
    [Display(Name = "Турнір")]
    [Required(ErrorMessage = "Вибір турніру є обов'язковим")]
    public int Tournamentid { get; set; }

    [Display(Name = "Команда")]
    [Required(ErrorMessage = "Вибір команди є обов'язковим")]
    public int Teamid { get; set; }

    [Display(Name = "Дата приєднання")]
    [DataType(DataType.DateTime)]
    public DateTime? Joinedat { get; set; }

    [Display(Name = "Фінальне місце")]
    public int? FinalPlace { get; set; }

    [Display(Name = "Вибула")]
    public bool IsEliminated { get; set; } = false;

    [Display(Name = "Дата вибуття")]
    [DataType(DataType.DateTime)]
    public DateTime? EliminatedAt { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual Tournament Tournament { get; set; } = null!;
}