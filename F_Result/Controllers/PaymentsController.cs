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
    public class PaymentsController : Controller
    {
        private FRModel db = new FRModel();

        // GET: Payments
        public ActionResult Index()
        {
            return View(db.Payments.ToList());
        }

        // GET: Payments/Details/5
        public ActionResult Details(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments payments = db.Payments.Find(id);
            if (payments == null)
            {
                return HttpNotFound();
            }
            return View(payments);
        }

        // GET: Payments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AgrDate,Project,Client,Agreement,Summ,AgrType,Manager,Payment,PaymentDate,PaymentDesc")] Payments payments)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Add(payments);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(payments);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments payments = db.Payments.Find(id);
            if (payments == null)
            {
                return HttpNotFound();
            }
            return View(payments);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AgrDate,Project,Client,Agreement,Summ,AgrType,Manager,Payment,PaymentDate,PaymentDesc")] Payments payments)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payments).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(payments);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments payments = db.Payments.Find(id);
            if (payments == null)
            {
                return HttpNotFound();
            }
            return View(payments);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(DateTime id)
        {
            Payments payments = db.Payments.Find(id);
            db.Payments.Remove(payments);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


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

                              _payments = _payments.Where(x=>(x.Summ.ToString().Contains(_summtxt) || string.IsNullOrEmpty(_summtxt)) && (x.Payment.ToString().Contains(_paymenttxt) || string.IsNullOrEmpty(_paymenttxt))).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                totalRecords = _payments.Count();

                var data = _payments.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
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
