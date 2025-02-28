using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class OrderPaymentDto
    {
        public int OrderPaymentId { get; set; }
        public System.Guid OrderGUID { get; set; }
        public System.Guid PaymentGUID { get; set; }
        public string InvoiceNo { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.DateTime DatePaid { get; set; }
        public string PDate { get; set; }
        public string PaymentType { get; set; }
        public string PaymentDetails { get; set; }
        public System.DateTime LastModified { get; set; }
        public string SelectedPaymentMethod { get; set; }
        public int CustomerId { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TDS { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int ProvisionalBillId { get; set; }
        public bool Paid { get; set; }
        public string CustomerName { get; set; }
        public string ProvisionalBillNo { get; set; }
        public string Status { get; set; }

        public List<CustomerDto> Customers { get; set; }
    }
}