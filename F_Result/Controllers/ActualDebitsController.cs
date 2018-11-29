using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic; //!=====!
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Globalization;

namespace F_Result.Controllers
{
    public class ActualDebitsController : Controller
    {
        private FRModel db = new FRModel();

        // Исходящие платежи
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult ADShow(int? ProjectId, string startDate, string endDate, bool? firstPay)
        {
            if (ProjectId != null)
            {
                string ProjectName = db.Projects.FirstOrDefault(x => x.id == ProjectId).ShortName.ToString();
                ViewData["ProjectName"] = ProjectName;
            }

            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                if (firstPay ?? false)
                {
                    startDate = Convert.ToDateTime(db.ActualDebit
                            .Where(x => x.ProjectId == ProjectId)
                            .OrderBy(x => x.Date)
                            .Select(x => x.Date)
                            .FirstOrDefault()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                ViewData["Period"] = startDate + " - " + endDate;
            }

            ViewData["AD_Access"] = db.Settings.FirstOrDefault(z => z.SettingName == "AD_old_access").SettingValue.ToString();
            return View();
        }

        //Список исходящих платежей
        [HttpPost]
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult LoadAD(int[] filterPrjIDs)
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

                string _projectname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _chname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _organizationname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _appoinment = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _docnumber = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _userfn = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startagrdate = null;
                DateTime? _endagrdate = null;
                string _datetext = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_datetext))
                {
                    _datetext = _datetext.Trim();
                    int _length = (_datetext.Length) - (_datetext.IndexOf('-') + 2);
                    string _startagrdatetext = _datetext.Substring(0, _datetext.IndexOf('-')).Trim();
                    string _endagrdatetext = _datetext.Substring(_datetext.IndexOf('-') + 2, _length).Trim();
                    _startagrdate = DateTime.Parse(_startagrdatetext);
                    _endagrdate = DateTime.Parse(_endagrdatetext);
                }
                //--------------------------

                string _sum = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();

                var _ads = (from actualdebit in db.ActualDebit
                            join prg in db.Projects on actualdebit.ProjectId equals prg.id
                            join ipa in db.ActivityIndexes on actualdebit.ProjectId equals ipa.ProjectId into ipatmp
                            from ipa in ipatmp.DefaultIfEmpty()
                            join org in db.Organizations on actualdebit.OrganizationId equals org.id
                            join usr in db.IdentityUsers on actualdebit.UserId equals usr.Id into usrtmp
                            from usr in usrtmp.DefaultIfEmpty()
                            where (actualdebit.Date >= _startagrdate && actualdebit.Date <= _endagrdate || string.IsNullOrEmpty(_datetext)) //Диапазон дат
                                        && (prg.ShortName.Contains(_projectname) || string.IsNullOrEmpty(_projectname))
                                        && (prg.ChiefName.Contains(_chname) || string.IsNullOrEmpty(_chname))
                                        && (org.Title.Contains(_organizationname) || string.IsNullOrEmpty(_organizationname))
                                        && (actualdebit.Appointment.Contains(_appoinment) || string.IsNullOrEmpty(_appoinment))
                                        && (actualdebit.DocNumber.Contains(_docnumber) || string.IsNullOrEmpty(_docnumber))
                            select new
                            {
                                ActualDebitId = actualdebit.ActualDebitId,
                                Date = actualdebit.Date,
                                Sum = actualdebit.Sum,
                                ProjectId = actualdebit.ProjectId,
                                ipa = ipa.IPAValue,
                                OrgId = org.id,
                                Appointment = actualdebit.Appointment,
                                DocNumber = actualdebit.DocNumber,
                                UserId = actualdebit.UserId,
                                UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                                ProjectName = prg.ShortName,
                                ProjectType = prg.ProjectType,
                                ChiefName = prg.ChiefName,
                                ProjectManagerName = prg.ProjectManagerName,
                                StartDatePlan = prg.StartDatePlan,
                                StartDateFact = prg.StartDateFact,
                                OrgName = org.Title,
                                planBenefit = prg.planBenefit,
                                planExpand = prg.planExpand
                            }).AsEnumerable().Select(x => new ActualDebitView
                            {
                                ActualDebitId = x.ActualDebitId,
                                Date = x.Date,
                                Sum = x.Sum,
                                ProjectId = x.ProjectId,
                                IPA = x.ipa,
                                ProjectName = x.ProjectName,
                                ProjectType = x.ProjectType,
                                ChiefName = x.ChiefName,
                                ProjectManagerName = x.ProjectManagerName,
                                StartDatePlan = x.StartDatePlan,
                                StartDateFact = x.StartDateFact,
                                OrganizationId = x.OrgId,
                                OrganizationName = x.OrgName,
                                Appointment = x.Appointment,
                                DocNumber = x.DocNumber,
                                UserId = x.UserId,
                                UserFN = x.UserFN,
                                planBenefit = x.planBenefit,
                                planExpand = x.planExpand
                            }).ToList();

                _ads = _ads.Where(x => (x.Sum.ToString().Contains(_sum) || string.IsNullOrEmpty(_sum)) 
                            && (x.UserFN.Contains(_userfn) || String.IsNullOrEmpty(_userfn))
                            && (filterPrjIDs == null || filterPrjIDs.Length == 0 || filterPrjIDs.Contains(x.ProjectId))).ToList();

                //Список ID для передачи в ф-цию экспорта в Excel
                List<int> _IDsList = _ads.Select(x => x.ActualDebitId).ToList();

                List<APBFilterIDs> _prjList = _ads.GroupBy(x => x.ProjectId)
                    .Select(x => new APBFilterIDs {
                        PrjId = x.Select(z => z.ProjectId).First(),
                        ProjectName = x.Select(z => z.ProjectName).First(),
                        IPA = x.Select(z => z.IPA).First()
                    }).ToList();

                var jsonSerialiser = new JavaScriptSerializer();
                var _prjListJson = jsonSerialiser.Serialize(_prjList);
                var _IDsListJson = jsonSerialiser.Serialize(_IDsList);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", ActualDebitId desc").ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.ActualDebitId).ToList();
                }

                var fSum = _ads.Sum(x => x.Sum);

                totalRecords = _ads.Count();

                var data = _ads.Skip(skip).Take(pageSize);
                return Json(new { sum = fSum, draw = draw
                    , prjlist = _prjListJson
                    , idslist = _IDsListJson
                    , sortcolumn = sortColumn
                    , sortdir = sortColumnDir
                    , recordsFiltered = totalRecords
                    , recordsTotal = totalRecords
                    , data = data
                    , errormessage = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }


        // Подробная информация об исходящем платеже
        [Authorize(Roles = "Administrator, Chief, Accountant, Financier")]
        public ActionResult ADDetails(int? id)
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

            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            actualDebit.ProjectName = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            actualDebit.OrganizationName = _orgName;
            ViewData["AD_Access"] = db.Settings.FirstOrDefault(z => z.SettingName == "AD_old_access").SettingValue.ToString();
            return View(actualDebit);
        }

        //Добавление исходящего платежа GET
        [Authorize(Roles = "Administrator, Accountant, Financier")]
        public ActionResult ADCreate()
        {
            ActualDebit _model = new ActualDebit();
            _model.Date = DateTime.Today;
            return View(_model);
        }


        //Добавление исходящего платежа POST
        [Authorize(Roles = "Administrator, Accountant, Financier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADCreate([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,DocNumber,ApplicationUser")] ActualDebit actualDebit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        actualDebit.UserId = user;
                    }

                    db.ActualDebit.Add(actualDebit);
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация добавлена";
                    return RedirectToAction("ADShow");
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
            return View(actualDebit);
        }


        //Редактирование исходящего платежа GET
        [Authorize(Roles = "Administrator, Accountant, Financier")]
        public ActionResult ADEdit(int? id, string isClone)
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

            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            ViewData["ProjectName"] = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;

            if (Convert.ToBoolean(isClone))
            {
                actualDebit.Date = DateTime.Today;
                return View("ADClone", actualDebit);
            }
            else
            {
                return View(actualDebit);
            }
        }


        //Редактирование исходящего платежа POST
        [Authorize(Roles = "Administrator, Accountant, Financier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADEdit([Bind(Include = "ActualDebitId,Date,Sum,ProjectId,OrganizationId,Appointment,DocNumber")] ActualDebit actualDebit)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    //Получаем идентификатор текущего пользователя
                    using (ApplicationDbContext aspdb = new ApplicationDbContext())
                    {
                        var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        actualDebit.UserId = user;
                    }

                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
                    db.Entry(actualDebit).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["MessageOK"] = "Информация обновлена";
                    return RedirectToAction("ADShow");
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
            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            ViewData["ProjectName"] = _prgName;
            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            ViewData["OrganizationName"] = _orgName;
            return View(actualDebit);
        }


        //Удаление исходящего платежа GET
        [Authorize(Roles = "Administrator, Accountant")]
        public ActionResult ADDelete(int? id)
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

            string _prgName = db.Projects.Where(x => x.id == actualDebit.ProjectId).Select(x => x.ShortName).FirstOrDefault().ToString();
            actualDebit.ProjectName = _prgName;

            string _orgName = db.Organizations.Where(x => x.id == actualDebit.OrganizationId).Select(x => x.Title).FirstOrDefault().ToString();
            actualDebit.OrganizationName = _orgName;

            return View(actualDebit);
        }


        //Удаление исходящего платежа POST
        [Authorize(Roles = "Administrator, Accountant")]
        [HttpPost, ActionName("ADDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            try
            {
                ActualDebit actualDebit = db.ActualDebit.FirstOrDefault(x => x.ActualDebitId == id);
                if (actualDebit == null)
                {
                    TempData["MessageError"] = "Удаляемый объект отсутствует в базе данных";
                    return RedirectToAction("ADShow");
                }

                db.ActualDebit.Remove(actualDebit);
                db.SaveChanges();
                TempData["MessageOK"] = "Информация удалена";
                return RedirectToAction("ADShow");
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
