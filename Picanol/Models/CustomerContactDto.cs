using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class CustomerContactDto
    {
        public int CustomerContactId { get; set; }
        public int CustomerId { get; set; }
        public string Department { get; set; }
        public string ContactPersonName { get; set; }
        public string Mobile { get; set; }
        public string EmailId { get; set; }
    }
}