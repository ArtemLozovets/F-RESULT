using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("AccountsBalance")]
    public class AccountsBalance
    {
        public int AccountsBalanceId { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле  \"Дата\"")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Необходимо указать сумму остатка")]
        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Остаток")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Balance { get; set; }

        [StringLength(160, ErrorMessage = "Примечание должно содержать не более 160 символов")]
        [Display(Name = "Прмечание")]
        public string Note { get; set; }

        public int AccountId { get; set; }

        public string UserId { get; set; }

        //---------------------------------------------------------------
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Account Account { get; set; }

        [NotMapped]
        [Display(Name = "Пользователь")]
        public string UserFN { get; set; }

        [NotMapped]
        [Display(Name = "Счет")]
        public string AccountNumber { get; set; }

        [NotMapped]
        public int OrganizationId { get; set; }

        [NotMapped]
        [Display(Name = "Организация")]
        public string OrganizationName { get; set; }
    }
}