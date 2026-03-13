using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class BudgetViewModel
    {
        public int TripId { get; set; }
        public List<ExpenseViewModel> Expenses { get; set; } = new();
        public Dictionary<string, decimal> UserBalances { get; set; } = new();
        public List<(string From, string To, decimal Amount)> Settlements { get; set; } = new();
        public List<CollaboratorViewModel> Collaborators { get; set; } = new();
        public Dictionary<string, decimal> UserTotals { get; set; } = new();
    }
}
