using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class PartMovementDto
    {
        public int PartId { get; set; }
        public long? TrackingNumber { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? Date { get; set; }
        public int Quantity { get; set; }
        public string Particulars { get; set; }
        public int InQuantity { get; set; }
       
    }
}