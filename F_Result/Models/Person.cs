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
        public int PersonId { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30)]
        public string LastName { get; set; }

        [StringLength(100)]
        public string MiddleName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public int Sex { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
