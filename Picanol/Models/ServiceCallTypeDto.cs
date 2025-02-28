using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class ServiceCallTypeDto
    {
        public int CallTypeId { get; set; }
        public string CallTypeNumber { get; set; }
        public string Description { get; set; }
        public bool ServiceCharges { get; set; }
        public bool DelInd { get; set; }
    }
}