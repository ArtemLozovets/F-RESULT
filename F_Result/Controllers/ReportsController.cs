using F_Result.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Globalization;
using System.Web.Script.Serialization; //!=====!

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
        public JsonResult GetAOP(int? Year)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

            if (Year == null)
            {
                Year = DateTime.Today.Year;
            }

            // График входящих платежей -------------------------------------------
            var _inpaylist = (from t in db.Payments
                              group t by new { t.PaymentDate.Value.Year, t.PaymentDate.Value.Month } into g
                              where g.Key.Year == Year
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
            var _outpaylist = (from t in db.ActualDebit
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
                ChartDataA = _outsum
            }, JsonRequestBehavior.AllowGet);

        }

        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult AnalysisOfPlanPayments()
        {
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            return View();
        }

        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public JsonResult GetPlanData(int? Year)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

            if (Year == null)
            {
                Year = DateTime.Today.Year;
            }

            // График плановых доходов -------------------------------------------
            var _pclist = (from t in db.PlanCredits
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
            var _pdlist = (from t in db.PlanDebits
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
                ChartDataA = _pdsum
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult LoadTabRep(int? PeriodId)
        {
            if (PeriodId == null)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r Не указан плановый период.";
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }

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

                string _projectname = Request.Form.GetValues("search[value]")[0].ToString();

                List<TableReport> RepList = new List<TableReport>();

                var _ads_all = (from prg in db.Projects
                                join pc in db.PlanCredits on prg.id equals pc.ProjectId into pctmp
                                from pc in pctmp.DefaultIfEmpty()
                                join pd in db.PlanDebits on prg.id equals pd.ProjectId into pdtmp
                                from pd in pdtmp.DefaultIfEmpty()
                                where (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                      && (pd.PeriodId == PeriodId && pc.PeriodId == PeriodId)
                                select new
                                {
                                    ProjectName = prg.ShortName,
                                    ProjectId = prg.id,
                                    PlanCreditId = pc.PlanCreditId,
                                    PlanCreditDate = pc.Date,
                                    PlanCreditSum = pc.Sum,
                                    PlanDebitId = pd.PlanDebitId,
                                    PlanDebitDate = pd.Date,
                                    PlanDebitSum = pd.Sum,
                                }).Distinct();

                var _ads = (from prg in _ads_all
                            select new
                            {
                                ProjectName = prg.ProjectName,
                                d1c = (decimal)835.6,
                                d1d = (decimal)544.2,
                                d2c = (decimal)544.2,
                                d2d = (decimal)725.8,
                                d3c = (decimal)544.2,
                                d3d = (decimal)1253.4,
                                d4c = (decimal)544.2,
                                d4d = (decimal)544.2,
                                d5c = (decimal)544.2,
                                d5d = (decimal)544.2,
                                d6c = (decimal)544.2,
                                d6d = (decimal)544.2,
                                d7c = (decimal)544.2,
                                d7d = (decimal)544.2,
                                d8c = (decimal)544.2,
                                d8d = (decimal)544.2,
                                d9c = (decimal)544.2,
                                d9d = (decimal)544.2,
                                d10c = (decimal)544.2,
                                d10d = (decimal)544.2,
                                d11c = (decimal)544.2,
                                d11d = (decimal)544.2,
                                d12c = (decimal)544.2,
                                d12d = (decimal)544.2
                            }).AsEnumerable().Select(x => new TableReport
                            {
                                ProjectName = x.ProjectName,
                                d1c = x.d1c,
                                d1d = x.d1d,
                                d2c = x.d2c,
                                d2d = x.d2d,
                                d3c = x.d3c,
                                d3d = x.d3d,
                                d4c = x.d4c,
                                d4d = x.d4d,
                                d5c = x.d5c,
                                d5d = x.d5d,
                                d6c = x.d6c,
                                d6d = x.d6d,
                                d7c = x.d7c,
                                d7d = x.d7d,
                                d8c = x.d8c,
                                d8d = x.d8d,
                                d9c = x.d9c,
                                d9d = x.d9d,
                                d10c = x.d10c,
                                d10d = x.d10d,
                                d11c = x.d11c,
                                d11d = x.d11d,
                                d12c = x.d12c,
                                d12d = x.d12d,
                                dresc = x.d1c + x.d2c + x.d3c + x.d4c + x.d5c + x.d6c + x.d7c + x.d8c + x.d9c + x.d10c + x.d11c + x.d12c,
                                dresd = x.d1d + x.d2d + x.d3d + x.d4d + x.d5d + x.d6d + x.d7d + x.d8d + x.d9d + x.d10d + x.d11d + x.d12d
                            }).Distinct().ToList();


                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                totalRecords = _ads.Count();

                Summary SummaryRow = new Summary
                {
                    RowName = "Всего",
                    d1c = _ads.Sum(x => x.d1c),
                    d1d = _ads.Sum(x => x.d1d),
                    d2c = _ads.Sum(x => x.d2c),
                    d2d = _ads.Sum(x => x.d2d),
                    d3c = _ads.Sum(x => x.d3c),
                    d3d = _ads.Sum(x => x.d3d),
                    d4c = _ads.Sum(x => x.d4c),
                    d4d = _ads.Sum(x => x.d4d),
                    d5c = _ads.Sum(x => x.d5c),
                    d5d = _ads.Sum(x => x.d5d),
                    d6c = _ads.Sum(x => x.d6c),
                    d6d = _ads.Sum(x => x.d6d),
                    d7c = _ads.Sum(x => x.d7c),
                    d7d = _ads.Sum(x => x.d7d),
                    d8c = _ads.Sum(x => x.d8c),
                    d8d = _ads.Sum(x => x.d8d),
                    d9c = _ads.Sum(x => x.d9c),
                    d9d = _ads.Sum(x => x.d9d),
                    d10c = _ads.Sum(x => x.d10c),
                    d10d = _ads.Sum(x => x.d10d),
                    d11c = _ads.Sum(x => x.d11c),
                    d11d = _ads.Sum(x => x.d11d),
                    d12c = _ads.Sum(x => x.d12c),
                    d12d = _ads.Sum(x => x.d12d),
                    dresc = _ads.Sum(x => x.dresc),
                    dresd = _ads.Sum(x => x.dresd)
                };

                Deviation DeviationRow = new Deviation
                {
                    DevRowName = "Отклонение",
                    dev1 = SummaryRow.d1c - SummaryRow.d1d,
                    dev2 = SummaryRow.d2c - SummaryRow.d2d,
                    dev3 = SummaryRow.d3c - SummaryRow.d3d,
                    dev4 = SummaryRow.d4c - SummaryRow.d4d,
                    dev5 = SummaryRow.d5c - SummaryRow.d5d,
                    dev6 = SummaryRow.d6c - SummaryRow.d6d,
                    dev7 = SummaryRow.d7c - SummaryRow.d7d,
                    dev8 = SummaryRow.d8c - SummaryRow.d8d,
                    dev9 = SummaryRow.d9c - SummaryRow.d9d,
                    dev10 = SummaryRow.d10c - SummaryRow.d10d,
                    dev11 = SummaryRow.d11c - SummaryRow.d11d,
                    dev12 = SummaryRow.d12c - SummaryRow.d12d,
                    devres = SummaryRow.dresc - SummaryRow.dresd
                };

                var data = _ads.Skip(skip).Take(pageSize);
                var summary = SummaryRow;
                var deviation = DeviationRow;

                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, summary = summary, deviation = deviation, errormessage = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
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
        public JsonResult GetAPB(int? Period, DateTime? BaseDate, bool IsAllTimes, int[] filterPrjIDs)
        {
            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            try
            {

                if (Period == null || BaseDate == null)
                {
                    return Json(new { Result = false, data = "", errormessage = "Неверные параметры запроса" }, JsonRequestBehavior.AllowGet);
                }

                string PeriodName = db.PlanningPeriods.FirstOrDefault(x => x.PlanningPeriodId == Period).PeriodName.ToString();

                DateTime? StartPeriod = null;
                DateTime? EndPeriod = null;

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
                if (pageSize == -1) { pageSize =  2147483647;}
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                List<TableReport> RepList = new List<TableReport>();

                DateTime _stmp = Convert.ToDateTime(StartPeriod.ToString());
                string _startPeriod = _stmp.ToString("yyyyMMdd");
                DateTime _etmp = Convert.ToDateTime(EndPeriod.ToString());
                string _endPeriod = _etmp.ToString("yyyyMMdd");

                //Запрос вызывает пользовательскую функцию "ufnAPBReport" хранящуюся на SQL-сервере.
                List<APBTableReport> _ads = db.Database.SqlQuery<APBTableReport>(String.Format("Select * from dbo.ufnAPBReport('{0}', '{1}', {2})", _startPeriod, _endPeriod, Period)).ToList();

                List<APBFilterIDs> _prjList = _ads.Select(x => new APBFilterIDs {PrjId = x.prj, ProjectName = x.ProjectName}).OrderBy(x=>x.ProjectName).ToList();
                
                
                var jsonSerialiser = new JavaScriptSerializer();
                var _prjListJson = jsonSerialiser.Serialize(_prjList);

                _ads = _ads.Where(x => filterPrjIDs == null || filterPrjIDs.Length == 0 || filterPrjIDs.Contains(x.prj)).ToList();

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
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                totalRecords = _ads.Count();
                var data = _ads.Skip(skip).Take(pageSize);

                if (IsAllTimes && _ads.Count > 0)
                {
                    StartPeriod = _ads.Min(x => x.MinDate);
                    EndPeriod = _ads.Max(x => x.MaxDate);
                }

                return Json(new
                {
                    Result = true,
                    StartPeriod = StartPeriod,
                    EndPeriod = EndPeriod,
                    isAllTimes = IsAllTimes,
                    draw = draw,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords,
                    data = data,
                    total = total,
                    prjlist = _prjListJson,
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