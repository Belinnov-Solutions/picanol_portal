using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class DetailedExpenseDto
    {
        public int DetailedExpensesId { get; set; }
        public Guid OrderGUID { get; set; }
        public int ProvisionalBillId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string ExpenseFrom { get; set; }
        public string ExpenseTo { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public DateTime DateCreated { get; set; }
        public bool DelInd { get; set; }
    }
}