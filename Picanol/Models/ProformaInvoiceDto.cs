using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class ProformaInvoiceDto
    {


        public int ProformaInvoiceId { get; set; }
        public string ProformaInvoiceNo { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.Guid OrderGuid { get; set; }
        public decimal Amount { get; set; }
        public int CustomerId { get; set; }
        public int Status { get; set; }
        public bool DelInd { get; set; }
        public Nullable<System.DateTime> ProformaInvoiceDate { get; set; }
        public string FileName { get; set; }
        public Nullable<int> EditedBy { get; set; }
        public Nullable<System.DateTime> DateEdited { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public System.DateTime ToDate { get; set; }

        public string CustomerName { get; set; }
        public int TotalNumberRecord { get; set; }
        public DateTime StartDate { get; set; }
        public System.DateTime TillFromDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime FromDate { get; set; }
        public string FDate { get; set; }
        public string TDate { get; set; }

        public long TrackingNumber { get; set; }

        public string Zone { get; set; }
    }
}