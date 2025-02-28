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
	public class InvoiceViewModel
	{
		public InvoiceViewModel()
		{

		}
		//New format..changed by Gaurav on 26/07/2018
		public BusinessDto BusinessDetails { get; set; }
		public CustomerDto CustomerDetails { get; set; }
		public OrderDto OrderDetails { get; set; }
		public InvoiceDto InvoiceDetails { get; set; }
		public List<OrderPartDto> ConsumedParts { get; set; }
		public string SelectedAction { get; set; }
		public string PerformaInvoiceNumber { get; set; }

		public string previewOrigionalInv { get; set; }
		public string BillingAddresss { get; set; }
		public string BillingStateCode { get; set; }
		public string ShippingAddress { get; set; }
        public string ShippingStateCode { get; set; }




    }
    public class InvoiceImage
	{
		public int ProductImageId { get; set; }
		[Display(Name = "Image *(max size 500kb)")]
		public string InvoiceImages { get; set; }
		public bool Main { get; set; }

	}
	public class DisImage
	{
		public int ProductImageId { get; set; }
		[Display(Name = "Image *(max size 500kb)")]
		public string DisImages { get; set; }
		public bool Main { get; set; }
	}
	public class PaymentImage
	{
		public int ProductImageId { get; set; }
		[Display(Name = "Image *(max size 500kb)")]
		public string PaymentImages { get; set; }
		public bool Main { get; set; }
	}

    
}