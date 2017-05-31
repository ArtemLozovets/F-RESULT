using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;

namespace F_Result.Controllers
{
    public class ActualDebitsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: ActualDebits
        public ActionResult Index()
        {
            var actualDebit = db.ActualDebit.Include(a => a.Projects);
            return View(actualDebit.ToList());
        }

        // GET: ActualDebits/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            if (actualDebit == null)
            {
                return HttpNotFound();
            }
            return View(actualDebit);
        }

        // GET: ActualDebits/Create
        public ActionResult Create()
        {
            ViewBag.ProjectId = new SelectList(db.Projects, "id", "FullName");
            return View();
        }

        // POST: ActualDebits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,Appointment,DocNumber,UserId")] ActualDebit actualDebit)
        {
            if (ModelState.IsValid)
            {
                db.ActualDebit.Add(actualDebit);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectId = new SelectList(db.Projects, "id", "FullName", actualDebit.ProjectId);
            return View(actualDebit);
        }

        // GET: ActualDebits/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            if (actualDebit == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "id", "FullName", actualDebit.ProjectId);
            return View(actualDebit);
        }

        // POST: ActualDebits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,Appointment,DocNumber,UserId")] ActualDebit actualDebit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(actualDebit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "id", "FullName", actualDebit.ProjectId);
            return View(actualDebit);
        }

        // GET: ActualDebits/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            if (actualDebit == null)
            {
                return HttpNotFound();
            }
            return View(actualDebit);
        }

        // POST: ActualDebits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ActualDebit actualDebit = db.ActualDebit.Find(id);
            db.ActualDebit.Remove(actualDebit);
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
