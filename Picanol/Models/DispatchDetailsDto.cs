using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class DispatchDetailsDto
    {
        public int DispatchId { get; set; }
        public string InvoiceNo { get; set; }
        public string DocketNumber { get; set; }
        public Nullable<System.DateTime> DispatchDate { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string DispatchDetails { get; set; }
        public string Ddate { get; set; }
        public string ImageName { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public bool DelInd { get; set; }
        public long TrackingNumber { get; set; }
        public System.Guid OrderGUID { get; set; }
    }
}