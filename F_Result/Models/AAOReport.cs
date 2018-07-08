using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    public partial class AAOReport
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "ID сотрудника")]
        public int WorkerID { get; set; }

        [Display(Name = "Сотрудник")]
        public string WorkerName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Display(Name = "Документ")]
        public string DocNumber { get; set; }

        [Display(Name = "Операция")]
        public string Operation { get; set; }

        [Display(Name = "ID контрагента")]
        public int? Counteragent { get; set; }

        [Display(Name = "Контрагент")]
        public string CounteragentName { get; set; }

        [Display(Name = "Передано")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal Payed { get; set; }

        [Display(Name = "Получено")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal Received { get; set; }

        [Display(Name = "Валюта")]
        public string Currency { get; set; }
    }
}