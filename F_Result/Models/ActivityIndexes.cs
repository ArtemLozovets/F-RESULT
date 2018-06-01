using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    public class ActivityIndex
    {
        public int ActivityIndexId { get; set; }

        [Display(Name = "Проект")]
        public int ProjectId { get; set; }

        [Display(Name = "Индекс активности проекта")]
        public DateTime IPAValue { get; set; }

    }
}