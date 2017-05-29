using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace F_Result.Models
{

    public partial class Organizations
    {
        [Key]
        public int id { get; set; }

        [MaxLength(512)]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [MaxLength(10)]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [MaxLength(15)]
        [Display(Name = "ИНН")]
        public string INN { get; set; }

        [MaxLength(20)]
        [Display(Name = "ЕДРПОУ")]
        public string EDRPOU { get; set; }

        [MaxLength(1)]
        [Display(Name = "Состояние")]
        public string State { get; set; }

    }
}