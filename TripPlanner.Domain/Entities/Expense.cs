using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class Expense : Entity
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int PaidByUserId { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
        public List<ExpenseShare> Shares { get; set; } = new();
    }
}
