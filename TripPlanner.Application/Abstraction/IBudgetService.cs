using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface IBudgetService
    {
        Task AddExpense(int tripId, int paidByUserId, decimal amount, string description, List<int> sharedWithUserIds);
        Task<bool> DeleteExpense(int expenseId);
        Task<List<ExpenseViewModel>> GetByTrip(int tripId);
        Task<Dictionary<string, decimal>> CalculateUserBalances(int tripId);
        Task<Dictionary<string, decimal>> CalculateUserTotals(int tripId);
        Task<List<(string From, string To, decimal Amount)>> CalculateSettlement(int tripId);
    }
}
