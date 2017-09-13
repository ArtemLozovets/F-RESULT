using F_Result.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace F_Result.Controllers
{
    public class ReportsController : Controller
    {

        private FRModel db = new FRModel();

        // GET: Reports
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult AnalysisOfPayments()
        {
            return View();
        }

        [Authorize(Roles = "Administrator, Chief, Accountant")]
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


            return Json(new { 
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

        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult AnalysisOfPlanPayments()
        {
            ViewData["periodItems"] = new SelectList(db.PlanningPeriods, "PlanningPeriodId", "PeriodName");
            return View();
        }

        [Authorize(Roles = "Administrator, Chief, Accountant")]
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

            return Json(new { Result = true,
                              pcsum = _pcSum,
                              pcavg = _pcAvg,
                              pcmin = _pcMin,
                              pcmax = _pcMax,
                              pdsum = _pdSum,
                              pdavg = _pdAvg,
                              pdmin = _pdMin,
                              pdmax = _pdMax, 
                              ChartData = _pcsum, 
                              ChartDataA = _pdsum }, JsonRequestBehavior.AllowGet);
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