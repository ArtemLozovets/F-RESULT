using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Linq.Dynamic; //!=====!
using F_Result.Models;
using F_Result.Methods;
using System.Collections.Generic;

namespace F_Result.Controllers
{
    [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant, Financier")]
    public class DirectumController : Controller
    {
        private FRModel db = new FRModel();

        #region Входящие платежи Ф1
        public ActionResult ShowPayments(int? ProjectId, string startDate, string endDate)
        {
            if (ProjectId != null)
            {
                string ProjectName = db.Projects.FirstOrDefault(x => x.id == ProjectId).ShortName.ToString();
                ViewData["ProjectName"] = ProjectName;
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                ViewData["Period"] = startDate + " - " + endDate;
            }

            return View();
        }

        //Список входящих платежей 
        [HttpPost]
        public ActionResult LoadPayments()
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанного сотрудника для пользователя в роли "Руководитель проекта"

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                string _project = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _chief = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _client = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _agreement = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _manager = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _paymentdesc = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();

                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startpaymentdate = null;
                DateTime? _endpaymentdate = null;
                string _paymentdatetext = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_paymentdatetext))
                {
                    _paymentdatetext = _paymentdatetext.Trim();
                    int _length = (_paymentdatetext.Length) - (_paymentdatetext.IndexOf('-') + 2);
                    string _startpaymenttetxt = _paymentdatetext.Substring(0, _paymentdatetext.IndexOf('-')).Trim();
                    string _endpaymenttext = _paymentdatetext.Substring(_paymentdatetext.IndexOf('-') + 2, _length).Trim();
                    _startpaymentdate = DateTime.Parse(_startpaymenttetxt);
                    _endpaymentdate = DateTime.Parse(_endpaymenttext);
                }
                //--------------------------

                string _paymenttxt = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();

                var _payments = (from payment in db.Payments
                                 join prg in db.Projects on payment.ProjectId equals prg.id
                                 where (payment.Project.Contains(_project) || string.IsNullOrEmpty(_project))
                                        && (payment.Chief.Contains(_chief) || string.IsNullOrEmpty(_chief))
                                        && (payment.Client.Contains(_client) || string.IsNullOrEmpty(_client))
                                        && (payment.Agreement.Contains(_agreement) || string.IsNullOrEmpty(_agreement))
                                        && (payment.Manager.Contains(_manager) || string.IsNullOrEmpty(_manager))
                                        && (payment.PaymentDate >= _startpaymentdate && payment.PaymentDate <= _endpaymentdate || string.IsNullOrEmpty(_paymentdatetext)) //Диапазон дат
                                        && (payment.PaymentDesc.Contains(_paymentdesc) || string.IsNullOrEmpty(_paymentdesc))
                                        && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(prg.Chief ?? 0)) //Фильтрация записей по проектам для руководителей проектов
                                 select new
                                 {
                                     id = payment.id,
                                     Project = payment.Project,
                                     Chief = payment.Chief,
                                     Manager = payment.Manager,
                                     Client = payment.Client,
                                     Agreement = payment.Agreement,
                                     AgrDate = payment.AgrDate,
                                     Soder = payment.Soder,
                                     Summ = payment.Summ,
                                     AgrType = payment.AgrType,
                                     ProjectType = prg.ProjectType,
                                     ChiefName = prg.ChiefName,
                                     ProjectManagerName = prg.ProjectManagerName,
                                     StartDatePlan = prg.StartDatePlan,
                                     StartDateFact = prg.StartDateFact,
                                     Payment = payment.Payment,
                                     PaymentDate = payment.PaymentDate,
                                     PaymentDesc = payment.PaymentDesc,
                                     planBenefit = prg.planBenefit,
                                     planExpand = prg.planExpand
                                 }).AsEnumerable().Select(x => new PaymentsView
                                 {
                                     id = x.id,
                                     Project = x.Project,
                                     Chief = x.Chief,
                                     Manager = x.Manager,
                                     Client = x.Client,
                                     Agreement = x.Agreement,
                                     AgrDate = x.AgrDate,
                                     Soder = x.Soder,
                                     Summ = x.Summ,
                                     AgrType = x.AgrType,
                                     ProjectType = x.ProjectType,
                                     ChiefName = x.ChiefName,
                                     ProjectManagerName = x.ProjectManagerName,
                                     StartDatePlan = x.StartDatePlan,
                                     StartDateFact = x.StartDateFact,
                                     Payment = x.Payment,
                                     PaymentDate = x.PaymentDate,
                                     PaymentDesc = x.PaymentDesc,
                                     planBenefit = x.planBenefit,
                                     planExpand = x.planExpand
                                 }).ToList();

                _payments = _payments.Where(x => (x.Payment.ToString().Contains(_paymenttxt)) || string.IsNullOrEmpty(_paymenttxt)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir + ", id desc").ToList();
                }
                else
                {
                    _payments = _payments.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.id).ToList();
                }

                var pSum = _payments.GroupBy(x => new { x.Agreement, x.AgrDate, x.Summ }).Sum(x => x.Select(y => y.Summ).FirstOrDefault());
                var fSum = _payments.Sum(x => x.Payment);

                totalRecords = _payments.Count();

                var data = _payments.Skip(skip).Take(pageSize).ToList();
                return Json(new { psum = pSum, fsum = fSum, draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Входящие платежи Ф2
        public ActionResult ShowPaymentsF2(int? ProjectId, string startDate, string endDate)
        {
            if (ProjectId != null)
            {
                string ProjectName = db.Projects.FirstOrDefault(x => x.id == ProjectId).ShortName.ToString();
                ViewData["ProjectName"] = ProjectName;
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                ViewData["Period"] = startDate + " - " + endDate;
            }

            return View();
        }

        //Список входящих платежей 
        [HttpPost]
        public ActionResult LoadPaymentsF2()
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанного сотрудника для пользователя в роли "Руководитель проекта"

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                string _project = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _chief = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _client = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _agreement = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _manager = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _paymentdesc = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();

                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startpaymentdate = null;
                DateTime? _endpaymentdate = null;
                string _paymentdatetext = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_paymentdatetext))
                {
                    _paymentdatetext = _paymentdatetext.Trim();
                    int _length = (_paymentdatetext.Length) - (_paymentdatetext.IndexOf('-') + 2);
                    string _startpaymenttetxt = _paymentdatetext.Substring(0, _paymentdatetext.IndexOf('-')).Trim();
                    string _endpaymenttext = _paymentdatetext.Substring(_paymentdatetext.IndexOf('-') + 2, _length).Trim();
                    _startpaymentdate = DateTime.Parse(_startpaymenttetxt);
                    _endpaymentdate = DateTime.Parse(_endpaymenttext);
                }
                //--------------------------

                string _paymenttxt = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();

                var _payments = (from payment in db.Payments
                                 join prg in db.Projects on payment.ProjectId equals prg.id
                                 where (payment.Project.Contains(_project) || string.IsNullOrEmpty(_project))
                                        && (payment.Chief.Contains(_chief) || string.IsNullOrEmpty(_chief))
                                        && (payment.Client.Contains(_client) || string.IsNullOrEmpty(_client))
                                        && (payment.Agreement.Contains(_agreement) || string.IsNullOrEmpty(_agreement))
                                        && (payment.Manager.Contains(_manager) || string.IsNullOrEmpty(_manager))
                                        && (payment.PaymentDate >= _startpaymentdate && payment.PaymentDate <= _endpaymentdate || string.IsNullOrEmpty(_paymentdatetext)) //Диапазон дат
                                        && (payment.PaymentDesc.Contains(_paymentdesc) || string.IsNullOrEmpty(_paymentdesc))
                                        && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(prg.Chief ?? 0)) //Фильтрация записей по проектам для руководителей проектов
                                 select new
                                 {
                                     id = payment.id,
                                     Project = payment.Project,
                                     Chief = payment.Chief,
                                     Manager = payment.Manager,
                                     Client = payment.Client,
                                     Agreement = payment.Agreement,
                                     AgrDate = payment.AgrDate,
                                     Soder = payment.Soder,
                                     Summ = payment.Summ,
                                     AgrType = payment.AgrType,
                                     ProjectType = prg.ProjectType,
                                     ChiefName = prg.ChiefName,
                                     ProjectManagerName = prg.ProjectManagerName,
                                     StartDatePlan = prg.StartDatePlan,
                                     StartDateFact = prg.StartDateFact,
                                     Payment = payment.Payment,
                                     PaymentDate = payment.PaymentDate,
                                     PaymentDesc = payment.PaymentDesc,
                                     planBenefit = prg.planBenefit,
                                     planExpand = prg.planExpand
                                 }).AsEnumerable().Select(x => new PaymentsView
                                 {
                                     id = x.id,
                                     Project = x.Project,
                                     Chief = x.Chief,
                                     Manager = x.Manager,
                                     Client = x.Client,
                                     Agreement = x.Agreement,
                                     AgrDate = x.AgrDate,
                                     Soder = x.Soder,
                                     Summ = x.Summ,
                                     AgrType = x.AgrType,
                                     ProjectType = x.ProjectType,
                                     ChiefName = x.ChiefName,
                                     ProjectManagerName = x.ProjectManagerName,
                                     StartDatePlan = x.StartDatePlan,
                                     StartDateFact = x.StartDateFact,
                                     Payment = x.Payment,
                                     PaymentDate = x.PaymentDate,
                                     PaymentDesc = x.PaymentDesc,
                                     planBenefit = x.planBenefit,
                                     planExpand = x.planExpand
                                 }).ToList();

                _payments = _payments.Where(x => (x.Payment.ToString().Contains(_paymenttxt)) || string.IsNullOrEmpty(_paymenttxt)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir + ", id desc").ToList();
                }
                else
                {
                    _payments = _payments.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.id).ToList();
                }

                var pSum = _payments.GroupBy(x => new { x.Agreement, x.AgrDate, x.Summ }).Sum(x => x.Select(y => y.Summ).FirstOrDefault());
                var fSum = _payments.Sum(x => x.Payment);

                totalRecords = _payments.Count();

                var data = _payments.Skip(skip).Take(pageSize).ToList();
                return Json(new { psum = pSum, fsum = fSum, draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Исходящие платежи Ф2
        public ActionResult ShowActualDebitsF2(int? ProjectId, string startDate, string endDate)
        {
            if (ProjectId != null)
            {
                string ProjectName = db.Projects.FirstOrDefault(x => x.id == ProjectId).ShortName.ToString();
                ViewData["ProjectName"] = ProjectName;
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                ViewData["Period"] = startDate + " - " + endDate;
            }

            return View();
        }

        //Список входящих платежей 
        [HttpPost]
        public ActionResult LoadActualDebitsF2()
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанного сотрудника для пользователя в роли "Руководитель проекта"

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                string _worker = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _docnum = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                // Парсинг диапазона дат из DateRangePicker
                DateTime? _startpaymentdate = null;
                DateTime? _endpaymentdate = null;
                string _paymentdatetext = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_paymentdatetext))
                {
                    _paymentdatetext = _paymentdatetext.Trim();
                    int _length = (_paymentdatetext.Length) - (_paymentdatetext.IndexOf('-') + 2);
                    string _startpaymenttetxt = _paymentdatetext.Substring(0, _paymentdatetext.IndexOf('-')).Trim();
                    string _endpaymenttext = _paymentdatetext.Substring(_paymentdatetext.IndexOf('-') + 2, _length).Trim();
                    _startpaymentdate = DateTime.Parse(_startpaymenttetxt);
                    _endpaymentdate = DateTime.Parse(_endpaymenttext);
                }
                //--------------------------
                string _itemdescr = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _itemname = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _project = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                string _paymenttxt = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                string _receipt = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();
                string _docdescr = Request.Form.GetValues("columns[8][search][value]").FirstOrDefault().ToString();
                string _tags = Request.Form.GetValues("columns[9][search][value]").FirstOrDefault().ToString();


                var _payments = (from payment in db.ActualDebitsF2
                                 join prg in db.Projects on payment.ProjectId equals prg.id
                                 where ((payment.WorkerName.Contains(_worker) || string.IsNullOrEmpty(_worker))
                                        && (payment.DocumentNumber.Contains(_docnum) || string.IsNullOrEmpty(_docnum))
                                        && (payment.DocumentDate >= _startpaymentdate && payment.DocumentDate <= _endpaymentdate || string.IsNullOrEmpty(_paymentdatetext)) //Диапазон дат
                                        && (payment.ItemDescr.Contains(_itemdescr) || string.IsNullOrEmpty(_itemdescr))
                                        && (payment.IncomeItemsName.Contains(_itemname) || string.IsNullOrEmpty(_itemname))
                                        && (payment.ProjectName.Contains(_project) || string.IsNullOrEmpty(_project))
                                        && (payment.Receipt.Contains(_receipt) || string.IsNullOrEmpty(_receipt))
                                        && (payment.DocumentDescr.Contains(_docdescr) || string.IsNullOrEmpty(_docdescr))
                                        && (payment.Tags.Contains(_tags) || string.IsNullOrEmpty(_tags))
                                        && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(prg.Chief ?? 0))) //Фильтрация записей по проектам для руководителей проектов
                                 select new
                                 {
                                     id = payment.id,
                                     WorkerID = payment.WorkerId,
                                     WorkerName = payment.WorkerName,
                                     DocumentNumber = payment.DocumentNumber,
                                     DocumentDate = payment.DocumentDate,
                                     ItemDescr = payment.ItemDescr,
                                     IncomeItems = payment.IncomeItems,
                                     IncomeItemsName = payment.IncomeItemsName,
                                     ProjectId = payment.ProjectId,
                                     ProjectName = payment.ProjectName,
                                     ItemSum = payment.ItemSum,
                                     Receipt = payment.Receipt,
                                     DocumentDescr = payment.DocumentDescr,
                                     Tags = payment.Tags,
                                     ProjectType = prg.ProjectType,
                                     ChiefName = prg.ChiefName,
                                     ProjectManagerName = prg.ProjectManagerName,
                                     StartDatePlan = prg.StartDatePlan,
                                     StartDateFact = prg.StartDateFact
                                 }).AsEnumerable().Select(x => new ActualDebitsF2
                                 {
                                     id = x.id,
                                     WorkerId = x.WorkerID,
                                     WorkerName = x.WorkerName,
                                     DocumentNumber = x.DocumentNumber,
                                     DocumentDate = x.DocumentDate,
                                     ItemDescr = x.ItemDescr,
                                     IncomeItems = x.IncomeItems,
                                     IncomeItemsName = x.IncomeItemsName,
                                     ProjectId = x.ProjectId,
                                     ProjectName = x.ProjectName,
                                     ItemSum = x.ItemSum,
                                     Receipt = x.Receipt,
                                     DocumentDescr = x.DocumentDescr,
                                     Tags = x.Tags,
                                     ProjectType = x.ProjectType,
                                     ChiefName = x.ChiefName,
                                     ProjectManagerName = x.ProjectManagerName,
                                     StartDatePlan = x.StartDatePlan,
                                     StartDateFact = x.StartDateFact
                                 }).ToList();

                _payments = _payments.Where(x => x.ItemSum.Value.ToString().Contains(_paymenttxt)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir + ", id desc").ToList();
                }
                else
                {
                    _payments = _payments.OrderByDescending(x => x.DocumentDate).ThenByDescending(x => x.id).ToList();
                }

                var fSum = _payments.Sum(x => x.ItemSum);

                totalRecords = _payments.Count();

                var data = _payments.Skip(skip).Take(pageSize).ToList();
                return Json(new { fsum = fSum, draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Проекты
        public ActionResult ShowProjects()
        {
            return View();
        }

        public ActionResult ShowProjectsPartial()
        {
            return PartialView();
        }

        [HttpPost]
        //Проекты
        public ActionResult LoadProjects()
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

                string _fullname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _shortname = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _desc = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _projecttype = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _state = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();

                // Парсинг диапазона дат из DateRangePicker
                DateTime? _fsstartdate = null;
                DateTime? _fsenddate = null;
                string _fstarttext = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                if (!String.IsNullOrEmpty(_fstarttext))
                {
                    _fstarttext = _fstarttext.Trim();
                    int _length = (_fstarttext.Length) - (_fstarttext.IndexOf('-') + 2);
                    string _fsstartdatetext = _fstarttext.Substring(0, _fstarttext.IndexOf('-')).Trim();
                    string _fsenddatetext = _fstarttext.Substring(_fstarttext.IndexOf('-') + 2, _length).Trim();
                    _fsstartdate = DateTime.Parse(_fsstartdatetext);
                    _fsenddate = DateTime.Parse(_fsenddatetext);
                }
                //--------------------------

                // Парсинг диапазона дат из DateRangePicker
                DateTime? _festartdate = null;
                DateTime? _feenddate = null;
                string _fendtext = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                if (!String.IsNullOrEmpty(_fendtext))
                {
                    _fendtext = _fendtext.Trim();
                    int _length = (_fendtext.Length) - (_fendtext.IndexOf('-') + 2);
                    string _festartdatetext = _fendtext.Substring(0, _fendtext.IndexOf('-')).Trim();
                    string _feenddatetext = _fendtext.Substring(_fendtext.IndexOf('-') + 2, _length).Trim();
                    _festartdate = DateTime.Parse(_festartdatetext);
                    _feenddate = DateTime.Parse(_feenddatetext);
                }
                //--------------------------

                // Парсинг диапазона дат из DateRangePicker
                DateTime? _psstartdate = null;
                DateTime? _psenddate = null;
                string _pstarttext = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(_pstarttext))
                {
                    _pstarttext = _pstarttext.Trim();
                    int _length = (_pstarttext.Length) - (_pstarttext.IndexOf('-') + 2);
                    string _psstartdatetext = _pstarttext.Substring(0, _pstarttext.IndexOf('-')).Trim();
                    string _psenddatetext = _pstarttext.Substring(_pstarttext.IndexOf('-') + 2, _length).Trim();
                    _psstartdate = DateTime.Parse(_psstartdatetext);
                    _psenddate = DateTime.Parse(_psenddatetext);
                }
                //--------------------------

                // Парсинг диапазона дат из DateRangePicker
                DateTime? _pestartdate = null;
                DateTime? _peenddate = null;
                string _pendtext = Request.Form.GetValues("columns[8][search][value]").FirstOrDefault().ToString();
                if (!String.IsNullOrEmpty(_pendtext))
                {
                    _pendtext = _pendtext.Trim();
                    int _length = (_pendtext.Length) - (_pendtext.IndexOf('-') + 2);
                    string _pestartdatetext = _pendtext.Substring(0, _pendtext.IndexOf('-')).Trim();
                    string _peenddatetext = _pendtext.Substring(_pendtext.IndexOf('-') + 2, _length).Trim();
                    _pestartdate = DateTime.Parse(_pestartdatetext);
                    _peenddate = DateTime.Parse(_peenddatetext);
                }
                //--------------------------

                var _projects = (from project in db.Projects
                                 where (project.FullName.Contains(_fullname) || string.IsNullOrEmpty(_fullname))
                                        && (project.ShortName.Contains(_shortname) || string.IsNullOrEmpty(_shortname))
                                        && (project.Desc.Contains(_desc) || string.IsNullOrEmpty(_desc))
                                        && (project.ProjectType.Contains(_projecttype) || string.IsNullOrEmpty(_projecttype))
                                        && (project.State.Contains(_state) || string.IsNullOrEmpty(_state))
                                        && (project.StartDateFact >= _fsstartdate && project.StartDateFact <= _fsenddate || string.IsNullOrEmpty(_fstarttext)) //Диапазон дат
                                        && (project.EndDateFact >= _festartdate && project.EndDateFact <= _feenddate || string.IsNullOrEmpty(_fendtext)) //Диапазон дат
                                        && (project.StartDatePlan >= _psstartdate && project.StartDatePlan <= _psenddate || string.IsNullOrEmpty(_pstarttext)) //Диапазон дат
                                        && (project.EndDatePlan >= _pestartdate && project.EndDatePlan <= _peenddate || string.IsNullOrEmpty(_pendtext)) //Диапазон дат
                                 select new
                                 {
                                     id = project.id,
                                     FullName = project.FullName,
                                     Name = project.ShortName,
                                     ChiefName = project.ChiefName,
                                     ProjectManagerName = project.ProjectManagerName,
                                     Desc = project.Desc,
                                     ProjectType = project.ProjectType,
                                     State = project.State,
                                     StartDateFact = project.StartDateFact,
                                     EndDateFact = project.EndDateFact,
                                     StartDatePlan = project.StartDatePlan,
                                     EndDatePlan = project.EndDatePlan,
                                     planBenefit = project.planBenefit,
                                     planExpand = project.planExpand

                                 }).AsEnumerable().Select(x => new Projects
                                 {
                                     id = x.id,
                                     FullName = x.FullName,
                                     ShortName = x.Name,
                                     ChiefName = x.ChiefName,
                                     ProjectManagerName = x.ProjectManagerName,
                                     Desc = x.Desc,
                                     ProjectType = x.ProjectType,
                                     State = x.State,
                                     StartDateFact = x.StartDateFact,
                                     EndDateFact = x.EndDateFact,
                                     StartDatePlan = x.StartDatePlan,
                                     EndDatePlan = x.EndDatePlan,
                                     planBenefit = x.planBenefit,
                                     planExpand = x.planExpand
                                 });

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _projects = _projects.OrderBy(sortColumn + " " + sortColumnDir + ", id desc");
                }
                else
                {
                    _projects = _projects.OrderByDescending(x => x.id).ThenByDescending(x => x.id).ToList();
                }

                totalRecords = _projects.Count();

                var data = _projects.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadProjectsPartial()
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанных сотрудников для пользователя в роли "Руководитель проекта"

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int totalRecords = 0;

                string _shortname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _desc = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _projecttype = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _startdatefacttxt = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                DateTime? _startdatefact = string.IsNullOrEmpty(_startdatefacttxt) ? (DateTime?)null : DateTime.Parse(_startdatefacttxt);
                string _enddatefacttxt = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                DateTime? _enddatefact = string.IsNullOrEmpty(_enddatefacttxt) ? (DateTime?)null : DateTime.Parse(_enddatefacttxt);
                string _startdateplantxt = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                DateTime? _startdateplan = string.IsNullOrEmpty(_startdateplantxt) ? (DateTime?)null : DateTime.Parse(_startdateplantxt);
                string _enddateplantxt = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                DateTime? _enddateplan = string.IsNullOrEmpty(_enddateplantxt) ? (DateTime?)null : DateTime.Parse(_enddateplantxt);

                var _projects = (from project in db.Projects
                                 where (project.ShortName.Contains(_shortname) || string.IsNullOrEmpty(_shortname))
                                        && (project.Desc.Contains(_desc) || string.IsNullOrEmpty(_desc))
                                        && (project.ProjectType.Contains(_projecttype) || string.IsNullOrEmpty(_projecttype))
                                        && (project.StartDateFact == _startdatefact || _startdatefact == null)
                                        && (project.EndDateFact == _enddatefact || _enddatefact == null)
                                        && (project.StartDatePlan == _startdateplan || _startdateplan == null)
                                        && (project.EndDatePlan == _enddateplan || _enddateplan == null)
                                        && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(project.Chief ?? 0)) //Фильтрация записей по проектам для руководителей проектов
                                 select new
                                 {
                                     id = project.id,
                                     Name = project.ShortName,
                                     Desc = project.Desc,
                                     ProjectType = project.ProjectType,
                                     State = project.State,
                                     StartDateFact = project.StartDateFact,
                                     EndDateFact = project.EndDateFact,
                                     StartDatePlan = project.StartDatePlan,
                                     EndDatePlan = project.EndDatePlan

                                 }).AsEnumerable().Select(x => new Projects
                                 {
                                     id = x.id,
                                     ShortName = x.Name,
                                     Desc = x.Desc,
                                     ProjectType = x.ProjectType,
                                     State = x.State,
                                     StartDateFact = x.StartDateFact,
                                     EndDateFact = x.EndDateFact,
                                     StartDatePlan = x.StartDatePlan,
                                     EndDatePlan = x.EndDatePlan
                                 });



                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _projects = _projects.OrderBy(sortColumn + " " + sortColumnDir);
                }
                else
                {
                    _projects = _projects.OrderByDescending(x => x.id).ToList();
                }

                totalRecords = _projects.Count();

                var data = _projects.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Организации

        public ActionResult ShowOrganizations()
        {
            return View();
        }

        public ActionResult ShowOrganizationsPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult LoadOrganizations()
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

                string _title = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _code = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _inn = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _edrpou = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _state = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();


                var _organizations = (from organ in db.Organizations
                                      where (organ.Title.Contains(_title) || string.IsNullOrEmpty(_title))
                                            && (organ.Code.Contains(_code) || string.IsNullOrEmpty(_code))
                                            && (organ.INN.Contains(_inn) || string.IsNullOrEmpty(_inn))
                                            && (organ.EDRPOU.Contains(_edrpou) || string.IsNullOrEmpty(_edrpou))
                                            && (organ.State.Contains(_state) || string.IsNullOrEmpty(_state))
                                      select organ);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _organizations = _organizations.OrderBy(sortColumn + " " + sortColumnDir + ", id desc");
                }
                else
                {
                    _organizations = _organizations.OrderByDescending(x => x.id).ThenByDescending(x => x.id);
                }


                totalRecords = _organizations.Count();

                var data = _organizations.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Пользователи Directum

        public ActionResult ShowUsers()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadDUsers()
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

                string _fullname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _name = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _code = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _state = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();

                var _dusers = (from dusr in db.DUsers
                               where (dusr.FullName.Contains(_fullname) || string.IsNullOrEmpty(_fullname))
                                     && (dusr.Name.Contains(_name) || string.IsNullOrEmpty(_name))
                                     && (dusr.Code.Contains(_code) || string.IsNullOrEmpty(_code))
                                     && (dusr.State.Contains(_state) || string.IsNullOrEmpty(_state))
                               select dusr);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _dusers = _dusers.OrderBy(sortColumn + " " + sortColumnDir + ", id desc");
                }
                else
                {
                    _dusers = _dusers.OrderByDescending(x => x.id).ThenByDescending(x => x.id);
                }

                totalRecords = _dusers.Count();

                var data = _dusers.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Справочник статей расходов
        public ActionResult ShowExpenditures()
        {
            return View();
        }

        public ActionResult ShowExpendituresPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult LoadExpenditures()
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

                string _article = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _name = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _groupname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();

                var _expenditures = (from expdt in db.Expenditures
                                     where (expdt.Article.Contains(_article) || string.IsNullOrEmpty(_article))
                                           && (expdt.Name.Contains(_name) || string.IsNullOrEmpty(_name))
                                           && (expdt.GroupName.Contains(_groupname) || string.IsNullOrEmpty(_groupname))
                                     select expdt);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _expenditures = _expenditures.OrderBy(sortColumn + " " + sortColumnDir + ", id desc");
                }
                else
                {
                    _expenditures = _expenditures.OrderByDescending(x => x.Article);
                }

                totalRecords = _expenditures.Count();

                var data = _expenditures.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Справочник статей доходов
        public ActionResult ShowIncomes()
        {
            return View();
        }

        public ActionResult ShowIncomesPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult LoadIncomes()
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

                string _article = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _name = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _groupname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();

                var _incomes = (from _inc in db.Incomes
                                where (_inc.Article.Contains(_article) || string.IsNullOrEmpty(_article))
                                      && (_inc.Name.Contains(_name) || string.IsNullOrEmpty(_name))
                                      && (_inc.GroupName.Contains(_groupname) || string.IsNullOrEmpty(_groupname))
                                select _inc);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _incomes = _incomes.OrderBy(sortColumn + " " + sortColumnDir + ", id desc");
                }
                else
                {
                    _incomes = _incomes.OrderByDescending(x => x.Article);
                }

                totalRecords = _incomes.Count();

                var data = _incomes.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

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
