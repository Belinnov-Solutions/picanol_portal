using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class DebitNoteModel
    {
        public Nullable<System.DateTime> InvoiceDate { get; set; }

        public int DebitNoteId { get; set; }
        public System.Guid OrderGuid { get; set; }
        public string DebitNoteNo { get; set; }
        public string CreditNoteNo { get; set; }
        public System.DateTime DebitNoteDate { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public bool DelInd { get; set; }
        public string DebitNoteFileName { get; set; }

        public string InvoiceType { get; set; }

        public int ProvisionalBillId;

    }
}