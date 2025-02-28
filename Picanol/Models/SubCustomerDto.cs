using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class SubCustomerDto
    {
        public string SubCustomerName { get; set; }
        public int SubCustomerId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string PIN { get; set; }
        public string ConatctPerson { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string GSTIN { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int CountList { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }


    }
}