using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("PlanDebitF2")]
    public class PlanDebitF2
    {
        public int PlanDebitF2Id { get; set; }

        [Required(ErrorMessage = "Необходимо указать дату")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Необходимо указать сумму")]
        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Сумма")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Sum { get; set; }

        [Required(ErrorMessage = "Необходимо указать проект")]
        [Display(Name = "Проект")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Необходимо указать организацию")]
        [Display(Name = "Организация")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Необходимо указать статью расходов")]
        [Display(Name = "Статья расходов")]
        public int ExpenditureId { get; set; }

        [StringLength(200, ErrorMessage = "Значение должно содержать не больше 200 символов")]
        [Display(Name = "Назначение")]
        public string Appointment { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int PeriodId { get; set; }
        public virtual PlanningPeriod PlanningPeriod { get; set; }

        [NotMapped]
        [Display(Name = "Пользователь")]
        public string UserFN { get; set; }

        [NotMapped]
        [Display(Name = "Проект")]
        public string ProjectName { get; set; }

        [NotMapped]
        [Display(Name = "Организация")]
        public string OrganizationName { get; set; }

        [NotMapped]
        [Display(Name = "Статья расходов")]
        public string ExpenditureName { get; set; }

        [NotMapped]
        [Display(Name = "Период")]
        public string PeriodName { get; set; }
    }


    //Модель представления для сущности "План расходов"
    [NotMapped]
    public class PlanDebitViewF2 : PlanDebitF2
    {
        [Display(Name = "Тип проекта")]
        public string ProjectType { get; set; }

        public DateTime? IPA { get; set; }

        [Display(Name = "Менеджер")]
        public string ProjectManagerName { get; set; }

        [Display(Name = "Руководитель проекта")]
        public string ChiefName { get; set; }

        [Display(Name = "Дата начала (План)")]
        public DateTime? StartDateFact { get; set; }

        [Display(Name = "Дата начала (Факт)")]
        public DateTime? StartDatePlan { get; set; }

        [Display(Name = "План доходов")]
        public decimal planBenefit { get; set; }

        [Display(Name = "План расходов")]
        public decimal planExpand { get; set; }

    }
}