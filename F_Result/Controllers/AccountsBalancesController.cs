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
    public class AccountsBalancesController : Controller
    {
        private FRModel db = new FRModel();

        // GET: AccountsBalances
        public ActionResult Index()
        {
            var accountsBalances = db.AccountsBalances.Include(a => a.Account);
            return View(accountsBalances.ToList());
        }

        public ActionResult ABSHow()
        {
            return View();
        }

        // GET: AccountsBalances/Details/5
        public ActionResult ABDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsBalance accountsBalance = db.AccountsBalances.Find(id);
            if (accountsBalance == null)
            {
                return HttpNotFound();
            }
            return View(accountsBalance);
        }

        // GET: AccountsBalances/Create
        public ActionResult ABCreate()
        {
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "MFO");
            return View();
        }

        // POST: AccountsBalances/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ABCreate([Bind(Include = "AccountsBalanceId,Date,Balance,Note,AccountId")] AccountsBalance accountsBalance)
        {
            if (ModelState.IsValid)
            {
                db.AccountsBalances.Add(accountsBalance);
                db.SaveChanges();
                return RedirectToAction("ABShow");
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "MFO", accountsBalance.AccountId);
            return View(accountsBalance);
        }

        // GET: AccountsBalances/Edit/5
        public ActionResult ABEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsBalance accountsBalance = db.AccountsBalances.Find(id);
            if (accountsBalance == null)
            {
                return HttpNotFound();
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "MFO", accountsBalance.AccountId);
            return View(accountsBalance);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ABEdit([Bind(Include = "AccountsBalanceId,Date,Balance,Note,AccountId")] AccountsBalance accountsBalance)
        {
            if (ModelState.IsValid)
            {
                db.Entry(accountsBalance).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ABShow");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "MFO", accountsBalance.AccountId);
            return View(accountsBalance);
        }

        // GET: AccountsBalances/Delete/5
        public ActionResult ABDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsBalance accountsBalance = db.AccountsBalances.Find(id);
            if (accountsBalance == null)
            {
                return HttpNotFound();
            }
            return View(accountsBalance);
        }

        // POST: AccountsBalances/Delete/5
        [HttpPost, ActionName("ABDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AccountsBalance accountsBalance = db.AccountsBalances.Find(id);
            db.AccountsBalances.Remove(accountsBalance);
            db.SaveChanges();
            return RedirectToAction("ABShow");
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
