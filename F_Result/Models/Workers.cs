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
        
        [MaxLength(16)]
        [Display(Name = "Код сотрудника")]
        public string EmpCode { get; set; }
        
        [MaxLength(16)]
        [Display(Name = "Код пользователя")]
        public string UserCode { get; set; }
    }
}