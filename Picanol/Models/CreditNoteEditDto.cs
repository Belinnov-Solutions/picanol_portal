using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class CreditNoteEditDto
    { public int ProvisionalBillId { get; set; }
        public int workOrderId { get; set; }
        public string PreviewType { get; set; }
        public string Initiator { get; set; }
        public string invoiceNumber { get; set; }
        public decimal ServiceChargeQty { get; set; }
        public decimal ServiceChargeRate { get; set; }
        public decimal ServiceChargeAmount { get; set; }
        public decimal railairfareQty { get; set; }
        public decimal railairfareRate { get; set; }
        public decimal railairfareAmount { get; set; }
        public decimal PocketexpenceQty { get; set; }
        public decimal PocketexpenceRate { get; set; }
        public decimal PocketexpenceAmount { get; set; }
        public decimal LoadingBoardingQty { get; set; }
        public decimal LoadingBoardingRate { get; set; }
        public decimal LoadingBoardingAmount { get; set; }

        public decimal ConvenyanceInceidentalExpQty { get; set; }
        public decimal ConvenyanceInceidentalRate { get; set; }
        public decimal ConvenyanceInceidentalAmount { get; set; }
        public decimal OverTimehourQty { get; set; }
        public decimal OverTimehourRate { get; set; }
        public decimal OverTimehourAmount { get; set; }


        public decimal AmountBeforeTax { get; set; }
        public decimal GstAmount { get; set; }

        public decimal TotalAmount { get; set; }
    }
}