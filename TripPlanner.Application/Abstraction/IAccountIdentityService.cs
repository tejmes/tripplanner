using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface IAccountIdentityService
    {
        Task<string[]> Register(RegisterViewModel vm);
        Task<bool> Login(LoginViewModel vm);
        Task Logout();
        Task<SettingsViewModel> GetUserInfo(int userId);
        Task<bool> UpdateUserInfo(int userId, SettingsViewModel vm);
    }
}
