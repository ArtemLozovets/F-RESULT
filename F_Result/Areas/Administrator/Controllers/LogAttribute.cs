using System.Web.Mvc;
using F_Result.Models;
using System;
//.....................
public class LoginAuditAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        
    }

    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        var request = filterContext.HttpContext.Request;
        bool? _resbool = null;
        if ((string)filterContext.Controller.TempData["LoginResult"] == "Success") _resbool = true;
        if ((string)filterContext.Controller.TempData["LoginResult"] == "Failure") _resbool = false;


        AspVisitor visitor = new AspVisitor()
        {
            Login = filterContext.Controller.TempData["LoginName"].ToString(),
            Ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
            Url = filterContext.Controller.TempData["ReturnUrl"].ToString(),
            Password = filterContext.Controller.TempData["Password"].ToString(),
            Result = _resbool,
            Date = DateTime.Now,
        };

        using (FRModel db = new FRModel())
        {
            db.AspVisitors.Add(visitor);
            db.SaveChanges();
        }
        base.OnActionExecuted(filterContext);
    }
}