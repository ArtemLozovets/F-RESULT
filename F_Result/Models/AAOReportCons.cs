using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    public partial class AAOReportCons
    {
        [Key, Column(Order = 0)]
        [Display(Name = "Дата")]
        public DateTime CDate { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "ID сотрудника")]

        public int WorkerID { get; set; }

        [Display(Name = "Сотрудник")]
        public string WorkerName { get; set; }

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