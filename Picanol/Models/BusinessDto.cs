using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class BusinessDto
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PIN { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string GSTIN { get; set; }
        public string PAN { get; set; }
        public Nullable<int> ParentBusinessID { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
    }
}