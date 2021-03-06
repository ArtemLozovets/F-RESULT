﻿using System;
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
            if (!string.IsNullOrEmpty(result) && result == "success")
            {
                TempData["MessageOk"] = "Операция завершена успешно";
            }

            return View();
        }

        [HttpPost]
        public ActionResult GetUsers()
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

                string _fio = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                string _login = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                string _email = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                string _post = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();
                string _role = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();

                var _usr = (from useritem in dbModel.VAspUsers
                            where (
                                    (string.IsNullOrEmpty(_fio)
                                        || useritem.UFirstName.Contains(_fio)
                                        || useritem.ULastName.Contains(_fio)
                                        || useritem.UMiddleName.Contains(_fio))
                                    && (string.IsNullOrEmpty(_login) || useritem.UUserName.Contains(_login))
                                    && (string.IsNullOrEmpty(_email) || useritem.UEmail.Contains(_email))
                                    && (string.IsNullOrEmpty(_post) || useritem.UPost.Contains(_post))
                                    && (string.IsNullOrEmpty(_role) || useritem.URoleName.Contains(_role))
                            )
                            select new
                           {
                               Id = useritem.UId,
                               UserName = useritem.UUserName,
                               Email = useritem.UEmail,
                               FirstName = useritem.UFirstName,
                               LastName = useritem.ULastName,
                               MiddleName = useritem.UMiddleName,
                               FullName = useritem.UFullName,
                               Post = useritem.UPost,
                               RoleName = useritem.URoleName,
                               RoleDesc = useritem.URoleDesc,
                               Workers = useritem.UWorkers
                           }).AsEnumerable().Select(x => new VAspUsers
                           {
                               UId = x.Id,
                               UUserName = x.UserName,
                               UEmail = x.Email,
                               UFirstName = x.FirstName,
                               ULastName = x.LastName,
                               UMiddleName = x.MiddleName,
                               UFullName = x.LastName + " " + x.FirstName + " " + x.MiddleName,
                               UPost = x.Post,
                               URoleDesc = x.RoleDesc,
                               URoleName = x.RoleName,
                               UWorkers = x.Workers
                           }).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _usr = _usr.OrderBy(sortColumn + " " + sortColumnDir + ", UId desc").ToList();
                }
                else
                {
                    _usr = _usr.OrderBy(x => x.UUserName).ToList();
                }

                totalRecords = _usr.Count();
                var data = _usr.Skip(skip).Take(pageSize);

                return Json(new
                {
                    result = true,
                    draw = draw,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords,
                    data = data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Ошибка выполнения запроса! " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Метод генерации таблицы аудита входа пользователей --------------------------------------------
        [HttpGet]
        public ActionResult LoginAudit(String SearchDate)
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetLogins()
        {
            try
            {
                using (FRModel visitorcontext = new FRModel())
                {
                    visitorcontext.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    int totalRecords = 0;

                    string _login = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault().ToString();
                    string _ip = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault().ToString();
                    string _url = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault().ToString();
                    string _resulttext = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault().ToString();

                    bool _result = true;
                    bool.TryParse(_resulttext, out _result);

                    // Парсинг диапазона дат из DateRangePicker
                    DateTime? _startdate = null;
                    DateTime? _enddate = null;
                    string _datetext = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault().ToString();
                    if (!string.IsNullOrEmpty(_datetext))
                    {
                        _datetext = _datetext.Trim();
                        int _length = (_datetext.Length) - (_datetext.IndexOf('-') + 2);
                        string _startagrdatetext = _datetext.Substring(0, _datetext.IndexOf('-')).Trim();
                        string _endagrdatetext = _datetext.Substring(_datetext.IndexOf('-') + 2, _length).Trim();
                        _startdate = DateTime.Parse(_startagrdatetext);
                        _enddate = DateTime.Parse(_endagrdatetext);
                        _enddate = _enddate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                    }
                    //--------------------------

                    var _la = (from visitor in visitorcontext.AspVisitors
                               where (
                                    (string.IsNullOrEmpty(_login) || visitor.Login.Contains(_login))
                                    && (string.IsNullOrEmpty(_ip) || visitor.Ip.Contains(_ip))
                                    && (string.IsNullOrEmpty(_url) || visitor.Url.Contains(_url))
                                    && (string.IsNullOrEmpty(_ip) || visitor.Ip.Contains(_ip))
                                    && (string.IsNullOrEmpty(_resulttext) || visitor.Result == _result)
                                    && (string.IsNullOrEmpty(_datetext) || visitor.Date >= _startdate && visitor.Date <= _enddate) //Диапазон дат
                               )
                               select new
                               {
                                   Id = visitor.Id,
                                   Login = visitor.Login,
                                   Password = visitor.Password,
                                   Ip = visitor.Ip,
                                   Url = visitor.Url,
                                   Result = visitor.Result,
                                   Date = visitor.Date
                               }).AsEnumerable().Select(x => new AspVisitor
                               {
                                   Id = x.Id,
                                   Login = x.Login,
                                   Password = x.Password,
                                   Ip = x.Ip,
                                   Url = x.Url,
                                   Result = x.Result,
                                   Date = x.Date
                               }).ToList();

                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        _la = _la.OrderBy(sortColumn + " " + sortColumnDir + ", Id desc").ToList();
                    }
                    else
                    {
                        _la = _la.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToList();
                    }

                    totalRecords = _la.Count();
                    var data = _la.Skip(skip).Take(pageSize);

                    return Json(new
                    {
                        result = true,
                        draw = draw,
                        recordsFiltered = totalRecords,
                        recordsTotal = totalRecords,
                        data = data
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Ошибка выполнения запроса! " + ex.Message }, JsonRequestBehavior.AllowGet);
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
                ViewBag.ErMes = "Пользователь с Id " + UserId + " не найден";
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
            //var result = await SignInManager.PasswordSignInAsync(model.UserName.ToLowerInvariant(), model.Password, model.RememberMe, shouldLockout: true);
            var result = await SignInManager.PasswordSignInAsync(model.UserName.ToLowerInvariant(), model.Password, false, shouldLockout: true);

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
                dbModel.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

                List<int> WksArray = JsonConvert.DeserializeObject<List<int>>(WorkersIds);
                List<UsrWksRelation> WksList = new List<UsrWksRelation>();

                foreach (var wks in WksArray)
                {
                    UsrWksRelation _wks = new UsrWksRelation
                    {
                        UserId = UserId,
                        WorkerId = wks
                    };
                    WksList.Add(_wks);
                }

                using (var dbContextTransaction = dbModel.Database.BeginTransaction())
                {
                    try
                    {
                        //Удаляем ВСЕ сопоставления выбранного пользователя из БД
                        var DelWksList = dbModel.UsrWksRelations.Where(x => x.UserId == UserId).ToList();
                        var result = dbModel.UsrWksRelations.RemoveRange(DelWksList);
                        db.SaveChanges();

                        //Добавляем актуальные сопоставления в БД
                        dbModel.UsrWksRelations.AddRange(WksList);
                        dbModel.SaveChanges();

                        dbContextTransaction.Commit();
                        return Json(new { Result = true, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception exin)
                    {
                        dbContextTransaction.Rollback();

                        errormessage = "Ошибка выполнения запроса!\n\r" + exin.Message + "\n\r" + exin.StackTrace;
                        data = "";
                        return Json(new { Result = false, data = data, errormessage = errormessage }, JsonRequestBehavior.AllowGet);
                    }
                }
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
                dbModel.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s)); //Debug Information====================

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
                                  select new
                                  {
                                      id = worker.id,
                                      ShortName = worker.ShortName,
                                      Organization = worker.Organization,
                                      projects = worker.projects,
                                      selected = true
                                  }).Distinct();

                var _wksNotSelect = (from worker in dbModel.Workers
                                     join usrwks in dbModel.UsrWksRelations on worker.id equals usrwks.WorkerId into usrwkstmp
                                     from usrwks in usrwkstmp.DefaultIfEmpty()
                                     where usrwks.UserId != UserId
                                     select new
                                     {
                                         id = worker.id,
                                         ShortName = worker.ShortName,
                                         Organization = worker.Organization,
                                         projects = worker.projects,
                                         selected = false
                                     }).Distinct();

                wksList = _wksSelect.Union(_wksNotSelect).AsEnumerable().Select(x => new Workers
                {
                    id = x.id,
                    ShortName = x.ShortName,
                    Organization = x.Organization,
                    projects = x.projects,
                    selected = x.selected
                }).ToList();


                wksList = wksList.Where(x => (!wksList.Where(z => z.selected == true).Select(z => z.id).Contains(x.id))
                                            && ((x.ShortName.ToUpper().Contains(_name.ToUpper()) || string.IsNullOrEmpty(_name))
                                            && (x.Organization.ToUpper().Contains(_orgname.ToUpper()) || string.IsNullOrEmpty(_orgname))
                                            && ((x.projects != null && x.projects.ToUpper().Contains(_prjname.ToUpper())) || string.IsNullOrEmpty(_prjname)))
                                            || (x.selected == true)).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    wksList = wksList.OrderBy("selected desc," + sortColumn + " " + sortColumnDir + ", id desc").ToList();
                }
                else
                {
                    wksList = wksList.OrderByDescending(x => x.selected).ThenByDescending(x => x.id).ToList();
                }

                totalRecords = wksList.Count();

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
