using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class PcbNameSummaryDto
    {
        public string PartName { get; set; }
        public string RepairType { get; set; }
        public int Qty { get; set; }
    }
}