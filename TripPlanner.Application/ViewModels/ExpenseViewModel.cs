using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class ExpenseViewModel
    {
        public int ExpenseId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string PaidByUser { get; set; }
        public List<string> SharedWith { get; set; }
    }
}
