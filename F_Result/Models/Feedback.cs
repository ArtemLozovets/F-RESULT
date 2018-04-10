using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace F_Result.Models
{
    [Table("Feedback")]
    public class Feedback
    {
        public enum StateEnum
        {
            Добавлен = 1,
            Рассмотрение = 4,
            Принят = 8,
            Отклонен = 12
        }

        public int FeedbackId { get; set; }

        [StringLength(1000, ErrorMessage = "Отзыв должен содержать не более 1000 символов")]
        [Display(Name = "Отзыв")]
        public string Comment { get; set; }

        [StringLength(128, ErrorMessage = "Ссылка должна содержать не более 128 символов")]
        [Display(Name = "Ссылка")]
        public string SbUrl { get; set; }

        [Display(Name = "Дата добавления")]
        public DateTime DateOfCreation { get; set; }

        public string UserId { get; set; }

        public string ApprovedUserId { get; set; }

        [Display(Name = "Статус")]
        public StateEnum Status { get; set; }

        [Display(Name = "Дата рассмотрения")]
        public DateTime? DateOfApproved { get; set; }

        //------------------------------------------------------
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ApplicationUser ApplicationUser1 { get; set; }

        [NotMapped]
        [Display(Name = "Пользователь")]
        public string UserFN { get; set; }

        [NotMapped]
        [Display(Name = "Рассмотрено")]
        public string ApprovedUserFN { get; set; }

    }
}