using F_Result.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Web.Script.Serialization;
using F_Result.Methods; //!=====!
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace F_Result.Controllers
{
    public class ReportsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: Reports
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult AnalysisOfPayments()
        {
            return View();
        }

        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public JsonResult GetAOP(int? Year, string filterPrjIDs)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

            List<int> _filterPrjIDs = JsonConvert.DeserializeObject<List<int>>(filterPrjIDs);

            if (Year == null)
            {
                Year = DateTime.Today.Year;
            }

            var flt = _filterPrjIDs ?? Enumerable.Empty<int>(); //!---IF 'filterPrjIDs' is null 'filterPrjIDs.Contains(inpay.ProjectId)' raise exception 

            // График входящих платежей -------------------------------------------
            var _inPayments = (from inpay in db.Payments
                               join prg in db.Projects on inpay.ProjectId equals prg.id
                               join ipa in db.ActivityIndexes on inpay.ProjectId equals ipa.ProjectId into ipatmp
                               from ipa in ipatmp.DefaultIfEmpty()
                               where (inpay.PaymentDate.Value.Year == Year) && (filterPrjIDs == null || flt.Count() == 0 || flt.Contains(inpay.ProjectId))
                               select new
                               {
                                   PaymentDate = inpay.PaymentDate,
                                   Payment = inpay.Payment,
                                   ProjectId = prg.id,
                                   Project = prg.ShortName,
                                   IPA = ipa.IPAValue
                               }).ToList();

            var _inpaylist = (from t in _inPayments
                              group t by new { t.PaymentDate.Value.Year, t.PaymentDate.Value.Month } into g
                              where (g.Key.Year == Year)
                              select new
                              {
                                  Month = g.Key.Month,
                                  Total = g.Sum(t => t.Payment)
                              }).ToList();

            List<decimal?> _insum = new List<decimal?>();
            for (int i = 1; i < 13; i++)
            {
                if (_inpaylist.FirstOrDefault(x => x.Month == i) != null)
                {
                    _insum.Add(_inpaylist.FirstOrDefault(x => x.Month == i).Total);
                }
                else _insum.Add(0);
            }

            // График исходящих платежей -------------------------------------------
            var _outPayments = (from outpay in db.ActualDebit
                                join prg in db.Projects on outpay.ProjectId equals prg.id
                                join ipa in db.ActivityIndexes on outpay.ProjectId equals ipa.ProjectId into ipatmp
                                from ipa in ipatmp.DefaultIfEmpty()
                                where (outpay.Date.Year == Year) && (filterPrjIDs == null || flt.Count() == 0 || flt.Contains(outpay.ProjectId))
                                select new
                                {
                                    Date = outpay.Date,
                                    Sum = outpay.Sum,
                                    ProjectId = prg.id,
                                    Project = prg.ShortName,
                                    IPA = ipa.IPAValue
                                }).ToList();

            var _outpaylist = (from t in _outPayments
                               group t by new { t.Date.Year, t.Date.Month } into g
                               where g.Key.Year == Year
                               select new
                               {
                                   Month = g.Key.Month,
                                   Total = g.Sum(t => t.Sum)
                               }).ToList();

            List<decimal?> _outsum = new List<decimal?>();
            for (int i = 1; i < 13; i++)
            {
                if (_outpaylist.FirstOrDefault(x => x.Month == i) != null)
                {
                    _outsum.Add(_outpaylist.FirstOrDefault(x => x.Month == i).Total);
                }
                else _outsum.Add(0);
            }

            var _inSum = _insum.Sum(x => x.Value);
            var _inAvg = _insum.Average(x => x.Value);
            var _inMin = _insum.Min(x => x.Value);
            var _inMax = _insum.Max(x => x.Value);

            var _outSum = _outsum.Sum(x => x.Value);
            var _outAvg = _outsum.Average(x => x.Value);
            var _outMin = _outsum.Min(x => x.Value);
            var _outMax = _outsum.Max(x => x.Value);

            List<APBFilterIDs> _prjList = _inPayments
                   .GroupBy(x => x.ProjectId)
                   .Select(x => new APBFilterIDs
                   {
                       PrjId = x.Select(z => z.ProjectId).First(),
                       ProjectName = x.Select(z => z.Project).First(),
                       IPA = x.Select(z => z.IPA).First()
                   })
                   .OrderBy(x => x.IPA).ToList();

            List<APBFilterIDs> _prjList1 = _outPayments
                   .GroupBy(x => x.ProjectId)
                   .Select(x => new APBFilterIDs
                   {
                       PrjId = x.Select(z => z.ProjectId).First(),
                       ProjectName = x.Select(z => z.Project).First(),
                       IPA = x.Select(z => z.IPA).First()
                   })
                   .OrderBy(x => x.IPA).ToList();

            _prjList.Union(_prjList1);

            var jsonSerialiser = new JavaScriptSerializer();
            var _prjListJson = jsonSerialiser.Serialize(_prjList);

            return Json(new
            {
                Result = true,
                insum = _inSum,
                inavg = _inAvg,
                inmin = _inMin,
                inmax = _inMax,
                outsum = _outSum,
                outavg = _outAvg,
                outmin = _outMin,
                outmax = _outMax,
                ChartData = _insum,
                ChartDataA = _outsum,
                prjlist = _prjListJson
            }, JsonRequestBehavior.AllowGet);

        }

        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult AnalysisOfPlanPayments()
        {
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            return View();
        }

        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public JsonResult GetPlanData(int? Year, string filterPrjIDs)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

            List<int> _filterPrjIDs = JsonConvert.DeserializeObject<List<int>>(filterPrjIDs);

            if (Year == null)
            {
                Year = DateTime.Today.Year;
            }

            var flt = _filterPrjIDs ?? Enumerable.Empty<int>(); //!---IF 'filterPrjIDs' is null 'filterPrjIDs.Contains(inpay.ProjectId)' raise exception 

            // График плановых доходов -------------------------------------------

            var _planCredits = (from pc in db.PlanCredits
                                join prg in db.Projects on pc.ProjectId equals prg.id
                                join ipa in db.ActivityIndexes on pc.ProjectId equals ipa.ProjectId into ipatmp
                                from ipa in ipatmp.DefaultIfEmpty()
                                where (pc.Date.Year == Year) && (filterPrjIDs == null || flt.Count() == 0 || flt.Contains(pc.ProjectId))
                                select new
                                {
                                    Date = pc.Date,
                                    Sum = pc.Sum,
                                    ProjectId = prg.id,
                                    Project = prg.ShortName,
                                    IPA = ipa.IPAValue
                                }).ToList();

            var _pclist = (from t in _planCredits
                           group t by new { t.Date.Year, t.Date.Month } into g
                           where g.Key.Year == Year
                           select new
                           {
                               Month = g.Key.Month,
                               Total = g.Sum(t => t.Sum)
                           }).ToList();

            List<decimal?> _pcsum = new List<decimal?>();
            for (int i = 1; i < 13; i++)
            {
                if (_pclist.FirstOrDefault(x => x.Month == i) != null)
                {
                    _pcsum.Add(_pclist.FirstOrDefault(x => x.Month == i).Total);
                }
                else _pcsum.Add(0);
            }

            // График плановых расходов -------------------------------------------

            var _planDebits = (from pd in db.PlanDebits
                               join prg in db.Projects on pd.ProjectId equals prg.id
                               join ipa in db.ActivityIndexes on pd.ProjectId equals ipa.ProjectId into ipatmp
                               from ipa in ipatmp.DefaultIfEmpty()
                               where (pd.Date.Year == Year) && (filterPrjIDs == null || flt.Count() == 0 || flt.Contains(pd.ProjectId))
                               select new
                               {
                                   Date = pd.Date,
                                   Sum = pd.Sum,
                                   ProjectId = prg.id,
                                   Project = prg.ShortName,
                                   IPA = ipa.IPAValue
                               }).ToList();

            var _pdlist = (from t in _planDebits
                           group t by new { t.Date.Year, t.Date.Month } into g
                           where g.Key.Year == Year
                           select new
                           {
                               Month = g.Key.Month,
                               Total = g.Sum(t => t.Sum)
                           }).ToList();

            List<decimal?> _pdsum = new List<decimal?>();
            for (int i = 1; i < 13; i++)
            {
                if (_pdlist.FirstOrDefault(x => x.Month == i) != null)
                {
                    _pdsum.Add(_pdlist.FirstOrDefault(x => x.Month == i).Total);
                }
                else _pdsum.Add(0);
            }

            var _pcSum = _pcsum.Sum(x => x.Value);
            var _pcAvg = _pcsum.Average(x => x.Value);
            var _pcMin = _pcsum.Min(x => x.Value);
            var _pcMax = _pcsum.Max(x => x.Value);

            var _pdSum = _pdsum.Sum(x => x.Value);
            var _pdAvg = _pdsum.Average(x => x.Value);
            var _pdMin = _pdsum.Min(x => x.Value);
            var _pdMax = _pdsum.Max(x => x.Value);

            List<APBFilterIDs> _prjList = _planCredits
                    .GroupBy(x => x.ProjectId)
                    .Select(x => new APBFilterIDs
                    {
                        PrjId = x.Select(z => z.ProjectId).First(),
                        ProjectName = x.Select(z => z.Project).First(),
                        IPA = x.Select(z => z.IPA).First()
                    })
                    .OrderBy(x => x.IPA).ToList();

            List<APBFilterIDs> _prjList1 = _planDebits
                   .GroupBy(x => x.ProjectId)
                   .Select(x => new APBFilterIDs
                   {
                       PrjId = x.Select(z => z.ProjectId).First(),
                       ProjectName = x.Select(z => z.Project).First(),
                       IPA = x.Select(z => z.IPA).First()
                   })
                   .OrderBy(x => x.IPA).ToList();

            _prjList.Union(_prjList1);

            var jsonSerialiser = new JavaScriptSerializer();
            var _prjListJson = jsonSerialiser.Serialize(_prjList);

            return Json(new
            {
                Result = true,
                pcsum = _pcSum,
                pcavg = _pcAvg,
                pcmin = _pcMin,
                pcmax = _pcMax,
                pdsum = _pdSum,
                pdavg = _pdAvg,
                pdmin = _pdMin,
                pdmax = _pdMax,
                ChartData = _pcsum,
                ChartDataA = _pdsum,
                prjlist = _prjListJson
            }, JsonRequestBehavior.AllowGet);
        }


        //Отчет "Бюджетирование" GET
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier, ProjectManager")]
        public ActionResult AnalysisOfTheProjectBudget()
        {
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            return View();
        }


        //Получение данных для построения отчета "Бюджетирование" POST
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier, ProjectManager")]
        public JsonResult GetAPB(int? Period, DateTime? BaseDate, bool IsAllTimes, int[] filterPrjIDs, string ProjectName)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            try
            {
                if (Period == null || BaseDate == null)
                {
                    return Json(new { Result = false, data = "", errormessage = "Неверные параметры запроса" }, JsonRequestBehavior.AllowGet);
                }

                string PeriodName = db.PlanningPeriods.FirstOrDefault(x => x.PlanningPeriodId == Period).PeriodName.ToString();

                DateTime StartPeriod = DateTime.Now;
                DateTime EndPeriod = DateTime.Now;

                if (!IsAllTimes)
                {

                    int Year = BaseDate.Value.Year;
                    int Month = BaseDate.Value.Month;
                    int WeekDay = (Convert.ToInt32(BaseDate.Value.DayOfWeek) == 0) ? 7 : Convert.ToInt32(BaseDate.Value.DayOfWeek);

                    switch (PeriodName)
                    {
                        case "Год":
                            StartPeriod = new DateTime(Year, 1, 1);
                            EndPeriod = new DateTime(Year, 12, 31);
                            break;

                        case "Месяц":
                            StartPeriod = new DateTime(Year, Month, 1);
                            EndPeriod = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
                            break;

                        case "Неделя":
                            StartPeriod = BaseDate.Value.AddDays(1 - WeekDay);
                            EndPeriod = BaseDate.Value.AddDays(7 - WeekDay);
                            break;

                        case "Квартал":
                            int qNum = (BaseDate.Value.Month + 2) / 3;
                            switch (qNum)
                            {
                                case 1:
                                    StartPeriod = new DateTime(Year, 1, 1);
                                    EndPeriod = new DateTime(Year, 3, 31);
                                    break;

                                case 2:
                                    StartPeriod = new DateTime(Year, 4, 1);
                                    EndPeriod = new DateTime(Year, 6, 30);
                                    break;

                                case 3:
                                    StartPeriod = new DateTime(Year, 7, 1);
                                    EndPeriod = new DateTime(Year, 9, 30);
                                    break;

                                default:
                                    StartPeriod = new DateTime(Year, 10, 1);
                                    EndPeriod = new DateTime(Year, 12, 31);
                                    break;
                            }
                            break;

                        default:
                            StartPeriod = new DateTime(Year, Month, 1);
                            EndPeriod = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
                            break;
                    }
                }
                else
                {
                    StartPeriod = new DateTime(1900, 1, 1);
                    EndPeriod = new DateTime(2100, 12, 31);
                }

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                DateTime _stmp = Convert.ToDateTime(StartPeriod.ToString());
                string _startPeriod = _stmp.ToString("yyyyMMdd");
                DateTime _etmp = Convert.ToDateTime(EndPeriod.ToString());
                string _endPeriod = _etmp.ToString("yyyyMMdd");

                var _isAllTimes = IsAllTimes ? 1 : 0;


                //Запрос вызывает пользовательскую функцию "ufnAPBReport" хранящуюся на SQL-сервере.
                List<APBTableReport> _ads = db.Database.SqlQuery<APBTableReport>(String.Format("Select * from dbo.ufnAPBReport('{0}', '{1}', {2}, '{3}', {4})", _startPeriod, _endPeriod, Period, ProjectName, _isAllTimes)).ToList();

                //Проверяем роль пользователя
                bool isAdministrator = System.Web.HttpContext.Current.User.IsInRole("Administrator");
                bool isChief = System.Web.HttpContext.Current.User.IsInRole("Chief");
                bool isAccountant = System.Web.HttpContext.Current.User.IsInRole("Accountant");
                bool isFinancier = System.Web.HttpContext.Current.User.IsInRole("Financier");
                decimal? _PlanningBalance = null;
                if (isAdministrator || isChief || isAccountant || isFinancier)
                {
                    //Запрос вызывает пользовательскую функцию "ufnPlanningBalance" хранящуюся на SQL-сервере.
                    _PlanningBalance = db.Database.SqlQuery<decimal>("Select dbo.ufnPlanningBalance() as PlanningBalance").FirstOrDefault();
                }

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанных сотрудников для пользователя в роли "Руководитель проекта"

                _ads = _ads.Where(x =>
                            (filterPrjIDs == null
                            || filterPrjIDs.Length == 0
                            || filterPrjIDs.Contains(x.prj))
                            && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(x.Chief)) //Фильтрация записей по проектам для руководителей проектов
                            ).ToList();

                List<APBFilterIDs> _prjList = _ads
                    .Select(x => new APBFilterIDs
                    {
                        PrjId = x.prj,
                        ProjectName = x.ProjectName,
                        IPA = x.IPA
                    }).OrderBy(x => x.ProjectName).ToList();

                var jsonSerialiser = new JavaScriptSerializer();
                var _prjListJson = jsonSerialiser.Serialize(_prjList);

                APBTableReportTotal total = new APBTableReportTotal
                {
                    DebitPlanTotal = _ads.Sum(x => x.debitplan),
                    DebitFactTotal = _ads.Sum(x => x.debitfact),
                    dDeltaTotal = _ads.Sum(x => x.ddelta),
                    CreditPlanTotal = _ads.Sum(x => x.creditplan),
                    CreditFactTotal = _ads.Sum(x => x.creditfact),
                    cDeltaTotal = _ads.Sum(x => x.cdelta)
                };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (sortColumn == "ProjectName")
                    {
                        _ads = _ads.OrderBy("IPA " + sortColumnDir).ToList();
                    }
                    else
                    {
                        _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }

                totalRecords = _ads.Count();
                var data = _ads.Skip(skip).Take(pageSize);

                if (IsAllTimes && _ads.Count > 0)
                {
                    StartPeriod = _ads.Min(x => x.MinDate);
                    EndPeriod = _ads.Max(x => x.MaxDate);
                }

                var _startDate = StartPeriod.ToString("yyyyMMdd");
                var _endDate = EndPeriod.ToString("yyyyMMdd");

                return Json(new
                {
                    Result = true,
                    StartPeriod = _startDate,
                    EndPeriod = _endDate,
                    isAllTimes = IsAllTimes,
                    Period = Period,
                    ProjectName = ProjectName,
                    draw = draw,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords,
                    data = data,
                    total = total,
                    planningbalance = _PlanningBalance,
                    prjlist = _prjListJson,
                    sortcolumn = sortColumn,
                    sortdir = sortColumnDir,
                    errormessage = ""
                }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { Result = false, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //Отчет "Прибыльность проектов" GET
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier, ProjectManager")]
        public ActionResult AnalysisOfTheProjectProfitability()
        {
            return View();
        }

        //Получение данных для построения отчета "Прибыльность проектов" POST
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier, ProjectManager")]
        public JsonResult GetAPP(DateTime? RepDate, int[] filterPrjIDs, string ProjectName)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            try
            {

                if (RepDate == null)
                {
                    return Json(new { Result = false, data = "", errormessage = "Неверные параметры запроса" }, JsonRequestBehavior.AllowGet);
                }

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                List<APPTableReport> RepList = new List<APPTableReport>();

                string repdt = RepDate.Value.ToString("yyyyMMdd");

                //Запрос вызывает пользовательскую функцию "ufnAPPReport" хранящуюся на SQL-сервере.
                List<APPTableReport> _ads = db.Database.SqlQuery<APPTableReport>(String.Format("Select * from dbo.ufnAPPReport('{0}', '{1}') ORDER BY prj DESC", repdt, ProjectName)).ToList();

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанных сотрудников для пользователя в роли "Руководитель проекта"

                _ads = _ads.Where(x =>
                            (filterPrjIDs == null
                            || filterPrjIDs.Length == 0
                            || filterPrjIDs.Contains(x.prj))
                            && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(x.Chief)) //Фильтрация записей по проектам для руководителей проектов
                            ).ToList();

                List<APBFilterIDs> _prjList = _ads
                    .Select(x => new APBFilterIDs
                    {
                        PrjId = x.prj,
                        ProjectName = x.ProjectName,
                        IPA = x.IPA
                    }).OrderByDescending(x => x.PrjId).ToList();

                var jsonSerialiser = new JavaScriptSerializer();
                var _prjListJson = jsonSerialiser.Serialize(_prjList);

                APPTableReportTotal total = new APPTableReportTotal
                {
                    FactCreditF1Total = _ads.Sum(x => x.FactCreditF1),
                    FactCreditF2Total = _ads.Sum(x => x.FactCreditF2),
                    FCF1F2Total = _ads.Sum(x => x.FCTotalF1F2),
                    FactDebitF1Total = _ads.Sum(x => x.FactDebitF1),
                    FactDebitF2Total = _ads.Sum(x => x.FactDebitF2),
                    FDF1F2Total = _ads.Sum(x => x.FDTotalF1F2),
                    IncomeF1Total = _ads.Sum(x => x.IncomeF1),
                    IncomeF2Total = _ads.Sum(x => x.IncomeF2),
                    IncomeTotal = _ads.Sum(x => x.IncomeTotal)

                };

                if (sortColumn == "ProjectName")
                {
                    _ads = _ads.OrderBy("IPA " + sortColumnDir).ToList();
                }
                else
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                totalRecords = _ads.Count();
                var data = _ads.Skip(skip).Take(pageSize);

                return Json(new
                {
                    Result = true,
                    draw = draw,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords,
                    sortcolumn = sortColumn,
                    sortdir = sortColumnDir,
                    data = data,
                    total = total,
                    prjlist = _prjListJson,
                    repDate = repdt,
                    errormessage = ""
                }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { Result = false, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }


        #region -------------- Отчет по Ф2 "Альтернативный авансовый отчет" -------------

        //Отчет по Ф2 "Альтернативный авансовый отчет" GET
        [Authorize]
        public ActionResult AlternativeAdvance()
        {
            return View();
        }

        //Получение данных для построения отчета  по Ф2 "Альтернативный авансовый отчет" POST
        [Authorize]
        public JsonResult GetAAR(int[] filterWksIDs)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            try
            {
                // Проверяем принадлежность текущего пользователя к ролям "Руководитель", "Финансист", "Администратор"
                bool isChief = System.Web.HttpContext.Current.User.IsInRole("Chief");
                bool isFinancier = System.Web.HttpContext.Current.User.IsInRole("Financier");
                bool isAdministrator = System.Web.HttpContext.Current.User.IsInRole("Administrator");

                //Список связанных сотрудников
                List<int> WorkerIdsList = new List<int>() { -1 };

                if (!isChief && !isFinancier && !isAdministrator)
                {
                    //Получаем идентификатор текущего пользователя
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    //Получаем список связанных сотрудников
                    WorkerIdsList = db.UsrWksRelations.Where(x => x.UserId == user).Select(x => x.WorkerId).ToList();
                }

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;


                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startpaymentdate = null;
                DateTime? _endpaymentdate = null;
                string _paymentdatetext = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_paymentdatetext))
                {
                    _paymentdatetext = _paymentdatetext.Trim();
                    int _length = (_paymentdatetext.Length) - (_paymentdatetext.IndexOf('-') + 2);
                    string _startpaymenttetxt = _paymentdatetext.Substring(0, _paymentdatetext.IndexOf('-')).Trim();
                    string _endpaymenttext = _paymentdatetext.Substring(_paymentdatetext.IndexOf('-') + 2, _length).Trim();
                    _startpaymentdate = DateTime.Parse(_startpaymenttetxt);
                    _endpaymentdate = DateTime.Parse(_endpaymenttext);
                }
                //--------------------------
                string _worker = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _docNum = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _operation = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _counteragent = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _payed = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                string _received = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                string _currency = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();

                var _aao = (from aaoRep in db.AAOReports
                            where (
                                 (aaoRep.Date >= _startpaymentdate && aaoRep.Date <= _endpaymentdate || string.IsNullOrEmpty(_paymentdatetext)) //Диапазон дат   
                                 && (aaoRep.WorkerName.Contains(_worker) || string.IsNullOrEmpty(_worker))
                                 && (aaoRep.DocNumber.Contains(_docNum) || string.IsNullOrEmpty(_docNum))
                                 && (aaoRep.Operation.Contains(_operation) || string.IsNullOrEmpty(_operation))
                                 && (string.IsNullOrEmpty(_counteragent) ||  aaoRep.CounteragentName.ToString().Contains(_counteragent))
                                 && (aaoRep.Currency.Contains(_currency) || string.IsNullOrEmpty(_currency))
                                 && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(aaoRep.WorkerID)) //Фильтрация записей по связанным сотрудникам
                            )
                            select new
                            {
                                ID = aaoRep.ID,
                                WorkerID = aaoRep.WorkerID,
                                WorkerName = aaoRep.WorkerName,
                                Date = aaoRep.Date,
                                DocNumber = aaoRep.DocNumber,
                                Operation = aaoRep.Operation,
                                Counteragent = aaoRep.Counteragent,
                                CounteragentName = aaoRep.CounteragentName,
                                Payed = aaoRep.Payed,
                                Received = aaoRep.Received,
                                Currency = aaoRep.Currency
                            }).AsEnumerable().Select(x => new AAOReport
                                 {
                                     ID = x.ID,
                                     WorkerID = x.WorkerID,
                                     WorkerName = x.WorkerName,
                                     Date = x.Date,
                                     DocNumber = x.DocNumber,
                                     Operation = x.Operation,
                                     Counteragent = x.Counteragent,
                                     CounteragentName = x.CounteragentName,
                                     Payed = x.Payed,
                                     Received = x.Received,
                                     Currency = x.Currency
                                 }).ToList();

                _aao = _aao.Where(x => (
                                      (string.IsNullOrEmpty(_payed) || x.Payed.ToString().Contains(_payed)) 
                                   && (string.IsNullOrEmpty(_received) || x.Received.ToString().Contains(_received))
                                   && (filterWksIDs == null || filterWksIDs.Length == 0 || filterWksIDs.Contains(x.WorkerID))
                                  )).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _aao = _aao.OrderBy(sortColumn + " " + sortColumnDir + ", ID desc").ToList();
                }
                else
                {
                    _aao = _aao.OrderByDescending(x => x.Date).ThenByDescending(x => x.ID).ToList();
                }

                var _wksList = _aao
                    .Select(x => new 
                    {
                        WorkerId = x.WorkerID,
                        WorkerName = x.WorkerName,
                    }).Distinct().ToList();

                var currTotal = _aao.GroupBy(x => x.Currency).Select(x=>new {
                    Currency = x.Select(z=>z.Currency).FirstOrDefault(),
                    Payed = x.Sum(z => z.Payed),
                    Received = x.Sum(z=>z.Received)}).ToList();

                var jsonSerialiser = new JavaScriptSerializer();
                var _wksListJson = jsonSerialiser.Serialize(_wksList);
                var _currTotalJson = jsonSerialiser.Serialize(currTotal);

                totalRecords = _aao.Count();
                var data = _aao.Skip(skip).Take(pageSize).ToList();


                return Json(new
                {
                    draw = draw,
                    sortcolumn = sortColumn,
                    sortdir = sortColumnDir,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords,
                    wkslist = _wksListJson,
                    data = data,
                    currTotal = _currTotalJson,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Ошибка выполнения запроса!\n\r" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

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