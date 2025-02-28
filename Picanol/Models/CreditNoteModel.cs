using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class CreditNoteModel
    {
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public decimal ComponentCost { get; set; }
        public decimal LabourCost { get; set; }

        public int CreditNoteId { get; set; }
        public System.Guid OrderGuid { get; set; }
        public string CreditNoteNo { get; set; }
        public System.DateTime CreditNoteDate { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal TaxableAmount { get; set; }
        public int CustomerId { get; set; }
        public decimal TaxAmount { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public bool DelInd { get; set; }
        public string CreditNoteFileName { get; set; }


        public string InvoiceType { get; set; }

        public int ProvisionalBillId;

        public bool partialAmount { get; set; }

        public string ErrorMessage { get; set; }

        public decimal PckageForwarding { get; set; }


    }
}