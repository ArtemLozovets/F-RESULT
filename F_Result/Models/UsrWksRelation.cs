using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("UsrWksRelations")]
    public class UsrWksRelation
    {
        public int UsrWksRelationId { get; set; }

        [Required(ErrorMessage = "Необходимо указать пользователя")]
        [Display(Name = "Пользователь")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required(ErrorMessage = "Необходимо указать сотрудника")]
        [Display(Name = "Сотрудник")]
        public int WorkerId { get; set; }

    }
}