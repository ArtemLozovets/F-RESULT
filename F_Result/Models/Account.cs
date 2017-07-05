using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("Account")]
    public class Account
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Необходимо указать организацию")]
        [Display(Name = "Организация")]
        public int OrganizationId { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(6, ErrorMessage = "Значение поля должно состоять из шести цифр")]
        [RegularExpression("(^\\d{6}?$)", ErrorMessage = "Значение поля должно состоять из шести цифр")]
        [Required(ErrorMessage = "Необходимо указать МФО")]
        [Display(Name = "МФО")]
        public string MFO { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(21, ErrorMessage = "Номер счета должен содержать от 7 до 21 цифр")]
        [RegularExpression("(^\\d{7,21}?$)", ErrorMessage = "Номер счета должен содержать от 7 до 21 цифр")]
        [Required(ErrorMessage = "Необходимо указать номер счета")]
        [Display(Name = "Номер счета")]
        public string AccountNumber { get; set; }

        [Display(Name = "Статус")]
        [Required()]
        public bool Status { get; set; }

        [StringLength(160, ErrorMessage = "Примечание должно содержать не более 160 символов")]
        [Display(Name = "Примечание")]
        public string Note { get; set; }
        
        public string UserId { get; set; }
  
        //------------------------------------------------------
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<AccountsBalance> AccountsBalance { get; set; }

        [NotMapped]
        [Display(Name = "Пользователь")]
        public string UserFN { get; set; }

        [NotMapped]
        [Display(Name = "Организация")]
        public string OrganizationName { get; set; }
    }
}