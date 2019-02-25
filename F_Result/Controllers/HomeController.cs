using F_Result.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using F_Result.Methods;


namespace F_Result.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //Получаем идентификатор текущего пользователя
                using (ApplicationDbContext aspdb = new ApplicationDbContext())
                {
                    using (RoleManager<ApplicationRole> roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(aspdb)))
                    {
                        var _UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        ApplicationRole currentRole = roleManager.FindById(aspdb.Users.FirstOrDefault(x => x.Id == _UserId).Roles.FirstOrDefault().RoleId);

                        @ViewData["FirstName"] = aspdb.Users.FirstOrDefault(x => x.Id == _UserId).FirstName.ToString();
                        @ViewData["UserName"] = aspdb.Users.FirstOrDefault(x => x.Id == _UserId).UserName.ToString();
                        @ViewData["UserRole"] = currentRole.Description.ToString();
                    }
                }

                using (FRModel db = new FRModel())
                {
                    db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

                    List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанных сотрудников для пользователя в роли "Руководитель проекта"
                    //Количество действующих проектов
                    @ViewData["PrjCount"] = db.Projects.Where(x => (x.EndDateFact == null)
                                                                    && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(x.Chief ?? 0)))
                                                                    .Count();
                }

            }
            return View();
        }

        public ActionResult InDeveloping()
        {
            return View();
        }

        [AllowAnonymous]
        [DoNotResetAuthCookie]
        public ActionResult CheckAuth()
        {
            bool isAuth = false;
            if (Request.IsAuthenticated)
            {
                isAuth = true;
            }
            return Json(isAuth, JsonRequestBehavior.AllowGet);
        }


        // Фильтр блокирует работу механизма SlidingExpiration путем запрета отправки клиенту нового тикета аутентификации. 
        // Необходим в случаях, когда Ajax - запрос выполнется автоматически без участия пользователя 
        // и не должен продлять время сессии пользовталя через SlidingExpiration.
        public class DoNotResetAuthCookieAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var owinContext = System.Web.HttpContext.Current.GetOwinContext();
                var owinResponse = owinContext.Response;

                owinResponse.OnSendingHeaders(state =>
                { owinResponse.Cookies.Delete("System_CK"); }, null);
            }
        }

    }
}