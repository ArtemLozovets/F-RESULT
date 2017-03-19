using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [NotMapped]
    public class UsRoleViewModel
    {
        [Key]
        public string UId { get; set; }
        
        [Display(Name = "Логин")]
        public string UUserName { get; set; }
        
        [Display(Name = "Email")]
        public string UEmail { get; set; }
        
        [Display(Name = "Имя")]
        public string UFirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string ULastName { get; set; }

        [Display(Name = "Отчество")]
        public string UMiddleName { get; set; }

        [Display(Name = "Должность")]
        public string UPost { get; set; }

        [Display(Name = "Роль")]
        public string URole { get; set; }

    }

    public class UpdateUserInfoesViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Email\"")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Фамилия\"")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Имя\"")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Отчество\"")]
        [Display(Name = "Отчество")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Должность\"")]
        [Display(Name = "Должность")]
        public string Post { get; set; }

        [Display(Name = "Роль")]
        [Required(ErrorMessage = "Необходимо заполнить поле \"Роль\"")]
        public string Role { get; set; }
    }

    public class UserPasswordChangeViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Пароль\"")]
        [StringLength(100, ErrorMessage = "Поле \"{0}\" должно содержать больше {2} символів", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение")]
        [Compare("Password", ErrorMessage = "Введенные пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }

    public class SelfPasswordChangeViewModel
    {
        [Required(ErrorMessage = "Не заполнено поле \"Текущий пароль\"")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Не заполнено поле \"Новый пароль\"")]
        [StringLength(100, ErrorMessage = "Поле \"{0}\" должно содержать больше {2} символів", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Не заполнено поле \"Подтверждение пароля\"")]
        [Display(Name = "Подтверждение")]
        [Compare("NewPassword", ErrorMessage = "Введенные пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Не заполнено поле \"Логин\"")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Не заполнено поле \"Пароль\"")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Необходимо заполнить поле \"Email\"")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Логин\"")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Фамилия\"")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Необходимо заполнить поле \"Имя\"")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Отчество\"")]
        [Display(Name = "Отчество")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Должность\"")]
        [Display(Name = "Должность")]
        public string Post { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"Пароль\"")]
        [StringLength(100, ErrorMessage = "Поле \"{0}\" должно содержать не меньше {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение")]
        [Compare("Password", ErrorMessage = "Введенные пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Необходимо указать роль пользователя")]
        [Display(Name = "Роль")]
        public string UserRole { get; set; }
    }
}