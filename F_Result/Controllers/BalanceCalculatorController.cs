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
        public ActionResult AddBalance(string balanceDate, string currData)
        {
            if (string.IsNullOrEmpty(balanceDate) || string.IsNullOrEmpty(currData))
            {
                return Json(new { result = false, message = "Отсутствуют необходимые параметры" }, JsonRequestBehavior.AllowGet);
            }

            DateTime _balanceDate = new DateTime();

            if (!DateTime.TryParse(balanceDate, out _balanceDate)) {
                return Json(new { result = false, message = "Неверный формат даты остатка" }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    string user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    dynamic currDataJson = JsonConvert.DeserializeObject(currData);

                    foreach (var item in currDataJson)
                    {
                        Balance _balance = new Balance()
                        {
                            UserId = user,
                            DateOfCreation = DateTime.Now,
                            BalanceDate = _balanceDate,
                            CurrencyName = item.Name,
                            Sum = item.Value
                        };

                        db.Balance.Add(_balance);
                    }
                }

                db.SaveChanges();
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