using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class ServiceRequestViewModel:CustomerViewModel
    {
        public ServiceRequestViewModel()
        {
            Customers = new List<CustomerDto>();
            ServiceRequestList = new List<ServiceRequestDto>();
            CallTypeList = new List<ServiceCallTypeDto>();
            Username = new List<UserDto>();
            BusinessDetails = new BusinessDto();
            SelectPaymentMethodList = Enum.GetNames(typeof(SelectPaymentMethod)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name

            });

        }
        public List<CustomerDto> Customers { get; set; }
        public List<ServiceCallTypeDto> CallTypeList { get; set; }
        public ServiceCallTypeDto CallType { get; set; }
        public List<ServiceRequestDto> ServiceRequestList { get; set; }
        public ServiceRequestDto ServiceRequest { get; set; }
        public OrderPaymentDto OrderPayment { get; set; }
        public string WorkOrderNo { get; set; }
        public bool IsProvisional { get; set; }
        public CustomerDto CustomerDetails { get; set; }
        public List<UserDto> Username { get; set; }
        //Parameters for Provisional Bill Filter//
        public int? CustomerId { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public int? TypeId { get; set; }
        public string UserEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string ProvisionalNo { get; set; }
        public string InvoiceNo { get; set; }
        public int SelectedUserId { get; set; }
        public int CurrentPageIndex { get; set; }
        public int PageCount { get; set; }
        public int TotalNumberCounts { get; set; }
        public int CurrentPageNo { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public string  FDate { get; set; }
        public string  TDate { get; set; }

        public string CustomerName { get; set; }

        public BusinessDto BusinessDetails { get; set; }
        public IEnumerable<SelectListItem> SelectPaymentMethodList { get; set; }

        public Array MultipleZone { get; set; }

        public IEnumerable<SelectListItem> Zone { get; set; }

        public int SubCustomerId;

    }
}