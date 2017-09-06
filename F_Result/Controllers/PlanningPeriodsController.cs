using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Linq.Dynamic; //!=====!
using System.Web.Mvc;
using F_Result.Models;

namespace F_Result.Controllers
{
    public class PlanningPeriodsController : Controller
    {
        private FRModel db = new FRModel();

        [Authorize(Roles = "Administrator, Chief, Accountant")]
        // GET: PlanningPeriods
        public ActionResult Index()
        {
            return View(db.PlanningPeriods.ToList());
        }

        [Authorize(Roles = "Administrator, Chief, Accountant")]
        // GET: PlanningPeriods
        public ActionResult PPShow()
        {
            return View();
        }

        // GET: PlanningPeriods/Create
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: PlanningPeriods/Create
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PlanningPeriodId,PeriodName")] PlanningPeriod planningPeriod)
        {
            if (ModelState.IsValid)
            {
                db.PlanningPeriods.Add(planningPeriod);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(planningPeriod);
        }

        // GET: PlanningPeriods/Edit/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult Edit(int? id)
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
        public ActionResult Edit([Bind(Include = "PlanningPeriodId,PeriodName")] PlanningPeriod planningPeriod)
        {
            if (ModelState.IsValid)
            {
                db.Entry(planningPeriod).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(planningPeriod);
        }

        // GET: PlanningPeriods/Delete/5
        [Authorize(Roles = "Administrator, Chief, Accountant")]
        public ActionResult Delete(int? id)
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PlanningPeriod planningPeriod = db.PlanningPeriods.Find(id);
            db.PlanningPeriods.Remove(planningPeriod);
            db.SaveChanges();
            return RedirectToAction("Index");
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
