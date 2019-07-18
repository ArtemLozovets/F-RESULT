using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{
    [Table("Balance")]
    public class Balance
    {
        public int BalanceId { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Дата добавления")]
        public DateTime DateOfCreation { get; set; }

        [Display(Name = "Дата")]
        public DateTime BalanceDate { get; set; }

        [Required(ErrorMessage = "Необходимо указать код валюты")]
        [MaxLength(5)]
        [Display(Name = "Код валюты")]
        public string CurrencyName { get; set; }

        [Required(ErrorMessage = "Необходимо указать сумму")]
        [RegularExpression("(^\\d{1,10}(\\,\\d{1,2})?$)", ErrorMessage = "Проверьте правильность ввода суммы")]
        [Display(Name = "Сумма")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Sum { get; set; }

        //------------------------------------------------------
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}