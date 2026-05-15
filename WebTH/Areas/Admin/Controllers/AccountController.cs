using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebTH.Models;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
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

        // GET: Admin/Account
        public ActionResult Index()
        {
            var items = db.Users.ToList();

            return View(items);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");

            return View(new CreateAccountViewModel());
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Fullname = model.FullName, Phone = model.Phone };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");


            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // GET: /Admin/Account/Edit/{id}
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = UserManager.GetRoles(id);
            var model = new EditAccountViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.Fullname,
                Phone = user.Phone,
                Role = userRoles.FirstOrDefault()
            };

            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name", model.Role);
            return View(model);
        }

        // POST: /Admin/Account/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name", model.Role);
                return View(model);
            }

            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.Email = model.Email;
            user.Fullname = model.FullName;
            user.Phone = model.Phone;

            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Update role if changed
                var currentRoles = UserManager.GetRoles(id);
                if (currentRoles != null && currentRoles.Count > 0)
                {
                    foreach (var role in currentRoles)
                    {
                        await UserManager.RemoveFromRoleAsync(id, role);
                    }
                }

                if (!string.IsNullOrEmpty(model.Role))
                {
                    await UserManager.AddToRoleAsync(id, model.Role);
                }

                return RedirectToAction("Index");
            }

            AddErrors(result);
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name", model.Role);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAccount(string user, string id)
        {
            var code = new { Success = false }; // mac dinh khong xoa thanh cong
            var item = UserManager.FindByName(user);
            if (item != null)
            {
                var rolesForUser = UserManager.GetRoles(id);
                if (rolesForUser != null)
                {
                    foreach (var role in rolesForUser)
                    {
                        //roles.Add(role);
                        await UserManager.RemoveFromRoleAsync(id, role);
                    }
                }


                var res = await UserManager.DeleteAsync(item);
                code = new { Success = res.Succeeded };
            }
            return Json(code);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResetPassword(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User không tồn tại" });
            }

            string newPassword = "123456"; // hoặc random

            // Xoá password cũ
            var remove = await UserManager.RemovePasswordAsync(userId);
            if (!remove.Succeeded)
            {
                return Json(new { success = false, message = "Không thể xoá password cũ" });
            }

            // Tạo password mới
            var add = await UserManager.AddPasswordAsync(userId, newPassword);
            if (!add.Succeeded)
            {
                return Json(new { success = false, message = "Không thể tạo password mới" });
            }

            return Json(new { success = true, newPassword = newPassword });
        }

    
    [AllowAnonymous]
        public async Task<ActionResult> ResetAdminPasswordTemp()
        {
            var user = await UserManager.FindByNameAsync("admin");

            if (user == null)
            {
                return Content("Không tìm thấy user admin");
            }

            await UserManager.RemovePasswordAsync(user.Id);

            var result = await UserManager.AddPasswordAsync(user.Id, "123456");

            if (result.Succeeded)
            {
                return Content("Đã reset password admin thành: 123456");
            }

            return Content(string.Join("<br/>", result.Errors));
        }


    }
}