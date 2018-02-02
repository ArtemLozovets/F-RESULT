using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("Settings")]
    public class Settings
    {
        [Key]
        public int SettingId { get; set; }

        [Required(ErrorMessage = "Необходимо указать название параметра")]
        [StringLength(120, ErrorMessage = "Значение должно содержать не более 120 символов")]
        [Display(Name = "Параметр")]
        public string SettingName { get; set; }

        [Required(ErrorMessage = "Необходимо указать значение параметра")]
        [StringLength(500, ErrorMessage = "Значение должно содержать не более 500 символов")]
        [Display(Name = "Значение")]
        public string SettingValue { get; set; }

        [Display(Name = "Дата модификации")]
        public DateTime? ModificationDate { get; set; }
        
        [StringLength(500, ErrorMessage = "Примечание должно содержать не более 500 символов")]
        [Display(Name = "Примечание")]
        public string Note { get; set; }
 
        public string UserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
        
      
    }
}