using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class OrdersViewModel
    {
        public OrdersViewModel()
        {
            ordersList = new List<OrderDto>();
            Customers = new List<CustomerDto>();
            assignedUsersList = new List<UserDto>(); 
             ActionList = Enum.GetNames(typeof(Actions)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
            StatusList = Enum.GetNames(typeof(OrderStatus)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
            RepairList = Enum.GetNames(typeof(RepairType)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
        }
        public IEnumerable<SelectListItem> ActionList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> RepairList { get; set; }
        public List<OrderDto> ordersList { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public CustomerDto SelectedCustomer { get; set; }
        public DispatchDetailsDto DispatchDetails { get; set; }
        public List<UserDto> assignedUsersList { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int SelectedUserId { get; set; }
        public int SelecetCustomerId { get; set; }
        public Guid SelectedOrderId { get; set; }
        public long? TrackingNo { get; set; }
        public string SelectedAction { get; set; }
        public string SelectedRepairType { get; set; }
        public string SelectedStatus { get; set; }
        public string Email { get; set; }
        public string LastInvoiceNumber { get; set; }
        public int CurrentPageNo { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public int NumberOfRecord { get; set; }

        public string previewOrigionalInv { get; set; }

    }
}