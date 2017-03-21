using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using F_Result.Models;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace F_Result.Areas.Administrator.Controllers
{
   // [AllowAnonymous]
    [Authorize]
    public class InitController : Controller
    {
        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

        private bool StopFlag = true; //Выполнение запрещено

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Init

        public String CreateUsers()
        {
            if (StopFlag)
            {
                return "Выполнение запрещено администратором";
            }

            db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug information------------------------------------

            db.Database.ExecuteSqlCommand("DROP INDEX UserNameIndex ON AspNetUsers");
            db.Database.ExecuteSqlCommand("ALTER TABLE AspNetUsers ALTER COLUMN UserName NVARCHAR(64) COLLATE Latin1_General_CS_AS NOT NULL");
            db.Database.ExecuteSqlCommand("CREATE UNIQUE NONCLUSTERED INDEX UserNameIndex ON AspNetUsers (UserName ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
            db.SaveChanges();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            if (roleManager.Roles.Count() > 0)
            {
                return "Выполнение операции прекращено. В таблице ролей имеются записи";
            }

            //Создаем роли администратора и пользователя
            var role1 = new ApplicationRole { Name = "Blockeduser", Description = "Заблокирован" };
            var role2 = new ApplicationRole { Name = "Administrator", Description = "Администратор" };
            var role3 = new ApplicationRole { Name = "Chief", Description = "Руководитель" };
            var role4 = new ApplicationRole { Name = "ProjectManager", Description = "Руководитель проекта" };
            var role5 = new ApplicationRole { Name = "Accountant", Description = "Бухгалтер" };

            // добавляем роль в бд
            roleManager.Create(role1);
            roleManager.Create(role2);
            roleManager.Create(role3);
            roleManager.Create(role4);
            roleManager.Create(role5);
            db.SaveChanges();

            //Создаем пользователя и администратора
            var admin = new ApplicationUser
            {
                UserName = "techadmin",
                Email = "admin@tech.com",
                FirstName = "Администратор",
                LastName = "Технический",
                MiddleName = "Technical Administrator",
                Post = "Администратор",
                UserRole = "Administrator"
            };

            var chief = new ApplicationUser
            {
                UserName = "techchief",
                Email = "chief@tech.com",
                FirstName = "Руководитель",
                LastName = "Технический",
                MiddleName = "Technical Chief",
                Post = "Руководитель",
                UserRole = "Chief"
            };

            var projectmanager = new ApplicationUser
            {
                UserName = "techmanager",
                Email = "projectmanager@tech.com",
                FirstName = "Руководитель проекта",
                LastName = "Технический",
                MiddleName = "Technical Project Manager",
                Post = "Руководитель проекта",
                UserRole = "ProjectManager"
            };

            var accountant = new ApplicationUser
            {
                UserName = "techaccountant",
                Email = "аccountant@tech.com",
                FirstName = "Бухгалтер",
                LastName = "Технический",
                MiddleName = "Technical Accountant",
                Post = "Бухгалтер",
                UserRole = "Accountant"
            };

            //Добавляем пользователей в базу данных
            UserManager.Create(admin, "Qwe!123");
            UserManager.AddToRole(admin.Id, admin.UserRole);


            UserManager.Create(chief, "Qwe!123");
            UserManager.AddToRole(chief.Id, chief.UserRole);

            UserManager.Create(projectmanager, "Qwe!123");
            UserManager.AddToRole(projectmanager.Id, projectmanager.UserRole);

            UserManager.Create(accountant, "Qwe!123");
            UserManager.AddToRole(accountant.Id, accountant.UserRole);

          

            return "Учетные записи созданы";

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();

                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

            }
            base.Dispose(disposing);
        }
    }
}