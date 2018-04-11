using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Linq.Dynamic; //!=====!

namespace F_Result.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private FRModel db = new FRModel();

        public ActionResult CommentsShow()
        {
            return View();
        }

        //Список отзывов
        [HttpPost]
        public ActionResult LoadComments()
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

                string _dateTXT = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _commentTXT = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _linkTXT = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _appDateTXT = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _appUserTXT = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                string _statusTXT = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault().ToString();

                // Парсинг диапазона дат из DateRangePicker (Дата создания)
                DateTime? _startdate = null;
                DateTime? _enddate = null;
                if (!string.IsNullOrEmpty(_dateTXT ))
                {
                    _dateTXT = _dateTXT.Trim();
                    int _length = (_dateTXT.Length) - (_dateTXT.IndexOf('-') + 2);
                    string _startdateStr = _dateTXT.Substring(0, _dateTXT.IndexOf('-')).Trim();
                    string _enddateStr = _dateTXT.Substring(_dateTXT.IndexOf('-') + 2, _length).Trim();
                    _startdate = DateTime.Parse(_startdateStr);
                    _enddate = DateTime.Parse(_enddateStr);
                }
                //--------------------------

                // Парсинг диапазона дат из DateRangePicker (Дата рассмотрения)
                DateTime? _app_startdate = null;
                DateTime? _app_enddate = null;
                if ((!string.IsNullOrEmpty(_appDateTXT)) && (_appDateTXT != "Нерассмотренные"))
                {
                    _appDateTXT = _appDateTXT.Trim();
                    int _length = (_appDateTXT.Length) - (_appDateTXT.IndexOf('-') + 2);
                    string _app_startdateStr = _appDateTXT.Substring(0, _appDateTXT.IndexOf('-')).Trim();
                    string _app_enddateStr = _appDateTXT.Substring(_appDateTXT.IndexOf('-') + 2, _length).Trim();
                    _app_startdate = DateTime.Parse(_app_startdateStr);
                    _app_enddate = DateTime.Parse(_app_enddateStr);
                }
                //--------------------------

                int _status;
                Int32.TryParse(_statusTXT, out _status);
                Feedback.StateEnum _state = (Feedback.StateEnum)_status;

                string curr_user = "";
                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    curr_user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                }

                var _cm = (from comment in db.Feedback
                           join usr in db.IdentityUsers on comment.UserId equals usr.Id into usrtmp
                           from usr in usrtmp.DefaultIfEmpty()
                           join app_usr in db.IdentityUsers on comment.ApprovedUserId equals app_usr.Id into appusrtmp
                           from app_usr in appusrtmp.DefaultIfEmpty()
                           where (comment.UserId == curr_user
                                    && (_status == 0 || comment.Status == _state)
                                    && (comment.DateOfCreation >= _startdate && comment.DateOfCreation <= _enddate || string.IsNullOrEmpty(_dateTXT)) //Дата создания
                                    && ((string.IsNullOrEmpty(_appDateTXT)) 
                                            || (comment.DateOfApproved >= _app_startdate && comment.DateOfApproved <= _app_enddate)
                                            || (_appDateTXT == "Нерассмотренные" && comment.DateOfApproved == null)    
                                        ) //Дата рассмотрения
                                    && (string.IsNullOrEmpty(_commentTXT) || comment.Comment.Contains(_commentTXT))
                                    && (string.IsNullOrEmpty(_linkTXT) || comment.SbUrl.Contains(_linkTXT))
                                    && (string.IsNullOrEmpty(_appUserTXT) || app_usr.LastName.Contains(_appUserTXT))
                                    )
                           select new
                           {
                               FeedbackId = comment.FeedbackId,
                               Comment = comment.Comment,
                               SbUrl = comment.SbUrl,
                               DateOfCreation = comment.DateOfCreation,
                               UserId = comment.UserId,
                               UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                               ApprovedUserId = comment.ApprovedUserId,
                               ApprovedUserFN = comment.ApprovedUserId == null ? "" : app_usr.LastName + " " + app_usr.FirstName.Substring(0, 1) + "." + app_usr.MiddleName.Substring(0, 1) + ".",
                               Status = comment.Status,
                               DateOfApproved = comment.DateOfApproved
                           }).AsEnumerable().Select(x => new Feedback
                           {
                               FeedbackId = x.FeedbackId,
                               Comment = x.Comment,
                               SbUrl = x.SbUrl,
                               DateOfCreation = x.DateOfCreation,
                               UserId = x.UserId,
                               UserFN = x.UserFN,
                               ApprovedUserId = x.ApprovedUserId,
                               ApprovedUserFN = x.ApprovedUserFN,
                               Status = (Feedback.StateEnum)x.Status,
                               DateOfApproved = x.DateOfApproved
                           }).ToList();


                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _cm = _cm.OrderBy(sortColumn + " " + sortColumnDir + ", FeedbackId desc").ToList();
                }
                else
                {
                    _cm = _cm.OrderByDescending(x => x.DateOfCreation).ThenByDescending(x => x.FeedbackId).ToList();
                }

                totalRecords = _cm.Count();
                var data = _cm.Skip(skip).Take(pageSize);

                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var message = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                var data = "";
                return Json(new { data = data, result = false, message = message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CommentsCreate(string cmText, string cmUrl)
        {
            if (string.IsNullOrEmpty(cmText) || string.IsNullOrEmpty(cmUrl))
            {
                return Json(new { result = false, message = "Отсутствуют необходимые параметры" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Feedback fb = new Feedback()
                {
                    DateOfCreation = DateTime.Today,
                    Comment = cmText,
                    SbUrl = cmUrl,
                    Status = Feedback.StateEnum.Добавлен
                };

                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    fb.UserId = user;
                }

                db.Feedback.Add(fb);
                db.SaveChanges();
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
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