using System.ComponentModel.DataAnnotations;

namespace Custom_Tournaments_Infrastructure.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Нікнейм обов'язковий")]
        [Display(Name = "Нікнейм")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Нікнейм має бути від 2 до 100 символів")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Некоректний Email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Підтвердження паролю обов'язкове")]
        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = null!;
    }
}