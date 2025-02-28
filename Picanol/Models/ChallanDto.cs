using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class ChallanDto
    {
        public int ChallanId { get; set; }
        public string ChallanNumber { get; set; }
        public Nullable<System.DateTime> ChallanDate { get; set; }
        public string ReferenceNo { get; set; }
        public Nullable<System.DateTime> DateReceived { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string ChallanType { get; set; }
        public int CustomerId { get; set; }
        public string Remarks { get; set; }
        public string OrderDate { get; set; }
        public string CDate { get; set; }
        public string CustomerName { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int CountList { get; set; }
    }
}