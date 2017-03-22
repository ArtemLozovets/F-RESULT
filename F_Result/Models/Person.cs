namespace F_Result.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Persons")]
    public partial class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int emp_id { get; set; }

        [Required]
        [StringLength(20)]
        public string fname { get; set; }

        [StringLength(1)]
        public string minit { get; set; }

        [Required]
        [StringLength(30)]
        public string lname { get; set; }

        public int job_id { get; set; }

        public int job_lvl { get; set; }

        [Required]
        [StringLength(4)]
        public string pub_id { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd'.'MM'.'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime hire_date { get; set; }
    }
}
