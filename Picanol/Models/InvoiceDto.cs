using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class InvoiceDto
    {
        public InvoiceDto()
        {
            InvoiceItems = new List<InvoiceItemDto>();
        }
        public long TrackingNo { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string CreditNoteNo { get; set; }//credit note
        public string DebitNoteNo { get; set; }//debit note
        public DateTime CreditNoteDate { get; set; }
        public String DebitNoteDate { get; set; }
        public string InvoiceFileName { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.Guid OrderGuid { get; set; }
        public decimal Amount { get; set; }
        public decimal OrgAmount { get; set; }
        public int CustomerId { get; set; }
        public int Status { get; set; }
        public bool DelInd { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public Nullable<decimal> RepairCharges { get; set; }
        public string CustomerName { get; set; }
        public bool Paid { get; set; }
        public decimal AmountBTax { get; set; }
        public decimal SGSTTax { get; set; }
        public decimal IGSTTax { get; set; }
        public decimal CGSTTax { get; set; }
        public decimal TotalTax { get; set; }
        public decimal OrgTotalTax { get; set; }
        public string AmountWords { get; set; }

        public decimal ComponentsCost { get; set; }

        public decimal PackingCharges { get; set; }

        public decimal LabourCost { get; set; }
        public decimal ForwardingCharges { get; set; }
        public decimal OrgComponentCost { get; set; }
        public int ComponentQty { get; set; }
        public List<InvoiceItemDto> InvoiceItems { get; set; }
        public string ErrorMessage { get; set; }
        public string ProvisionalBillNo { get; set; }
        public string AttendedFrom { get; set; }
        public string AttendedTo { get; set; }
        public string EngineerName { get; set; }
        public string CustomerEmail { get; set; }
        public int  SubCustomerId { get; set; }
        public string RepairType { get; set; }


        public string EInVoiceQrCode { get; set; }

        public string IRN { get; set; }

        public int IRNGeneratedBy { get; set; }

        public string eInvType { get; set; }

        public string selectedItem { get; set; }

        public Nullable<bool> Cancelled { get; set; }

        public Nullable<System.DateTime> CancelledDate { get; set; }

        public string creditNote { get; set; }
        public decimal CreditNoteAmount { get; set; }

        public int ProvisionalBillId { get; set; }
        
        public int  WorkOrderId { get; set; }
        public string DebitNoteId { get; set; }
    }
}