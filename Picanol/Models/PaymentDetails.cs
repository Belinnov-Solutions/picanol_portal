using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class PaymentDetails
    {   public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public decimal INR { get; set; }
        public int AmountPayble { get; set; }
        public bool DelInd { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal  PaymentAmount { get; set; }
        public decimal TDS { get; set; }
        public string PaymentMode { get; set; }
        public string PerformaInvoiceNumber { get; set; }
        
    }
}