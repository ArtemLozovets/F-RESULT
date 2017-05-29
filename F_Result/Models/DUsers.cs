using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace F_Result.Models
{
    public partial class DUsers
    {

        [Key]
        public int id { get; set; }

        [MaxLength(120)]
        [Display(Name = "Полное имя")]
        public string FullName { get; set; }


        [MaxLength(120)]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [MaxLength(10)]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [MaxLength(1)]
        [Display(Name = "Состояние")]
        public string State { get; set; }

        [NotMapped]
        public string DT_RowId { get; set; }

    }
}