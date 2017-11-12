using System;
using System.ComponentModel.DataAnnotations;

namespace F_Result.Models
{
    public partial class Projects
    {
        [Key]
        public int id { get; set; }

        [MaxLength(120)]
        [Display(Name = "Название")]
        public string FullName {get; set;}
 
        [MaxLength(512)]
        [Display(Name = "Короткое название")]
        public string ShortName {get; set;}
        
        public int? Chief {get; set;}

        [MaxLength(512)]
        [Display(Name = "Руководитель проекта")]
        public string ChiefName { get; set; }

        public int? ProjectManager{get; set;}
        
        [MaxLength(512)]
        [Display(Name = "Менеджер проекта")]
        public string ProjectManagerName { get; set; }
        

        [Display(Name = "Инженер проекта")]
        public int? GIP {get; set;}


        public DateTime? StartDateFact{get; set;}
        public DateTime? EndDateFact{get; set;}
        public DateTime? StartDatePlan{get; set;}
        public DateTime? EndDatePlan{get; set;}
        
        [MaxLength(255)]
        [Display(Name = "Описание")]
        public string Desc{get; set;}

        [MaxLength(512)]
        [Display(Name = "Тип проекта")]
        public string ProjectType{get; set;}

        [MaxLength(1)]
        [Display(Name = "Состояние")]
        public string State{get; set;}
		
    }
}