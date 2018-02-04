﻿using F_Result.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using Microsoft.AspNet.Identity.EntityFramework;

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
            }
            return View();
        }

        public ActionResult InDeveloping()
        {
            return View();
        }

    }
}