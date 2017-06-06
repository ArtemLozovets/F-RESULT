using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace F_Result.Models
{
    [Table("AspVisitor")]
    public class AspVisitor
    {
        public int Id { get; set; }

        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "ІР")]
        public string Ip { get; set; }

        [Display(Name = "Маршрут")]
        public string Url { get; set; }

        [Display(Name = "Результат")]
        public bool? Result { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH':'mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата/Время")]
        public DateTime Date { get; set; }
    }
}