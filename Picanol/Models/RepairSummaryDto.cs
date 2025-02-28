using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class RepairSummaryDto
    {
        public string Month { get; set; }
        public string UserName { get; set; }
        public int Count { get; set; }

        public int PICount { get; set; }
        public string RepairType { get; set; }
        public int Mth { get; set; }

        public Guid OrderGuid { get; set; }

        public int InvoiceCount { get; set; }

    }
}