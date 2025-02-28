using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class ProvisionalServiceViewModel
    {
        public ProvisionalServiceViewModel()
        {
            Customers = new List<CustomerDto>();
            provisionalservice = new List<ServiceRequestDto>();
           
        }
        public List<CustomerDto> Customers { get; set; }
        public ServiceCallTypeDto CallType { get; set; }
        public IEnumerable<SelectListItem> CallTypeList { get; set; }
        public List<ServiceRequestDto> provisionalservice { get; set; }
        public ServiceRequestDto ProvisionalService { get; set; }
    }
}