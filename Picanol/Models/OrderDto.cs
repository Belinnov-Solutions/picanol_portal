using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.Models
{
    public class OrderDto
    {
        public decimal? Discount { get; set; }
        public System.Guid OrderGUID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public DateTime? ChallanDate { get; set; }
        public long? TrackingNumber { get; set; }
        public int CustomerId { get; set; }
        public int ChallanId { get; set; }
        public Nullable<int> AssignedUserId { get; set; }
        public string AssignedUserName { get; set; }
        public string RepairType { get; set; }
        public string Status { get; set; }
        public decimal? TimeTaken { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public Nullable<bool> DelInd { get; set; }
        public int PartId { get; set; }
        public int? Qty { get; set; }
        public string PackingType { get; set; }
        public String UserName { get; set; }
        public string PartName { get; set; }
        public string ItemDescription { get; set; }
        public string PartNo { get; set; }
        public string SerialNo { get; set; }
        public string Quantity { get; set; }
        public string User { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerRef { get; set; }
        public DateTime OrderDate { get; set; }
        public int URDCheck { get; set; }
        public bool InvoiceGenerated { get; set; }
        public bool InvoiceCancelled { get; set; }
        public bool LoanedPart { get; set; }
        public bool? Dispatched { get; set; }
        public bool? Paid { get; set; }
        public string InvoiceNo { get; set; }
        public bool ExemptLabourCost { get; set; }
        public bool ExemptComponentCost { get; set; }

        public string PrintableRepairType { get; set; }
        public string WarrantyTime { get; set; }
        public string WarrantyTimeNotes { get; set; }
        public string PerformaInvoiceNumber { get; set; }

        public string ProformaInvoiceNo { get; set; }

        public string IRN { get; set; }

        public Nullable<System.DateTime> ProformaInvoiceDate { get; set; }

        public int Editby { get; set; }

        public System.DateTime EditDate { get; set; }

        public int ProvisionalBillId { get; set; }
    }
}