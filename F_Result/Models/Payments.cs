using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace F_Result.Models
{

    public partial class Payments
    {
        [Key]
        public int id { get; set; }

        [Display(Name = "Проект")]
        public string Project { get; set; }

        [Display(Name = "Клиент")]
        public string Client { get; set; }

        [Display(Name = "Договор")]
        public string Agreement { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата договора")]
        [DataType(DataType.Date)]
        public DateTime? AgrDate { get; set; }

        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Сумма договора")]
        public decimal? Summ { get; set; }

        [Display(Name = "Тип договора")]
        public string AgrType { get; set; }

        [Display(Name = "Менеджер")]
        public string Manager { get; set; }

        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Оплата")]
        public decimal? Payment { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата оплаты")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Тип оплаты")]
        public string PaymentDesc { get; set; }
    }
}