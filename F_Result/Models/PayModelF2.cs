using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{

    public partial class PayModelF2
    {
        [Key]
        public int id { get; set; }

        [Display(Name = "ID сотрудника")]
        public int WorkerId { get; set; }

        [Display(Name = "Сотрудник")]
        public string WorkerName { get; set; }

        [Display(Name = "Код")]
        public string DocumentNumber { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        public DateTime? DocumentDate { get; set; }

        [Display(Name = "Назначение платежа")]
        public string ItemDescr { get; set; }

        [Display(Name = "Код статьи расходов")]
        public int IncomeItems { get; set; }

        [Display(Name = "Статья расходов")]
        public string IncomeItemsName { get; set; }

        [Display(Name = "ID проекта")]
        public int ProjectId { get; set; }

        [Display(Name = "Проект")]
        public string ProjectName { get; set; }

        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Сумма")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? ItemSum { get; set; }

        [Display(Name = "Валюта")]
        public string Currency { get; set; }

        [Display(Name = "Квитанция")]
        public string Receipt { get; set; }

        [Display(Name = "Примечание")]
        public string DocumentDescr { get; set; }

        [Display(Name = "Теги")]
        public string Tags { get; set; }

        //==============================================
        [NotMapped]
        [Display(Name = "Тип проекта")]
        public string ProjectType { get; set; }

        [NotMapped]
        [Display(Name = "Менеджер")]
        public string ProjectManagerName { get; set; }

        [NotMapped]
        [Display(Name = "Руководитель проекта")]
        public string ChiefName { get; set; }

        [NotMapped]
        [Display(Name = "Дата начала (План)")]
        public DateTime? StartDateFact { get; set; }

        [NotMapped]
        [Display(Name = "Дата начала (Факт)")]
        public DateTime? StartDatePlan { get; set; }

        [NotMapped]
        [Display(Name = "План доходов")]
        public decimal planBenefit { get; set; }

        [NotMapped]
        [Display(Name = "План расходов")]
        public decimal planExpand { get; set; }

    }


    public class ActualDebitsF2 : PayModelF2
    {

    }

    public class PaymentsF2 : PayModelF2
    {

    }


}