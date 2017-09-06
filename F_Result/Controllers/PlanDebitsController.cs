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
    public class PlanDebitsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
        public ActionResult PDShow()
        {
            return View();
        }

        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
        [HttpPost]
        public ActionResult LoadPD()
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
                string _appointment = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startagrdate = null;
                DateTime? _endagrdate = null;
                string _datetext = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
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
                string _sum = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();

                var _ads = (from plandebit in db.PlanDebits
                            join prg in db.Projects on plandebit.ProjectId equals prg.id
                            join org in db.Organizations on plandebit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on plandebit.UserId equals usr.Id
                            where (plandebit.Date >= _startagrdate && plandebit.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (plandebit.Appointment.Contains(_appointment) || string.IsNullOrEmpty(_appointment))
                                        && (usr.LastName.Contains(_userfn)
                                            || usr.FirstName.Contains(_userfn)
                                            || usr.MiddleName.Contains(_userfn)
                                            || string.IsNullOrEmpty(_userfn))
                            select new
                            {
                                PlanDebitId = plandebit.PlanDebitId,
                                Date = plandebit.Date,
                                Sum = plandebit.Sum,
                                ProjectId = plandebit.ProjectId,
                                OrgId = org.id,
                                Appointment = plandebit.Appointment,
                                UserId = plandebit.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                                ProjectName = prg.ShortName,
                                OrgName = org.Title
                            }).AsEnumerable().Select(x => new PlanDebit
                            {
                                PlanDebitId = x.PlanDebitId,
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
        public ActionResult PDDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDebit planDebit = db.PlanDebits.Find(id);
            if (planDebit == null)
            {
                return HttpNotFound();
            }

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();

            return View(planDebit);
        }

        // GET: PlanCredits/Create
        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PDCreate()
        {
            PlanDebit _model = new PlanDebit();
            _model.Date = DateTime.Today;
            return View(_model);
        }

        // POST: PlanCredits/Create
        [Authorize(Roles = "Administrator, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PDCreate([Bind(Include = "PlanDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,UserId,PeriodId")] PlanDebit planDebit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        planDebit.UserId = user;
                    }

                    db.PlanDebits.Add(planDebit);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("PDShow");
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
            return View(planDebit);
        }

        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PDEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDebit planDebit = db.PlanDebits.Find(id);
            if (planDebit == null)
            {
                return HttpNotFound();
            }

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();

            return View(planDebit);
        }


        [Authorize(Roles = "Administrator, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PDEdit([Bind(Include = "PlanDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,PeriodId")] PlanDebit planDebit)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        planDebit.UserId = user;
                    }

                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

                    db.Entry(planDebit).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
                    return RedirectToAction("PDShow");
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

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();

            return View(planDebit);
        }


        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PDDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDebit planDebit = db.PlanDebits.Find(id);
            if (planDebit == null)
            {
                return HttpNotFound();
            }

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();

            return View(planDebit);
        }


        [Authorize(Roles = "Administrator, ProjectManager")]
        // POST: ActualDebits/Delete/5
        [HttpPost, ActionName("PDDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            try
            {
                PlanDebit planDebit = db.PlanDebits.FirstOrDefault(x => x.PlanDebitId == id);
                if (planDebit == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("PDShow");
                }

                db.PlanDebits.Remove(planDebit);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
                return RedirectToAction("PDShow");
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
