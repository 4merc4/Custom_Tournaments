using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments.Domain.Model;

public partial class Team : Entity
{
    [Display(Name = "Власник")]
    [Required(ErrorMessage = "Необхідно вказати власника команди")]
    public int Ownerid { get; set; }

    [Display(Name = "Назва команди")]
    [Required(ErrorMessage = "Назва команди є обов'язковою")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має бути від 2 до 100 символів")]
    public string Name { get; set; } = null!;

    [Display(Name = "Посилання на логотип")]
    [Url(ErrorMessage = "Введіть коректну адресу посилання (напр. https://...)")]
    [StringLength(500, ErrorMessage = "Посилання занадто довге")]
    public string? Logourl { get; set; }

    [Display(Name = "Дата створення")]
    [DataType(DataType.DateTime)]
    public DateTime? Createdat { get; set; }

    [Display(Name = "Дата оновлення")]
    [DataType(DataType.DateTime)]
    public DateTime? Updatedat { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Teammatchresult> Teammatchresults { get; set; } = new List<Teammatchresult>();

    public virtual ICollection<Teammember> Teammembers { get; set; } = new List<Teammember>();

    public virtual ICollection<Tournamentparticipant> Tournamentparticipants { get; set; } = new List<Tournamentparticipant>();
}