using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Database;

namespace TripPlanner.Application.Implementation
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext dbContext;

        public BudgetService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddExpense(int tripId, int paidByUserId, decimal amount, string description, List<int> sharedWithUserIds)
        {
            var expense = new Expense
            {
                TripId = tripId,
                PaidByUserId = paidByUserId,
                Amount = amount,
                Description = description,
                Date = DateTime.UtcNow
            };

            dbContext.Expenses.Add(expense);
            await dbContext.SaveChangesAsync();

            var participantIds = sharedWithUserIds
                .Append(paidByUserId)
                .Distinct()
                .ToList();

            var perShare = Math.Round(amount / participantIds.Count, 2);

            var shares = participantIds.Select(uid => new ExpenseShare
            {
                ExpenseId = expense.Id,
                UserId = uid,
                ShareAmount = perShare
            });

            dbContext.ExpenseShares.AddRange(shares);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteExpense(int expenseId)
        {
            var expense = await dbContext.Expenses
                .Include(e => e.Shares)
                .FirstOrDefaultAsync(e => e.Id == expenseId);
            if (expense == null) return false;

            dbContext.ExpenseShares.RemoveRange(expense.Shares);
            dbContext.Expenses.Remove(expense);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<Dictionary<string, decimal>> CalculateUserBalances(int tripId)
        {
            var expenses = await dbContext.Expenses
                .Where(e => e.TripId == tripId)
                .Include(e => e.Shares)
                .ToListAsync();

            var users = new HashSet<int>();
            var paid = new Dictionary<int, decimal>();
            var owed = new Dictionary<int, decimal>();

            foreach (var e in expenses)
            {
                if (!paid.ContainsKey(e.PaidByUserId))
                    paid[e.PaidByUserId] = 0;
                paid[e.PaidByUserId] += e.Amount;

                foreach (var share in e.Shares)
                {
                    if (!owed.ContainsKey(share.UserId))
                        owed[share.UserId] = 0;
                    owed[share.UserId] += share.ShareAmount;
                    users.Add(share.UserId);
                }

                users.Add(e.PaidByUserId);
            }

            var allUserIds = users.Union(paid.Keys).Union(owed.Keys).ToHashSet();

            var userMap = await dbContext.Users
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var balances = new Dictionary<string, decimal>();
            foreach (var uid in allUserIds)
            {
                var userName = userMap[uid];
                var totalPaid = paid.ContainsKey(uid) ? paid[uid] : 0m;
                var totalOwed = owed.ContainsKey(uid) ? owed[uid] : 0m;
                balances[userName] = totalPaid - totalOwed;
            }

            return balances;
        }

        public async Task<Dictionary<string, decimal>> CalculateUserTotals(int tripId)
        {
            var expenses = await GetByTrip(tripId);
            var allUsers = expenses
                .Select(e => e.PaidByUser)
                .Concat(expenses.SelectMany(e => e.SharedWith))
                .Distinct();

            var result = new Dictionary<string, decimal>();

            foreach (var user in allUsers)
            {
                decimal userTotal = 0;
                foreach (var e in expenses)
                {
                    var shared = e.SharedWith.Any() ? e.SharedWith : new List<string>();
                    var participants = shared.Contains(e.PaidByUser)
                        ? shared
                        : shared.Append(e.PaidByUser).ToList();

                    if (participants.Contains(user))
                    {
                        userTotal += e.Amount / participants.Count;
                    }
                }
                result[user] = userTotal;
            }

            return result;
        }

        public async Task<List<(string From, string To, decimal Amount)>> CalculateSettlement(int tripId)
        {
            var balances = await CalculateUserBalances(tripId);

            var debtors = balances
                .Where(kv => kv.Value < 0)
                .Select(kv => new { User = kv.Key, Balance = -kv.Value })
                .OrderBy(x => x.User)
                .ToList();

            var creditors = balances
                .Where(kv => kv.Value > 0)
                .Select(kv => new { User = kv.Key, Balance = kv.Value })
                .OrderBy(x => x.User)
                .ToList();

            var result = new List<(string From, string To, decimal Amount)>();
            int i = 0, j = 0;
            while (i < debtors.Count && j < creditors.Count)
            {
                var d = debtors[i];
                var c = creditors[j];
                var amt = Math.Min(d.Balance, c.Balance);

                result.Add((d.User, c.User, Math.Round(amt, 2)));

                debtors[i] = new { d.User, Balance = d.Balance - amt };
                creditors[j] = new { c.User, Balance = c.Balance - amt };

                if (debtors[i].Balance == 0) i++;
                if (creditors[j].Balance == 0) j++;
            }

            return result;
        }

        public async Task<List<ExpenseViewModel>> GetByTrip(int tripId)
        {
            var expenses = await dbContext.Expenses
                .Where(e => e.TripId == tripId)
                .Include(e => e.Shares)
                .ToListAsync();

            var vmList = new List<ExpenseViewModel>();
            foreach (var e in expenses)
            {
                var paidBy = await dbContext.Users
                    .Where(u => u.Id == e.PaidByUserId)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();

                var sharedWith = await dbContext.Users
                    .Where(u => e.Shares.Select(s => s.UserId).Contains(u.Id))
                    .Select(u => u.UserName)
                    .ToListAsync();

                vmList.Add(new ExpenseViewModel
                {
                    ExpenseId = e.Id,
                    Amount = e.Amount,
                    Description = e.Description,
                    Date = e.Date,
                    PaidByUser = paidBy,
                    SharedWith = sharedWith
                });
            }

            return vmList;
        }
    }
}
