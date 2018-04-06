using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;

namespace F_Result.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private FRModel db = new FRModel();

        public ActionResult CommentsShow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CommentsCreate(string cmText, string cmUrl)
        {
            if (string.IsNullOrEmpty(cmText) || string.IsNullOrEmpty(cmUrl))
            {
                return Json(new { result = false, message = "Отсутствуют необходимые параметры" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Feedback fb = new Feedback()
                {
                    DateOfCreation = DateTime.Today,
                    Comment = cmText,
                    SbUrl = cmUrl,
                    Status = 1
                };

                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    fb.UserId = user;
                }

                db.Feedback.Add(fb);
                db.SaveChanges();
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}