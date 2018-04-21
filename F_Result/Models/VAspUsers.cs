using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("VAspUsers")]
    public class VAspUsers 
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

        [Display(Name = "ФИО")]
        public string UFullName { get; set; }

        [Display(Name = "Сотрудники")]
        public string UWorkers { get; set; }

        [Display(Name = "Должность")]
        public string UPost { get; set; }

        [Display(Name = "Роль")]
        public string URoleDesc { get; set; }

        [Display(Name = "Название роли")]
        public string URoleName { get; set; }
    }
}