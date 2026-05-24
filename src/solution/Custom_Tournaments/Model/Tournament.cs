using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Tournament : Entity
{
    [Display(Name = "ID Організатора")]
    [Required(ErrorMessage = "Організатор має бути обов'язково вказаний")]
    public int Organizerid { get; set; }

    [Display(Name = "Назва турніру")]
    [Required(ErrorMessage = "Назва обов'язкова")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Назва не може бути довшою за 200 символів та коротшою за 2 символи")]
    public string Title { get; set; } = null!;

    [Display(Name = "Гра")]
    [StringLength(100, ErrorMessage = "Назва гри не може бути довшою за 100 символів")]
    public string? Gamename { get; set; }

    [Display(Name = "Призовий фонд")]
    [Range(0, 1000000000, ErrorMessage = "Призовий фонд не може бути від'ємним")]
    [DataType(DataType.Currency)]
    public decimal? Prizepool { get; set; }

    [Display(Name = "Правила")]
    [StringLength(2000, ErrorMessage = "Опис правил занадто довгий (макс. 2000 символів)")]
    public string? Rules { get; set; }

    [Display(Name = "Приватний турнір")]
    public bool? Isprivate { get; set; }

    // ===== Нові поля =====

    [Display(Name = "Статус турніру")]
    [StringLength(20)]
    public string Status { get; set; } = "pending";
    // pending = очікує старту, active = йде, finished = завершений

    [Display(Name = "Формат турніру")]
    [StringLength(20)]
    public string Format { get; set; } = "elimination";
    // elimination / roundrobin / battleroyal

    [Display(Name = "Балова система")]
    public bool ScoringEnabled { get; set; } = false;

    [Display(Name = "Очки за перемогу")]
    [Range(0, 100)]
    public int ScoringWin { get; set; } = 3;

    [Display(Name = "Очки за нічию")]
    [Range(0, 100)]
    public int ScoringDraw { get; set; } = 1;

    [Display(Name = "Очки за поразку")]
    [Range(0, 100)]
    public int ScoringLoss { get; set; } = 0;

    [Display(Name = "Максимум раундів")]
    [Range(1, 100)]
    public int MaxRounds { get; set; } = 1;

    [Display(Name = "Дата створення")]
    [DataType(DataType.DateTime)]
    public DateTime? Createdat { get; set; }

    [Display(Name = "Дата оновлення")]
    [DataType(DataType.DateTime)]
    public DateTime? Updatedat { get; set; }

    public virtual User Organizer { get; set; } = null!;

    public virtual ICollection<Tournamentparticipant> Tournamentparticipants { get; set; } = new List<Tournamentparticipant>();

    public virtual ICollection<Teammatchresult> Teammatchresults { get; set; } = new List<Teammatchresult>();

    public virtual ICollection<Tournamentmatch> Tournamentmatches { get; set; } = new List<Tournamentmatch>();
}