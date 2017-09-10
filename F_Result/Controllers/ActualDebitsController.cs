using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic; //!=====!

namespace F_Result.Controllers
{
    public class ActualDebitsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult ADShow()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Chief, Accountant")]
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
               
                string _projectname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _organizationname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _appoinment = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _docnumber = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startagrdate = null;
                DateTime? _endagrdate = null;
                string _datetext = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                if (!String.IsNullOrEmpty(_datetext))
                {
                    _datetext = _datetext.Trim();
                    int _length = (_datetext.Length) - (_datetext.IndexOf('-') + 2);
                    string _startagrdatetext = _datetext.Substring(0, _datetext.IndexOf('-')).Trim();
                    string _endagrdatetext = _datetext.Substring(_datetext.IndexOf('-') + 2, _length).Trim();
                    _startagrdate = DateTime.Parse(_startagrdatetext);
                    _endagrdate = DateTime.Parse(_endagrdatetext);
                }
                //--------------------------

                string _sum = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();

                var _ads = (from actualdebit in db.ActualDebit
                            join prg in db.Projects on actualdebit.ProjectId equals prg.id
                            join org in db.Organizations on actualdebit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on actualdebit.UserId equals usr.Id
                            where (actualdebit.Date >= _startagrdate && actualdebit.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (actualdebit.Appointment.Contains(_appoinment) || string.IsNullOrEmpty(_appoinment))
                                        && (actualdebit.DocNumber.Contains(_docnumber) || string.IsNullOrEmpty(_docnumber))
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
                             UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
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

                _ads = _ads.Where(x => ((x.Sum.ToString().Contains(_sum)) || string.IsNullOrEmpty(_sum)) && (x.UserFN.Contains(_userfn) || String.IsNullOrEmpty(_userfn))).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.Date).ToList();
                }

                var fSum = _ads.Sum(x => x.Sum);

                totalRecords = _ads.Count();

                var data = _ads.Skip(skip).Take(pageSize);
                return Json(new { sum = fSum, draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: PlanCredits/Details/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult ADDetails(int? id)
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
            actualDebit.ProjectName = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            actualDebit.OrganizationName = _orgName;

            return View(actualDebit);
        }


        [Authorize(Roles = "Administrator, Accountant")]
        public ActionResult ADCreate()
        {
            ActualDebit _model = new ActualDebit();
            _model.Date = DateTime.Today;
            return View(_model);
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
            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;
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

            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            actualDebit.ProjectName = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            actualDebit.OrganizationName = _orgName;

            return View(actualDebit);
        }


        // POST: ActualDebits/Delete/5
        [Authorize(Roles = "Administrator, Accountant")]
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
