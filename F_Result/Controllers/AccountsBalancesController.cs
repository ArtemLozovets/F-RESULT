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
using Microsoft.AspNet.Identity;

namespace F_Result.Controllers
{
    public class AccountsBalancesController : Controller
    {
        private FRModel db = new FRModel();

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
                                AccountId = account.AccountId,
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
                                AccountId = x.AccountId,
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
                    _ads = _ads.OrderByDescending(x => x.Date).ToList();
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

        // GET: AccountsBalances/Create
        public ActionResult ABCreate(int? AccountId)
        {
            if (AccountId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string _accNum = db.Accounts.FirstOrDefault(x => x.AccountId == AccountId).AccountNumber.ToString();
            
            DateTime _date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            AccountsBalance _acb = new AccountsBalance();
            _acb.AccountNumber = _accNum;
            _acb.Date = _date;

            return View(_acb);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ABCreate([Bind(Include = "AccountsBalanceId,Date,Balance,Note,AccountId")] AccountsBalance accountsBalance)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        accountsBalance.UserId = user;
                    }

                    db.AccountsBalances.Add(accountsBalance);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("ABShow");
                }
                catch (Exception ex)
                {
                    ViewBag.ErMes = ex.Message;
                    ViewBag.ErStack = ex.StackTrace;
                    ViewBag.ErInner = ex.InnerException.InnerException.Message;
                    return View("Error");
                }
            }

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

            accountsBalance.AccountNumber = db.Accounts.FirstOrDefault(x => x.AccountId == accountsBalance.AccountId).AccountNumber.ToString();
            return View(accountsBalance);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ABEdit([Bind(Include = "AccountsBalanceId,Date,Balance,Note,AccountId")] AccountsBalance accountsBalance)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        accountsBalance.UserId = user;
                    }

                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //===================Debug===============
                    db.Entry(accountsBalance).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
                    return RedirectToAction("ABShow");
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
            accountsBalance.AccountNumber = db.Accounts.FirstOrDefault(x => x.AccountId == accountsBalance.AccountId).AccountNumber.ToString();
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
            try
            {
                AccountsBalance _acb = db.AccountsBalances.FirstOrDefault(x => x.AccountId == id);
                if (_acb == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("ABShow");
                }

                db.AccountsBalances.Remove(_acb);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
                return RedirectToAction("ABShow");
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
