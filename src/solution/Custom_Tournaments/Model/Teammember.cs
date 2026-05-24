using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Teammember : Entity
{
    [Display(Name = "Команда")]
    [Required(ErrorMessage = "Вибір команди є обов'язковим")]
    public int Teamid { get; set; }

    [Display(Name = "Користувач")]
    [Required(ErrorMessage = "Вибір користувача є обов'язковим")]
    public int Userid { get; set; }

    [Display(Name = "Роль у команді")]
    [StringLength(50, ErrorMessage = "Назва ролі не може перевищувати 50 символів")]
    public string? Role { get; set; }

    [Display(Name = "Дата вступу")]
    [DataType(DataType.DateTime)]
    public DateTime? Joinedat { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}