using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels
{
    public class ProformaViewModel : CustomerViewModel
    {
        public ProformaViewModel()
        {
            List = new List<ProformaInvoiceDto>();
            CustomersName = new List<CustomerDto>();
        }
        public List<CustomerDto> CustomersName { get; set; }
        public List<ProformaInvoiceDto> List { get; set; }
        public long LastTrackingNumber { get; set; }
        public int TotalNumberCounts { get; set; }
        public int PageCount { get; set; }
        public int SelectedUserId { get; set; }
        public int CurrentPageNo { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public string FDate { get; set; }
        public string TDate { get; set; }
        public System.DateTime TillFromDate { get; set; }
        public DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int? TypeId { get; set; }
        public int CustomerId { get; set; }
        public string ProformaInvoiceNo { get; set; }
        public int CurrentPageIndex { get; set; }
        public int ProformaInvoiceId { get; set; }

    }
}