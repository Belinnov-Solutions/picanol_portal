using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class ServiceDaysDto
    {
        public int ServiceBillDayId { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public Nullable<int> ProvisionalBillId { get; set; }
    }
}