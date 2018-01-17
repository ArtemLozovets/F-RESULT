using System.ComponentModel.DataAnnotations;

namespace F_Result.Models
{
    //Справочник статей
    public class Articles
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

    //Справочник статей расходов
    public class Expenditure : Articles { }

    //Справочник статей доходов
    public class Income : Articles { }
}