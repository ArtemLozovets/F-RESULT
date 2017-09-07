﻿using System;
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
    public class PlanCreditsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
        public ActionResult PCShow()
        {
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
        public ActionResult LoadPC()
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
                string _userfn = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _periodtxt = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                int _period;
                Int32.TryParse(_periodtxt, out _period);
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

                var _ads = (from plancredit in db.PlanCredits
                            join prg in db.Projects on plancredit.ProjectId equals prg.id
                            join org in db.Organizations on plancredit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on plancredit.UserId equals usr.Id
                            join pperiod in db.PlanningPeriods on plancredit.PeriodId equals pperiod.PlanningPeriodId
                            where (plancredit.Date >= _startagrdate && plancredit.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (plancredit.Appointment.Contains(_appoinment) || string.IsNullOrEmpty(_appoinment))
                                        && (pperiod.PlanningPeriodId == _period || String.IsNullOrEmpty(_periodtxt))
                                        && (usr.LastName.Contains(_userfn)
                                            || usr.FirstName.Contains(_userfn)
                                            || usr.MiddleName.Contains(_userfn)
                                            || string.IsNullOrEmpty(_userfn))
                            select new
                            {
                                PlanCreditId = plancredit.PlanCreditId,
                                Date = plancredit.Date,
                                Sum = plancredit.Sum,
                                ProjectId = plancredit.ProjectId,
                                OrgId = org.id,
                                Appointment = plancredit.Appointment,
                                UserId = plancredit.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                                ProjectName = prg.ShortName,
                                OrgName = org.Title,
                                PeriodName = pperiod.PeriodName
                            }).AsEnumerable().Select(x => new PlanCredit
                            {
                                PlanCreditId = x.PlanCreditId,
                                Date = x.Date,
                                Sum = x.Sum,
                                ProjectId = x.ProjectId,
                                ProjectName = x.ProjectName,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                Appointment = x.Appointment,
                                UserId = x.UserId,
                                UserFN = x.UserFN,
                                PeriodName = x.PeriodName
                            }).ToList();

                _ads = _ads.Where(x => (x.Sum.ToString().Contains(_sum)) || string.IsNullOrEmpty(_sum)).ToList();

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
                return Json(new { fsum = fSum, draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: PlanCredits/Details/5
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
        public ActionResult PCDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanCredit planCredit = (from _pc in db.PlanCredits
                                   join _pname in db.Projects on _pc.ProjectId equals _pname.id
                                   join _org in db.Organizations on _pc.OrganizationId equals _org.id
                                   join _period in db.PlanningPeriods on _pc.PeriodId equals _period.PlanningPeriodId
                                   where (_pc.PlanCreditId == id)
                                   select new
                                   {
                                       PlanCreditId = _pc.PlanCreditId,
                                       Sum = _pc.Sum,
                                       Date = _pc.Date,
                                       ProjectName = _pname.ShortName,
                                       OrganizationName = _org.Title,
                                       PeriodName = _period.PeriodName,
                                       Appointment = _pc.Appointment
                                   }).AsEnumerable().Select(x => new PlanCredit
                                   {
                                       PlanCreditId = x.PlanCreditId,
                                       Sum = x.Sum,
                                       Date = x.Date,
                                       ProjectName = x.ProjectName,
                                       OrganizationName = x.OrganizationName,
                                       PeriodName = x.PeriodName,
                                       Appointment = x.Appointment
                                   }).FirstOrDefault();
            if (planCredit == null)
            {
                return HttpNotFound();
            }

            return View(planCredit);
        }

        // GET: PlanCredits/Create
        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PCCreate()
        {
            PlanCredit _model = new PlanCredit();
            _model.Date = DateTime.Today;
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            return View(_model);
        }

        // POST: PlanCredits/Create
        [HttpPost]
        [Authorize(Roles = "Administrator, ProjectManager")]
        [ValidateAntiForgeryToken]
        public ActionResult PCCreate([Bind(Include = "PlanCreditId,Date,Sum,ProjectId,OrganizationId,Appointment,UserId,PeriodId")] PlanCredit planCredit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        planCredit.UserId = user;
                    }
                    db.PlanCredits.Add(planCredit);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("PCShow");
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
            return View(planCredit);
        }


        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PCEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanCredit planCredit = db.PlanCredits.Find(id);
            if (planCredit == null)
            {
                return HttpNotFound();
            }

            string _prgName = db.Projects.Where(x => x.id == planCredit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            ViewData["ProjectName"] = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == planCredit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;

            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");

            return View(planCredit);
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, ProjectManager")]
        [ValidateAntiForgeryToken]
        public ActionResult PCEdit([Bind(Include = "PlanCreditId,Date,Sum,ProjectId,OrganizationId,Appointment,PeriodId")] PlanCredit planCredit)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        planCredit.UserId = user;
                    }

                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
                    db.Entry(planCredit).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
                    return RedirectToAction("PCShow");
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
            string _prgName = db.Projects.Where(x => x.id == planCredit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            ViewData["ProjectName"] = _prgName;
            return View(planCredit);
        }

        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PCDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanCredit planCredit = db.PlanCredits.Find(id);
            if (planCredit == null)
            {
                return HttpNotFound();
            }

            string _prgName = db.Projects.Where(x => x.id == planCredit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planCredit.ProjectName = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == planCredit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            planCredit.OrganizationName = _orgName;

            return View(planCredit);
        }


        // POST: ActualDebits/Delete/5
        [Authorize(Roles = "Administrator, ProjectManager")]
        [HttpPost, ActionName("PCDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                PlanCredit planCredit = db.PlanCredits.FirstOrDefault(x => x.PlanCreditId == id);
                if (planCredit == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("PCShow");
                }

                db.PlanCredits.Remove(planCredit);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
                return RedirectToAction("PCShow");
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
