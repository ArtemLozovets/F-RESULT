using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using F_Result.Models;
using System.Linq.Dynamic; //!=====!
using Microsoft.AspNet.Identity;
using System.Web.Script.Serialization;

namespace F_Result.Controllers
{
    public class AccountsBalancesController : Controller
    {
        private FRModel db = new FRModel();

        private class JSONBalances
        {
            public int AccountId { get; set; }
            public string Balance { get; set; }
            public string Note { get; set; }
        }

        //Остатки на счетах
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult ABShow(string result)
        {
            if (result == "BatchSuccess")
            {
                TempData["MessageOk"] = "Пакетное добавление остатков выполнено успешно";
            }

            return View();
        }

        //Таблица остатков на счетах
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
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
                string _bankname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _accnum = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _note = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startagrdate = null;
                DateTime? _endagrdate = null;
                string _datetext = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                if (!String.IsNullOrEmpty(_datetext))
                {
                    _datetext = _datetext.Trim();
                    int _length = (_datetext.Length) - (_datetext.IndexOf('-') + 2);
                    string _startagrdatetext = _datetext.Substring(0, _datetext.IndexOf('-')).Trim();
                    string _endagrdatetext = _datetext.Substring(_datetext.IndexOf('-') + 2, _length).Trim();
                    _startagrdate = DateTime.Parse(_startagrdatetext);
                    _endagrdate = DateTime.Parse(_endagrdatetext);
                }
                //--------------------------

                string _balance = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();

                var _ads = (from accbalance in db.AccountsBalances
                            join account in db.Accounts on accbalance.AccountId equals account.AccountId
                            join org in db.Organizations on account.OrganizationId equals org.id
                            join usr in db.IdentityUsers on accbalance.UserId equals usr.Id
                            where (account.AccountNumber.Contains(_accnum) || string.IsNullOrEmpty(_accnum))
                                    && (account.BankName.Contains(_bankname) || string.IsNullOrEmpty(_bankname))
                                    && (accbalance.Date >= _startagrdate && accbalance.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                    && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                    && (accbalance.Note.Contains(_note) || string.IsNullOrEmpty(_note))
                            select new
                            {
                                AccountBalanceId = accbalance.AccountsBalanceId,
                                AccountId = account.AccountId,
                                OrgId = org.id,
                                OrgName = org.Title,
                                BankName = account.BankName,
                                AccountNumber = account.AccountNumber,
                                Date = accbalance.Date,
                                Balance = accbalance.Balance,
                                Note = accbalance.Note,
                                UserId = account.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                            }).AsEnumerable().Select(x => new AccountsBalance
                            {
                                AccountsBalanceId = x.AccountBalanceId,
                                AccountId = x.AccountId,
                                OrganizationId = x.OrgId,
                                BankName = x.BankName,
                                AccountNumber = x.AccountNumber,
                                OrganizationName = x.OrgName,
                                Balance = x.Balance,
                                Date = x.Date,
                                Note = x.Note,
                                UserId = x.UserId,
                                UserFN = x.UserFN
                            }).ToList();

                _ads = _ads.Where(x => ((x.Balance.ToString().Contains(_balance)) || string.IsNullOrEmpty(_balance)) && (x.UserFN.Contains(_userfn) || String.IsNullOrEmpty(_userfn))).ToList();

                //Список ID для передачи в ф-цию экспорта в Excel
                List<int> _IDsList = _ads.Select(x => x.AccountsBalanceId).ToList();
                var jsonSerialiser = new JavaScriptSerializer();
                var _IDsListJson = jsonSerialiser.Serialize(_IDsList);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", AccountsBalanceId desc").ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.AccountsBalanceId).ToList();
                }

                var fSum = _ads.Sum(x => x.Balance);

                totalRecords = _ads.Count();

                var data = _ads.Skip(skip).Take(pageSize);
                return Json(new
                {
                    fsum = fSum
                    ,
                    draw = draw
                    ,
                    recordsFiltered = totalRecords
                    ,
                    recordsTotal = totalRecords
                    ,
                    data = data
                    ,
                    idslist = _IDsListJson
                    ,
                    sortcolumn = sortColumn
                    ,
                    sortdir = sortColumnDir
                    ,
                    errormessage = ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //Добавление остатков на счетах GET
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
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

        //Добавление остатков на счетах POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
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

        //Редактирование остатков на счетах GET
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
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


        //Редактирование остатков на счетах POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
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

        //Удаление остатков на счетах GET
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
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

        //Удаление остатков на счетах POST
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        [HttpPost, ActionName("ABDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                AccountsBalance _acb = db.AccountsBalances.FirstOrDefault(x => x.AccountsBalanceId == id);
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

        //Пакетное добавление остатков на счетах 
        //Генерация представления
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult ABBatchCreate()
        {
            ViewData["Date"] = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd'/'MM'/'yyyy");

            var _acc = (from acc in db.Accounts
                        join org in db.Organizations on acc.OrganizationId equals org.id
                        where acc.Status
                        select new
                        {
                            AccountId = acc.AccountId,
                            AccountNumber = acc.AccountNumber,
                            OrgName = org.Title,
                            BankName = acc.BankName
                        }).AsEnumerable().Select(x => new Account
                        {
                            AccountId = x.AccountId,
                            AccountNumber = x.AccountNumber,
                            OrganizationName = x.OrgName,
                            BankName = x.BankName
                        }).OrderBy(x => x.OrganizationName).ThenBy(x => x.AccountNumber);

            return View(_acc.ToList());
        }

        //Пакетное добавление остатков на счетах 
        //Запись в БД
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        [HttpPost]
        public JsonResult SaveBatch(DateTime? Date, String dataJSON)
        {

            if (Date == null || String.IsNullOrEmpty(dataJSON))
            {
                return Json(new { Result = false, Message = "Ошибка валидации модели!" }, JsonRequestBehavior.AllowGet);
            }

            if (db.AccountsBalances.Where(x => x.Date == Date).Count() > 0)
            {
                return Json(new { Result = false, Message = "В базе данных присутствует информация по остаткам на счетах на указанную дату!" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string _user = String.Empty;
                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    _user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                }

                string _jsonObject = dataJSON.Replace(@"\", string.Empty);
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                IList<JSONBalances> jsonObject = serializer.Deserialize<IList<JSONBalances>>(_jsonObject);
                List<AccountsBalance> _balancelist = new List<AccountsBalance>();

                DateTime _date;
                DateTime.TryParse(Date.ToString(), out _date);


                foreach (var Balances in jsonObject)
                {
                    decimal _balance;
                    Decimal.TryParse(Balances.Balance.ToString(), out _balance);

                    AccountsBalance _acb = new AccountsBalance
                    {
                        AccountId = Balances.AccountId,
                        Date = _date,
                        Balance = _balance,
                        Note = Balances.Note,
                        UserId = _user
                    };

                    _balancelist.Add(_acb);
                }

                db.AccountsBalances.AddRange(_balancelist);
                db.SaveChanges();

                return Json(new { Result = true, Message = "Информация добавлена " }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
                ViewBag.ErInner = ex.InnerException.InnerException.Message;
                return Json(new { Result = false, Message = "Ошибка выполнения запроса! " + ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        [HttpPost]
        public ActionResult getPBalance(DateTime Date)
        {
            try
            {
                if (Date == null)
                {
                    return Json(new { Result = false, Message = "Ошибка выполнения запроса! Не указана дата."}, JsonRequestBehavior.AllowGet);
                }

                decimal _PlanningBalance = db.Database.SqlQuery<decimal>("Select dbo.ufnPDateBalance('"+ Date.ToString("yyyy.MM.dd")+"') as PlanningBalance").FirstOrDefault();

                return Json(new { Result = true, data = _PlanningBalance }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
                ViewBag.ErInner = ex.InnerException.InnerException.Message;
                return Json(new { Result = false, Message = "Ошибка выполнения запроса! " + ex.Message + ex.StackTrace }, JsonRequestBehavior.AllowGet);
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
