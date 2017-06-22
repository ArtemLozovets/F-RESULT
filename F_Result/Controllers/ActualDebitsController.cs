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
using System.Linq.Dynamic; //!=====!

namespace F_Result.Controllers
{
    [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
    public class ActualDebitsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        public ActionResult ADShow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadAD()
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;


                string _datetxt = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                DateTime? _date = string.IsNullOrEmpty(_datetxt) ? (DateTime?)null : DateTime.Parse(_datetxt);
                string _sum = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _projectname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _organizationname = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _appoinment = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _docnumber = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();



                var _ads = (from actualdebit in db.ActualDebit
                            join prg in db.Projects on actualdebit.ProjectId equals prg.id
                            join org in db.Organizations on actualdebit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on actualdebit.UserId equals usr.Id
                            where (actualdebit.Date == _date || _date == null)
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (actualdebit.Appointment.Contains(_appoinment) || string.IsNullOrEmpty(_appoinment))
                                        && (actualdebit.DocNumber.Contains(_docnumber) || string.IsNullOrEmpty(_docnumber))
                                        && (usr.LastName.Contains(_userfn)
                                            || usr.FirstName.Contains(_userfn)
                                            || usr.MiddleName.Contains(_userfn) 
                                            || string.IsNullOrEmpty(_userfn))
                            select new
                         {
                             ActualDebitId = actualdebit.ActualDebitId,
                             Date = actualdebit.Date,
                             Sum = actualdebit.Sum,
                             ProjectId = actualdebit.ProjectId,
                             OrgId = org.id,
                             Appointment = actualdebit.Appointment,
                             DocNumber = actualdebit.DocNumber,
                             UserId = actualdebit.UserId,
                             UserFN = usr.LastName + " " + usr.FirstName + " " + usr.MiddleName,
                             ProjectName = prg.ShortName,
                             OrgName = org.Title
                         }).AsEnumerable().Select(x => new ActualDebit
                            {
                                ActualDebitId = x.ActualDebitId,
                                Date = x.Date,
                                Sum = x.Sum,
                                ProjectId = x.ProjectId,
                                ProjectName = x.ProjectName,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                Appointment = x.Appointment,
                                DocNumber = x.DocNumber,
                                UserId = x.UserId,
                                UserFN = x.UserFN
                            }).ToList();

                _ads = _ads.Where(x => (x.Sum.ToString().Contains(_sum)) || string.IsNullOrEmpty(_sum)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                totalRecords = _ads.Count();

                var data = _ads.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
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


        [Authorize(Roles = "Administrator, Accountant")]
        public ActionResult ADCreate()
        {
            return View();
        }


        [Authorize(Roles = "Administrator, Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADCreate([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,DocNumber,ApplicationUser")] ActualDebit actualDebit)
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


        [Authorize(Roles = "Administrator, Accountant")]
        public ActionResult ADEdit(int? id)
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

            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            ViewData["ProjectName"] = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;

            return View(actualDebit);
        }


        [Authorize(Roles = "Administrator, Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADEdit([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,DocNumber")] ActualDebit actualDebit)
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

                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
                    db.Entry(actualDebit).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
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
            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            ViewData["ProjectName"] = _prgName;
            return View(actualDebit);
        }


        [Authorize(Roles = "Administrator, Accountant")]
        public ActionResult ADDelete(int? id)
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


        [Authorize(Roles = "Administrator, Accountant")]
        // POST: ActualDebits/Delete/5
        [HttpPost, ActionName("ADDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            try
            {
                ActualDebit actualDebit = db.ActualDebit.FirstOrDefault(x => x.ActualDebitId == id);
                if (actualDebit == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("ADShow");
                }

                db.ActualDebit.Remove(actualDebit);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
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
