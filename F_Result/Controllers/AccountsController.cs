using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic; //!=====!

namespace F_Result.Controllers
{
    public class AccountsController : Controller
    {
        private FRModel db = new FRModel();


        public ActionResult ACShow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadAC()
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
                string _mfo = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _accountnumber = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _statusTXT = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                bool _status = true;
                if (_statusTXT == "all") { _statusTXT = String.Empty; }
                else { Boolean.TryParse(_statusTXT, out _status); }
                string _note = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();

                var _ads = (from account in db.Accounts
                            join org in db.Organizations on account.OrganizationId equals org.id
                            join usr in db.IdentityUsers on account.UserId equals usr.Id
                            where (account.MFO.Contains(_mfo) || string.IsNullOrEmpty(_mfo))
                                    && (account.AccountNumber.Contains(_accountnumber) || string.IsNullOrEmpty(_accountnumber))
                                    && (account.Status == _status || string.IsNullOrEmpty(_statusTXT))
                                    && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                    && (account.Note.Contains(_note) || string.IsNullOrEmpty(_note))
                                    && (usr.LastName.Contains(_userfn)
                                            || usr.FirstName.Contains(_userfn)
                                            || usr.MiddleName.Contains(_userfn)
                                            || string.IsNullOrEmpty(_userfn))

                            select new
                            {
                                AccountId = account.AccountId,
                                OrgId = org.id,
                                OrgName = org.Title,
                                MFO = account.MFO,
                                AccountNumber = account.AccountNumber,
                                Status = account.Status,
                                Note = account.Note,
                                UserId = account.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName + " " + usr.MiddleName,
                            }).AsEnumerable().Select(x => new Account
                            {
                                AccountId = x.AccountId,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                MFO = x.MFO,
                                AccountNumber = x.AccountNumber,
                                Status = x.Status,
                                Note = x.Note,
                                UserId = x.UserId,
                                UserFN = x.UserFN
                            }).ToList();

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


        // GET: Accounts/Details/5
        public ActionResult ACDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: Accounts/Create
        public ActionResult ACCreate()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ACCreate([Bind(Include = "AccountId,MFO,AccountNumber,Status,Note,OrganizationId")] Account account)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        account.UserId = user;
                    }

                    db.Accounts.Add(account);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("ACShow");
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
            return View(account);
        }

        // GET: Accounts/Edit/5
        public ActionResult ACEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }

            string _orgName = db.Organizations.Where(x => x.id == account.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ACEdit([Bind(Include = "AccountId,MFO,AccountNumber,Status,Note,OrganizationId")] Account account)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        account.UserId = user;
                    }

                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //===================Debug===============
                    db.Entry(account).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
                    return RedirectToAction("ACShow");
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
            string _orgName = db.Organizations.Where(x => x.id == account.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;
            return View(account);
        }

        // GET: Accounts/Delete/5
        public ActionResult ACDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }

            string _orgName = db.Organizations.Where(x => x.id == account.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            account.OrganizationName = _orgName;
            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("ACDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Account account = db.Accounts.FirstOrDefault(x => x.AccountId == id);
                if (account == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("ACShow");
                }

                db.Accounts.Remove(account);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
                return RedirectToAction("ACShow");
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
