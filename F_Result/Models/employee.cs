namespace F_Result.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("employee")]
    public partial class employee
    {
        [Key]
        public int emp_id { get; set; }

        [Column(Order = 0)]
        [StringLength(20)]
        public string fname { get; set; }

        [StringLength(1)]
        public string minit { get; set; }

        [StringLength(30)]
        public string lname { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int job_id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int job_lvl { get; set; }

        [StringLength(4)]
        public string pub_id { get; set; }

        public DateTime? hire_date { get; set; }
    }
}
