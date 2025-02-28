using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class ProvisionalServiceDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateCreated { get; set; }
        public string Service { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public int NoOfDays { get; set; }
        public int Rate { get; set; }
        public int Amount { get; set; }
        public int LBAmount { get; set; }
        public int LBDays { get; set; }
        public int LBTotal { get; set; }
        public int Fare { get; set; }
        public int PocketExpense { get; set; }
        public int ConveyanceExpense { get; set; }
        public int TotalAmount { get; set; }
        public string GST { get; set; }
        public int FinalAmount { get; set; }
    }
}