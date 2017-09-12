using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Linq.Dynamic; //!=====!
using System.Web.Mvc;
using F_Result.Models;
using System;

namespace F_Result.Controllers
{
    public class PlanningPeriodsController : Controller
    {
        private FRModel db = new FRModel();

        [Authorize(Roles = "Administrator, Chief, Accountant")]
        // GET: PlanningPeriods
        public ActionResult PPShow()
        {
            return View();
        }


        [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
        [HttpPost]
        public ActionResult LoadPP()
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

                string _ppid = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _ppname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();

                var _pperiod = (from period in db.PlanningPeriods
                            where (period.PeriodName.Contains(_ppname) || String.IsNullOrEmpty(_ppname))
                            select new
                            {
                                PlanningPeriodId = period.PlanningPeriodId,
                                PeriodName = period.PeriodName
                            }).AsEnumerable().Select(x => new PlanningPeriod
                            {
                                PlanningPeriodId = x.PlanningPeriodId,
                                PeriodName = x.PeriodName
                            }).ToList();

                _pperiod = _pperiod.Where(x => (x.PlanningPeriodId.ToString().Contains(_ppid)) || string.IsNullOrEmpty(_ppid)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _pperiod = _pperiod.OrderBy(sortColumn + " " + sortColumnDir + ", PlanningPeriodId desc").ToList();
                }
                else
                {
                    _pperiod = _pperiod.OrderBy(x => x.PlanningPeriodId).ThenByDescending(x=>x.PlanningPeriodId).ToList();
                }

                totalRecords = _pperiod.Count();

                var data = _pperiod.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: PlanningPeriods/Create
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult PPCreate()
        {
            return View();
        }

        // POST: PlanningPeriods/Create
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PPCreate([Bind(Include = "PlanningPeriodId,PeriodName")] PlanningPeriod planningPeriod)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.PlanningPeriods.Add(planningPeriod);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("PPShow");
                }
                catch (Exception ex)
                {
                    ViewBag.ErMes = ex.Message;
                    ViewBag.ErStack = ex.StackTrace;
                    ViewBag.ErInner = ex.InnerException.InnerException.Message;
                    return View("Error");
                }

            }

            return View(planningPeriod);
        }

        // GET: PlanningPeriods/Edit/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult PPEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanningPeriod planningPeriod = db.PlanningPeriods.Find(id);
            if (planningPeriod == null)
            {
                return HttpNotFound();
            }
            return View(planningPeriod);
        }

        // POST: PlanningPeriods/Edit/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PPEdit([Bind(Include = "PlanningPeriodId,PeriodName")] PlanningPeriod planningPeriod)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    db.Entry(planningPeriod).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
                    return RedirectToAction("PPShow");
                }
                catch (Exception ex)
                {
                    ViewBag.ErMes = ex.Message;
                    ViewBag.ErStack = ex.StackTrace;
                    ViewBag.ErInner = ex.InnerException.InnerException.Message;
                    return View("Error");
                }

            }
            return View(planningPeriod);
        }

        // GET: PlanningPeriods/Delete/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult PPDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanningPeriod planningPeriod = db.PlanningPeriods.Find(id);
            if (planningPeriod == null)
            {
                return HttpNotFound();
            }
            return View(planningPeriod);
        }

        // POST: PlanningPeriods/Delete/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        [HttpPost, ActionName("PPDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                PlanningPeriod planningPeriod = db.PlanningPeriods.Find(id);
                db.PlanningPeriods.Remove(planningPeriod);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
                return RedirectToAction("PPShow");
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
