using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Teammatchresult : Entity
{
    [Display(Name = "Команда")]
    [Required(ErrorMessage = "Необхідно вказати команду")]
    public int Teamid { get; set; }

    [Display(Name = "Турнір")]
    public int Tournamentid { get; set; }

    [Display(Name = "Назва гри")]
    [Required(ErrorMessage = "Назва гри обов'язкова")]
    [StringLength(100, ErrorMessage = "Назва гри не може перевищувати 100 символів")]
    public string Gamename { get; set; } = null!;

    [Display(Name = "Результат")]
    [StringLength(20, ErrorMessage = "Результат має бути коротким (до 20 символів)")]
    public string? Result { get; set; }

    [Display(Name = "Рахунок")]
    [StringLength(50, ErrorMessage = "Запис рахунку не може перевищувати 50 символів")]
    public string? Score { get; set; }

    [Display(Name = "Тривалість")]
    [StringLength(20, ErrorMessage = "Тривалість має бути короткою (напр. 20:15)")]
    public string? Duration { get; set; }

    [Display(Name = "Статистика гри (JSON)")]
    public string? Gamestats { get; set; }

    [Display(Name = "Статус матчу")]
    [StringLength(20)]
    public string Status { get; set; } = "planned"; // planned / ongoing / finished

    [Display(Name = "Дата гри")]
    [DataType(DataType.DateTime)]
    public DateTime? Playedat { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual Tournament Tournament { get; set; } = null!;
}