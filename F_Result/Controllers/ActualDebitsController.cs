using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;

namespace F_Result.Controllers
{
    [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
    public class ActualDebitsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        public ActionResult ADShow()
        {
            return View(db.ActualDebit.ToList());
        }

        // GET: ActualDebits/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            if (actualDebit == null)
            {
                return HttpNotFound();
            }
            return View(actualDebit);
        }

        public ActionResult ADCreate()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADCreate([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,Appointment,DocNumber,ApplicationUser")] ActualDebit actualDebit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        actualDebit.UserId = user;
                    }

                    db.ActualDebit.Add(actualDebit);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("ADShow");
                }
                catch (Exception ex)
                {
                    ViewBag.ErMes = ex.Message;
                    ViewBag.ErStack = ex.StackTrace;
                    ViewBag.ErInner = ex.InnerException.InnerException.Message;
                    return View("Error");
                }
            }

            TempData["MessageError"] = "Ошибка валидации модели";
            return View(actualDebit);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            if (actualDebit == null)
            {
                return HttpNotFound();
            }
            return View(actualDebit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,Appointment,DocNumber")] ActualDebit actualDebit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(actualDebit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ADShow");
            }
            return View(actualDebit);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            if (actualDebit == null)
            {
                return HttpNotFound();
            }
            return View(actualDebit);
        }

        // POST: ActualDebits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            db.ActualDebit.Remove(actualDebit);
            db.SaveChanges();
            return RedirectToAction("ADShow");
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
