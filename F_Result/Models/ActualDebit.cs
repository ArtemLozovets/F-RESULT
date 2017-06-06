using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace F_Result.Models
{
     [Table("ActualDebit")]
    public class ActualDebit
    {
         public int ActualDebitId { get; set; }

         [Required(ErrorMessage = "Необходимо указать дату")]
         [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
         [Display(Name = "Дата")]
         [DataType(DataType.Date)]
         public DateTime Date { get; set; }

         [Required(ErrorMessage = "Необходимо указать сумму")]
         [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
         [Display(Name = "Сумма")]
         public decimal Sum { get; set; }

         [Required(ErrorMessage = "Необходимо указать проект")]
        [Display(Name = "Проект")]
        public int ProjectId { get; set; }

         [StringLength(200, ErrorMessage = "Значение должно содержать не больше 200 символов")]
         [Display(Name = "Назначение")]
         public string Appointment { get; set; }

         [StringLength(50, ErrorMessage = "Значение должно содержать не больше 50 символов")]
         [Display(Name = "Номер документа")]
         public string DocNumber { get; set; }

         public string UserId { get; set; }
         public virtual ApplicationUser ApplicationUser { get; set; }
    }
}