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
using System.Web.Script.Serialization;

namespace F_Result.Controllers
{
    public class PlanDebitsController : Controller
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

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                ViewData["Period"] = startDate + " - " + endDate;
            }

            return View();
        }

        // Таблица плана расходов
        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant, Financier")]
        [HttpPost]
        public ActionResult LoadPD(int[] filterPrjIDs, int[] filterOrgIDs)
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

                string _projectname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _chname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _organizationname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _appointment = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startagrdate = null;
                DateTime? _endagrdate = null;
                string _datetext = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_datetext))
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
                string _periodtxt = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();
                int _period;
                int.TryParse(_periodtxt, out _period);

                var _ads = (from plandebit in db.PlanDebits
                            join prg in db.Projects on plandebit.ProjectId equals prg.id
                            join ipa in db.ActivityIndexes on plandebit.ProjectId equals ipa.ProjectId into ipatmp
                            from ipa in ipatmp.DefaultIfEmpty()
                            join org in db.Organizations on plandebit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on plandebit.UserId equals usr.Id into usrtmp
                            from usr in usrtmp.DefaultIfEmpty()
                            join pperiod in db.PlanningPeriods on plandebit.PeriodId equals pperiod.PlanningPeriodId
                            where (plandebit.Date >= _startagrdate && plandebit.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (prg.ChiefName.Contains(_chname) || string.IsNullOrEmpty(_chname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (plandebit.Appointment.Contains(_appointment) || string.IsNullOrEmpty(_appointment))
                                        && (pperiod.PlanningPeriodId == _period || string.IsNullOrEmpty(_periodtxt))
                                //Фильтрация записей по проектам для руководителей проектов                                               
                                        && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(prg.Chief ?? 0) || WorkerIdsList.Contains(prg.ProjectManager ?? 0))
                            select new
                            {
                                PlanDebitId = plandebit.PlanDebitId,
                                Date = plandebit.Date,
                                Sum = plandebit.Sum,
                                ProjectId = plandebit.ProjectId,
                                ipa = ipa.IPAValue,
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
                                PeriodName = pperiod.PeriodName,
                                planBenefit = prg.planBenefit,
                                planExpand = prg.planExpand
                            }).AsEnumerable().Select(x => new PlanDebitView
                            {
                                PlanDebitId = x.PlanDebitId,
                                Date = x.Date,
                                Sum = x.Sum,
                                ProjectId = x.ProjectId,
                                IPA = x.ipa,
                                ProjectName = x.ProjectName,
                                ProjectType = x.ProjectType,
                                ChiefName = x.ChiefName,
                                ProjectManagerName = x.ProjectManagerName,
                                StartDatePlan = x.StartDatePlan,
                                StartDateFact = x.StartDateFact,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                Appointment = x.Appointment,
                                UserId = x.UserId,
                                UserFN = x.UserFN,
                                PeriodName = x.PeriodName,
                                planBenefit = x.planBenefit,
                                planExpand = x.planExpand
                            }).ToList();

                _ads = _ads.Where(x => (x.Sum.ToString().Contains(_sum) || string.IsNullOrEmpty(_sum))
                                       && (x.UserFN.Contains(_userfn) || string.IsNullOrEmpty(_userfn))
                                       && (filterPrjIDs == null || filterPrjIDs.Length == 0 || filterPrjIDs.Contains(x.ProjectId))
                                       && (filterOrgIDs == null || filterOrgIDs.Length == 0 || filterOrgIDs.Contains(x.OrganizationId))
                                  ).ToList();

                //Список ID для передачи в ф-цию экспорта в Excel
                List<int> _IDsList = _ads.Select(x => x.PlanDebitId).ToList();

                List<APBFilterIDs> _prjList = _ads.GroupBy(x => x.ProjectId)
                    .Select(x => new APBFilterIDs {
                        PrjId = x.Select(z => z.ProjectId).First(),
                        ProjectName = x.Select(z => z.ProjectName).First(),
                        IPA = x.Select(z => z.IPA).First()
                    }).ToList();

                List<ArticlesIDs> _organizationList = _ads.GroupBy(x => x.OrganizationId).
                    Select(x => new ArticlesIDs
                    {
                        AtId = x.Select(a => a.OrganizationId).FirstOrDefault(),
                        AtName = x.Select(a => a.OrganizationName).FirstOrDefault()
                    }).ToList();

                var jsonSerialiser = new JavaScriptSerializer();
                var _prjListJson = jsonSerialiser.Serialize(_prjList);
                var _orgListJson = jsonSerialiser.Serialize(_organizationList);
                var _IDsListJson = jsonSerialiser.Serialize(_IDsList);


                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", PlanDebitId desc").ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.PlanDebitId).ToList();
                }

                var fSum = _ads.Sum(x => x.Sum);

                totalRecords = _ads.Count();

                var data = _ads.Skip(skip).Take(pageSize);
                return Json(new { fsum = fSum
                    , draw = draw
                    , prjlist = _prjListJson
                    , orglist = _orgListJson
                    , idslist = _IDsListJson
                    , sortcolumn = sortColumn
                    , sortdir = sortColumnDir
                    , recordsFiltered = totalRecords
                    , recordsTotal = totalRecords
                    , data = data
                    , errormessage = "" }, JsonRequestBehavior.AllowGet);

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

            PlanDebit planDebit = (from _pd in db.PlanDebits
                                   join _pname in db.Projects on _pd.ProjectId equals _pname.id
                                   join _org in db.Organizations on _pd.OrganizationId equals _org.id
                                   join _period in db.PlanningPeriods on _pd.PeriodId equals _period.PlanningPeriodId
                                   where (_pd.PlanDebitId == id)
                                   select new
                                   {
                                       PlanDebitId = _pd.PlanDebitId,
                                       Sum = _pd.Sum,
                                       Date = _pd.Date,
                                       ProjectId = _pd.ProjectId,
                                       ProjectName = _pname.ShortName,
                                       OrganizationName = _org.Title,
                                       PeriodName = _period.PeriodName,
                                       Appointment = _pd.Appointment
                                   }).AsEnumerable().Select(x => new PlanDebit
                                   {
                                       PlanDebitId = x.PlanDebitId,
                                       Sum = x.Sum,
                                       Date = x.Date,
                                       ProjectId = x.ProjectId,
                                       ProjectName = x.ProjectName,
                                       OrganizationName = x.OrganizationName,
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
            PlanDebit _model = new PlanDebit();
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            _model.Date = DateTime.Today;
            return View(_model);
        }

        // Добавление плана расходов POST
        [Authorize(Roles = "Administrator, ProjectManager, Financier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PDCreate([Bind(Include = "PlanDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,UserId,PeriodId")] PlanDebit planDebit)
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

        // Редактирование плана расходов GET
        [Authorize(Roles = "Administrator, ProjectManager, Financier")]
        public ActionResult PDEdit(int? id, string isClone)
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

            //Проверяем наличие у пользователя прав для работы с данной сущностью
            if (!UsrWksMethods.isAllowed(db, planDebit.ProjectId))
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }

            planDebit.ProjectName = db.Projects.Where(x => x.id == planDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            planDebit.OrganizationName = db.Organizations.Where(x => x.id == planDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();

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
        public ActionResult PDEdit([Bind(Include = "PlanDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,PeriodId")] PlanDebit planDebit)
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
            PlanDebit planDebit = db.PlanDebits.Find(id);
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
                PlanDebit planDebit = db.PlanDebits.FirstOrDefault(x => x.PlanDebitId == id);
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
