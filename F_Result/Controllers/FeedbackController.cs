using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace F_Result.Controllers
{
    public class FeedbackController : Controller
    {
        // GET: Feedback
        public ActionResult CommentsShow(string cUrl)
        {
            if (!string.IsNullOrEmpty(cUrl))
            {
                ViewData["cUrl"] = cUrl;
            }
            return View();
        }
    }
}