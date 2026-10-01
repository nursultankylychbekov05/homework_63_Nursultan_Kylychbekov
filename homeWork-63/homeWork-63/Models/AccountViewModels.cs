using System.ComponentModel.DataAnnotations;

namespace homeWork_63.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Поле Имя пользователя (UserName) обязательно для заполнения.")]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле Email обязательно для заполнения.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата рождения обязательна для заполнения.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    [MinimumAge(18, ErrorMessage = "Вам должно исполниться не менее 18 лет.")]
    public DateTime BirthDate { get; set; }

    [Display(Name = "Аватар")]
    public IFormFile? AvatarFile { get; set; }

    [Required(ErrorMessage = "Пароль обязателен для заполнения.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать не менее 6 символов.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Пароль должен содержать минимум 1 букву верхнего регистра, 1 букву нижнего регистра и 1 цифру.")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Подтвердите пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя или email.")]
    [Display(Name = "Имя пользователя или Email")]
    public string LoginOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}

public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minAge;

    public MinimumAgeAttribute(int minAge)
    {
        _minAge = minAge;
    }

    public override bool IsValid(object? value)
    {
        if (value is DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age >= _minAge;
        }
        return false;
    }
}