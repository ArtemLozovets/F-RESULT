using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Linq.Dynamic; //!=====!
using F_Result.Models;

namespace F_Result.Controllers
{
    [Authorize(Roles = "Administrator, Chief, ProjectManager, Accountant")]
    public class DirectumController : Controller
    {
        private FRModel db = new FRModel();

        public ActionResult ShowPayments()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadPayments()
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

                string _project = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _client = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _agreement = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _agrdatetxt = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                DateTime? _agrdate = string.IsNullOrEmpty(_agrdatetxt) ? (DateTime?)null : DateTime.Parse(_agrdatetxt);
                string _summtxt = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _agrtype = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                string _manager = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                string _paymenttxt = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();
                string _paymentdatetxt = Request.Form.GetValues("columns[8][search][value]").FirstOrDefault().ToString();
                DateTime? _paymentdate = string.IsNullOrEmpty(_paymentdatetxt) ? (DateTime?)null : DateTime.Parse(_paymentdatetxt);
                string _paymentdesc = Request.Form.GetValues("columns[9][search][value]").FirstOrDefault().ToString();

                var _payments = (from payment in db.Payments
                                 where (payment.Project.Contains(_project) || string.IsNullOrEmpty(_project))
                                        && (payment.Client.Contains(_client) || string.IsNullOrEmpty(_client))
                                        && (payment.Agreement.Contains(_agreement) || string.IsNullOrEmpty(_agreement))
                                        && (payment.AgrDate == _agrdate || _agrdate == null)
                                        && (payment.AgrType.Contains(_agrtype) || string.IsNullOrEmpty(_agrtype))
                                        && (payment.Manager.Contains(_manager) || string.IsNullOrEmpty(_manager))
                                        && (payment.PaymentDate == _paymentdate || _paymentdate == null)
                                        && (payment.PaymentDesc.Contains(_paymentdesc) || string.IsNullOrEmpty(_paymentdesc))
                                 select payment).AsEnumerable().Select(x => new Payments
                                 {
                                     Project = x.Project,
                                     Client = x.Client,
                                     Agreement = x.Agreement,
                                     AgrDate = x.AgrDate,
                                     Summ = x.Summ,
                                     AgrType = x.AgrType,
                                     Manager = x.Manager,
                                     Payment = x.Payment,
                                     PaymentDate = x.PaymentDate,
                                     PaymentDesc = x.PaymentDesc
                                 }).ToList();

                _payments = _payments.Where(x => (x.Summ.ToString().Contains(_summtxt) || string.IsNullOrEmpty(_summtxt)) && (x.Payment.ToString().Contains(_paymenttxt) || string.IsNullOrEmpty(_paymenttxt))).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                totalRecords = _payments.Count();

                var data = _payments.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = "" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult ShowProjects()
        {
            return View();
        }

        [HttpPost]
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

                string _startdatefacttxt = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();
                DateTime? _startdatefact = string.IsNullOrEmpty(_startdatefacttxt) ? (DateTime?)null : DateTime.Parse(_startdatefacttxt);

                string _enddatefacttxt = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault().ToString();
                DateTime? _enddatefact = string.IsNullOrEmpty(_enddatefacttxt) ? (DateTime?)null : DateTime.Parse(_enddatefacttxt);

                string _startdateplantxt = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault().ToString();
                DateTime? _startdateplan = string.IsNullOrEmpty(_startdateplantxt) ? (DateTime?)null : DateTime.Parse(_startdateplantxt);

                string _enddateplantxt = Request.Form.GetValues("columns[8][search][value]").FirstOrDefault().ToString();
                DateTime? _enddateplan = string.IsNullOrEmpty(_enddateplantxt) ? (DateTime?)null : DateTime.Parse(_enddateplantxt);

                var _projects = (from project in db.Projects
                                 where (project.FullName.Contains(_fullname) || string.IsNullOrEmpty(_fullname))
                                        && (project.ShortName.Contains(_shortname) || string.IsNullOrEmpty(_shortname))
                                        && (project.Desc.Contains(_desc) || string.IsNullOrEmpty(_desc))
                                        && (project.ProjectType.Contains(_projecttype) || string.IsNullOrEmpty(_projecttype))
                                        && (project.State.Contains(_state) || string.IsNullOrEmpty(_state))
                                        && (project.StartDateFact == _startdatefact || _startdatefact == null)
                                        && (project.EndDateFact == _enddatefact || _enddatefact == null)
                                        && (project.StartDatePlan == _startdateplan || _startdateplan == null)
                                        && (project.EndDatePlan == _enddateplan || _enddateplan == null)
                                 select new { 
                                    id = project.id,
                                    FullName = project.FullName,
                                    Name = project.ShortName,
                                    Desc = project.Desc,
                                    ProjectType = project.ProjectType,
                                    State = project.State,
                                    StartDateFact = project.StartDateFact,
                                    EndDateFact = project.EndDateFact,
                                    StartDatePlan = project.StartDatePlan,
                                    EndDatePlan = project.EndDatePlan

                                 }).AsEnumerable().Select(x => new Projects { 
                                     id = x.id,
                                     FullName = x.FullName,
                                     ShortName = x.Name,
                                     Desc = x.Desc,
                                     ProjectType = x.ProjectType,
                                     State = x.State,
                                     StartDateFact =x.StartDateFact,
                                     EndDateFact = x.EndDateFact,
                                     StartDatePlan = x.StartDatePlan,
                                     EndDatePlan = x.EndDatePlan
                                 
                                 });



                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _projects = _projects.OrderBy(sortColumn + " " + sortColumnDir);
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


        public ActionResult ShowOrganizations()
        {
            return View();
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
                    _organizations = _organizations.OrderBy(sortColumn + " " + sortColumnDir);
                }

                totalRecords = _organizations.Count();

                var data = _organizations.Skip(skip).Take(pageSize);
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, errormessage = ""}, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                var errormessage = "Ошибка выполнения запроса!\n\r"+ex.Message+"\n\r"+ex.StackTrace;
                var data = "";
                return Json(new {data = data, errormessage = errormessage}, JsonRequestBehavior.AllowGet);
            }
        }

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
                    _dusers = _dusers.OrderBy(sortColumn + " " + sortColumnDir);
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
