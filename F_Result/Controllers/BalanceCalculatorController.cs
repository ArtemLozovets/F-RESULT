using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic;
using System.Data.Entity;
using Newtonsoft.Json; //!=====!

namespace F_Result.Controllers
{
    [Authorize]
    public class BalanceCalculatorController : Controller
    {
        private FRModel db = new FRModel();

        #region ----------- Добавление остатка -------------
        public ActionResult AddBalance(string balanceDate)
        {
            if (string.IsNullOrEmpty(balanceDate) || string.IsNullOrEmpty(balanceDate))
            {
                return Json(new { result = false, message = "Отсутствуют необходимые параметры" }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                }

                //  db.Feedback.Add(fb);
                //db.SaveChanges();
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}