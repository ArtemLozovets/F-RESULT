using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using System.Data.Entity;
using F_Result.Models;



namespace F_Result.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersManagementController : Controller
    {
        private ApplicationSignInManager _signInManager;

        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

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

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        #region ShowUsers Метод генерации списка пользователей-----------------------------------------------------

        // GET: UsersManagement
        public ActionResult ShowUsers()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            List<UsRoleViewModel> usrollist = new List<UsRoleViewModel>();
            UpdateUserInfoesViewModel usrolcontext = new UpdateUserInfoesViewModel();

            var roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(db));
            ApplicationRole currentRole = null;
            try
            {
                users = db.Users.ToList();
                foreach (var useritem in users)
                {
                    string RoleString = string.Empty;
                    foreach (var roleitem in useritem.Roles)
                    {
                        currentRole = roleManager.FindById(roleitem.RoleId);
                        if (currentRole == null) RoleString = "Неопределена";
                        else RoleString += currentRole.Description.ToString() + " ";
                    }

                    UsRoleViewModel currentuser = new UsRoleViewModel
                    {
                        UId = useritem.Id,
                        UUserName = useritem.UserName,
                        UEmail = useritem.Email,
                        UFirstName = useritem.FirstName,
                        ULastName = useritem.LastName,
                        UMiddleName = useritem.MiddleName,
                        UPost = useritem.Post,
                        URole = RoleString
                    };
                    usrollist.Add(currentuser);
                }

                return View(usrollist);

            }
            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
                return View("Error");
            }
        }
        #endregion

        #region Метод генерации таблицы аудита входа пользователей --------------------------------------------
        [HttpGet]
        public ActionResult LoginAudit(String SearchDate)
        {

            if (!String.IsNullOrEmpty(SearchDate))
            {
                ViewData["SearchDate"] = SearchDate;
            }

            DateTime _date;
            DateTime.TryParse(SearchDate, out _date);

            using (FRModel visitorcontext = new FRModel())
            {
                return View(visitorcontext.AspVisitors.Where(x=>DbFunctions.TruncateTime(x.Date) == _date || String.IsNullOrEmpty(SearchDate)).OrderByDescending(x=>x.Date).ToList());
            }
        }

        #endregion

        #region BlockUser Метод блокирования учетной записи--------------------------------------

        // GET: /UsersManagement/BlockUser
        [HttpGet]
        public async Task<ActionResult> BlockUser(string UserId)
        {
            if (UserId == null)
            {
                ViewBag.ErMes = "Не указан идентификатор пользователя";
                ViewBag.ErStack = "";
                return View("Error");
            }
            var CurrentUserName = User.Identity.Name;
            var userlist = UserManager.Users.ToList();
            foreach (var item in userlist)
            {
                if (item.UserName == CurrentUserName)
                {
                    if (item.Id == UserId) return View("SelfBlockError");
                }
            }

            var roleslist = UserManager.GetRoles(UserId).ToArray();
            if (roleslist != null)
            {
                var result = await UserManager.RemoveFromRolesAsync(UserId, roleslist);

                if (result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(UserId, "Blockeduser");
                    if (result.Succeeded)
                    {
                        TempData["MessageOk"] = "Учетная запись заблокирована";
                        return RedirectToAction("ShowUsers", "UsersManagement");
                    }
                    AddErrors(result);
                    return View();
                }
                AddErrors(result);
                return View();
            }
            TempData["MessageError"] = "Ошибка блокирования ученой записи";
            return RedirectToAction("ShowUsers", "UsersManagement");
        }

        #endregion

        #region UnblockUser Метод разблокирования учетной записи пользователя---------------------------------

        // GET: /UsersManagement/UnblockUser
        [HttpGet]
        public async Task<ActionResult> UnblockUser(string UserId)
        {
            if (UserId == null)
            {
                ViewBag.ErMes = "Не указан идентификатор пользователя";
                ViewBag.ErStack = "";
                return View("Error");
            }
            string OldRole = string.Empty;
            var userlist = UserManager.Users.ToList();
            foreach (var item in userlist)
            {
                if (item.Id == UserId) OldRole = item.UserRole;
            }

            var result = await UserManager.RemoveFromRoleAsync(UserId, "Blockeduser");
            if (result.Succeeded)
            {
                result = await UserManager.AddToRoleAsync(UserId, OldRole);
                if (result.Succeeded)
                {
                    TempData["MessageOk"] = "Учетная запись разблокирована";
                    return RedirectToAction("ShowUsers", "UsersManagement");
                }
                AddErrors(result);
                return View();
            }
            AddErrors(result);
            return View();
        }

        #endregion

        #region UpdateUserInfoes Метод редактирования данных пользователя-------------------------------------------

        // GET: /UsersManagement/UpdateUserInfoes
        [HttpGet]
        public ActionResult UpdateUserInfoes(string UserId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErMes = "Ошибка валидации модели";
                ViewBag.ErStack = "";
                return View("Error");
            }
            if (UserId == null)
            {
                ViewBag.ErMes = "Не указан идентификатор пользователя";
                ViewBag.ErStack = "";
                return View("Error");
            }
            try
            {
                ApplicationUser DUser = db.Users.Find(UserId);
                if (DUser != null)
                {

                    var _currentuser = HttpContext.User.Identity.GetUserId();
                    ViewBag.CurrentUser = _currentuser;
                    SelectList _roles = new SelectList(db.Roles.Where(Name => Name.Name != "blockeduser"), "Id", "Description", DUser.Roles.FirstOrDefault().RoleId);
                    ViewBag.RolesList = _roles;
                    UpdateUserInfoesViewModel currentuser = new UpdateUserInfoesViewModel
                    {
                        Id = DUser.Id,
                        UserName = DUser.UserName,
                        Email = DUser.Email,
                        FirstName = DUser.FirstName,
                        LastName = DUser.LastName,
                        MiddleName = DUser.MiddleName,
                        Post = DUser.Post,
                        Role = DUser.Roles.FirstOrDefault().RoleId
                    };
                    return View(currentuser);
                }
                ViewBag.ErMes = "Пользователь с Id " + UserId + " не нейден";
                ViewBag.ErStack = "";
                return View("Error");
            }

            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
                return View("Error");
            }
        }

        // POST: /UsersManagement/UpdateUserInfos        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateUserInfoes(UpdateUserInfoesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErMes = "Ошибка валидации модели";
                ViewBag.ErStack = "";
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErMes = "Пользователь с Id " + model.Id + " не найден";
                ViewBag.ErStack = "";
                return View("Error");
            }
            var _query =
                                    (from _role in db.Roles
                                     where _role.Id == model.Role
                                     select _role.Name)
                                    .First();
            user.Id = model.Id;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.Post = model.Post;
            user.Email = model.Email;
            user.UserRole = _query;
            var result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var roleslist = UserManager.GetRoles(model.Id).ToArray();
                result = await UserManager.RemoveFromRolesAsync(model.Id, roleslist);
                if (result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(user.Id, _query);
                    if (result.Succeeded)
                    {
                        TempData["MessageOk"] = "Учетные данные изменены";
                        return RedirectToAction("ShowUsers", "UsersManagement");
                    }
                    AddErrors(result);
                    return View(model);
                }
                AddErrors(result);
                return View(model);
            }
            AddErrors(result);
            return View(model);
        }

        #endregion

        #region ChangePassword Метод изменения пароля пользовтеля-------------------------------------------------

        // GET: /UsersManagement/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword(string UserId)
        {
            if (UserId == null)
            {
                ViewBag.ErMes = "Не указан идентификатор пользователя";
                ViewBag.ErStack = "";
                return View("Error");
            }
            try
            {
                ApplicationUser User = db.Users.Find(UserId);

                if (User != null)
                {
                    UserPasswordChangeViewModel passwordchange = new UserPasswordChangeViewModel();
                    passwordchange.Id = User.Id;
                    passwordchange.UserName = User.UserName;
                    passwordchange.Password = string.Empty;
                    passwordchange.ConfirmPassword = string.Empty;
                    return View(passwordchange);
                }
                else
                {
                    ViewBag.ErMes = "Пользователь с Id " + User.Id + " не найден";
                    ViewBag.ErStack = "";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
                return View("Error");
            }
        }

        // POST: /UsersManagement/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(UserPasswordChangeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErMes = "Ошибка валидации модели";
                ViewBag.ErStack = "";
                return View("Error");
            }
            ApplicationUser user = await UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErMes = "Пользователь с Id " + model.Id + " не найден";
                ViewBag.ErStack = "";
                return View("Error");
            }
            var result = await UserManager.PasswordValidator.ValidateAsync(model.Password);
            if (result.Succeeded)
            {
                user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["MessageOk"] = "Пароль изменен";
                    return RedirectToAction("ShowUsers", "UsersManagement");
                }
                AddErrors(result);
                return View(model);
            }
            AddErrors(result);
            return View(model);
        }

        #endregion

        #region SelfPasswordChange Метод изменения пароля пользователем-----------------------------------
        // GET: /UsersManagement/SelfPasswordChange
        [HttpGet]
        [AllowAnonymous]
        public ActionResult SelfPasswordChange()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Index", new { area = "", controller = "Home" });
            }
            else return View();
        }

        // POST: /UsersManagement/SelfPasswordChange
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelfPasswordChange(SelfPasswordChangeViewModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Index", new { area = "", controller = "Home" });
            }
            if (!ModelState.IsValid)
            {
                ViewBag.ErMes = "Ошибка валидации модели";
                ViewBag.ErStack = "";
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                ViewBag.ErMes = "Пользователь с Id " + user.Id + " не найден";
                ViewBag.ErStack = "";
                return View("Error");
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var _user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(_user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { area = "", controller = "Home" });
            }
            AddErrors(result);
            return View(model);
        }

        #endregion

        #region Метод входа пользователя в систему----------------------------------------------
        // GET: /UsersManagement/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /UsersManagement/Login
        [HttpPost]
        [AllowAnonymous]
        [LoginAudit]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName.ToLowerInvariant(), model.Password, model.RememberMe, shouldLockout: true);

            TempData["LoginName"] = model.UserName;
            TempData["ReturnUrl"] = "/" + returnUrl;

            switch (result)
            {
                case SignInStatus.Success:
                    TempData["Password"] = "************";
                    TempData["LoginResult"] = "Success";
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    TempData["Password"] = "************";
                    TempData["LoginResult"] = "Blocked";
                    return View("~/Areas/Administrator/Views/UsersManagement/ForbiddenError.cshtml");

                case SignInStatus.Failure:
                default:
                    TempData["Password"] = model.Password;
                    TempData["LoginResult"] = "Failure";
                    ModelState.AddModelError("", "Вы ввели неверный логин или пароль!");
                    return View(model);
            }
        }
        #endregion

        #region Метод выхода пользователя из системы(LogOff)-----------------------------------------
        // POST: /UsersManagement/LogOff
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            SignInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", new { area = "Administrator", controller = "UsersManagement" });
        }
        #endregion

        #region Метод регистрации нового пользователя-----------------------------------------------------------
        // GET: /UsersManagement/Register
        public ActionResult Register()
        {
            SelectList _roles = new SelectList(db.Roles.Where(Name => Name.Name != "blockeduser"), "Name", "Description");
            ViewBag.RolesList = _roles;
            return View();
        }

        // POST: /UsersManagement/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            SelectList _roles = new SelectList(db.Roles.Where(Name => Name.Name != "blockeduser"), "Name", "Name");
            ViewBag.RolesList = _roles;
            if (ModelState.IsValid)
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug information------------------------------------
                if (db.Users.Where(u => u.UserName.ToLower() == model.UserName.ToLower()).Count() > 0)
                {
                    TempData["MessageError"] = string.Format("Ошибка создания учетной записи. Пользователь с логином \"{0}\" уже существует", model.UserName);
                    return View(model);
                }
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, MiddleName = model.MiddleName, Post = model.Post, UserRole = model.UserRole };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, model.UserRole);
                    TempData["MessageOk"] = "Учетная запись создана";
                    return RedirectToAction("ShowUsers", new { area = "Administrator", controller = "UsersManagement" });
                }
                else
                {
                    TempData["MessageError"] = "Ошибка создания учетной записи";
                    return View(model);
                }
            }
            TempData["MessageError"] = "Ошибка валидации модели";
            return View(model);
        }
        #endregion

        #region Helpers --------------------------------------------------------------------------------------
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", new { area = "", controller = "Home" });
        }

        #endregion

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

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            base.Dispose(disposing);
        }

    }
}
