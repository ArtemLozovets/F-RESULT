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
    [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
    public class PlanCreditsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        public ActionResult PCShow()
        {
            return View();
        }

        [HttpPost]
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

                string _datetxt = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                DateTime? _date = string.IsNullOrEmpty(_datetxt) ? (DateTime?)null : DateTime.Parse(_datetxt);
                string _sum = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _projectname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _organizationname = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _appoinment = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();

                var _ads = (from plancredit in db.PlanCredits
                            join prg in db.Projects on plancredit.ProjectId equals prg.id
                            join org in db.Organizations on plancredit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on plancredit.UserId equals usr.Id
                            where (plancredit.Date == _date || _date == null)
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (plancredit.Appointment.Contains(_appoinment) || string.IsNullOrEmpty(_appoinment))
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
                                UserFN = usr.LastName + " " + usr.FirstName + " " + usr.MiddleName,
                                ProjectName = prg.ShortName,
                                OrgName = org.Title
                            }).AsEnumerable().Select(x => new ActualDebit
                            {
                                ActualDebitId = x.PlanCreditId,
                                Date = x.Date,
                                Sum = x.Sum,
                                ProjectId = x.ProjectId,
                                ProjectName = x.ProjectName,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                Appointment = x.Appointment,
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


        // GET: PlanCredits/Details/5
        public ActionResult Details(int? id)
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
            return View(planCredit);
        }

        // GET: PlanCredits/Create
        public ActionResult PCCreate()
        {
            return View();
        }

        // POST: PlanCredits/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PCCreate([Bind(Include = "PlanCreditId,Date,Sum,ProjectId,OrganizationId,Appointment,UserId")] PlanCredit planCredit)
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

        // GET: PlanCredits/Edit/5
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
            return View(planCredit);
        }

        // POST: PlanCredits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PCEdit([Bind(Include = "PlanCreditId,Date,Sum,ProjectId,OrganizationId,Appointment,UserId")] PlanCredit planCredit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(planCredit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PCShow");
            }
            return View(planCredit);
        }

        // GET: PlanCredits/Delete/5
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
            return View(planCredit);
        }

        // POST: PlanCredits/Delete/5
        [HttpPost, ActionName("PCDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PlanCredit planCredit = db.PlanCredits.Find(id);
            db.PlanCredits.Remove(planCredit);
            db.SaveChanges();
            return RedirectToAction("PCShow");
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
