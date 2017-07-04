﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("PlanDebit")]
    public class PlanDebit
    {
        public int PlanDebitId { get; set; }

        [Required(ErrorMessage = "Необходимо указать дату")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Необходимо указать сумму")]
        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Сумма")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal Sum { get; set; }

        [Required(ErrorMessage = "Необходимо указать проект")]
        [Display(Name = "Проект")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Необходимо указать организацию")]
        [Display(Name = "Организация")]
        public int OrganizationId { get; set; }

        [StringLength(200, ErrorMessage = "Значение должно содержать не больше 200 символов")]
        [Display(Name = "Назначение")]
        public string Appointment { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [NotMapped]
        [Display(Name = "Пользователь")]
        public string UserFN { get; set; }

        [NotMapped]
        [Display(Name = "Проект")]
        public string ProjectName { get; set; }

        [NotMapped]
        [Display(Name = "Организация")]
        public string OrganizationName { get; set; }
    }
}