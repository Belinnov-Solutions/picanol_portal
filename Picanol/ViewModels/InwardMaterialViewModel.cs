using Picanol.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class InwardMaterialViewModel
    {
        public InwardMaterialViewModel()
        {

            Customers = new List<CustomerDto>();
            Orders = new List<OrderDto>();
            SavedOrders = new List<OrderDto>();
            Parts = new List<PartDto>();
            Users = new List<UserDto>();
            InwardMaterialList = new List<ChallanDto>();
            RepairTypeList = Enum.GetNames(typeof(RepairType)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
        }
		
		public List<CustomerDto> Customers { get; set; }
        public List<OrderDto> Orders { get; set; }
        public List<OrderDto> SavedOrders { get; set; }
        public List<UserDto> Users { get; set; }
        public List<PartDto> Parts { get; set; }
        public OrderDto NewOrder { get; set; }
        public string ChallanNo { get; set; }
        public string files { get; set; }
        public int CustomerId { get; set; }
        public DateTime? ChallanDate { get; set; }
        public DateTime? DateCreated { get; set; }

        public string Comments { get; set; }
		
		[Required(ErrorMessage = "Reference should be less than 11 letters ")]
		public string CustomerRef { get; set; }
        public int SelectedCustomerId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string fdate { get; set; }
        public string tdate { get; set; }

       public RepairType RepairType { get; set; }
        public IEnumerable<SelectListItem> RepairTypeList { get; set; }
        public HttpPostedFileBase image { get; set; }
        public string DelSerialNo { get; set; }
        public string DelPartNo { get; set; }
        public bool LoanItem { get; set; }
        public bool ExistingOrder { get; set; }
        public List<ChallanDto> InwardMaterialList { get; set; }

        public ChallanDto CustomerDetails { get; set; }
        public string CName { get; set; }
        public long LastTrackingNumber { get; set; }
        public string LastInvoiceNumber { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public int CurrentPage { get; set; }
        public int CountList { get; set; }

        public bool proformaInvoiceGenerated { get; set; }

        public bool currentUserId { get; set; }
    }
    
}