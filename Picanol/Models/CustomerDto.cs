using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class CustomerDto
    {
        
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zone { get; set; }
        public string PIN { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string ContactPerson { get; set; }
        public string StateCode { get; set; }
        public string GSTIN { get; set; }
        public string PAN { get; set; }
        public int BusinessId { get; set; }
        public DateTime ToDate{ get; set; }
        public DateTime FromDate { get; set; }
        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingDistrict { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPIN { get; set; }
        public Nullable<decimal> RepairCharges { get; set; }
        public string ShippingStateCode { get; set; }

        public Nullable<decimal> BigPacking { get; set; }
        public Nullable<decimal> BigForwarding { get; set; }
        public Nullable<decimal> SmallPacking { get; set; }
        public Nullable<decimal> SmallForwarding { get; set; }
		public string Customers { get; internal set; }
		public int Id { get; internal set; }
		public int SubCustomerId { get; set; }
		public string SubCustomerName { get; set; }

        public SubCustomerDto subCustomerDto { get; set; }

        public bool? InActive { get; set; }
        public List<CustomerContactDto> CustomerContacts { get; set; }
        //public int PageSize { get; set; }
        //public int PageNo { get; set; }
        public int CountList { get; set; }
		
	}
}