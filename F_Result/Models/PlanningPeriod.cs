using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("PlanningPeriod")]
    public class PlanningPeriod
    {
        [Key]
        public int PlanningPeriodId { get; set; }

        [Required(ErrorMessage = "Необходимо указать название периода")]
        [StringLength(128, ErrorMessage = "Название периода должно содержать не более 128 символов")]
        [Display(Name = "Название периода")]
        public string PeriodName { get; set; }

        public virtual ICollection<PlanCredit> PlanCredit { get; set; }

        public virtual ICollection<PlanDebit> PlanDebit { get; set; }
    }
}