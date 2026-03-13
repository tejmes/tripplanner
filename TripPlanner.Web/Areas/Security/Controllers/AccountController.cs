using Microsoft.AspNetCore.Mvc;
using System.Data;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;
using TripPlanner.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace TripPlanner.Web.Areas.Security.Controllers
{
    [Area("Security")]
    public class AccountController : Controller
    {
        private readonly IAccountIdentityService accountIdentityService;

        public AccountController(IAccountIdentityService accountIdentityService)
        {
            this.accountIdentityService = accountIdentityService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerVM)
        {
            if (ModelState.IsValid)
            {
                string[] errors = await accountIdentityService.Register(registerVM);

                if (errors == null)
                {
                    LoginViewModel loginVM = new LoginViewModel()
                    {
                        UserName = registerVM.UserName,
                        Password = registerVM.Password
                    };

                    bool isLogged = await accountIdentityService.Login(loginVM);
                    if (isLogged)
                        return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace(nameof(Controller), String.Empty), new { area = String.Empty });
                    else
                        return RedirectToAction(nameof(Login));
                }
                else
                {
                    foreach (var err in errors)
                    {
                        ModelState.AddModelError(string.Empty, err);
                    }
                }
            }

            return View(registerVM);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                bool isLogged = await accountIdentityService.Login(loginVM);
                if (isLogged)
                    return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace(nameof(Controller), String.Empty), new { area = String.Empty });
                else
                    loginVM.LoginFailed = true;
            }

            return View(loginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await accountIdentityService.Logout();

            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace(nameof(Controller), String.Empty), new { area = String.Empty });
        }

        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var vm = await accountIdentityService.GetUserInfo(userId);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateSettings(SettingsViewModel vm)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (ModelState.IsValid)
            {
                bool updateResult = await accountIdentityService.UpdateUserInfo(userId, vm);

                if (updateResult)
                {
                    return RedirectToAction(nameof(Settings));
                }
            }

            return View("Settings", vm);
        }
    }
}
