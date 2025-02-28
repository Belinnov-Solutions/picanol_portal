using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class OrderPartDto
    {
        public int OrderPartId { get; set; }
        public System.Guid OrderGUID { get; set; }
        public int PartId { get; set; }
        public int Qty { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public bool DelInd { get; set; }
        public int UserId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
    }
}