using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;
using System.Linq.Dynamic; //!=====!

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

        public ActionResult ABShow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadAB()
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

                string _organizationname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _accnum = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _datetxt = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                DateTime? _date = string.IsNullOrEmpty(_datetxt) ? (DateTime?)null : DateTime.Parse(_datetxt);
                string _balance = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _note = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();


                var _ads = (from accbalance in db.AccountsBalances
                            join account in db.Accounts on accbalance.AccountId equals account.AccountId
                            join org in db.Organizations on account.OrganizationId equals org.id
                            join usr in db.IdentityUsers on account.UserId equals usr.Id
                            where   (account.AccountNumber.Contains(_accnum) || string.IsNullOrEmpty(_accnum))
                                    && (accbalance.Date == _date || _date == null)
                                    && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                    && (accbalance.Note.Contains(_note) || string.IsNullOrEmpty(_note))
                                    && (usr.LastName.Contains(_userfn)
                                            || usr.FirstName.Contains(_userfn)
                                            || usr.MiddleName.Contains(_userfn)
                                            || string.IsNullOrEmpty(_userfn))

                            select new
                            {
                                AccountBalanceId = accbalance.AccountsBalanceId,
                                OrgId = org.id,
                                OrgName = org.Title,
                                AccountNumber = account.AccountNumber,
                                Date = accbalance.Date,
                                Balance = accbalance.Balance,
                                Note = accbalance.Note,
                                UserId = account.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName + " " + usr.MiddleName,
                            }).AsEnumerable().Select(x => new AccountsBalance
                            {
                                AccountsBalanceId = x.AccountBalanceId,
                                OrganizationId = x.OrgId,
                                AccountNumber = x.AccountNumber,
                                OrganizationName = x.OrgName,
                                Balance = x.Balance,
                                Date = x.Date,
                                Note = x.Note,
                                UserId = x.UserId,
                                UserFN = x.UserFN
                            }).ToList();

                _ads = _ads.Where(x => (x.Balance.ToString().Contains(_balance)) || string.IsNullOrEmpty(_balance)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.OrganizationName).ToList();
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
