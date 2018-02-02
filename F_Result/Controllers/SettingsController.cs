using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic;
using System.Collections.Generic;

namespace F_Result.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SettingsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: Settings
        public ActionResult ShowSettings()
        {
            List<Settings> _settings = db.Settings.ToList();

            return View(_settings);
        }

        public JsonResult WriteSetting(string pName, string pValue, string pNote)
        {
            using (ApplicationDbContext aspdb = new ApplicationDbContext())
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();

                    Settings _set = db.Settings.FirstOrDefault(x => x.SettingName == pName);
                    if (_set == null)
                    {
                        _set = new Settings();
                        _set.ModificationDate = DateTime.Now;
                        _set.UserId = user;
                        _set.SettingName = pName;
                        _set.SettingValue = pValue;
                        _set.Note = pNote;

                        db.Settings.Add(_set);
                    }
                    else
                    {
                        _set.ModificationDate = DateTime.Now;
                        _set.UserId = user;
                        _set.SettingValue = pValue;
                        _set.Note = pNote;

                        db.Entry(_set).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    String _message = "Информация обновлена";

                    return Json(new { Result = true, Message = _message }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                    return Json(new { Result = false, Message = errormessage }, JsonRequestBehavior.AllowGet);
                }
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
