using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class WorkOrderImageDto
    {
        public int WorkOrderImageId { get; set; }
        public int? WorkOrderId { get; set; }
        public string ImageName { get; set; }
    }
}