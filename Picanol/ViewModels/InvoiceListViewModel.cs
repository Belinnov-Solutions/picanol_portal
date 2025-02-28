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
    public class InvoiceListViewModel
    {
        public InvoiceListViewModel()
        {
            InvoicesList = new List<InvoiceDto>();
            CustomersList = new List<CustomerDto>();
            SelectPaymentMethodList = Enum.GetNames(typeof(SelectPaymentMethod)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name

            });
            SelectActionTypeList = Enum.GetNames(typeof(SelectAction)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name

            });
        }

        public Guid OrderId { get; set; }
        public int SelectedCustomer { get; set; }
		
		public DateTime? FromDate { get; set; }		
		public DateTime? ToDate { get; set; }
		public string fDate { get; set; }
        public string tDate { get; set; }
        
            public decimal ComponentCost { get; set; }
        public decimal LabourCost { get; set; }
        public List<CustomerDto> CustomersList { get; set; }
        public List<InvoiceDto> InvoicesList { get; set; }
        public long LastTrackingNumber { get; set; }
        public string LastInvoiceNumber { get; set; }


        //Record Payment 
        public OrderPaymentDto OrderPayment { get; set; }
        
        public IEnumerable<SelectListItem> SelectPaymentMethodList { get; set; }
        public string SelectedAction { get; set; }
        //Dispatch 
       
        //Payment Information
        //public string PaymentDate { get; set; }
        //public string PaymentAmount { get; set; }
        //public string PaymentAccount { get; set; }
        //public string DetailText { get; set; }
        public PaymentImage PaymentImage { get; set; }
        //Action Type
        
        public IEnumerable<SelectListItem> SelectActionTypeList { get; set; }
        public string Default { get; set; }
        public IEnumerable<SelectListItem> CustomerNameList { get; set; }
        //public string FromDate { get; set; }
        //public string ToDate { get; set; }
        public string PaymentType { get; set; }
        public IEnumerable<SelectListItem> StatusNameList { get; set; }

        public List<OrderPartDto> ConsumedParts { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public int CurrentPage { get; set; }
        public int CountList { get; set; }

        public string InvoiceNo { get; set; }
    }

}