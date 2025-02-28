using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels
{
    public class CustomerViewModel
    {
        public CustomerViewModel()
        {
            CustomersList = new List<CustomerDto>();
            StateList = new List<GSTStateDto>();
            SubCustomerList = new List<SubCustomerDto>();
            SearchCustomersList = new List<CustomerDto>();
            ZoneList = new List<ZoneDto>();
        }
        public List<CustomerDto> CustomersList { get; set; }
        public List<CustomerDto> SearchCustomersList { get; set; }
        public CustomerDto Customer { get; set; }
        public List<GSTStateDto> StateList { get; set; }
        public List<ZoneDto> ZoneList { get; set; }
        public List<SubCustomerDto> SubCustomerList { get; set; }
        public SubCustomerDto SubCustomer { get; set; }
        public bool CustomerIsEdit { get; set; }
        public int CurrnetPageNo { get; set; }
        public int CountList { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
    }
}