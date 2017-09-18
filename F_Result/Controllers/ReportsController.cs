using F_Result.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Linq.Dynamic; //!=====!

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


        [HttpPost]
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult LoadTabRep()
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

                string _projectname = Request.Form.GetValues("search[value]")[0].ToString();
                var _ads = (from actualdebit in db.ActualDebit
                            join prg in db.Projects on actualdebit.ProjectId equals prg.id
                            join org in db.Organizations on actualdebit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on actualdebit.UserId equals usr.Id
                            where prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname)
                            select new
                            {
                                ProjectId = actualdebit.ProjectId,
                                ProjectName = prg.ShortName,
                                OrgName = org.Title
                            }).AsEnumerable().Select(x => new ActualDebit
                            {
                                ProjectId = x.ProjectId,
                                ProjectName = x.ProjectName,
                            }).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.ProjectName).ToList();
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