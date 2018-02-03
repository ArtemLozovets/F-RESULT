using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic;
using F_Result.Methods;
using System.Collections.Generic; //!=====!

namespace F_Result.Controllers
{
    public class PlanDebitsF2Controller : Controller
    {
        private FRModel db = new FRModel();

        // План расходов
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant, Financier")]
        public ActionResult PDShow(int? ProjectId, string startDate, string endDate)
        {
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");

            if (ProjectId != null)
            {
                string ProjectName = db.Projects.FirstOrDefault(x => x.id == ProjectId).ShortName.ToString();
                ViewData["ProjectName"] = ProjectName;
            }

            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                ViewData["Period"] = startDate + " - " + endDate;
            }

            return View();
        }

        // Таблица плана расходов
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant, Financier")]
        [HttpPost]
        public ActionResult LoadPD()
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанного сотрудника для пользователя в роли "Руководитель проекта"

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                string _expname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _projectname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _chname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _organizationname = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _appointment = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startagrdate = null;
                DateTime? _endagrdate = null;
                string _datetext = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
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
                string _sum = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();
                string _periodtxt = Request.Form.GetValues("columns[8][search][value]").FirstOrDefault().ToString();
                int _period;
                Int32.TryParse(_periodtxt, out _period);

                var _ads = (from plandebit in db.PlanDebitsF2
                            join prg in db.Projects on plandebit.ProjectId equals prg.id
                            join org in db.Organizations on plandebit.OrganizationId equals org.id
                            join exp in db.Expenditures on plandebit.ExpenditureId equals exp.Id
                            join usr in db.IdentityUsers on plandebit.UserId equals usr.Id into usrtmp
                            from usr in usrtmp.DefaultIfEmpty()
                            join pperiod in db.PlanningPeriods on plandebit.PeriodId equals pperiod.PlanningPeriodId
                            where (plandebit.Date >= _startagrdate && plandebit.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (prg.ChiefName.Contains(_chname) || string.IsNullOrEmpty(_chname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (exp.Name.Contains(_expname) || string.IsNullOrEmpty(_expname))
                                        && (plandebit.Appointment.Contains(_appointment) || string.IsNullOrEmpty(_appointment))
                                        && (pperiod.PlanningPeriodId == _period || String.IsNullOrEmpty(_periodtxt))
                                        && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(prg.Chief ?? 0)) //Фильтрация записей по проектам для руководителей проектов
                            select new
                            {
                                PlanDebitF2Id = plandebit.PlanDebitF2Id,
                                Date = plandebit.Date,
                                Sum = plandebit.Sum,
                                ProjectId = plandebit.ProjectId,
                                OrgId = org.id,
                                Appointment = plandebit.Appointment,
                                UserId = plandebit.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                                ProjectName = prg.ShortName,
                                ProjectType = prg.ProjectType,
                                ChiefName = prg.ChiefName,
                                ProjectManagerName = prg.ProjectManagerName,
                                StartDatePlan = prg.StartDatePlan,
                                StartDateFact = prg.StartDateFact,
                                OrgName = org.Title,
                                ExptId = exp.Id,
                                ExpName = exp.Name,
                                PeriodName = pperiod.PeriodName,
                                planBenefit = prg.planBenefit,
                                planExpand = prg.planExpand
                            }).AsEnumerable().Select(x => new PlanDebitViewF2
                            {
                                PlanDebitF2Id = x.PlanDebitF2Id,
                                Date = x.Date,
                                Sum = x.Sum,
                                ProjectId = x.ProjectId,
                                ProjectName = x.ProjectName,
                                ProjectType = x.ProjectType,
                                ChiefName = x.ChiefName,
                                ProjectManagerName = x.ProjectManagerName,
                                StartDatePlan = x.StartDatePlan,
                                StartDateFact = x.StartDateFact,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                ExpenditureId = x.ExptId,
                                ExpenditureName = x.ExpName,
                                Appointment = x.Appointment,
                                UserId = x.UserId,
                                UserFN = x.UserFN,
                                PeriodName = x.PeriodName,
                                planBenefit = x.planBenefit,
                                planExpand = x.planExpand
                            }).ToList();

                _ads = _ads.Where(x => ((x.Sum.ToString().Contains(_sum)) || string.IsNullOrEmpty(_sum)) && (x.UserFN.Contains(_userfn) || String.IsNullOrEmpty(_userfn))).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", PlanDebitF2Id desc").ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.PlanDebitF2Id).ToList();
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

        // План расходов, подробная информация
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant, Financier")]
        public ActionResult PDDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PlanDebitF2 planDebit = (from _pd in db.PlanDebitsF2
                                     join _pname in db.Projects on _pd.ProjectId equals _pname.id
                                     join _org in db.Organizations on _pd.OrganizationId equals _org.id
                                     join _exp in db.Expenditures on _pd.ExpenditureId equals _exp.Id
                                     join _period in db.PlanningPeriods on _pd.PeriodId equals _period.PlanningPeriodId
                                     where (_pd.PlanDebitF2Id == id)
                                     select new
                                     {
                                         PlanDebitF2Id = _pd.PlanDebitF2Id,
                                         Sum = _pd.Sum,
                                         Date = _pd.Date,
                                         ProjectId = _pd.ProjectId,
                                         ProjectName = _pname.ShortName,
                                         OrganizationName = _org.Title,
                                         ExpendituresName = _exp.Name,
                                         PeriodName = _period.PeriodName,
                                         Appointment = _pd.Appointment
                                     }).AsEnumerable().Select(x => new PlanDebitF2
                                   {
                                       PlanDebitF2Id = x.PlanDebitF2Id,
                                       Sum = x.Sum,
                                       Date = x.Date,
                                       ProjectId = x.ProjectId,
                                       ProjectName = x.ProjectName,
                                       OrganizationName = x.OrganizationName,
                                       ExpenditureName = x.ExpendituresName,
                                       PeriodName = x.PeriodName,
                                       Appointment = x.Appointment
                                   }).FirstOrDefault();
            if (planDebit == null)
            {
                return HttpNotFound();
            }

            //Проверяем наличие у пользователя прав для работы с данной сущностью
            if (!UsrWksMethods.isAllowed(db, planDebit.ProjectId))
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }

            return View(planDebit);
        }

        // Добавление плана расходов GET
        [Authorize(Roles = "Administrator, ProjectManager, Financier")]
        public ActionResult PDCreate()
        {
            PlanDebitF2 _model = new PlanDebitF2();
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            _model.Date = DateTime.Today;
            return View(_model);
        }

        // Добавление плана расходов POST
        [Authorize(Roles = "Administrator, ProjectManager, Financier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PDCreate([Bind(Include = "PlanDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,UserId,PeriodId,ExpenditureId")] PlanDebitF2 planDebit)
        {
            if (ModelState.IsValid)
            {
                //Запрещаем руководителям проектов добавление/редактирование планов текущего и предыдущих месяцев
                int NextMonth = DateTime.Today.Month + 1;
                int WYear = DateTime.Today.Year;
                if (NextMonth > 12)
                {
                    NextMonth = 1;
                    WYear += 1;
                }
                int PlanMonth = planDebit.Date.Month;
                int PlanYear = planDebit.Date.Year;
                bool isPrgManager = System.Web.HttpContext.Current.User.IsInRole("ProjectManager");
                if (isPrgManager && ((PlanMonth < NextMonth && PlanYear <= DateTime.Today.Year)))
                {
                    TempData["MessageError"] = "Руководителям проектов запрещено добавление/редактирование планов текущего и предыдущих месяцев";
                    ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
                    ViewData["prgSelect"] = db.Projects.FirstOrDefault(x => x.id == planDebit.ProjectId).ShortName;
                    ViewData["orgSelect"] = db.Organizations.FirstOrDefault(x => x.id == planDebit.OrganizationId).Title;
                    ViewData["expSelect"] = db.Expenditures.FirstOrDefault(x => x.Id == planDebit.ExpenditureId).Name;
                    return View(planDebit);
                }

                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        planDebit.UserId = user;
                    }

                    db.PlanDebitsF2.Add(planDebit);
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

        // Редактирование плана расходов GET
        [Authorize(Roles = "Administrator, ProjectManager, Financier")]
        public ActionResult PDEdit(int? id, string isClone)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDebitF2 planDebit = db.PlanDebitsF2.Find(id);
            if (planDebit == null)
            {
                return HttpNotFound();
            }

            //Проверяем наличие у пользователя прав для работы с данной сущностью
            if (!UsrWksMethods.isAllowed(db, planDebit.ProjectId))
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            planDebit.ExpenditureName = db.Expenditures.Where(x => x.Id == planDebit.ExpenditureId).Select(x => x.Name).FirstOrDefault().ToString();
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");

            if (Convert.ToBoolean(isClone))
            {
                planDebit.Date = DateTime.Today;
                return View("PDClone", planDebit);
            }
            else
            {
                return View(planDebit);
            }

        }


        // Редактирование плана расходов POST
        [Authorize(Roles = "Administrator, ProjectManager, Financier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PDEdit([Bind(Include = "PlanDebitF2Id,Date,Sum,ProjectId,OrganizationId,Appointment,PeriodId, ExpenditureId")] PlanDebitF2 planDebit)
        {

            //Проверяем наличие у пользователя прав для работы с данной сущностью
            if (!UsrWksMethods.isAllowed(db, planDebit.ProjectId))
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }

            if (ModelState.IsValid)
            {
                //Запрещаем руководителям проектов добавление/редактирование планов текущего и предыдущих месяцев
                int NextMonth = DateTime.Today.Month + 1;
                int WYear = DateTime.Today.Year;
                if (NextMonth > 12)
                {
                    NextMonth = 1;
                    WYear += 1;
                }
                int PlanMonth = planDebit.Date.Month;
                int PlanYear = planDebit.Date.Year;
                bool isPrgManager = System.Web.HttpContext.Current.User.IsInRole("ProjectManager");
                if (isPrgManager && ((PlanMonth < NextMonth && PlanYear <= DateTime.Today.Year)))
                {
                    TempData["MessageError"] = "Руководителям проектов запрещено добавление/редактирование планов текущего и предыдущих месяцев";
                    ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
                    ViewData["prgSelect"] = db.Projects.FirstOrDefault(x => x.id == planDebit.ProjectId).ShortName;
                    ViewData["orgSelect"] = db.Organizations.FirstOrDefault(x => x.id == planDebit.OrganizationId).Title;
                    ViewData["expSelect"] = db.Expenditures.FirstOrDefault(x => x.Id == planDebit.ExpenditureId).Name;
                    return View(planDebit);
                }

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
            planDebit.ExpenditureName = db.Expenditures.Where(x => x.Id == planDebit.ExpenditureId).Select(x => x.Name).FirstOrDefault().ToString();
            return View(planDebit);
        }


        // Удаление плана расходов GET
        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult PDDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDebitF2 planDebit = db.PlanDebitsF2.Find(id);
            if (planDebit == null)
            {
                return HttpNotFound();
            }

            //Проверяем наличие у пользователя прав для работы с данной сущностью
            if (!UsrWksMethods.isAllowed(db, planDebit.ProjectId))
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            planDebit.ExpenditureName = db.Expenditures.Where(x => x.Id == planDebit.ExpenditureId).Select(x => x.Name).FirstOrDefault().ToString();

            return View(planDebit);
        }


        // Удаление плана расходов POST
        [Authorize(Roles = "Administrator, ProjectManager")]
        [HttpPost, ActionName("PDDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            try
            {
                PlanDebitF2 planDebit = db.PlanDebitsF2.FirstOrDefault(x => x.PlanDebitF2Id == id);
                if (planDebit == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("PDShow");
                }

                //Проверяем наличие у пользователя прав для работы с данной сущностью
                if (!UsrWksMethods.isAllowed(db, planDebit.ProjectId))
                {
                    return View("~/Views/Shared/AccessDenied.cshtml");
                }

                db.PlanDebitsF2.Remove(planDebit);
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
