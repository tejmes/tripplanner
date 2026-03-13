using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class ExpenseShare : Entity
    {
        public decimal ShareAmount { get; set; }
        public int ExpenseId { get; set; }
        public Expense Expense { get; set; }
        public int UserId { get; set; }
    }
}
