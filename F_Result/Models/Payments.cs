using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{

    public partial class Payments
    {
        [Key]
        public int id { get; set; }

        [Display(Name = "ID проекта")]
        public int ProjectId { get; set; }

        [Display(Name = "Проект")]
        public string Project { get; set; }

        [Display(Name = "Руководитель проекта")]
        public string Chief { get; set; }

        [Display(Name = "Клиент")]
        public string Client { get; set; }

        [Display(Name = "Договор")]
        public string Agreement { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата договора")]
        [DataType(DataType.Date)]
        public DateTime? AgrDate { get; set; }

        [Display(Name = "Предмет договора")]
        public string Soder { get; set; }

        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Сумма договора")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? Summ { get; set; }

        [Display(Name = "Тип договора")]
        public string AgrType { get; set; }

        [Display(Name = "Менеджер")]
        public string Manager { get; set; }

        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Оплата")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? Payment { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата оплаты")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Тип оплаты")]
        public string PaymentDesc { get; set; }
    }

    //Модель представления для сущности "Входящие платежи"
    [NotMapped]
    public class PaymentsView : Payments
    {
        public DateTime? IPA { get; set; }

        [Display(Name = "Тип проекта")]
        public string ProjectType { get; set; }

        [MaxLength(512)]
        [Display(Name = "Руководитель проекта")]
        public string ChiefName { get; set; }

        [MaxLength(512)]
        [Display(Name = "Менеджер проекта")]
        public string ProjectManagerName { get; set; }

        [Display(Name = "Дата начала (План)")]
        public DateTime? StartDateFact { get; set; }

        [Display(Name = "Дата начала (Факт)")]
        public DateTime? StartDatePlan { get; set; }

        [NotMapped]
        [Display(Name = "План доходов")]
        public decimal planBenefit { get; set; }

        [NotMapped]
        [Display(Name = "План расходов")]
        public decimal planExpand { get; set; }

    }
}