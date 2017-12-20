using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    public class Expenditure
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        [Display(Name = "Код")]
        public string Article { get; set; }

        [MaxLength(512)]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [MaxLength(512)]
        [Display(Name = "Группа")]
        public string GroupName { get; set; }
    }
}