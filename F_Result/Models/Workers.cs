using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace F_Result.Models
{
    public class Workers
    {
        [Key]
        public int id { get; set; }

        [MaxLength(120)]
        [Display(Name = "Имя")]
        public string ShortName { get; set; }
        
        [MaxLength(512)]
        [Display(Name = "Организация")]
        public string Organization { get; set; }
        
        [Display(Name = "Проекты")]
        public string projects { get; set; }

        [NotMapped]
        public int? relation { get; set; } 
    }
}