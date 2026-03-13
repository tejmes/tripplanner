using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;
using TripPlanner.Infrastructure.Identity;

namespace TripPlanner.Application.Implementation
{
    public class AccountIdentityService : IAccountIdentityService
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> sigInManager;

        public AccountIdentityService(UserManager<User> userManager, SignInManager<User> sigInManager)
        {
            this.userManager = userManager;
            this.sigInManager = sigInManager;
        }

        public async Task<string[]> Register(RegisterViewModel vm)
        {
            User user = new User()
            {
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
            };

            string[] errors = null;

            var result = await userManager.CreateAsync(user, vm.Password);
            if (result.Errors != null && result.Errors.Count() > 0)
            {
                errors = new string[result.Errors.Count()];
                for (int i = 0; i < result.Errors.Count(); ++i)
                {
                    errors[i] = result.Errors.ElementAt(i).Description;
                }
            }

            return errors;
        }

        public async Task<bool> Login(LoginViewModel vm)
        {
            var result = await sigInManager.PasswordSignInAsync(vm.UserName, vm.Password, true, true);
            return result.Succeeded;
        }

        public Task Logout()
        {
            return sigInManager.SignOutAsync();
        }

        public async Task<SettingsViewModel> GetUserInfo(int userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user != null)
            {
                return new SettingsViewModel
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
            }

            return null;
        }

        public async Task<bool> UpdateUserInfo(int userId, SettingsViewModel vm)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null) return false;

            if (!string.Equals(user.Email, vm.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                var exists = await userManager.FindByEmailAsync(vm.Email);
                if (exists != null) return false;
            }

            user.UserName = vm.UserName;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Email = vm.Email;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}