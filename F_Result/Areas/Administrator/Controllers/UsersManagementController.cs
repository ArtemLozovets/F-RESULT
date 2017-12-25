using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Linq.Dynamic; //!=====!
using System.Data.Entity;
using F_Result.Models;
using Newtonsoft.Json;

namespace F_Result.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersManagementController : Controller
    {
        private ApplicationSignInManager _signInManager;

        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();
        private FRModel dbModel = new FRModel();

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
        public ActionResult ShowUsers(string result)
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

                if (!string.IsNullOrEmpty(result) && result == "success")
                {
                    TempData["MessageOk"] = "Учетная запись создана";
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
                var VisitorList = visitorcontext.AspVisitors.Where(x => DbFunctions.TruncateTime(x.Date) == _date || String.IsNullOrEmpty(SearchDate)).OrderByDescending(x => x.Date).Take(100).ToList();
                return View(VisitorList);
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
            SelectList _roles = new SelectList(db.Roles.Where(Name => Name.Name != "blockeduser"), "Name", "Description");
            ViewBag.RolesList = _roles;
            if (ModelState.IsValid)
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug information------------------------------------
                if (db.Users.Where(u => u.UserName.ToLower() == model.UserName.ToLower()).Count() > 0)
                {
                    TempData["MessageError"] = string.Format("Ошибка создания учетной записи. Пользователь с логином \"{0}\" уже существует", model.UserName);
                    return View(model);
                }
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Post = model.Post,
                    UserRole = model.UserRole
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, model.UserRole);
                    //TempData["MessageOk"] = "Учетная запись создана";
                    //return RedirectToAction("ShowUsers", new { area = "Administrator", controller = "UsersManagement" });
                    return RedirectToAction("UsrWksRelation", new { area = "Administrator", controller = "UsersManagement", @UserId = user.Id });
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

        #region Метод сопоставления пользователей приложения сотрудникам (представление Workers) --------------------------

        [Authorize(Roles = "Administrator")]
        public ActionResult UsrWksRelation(string UserId, string Mode)
        {
            if (String.IsNullOrEmpty(UserId))
            {
                TempData["MessageError"] = "Не указан идентификатор пользователя";
                return RedirectToAction("ShowUsers", new { area = "Administrator", controller = "UsersManagement" });
            }

            if (!String.IsNullOrEmpty(Mode))
            {
                ViewData["VMode"] = Mode;
            }
            else ViewData["VMode"] = "Create";

            ApplicationUser _user = db.Users.FirstOrDefault(x => x.Id == UserId);
            ViewData["UserId"] = UserId;
            ViewData["UserInfoes"] = _user.LastName + " " + _user.FirstName + " " + _user.MiddleName + " (" + _user.UserName + ")";
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonResult UsrWksAjax(string UserId, string WorkersIds)
        {
            var errormessage = String.Empty;
            var data = String.Empty;

            if (String.IsNullOrEmpty(UserId))
            {
                errormessage = "Ошибка валидации модели!";
                data = "";
                return Json(new { Result = false, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                List<int> WksArray = JsonConvert.DeserializeObject<List<int>>(WorkersIds);
                List<UsrWksRelation> WksList = new List<UsrWksRelation>();

                var WksExist = dbModel.UsrWksRelations.FirstOrDefault(x => x.UserId == UserId && WksArray.Contains(x.WorkerId));
                if (WksExist != null)
                {
                    string WksNAme = dbModel.Workers.FirstOrDefault(x => x.id == WksExist.WorkerId).ShortName.ToString();
                    errormessage = "Операция отклонена. Сопоставление с сотрудником \"" + WksNAme + "\" уже существует в базе данных.";
                    data = "";
                    return Json(new { Result = false, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
                }

                foreach (var wks in WksArray)
                {
                    UsrWksRelation _wks = new UsrWksRelation
                    {
                        UserId = UserId,
                        WorkerId = wks
                    };
                    WksList.Add(_wks);
                }

                dbModel.UsrWksRelations.AddRange(WksList);
                dbModel.SaveChanges();

                return Json(new { Result = true, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                errormessage = "Ошибка выполнения запроса!\n\r" + ex.Message + "\n\r" + ex.StackTrace;
                data = "";
                return Json(new { Result = false, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
            }

        }

        // Таблица сотрудников с проектами
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult LoadWks(String UserId)
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

                string _name = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _orgname = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _prjname = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();

                List<Workers> wksList = new List<Workers>();

                var _wksSelect = (from worker in dbModel.Workers
                                  join usrwks in dbModel.UsrWksRelations on worker.id equals usrwks.WorkerId into usrwkstmp
                                  from usrwks in usrwkstmp.DefaultIfEmpty()
                                  where usrwks.UserId == UserId
                                  select worker).Distinct();

                var _wks = (from worker in dbModel.Workers
                            join usrwks in dbModel.UsrWksRelations on worker.id equals usrwks.WorkerId into usrwkstmp
                            from usrwks in usrwkstmp.DefaultIfEmpty()
                            where (worker.ShortName.Contains(_name) || string.IsNullOrEmpty(_name))
                                  && (worker.ShortName.Contains(_name) || string.IsNullOrEmpty(_name))
                                  && (worker.Organization.Contains(_orgname) || string.IsNullOrEmpty(_orgname))
                                  && (worker.projects.Contains(_prjname) || string.IsNullOrEmpty(_prjname))
                            select worker).Distinct();


                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _wks = _wks.OrderBy(sortColumn + " " + sortColumnDir + ", id desc");
                }
                else
                {
                    _wks = _wks.OrderByDescending(x => x.id).ThenByDescending(x => x.id);
                }



                totalRecords = _wks.Count();

                wksList.AddRange(_wksSelect);
                wksList.AddRange(_wks);

                var data = wksList.Skip(skip).Take(pageSize);
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
                dbModel.Dispose();

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
