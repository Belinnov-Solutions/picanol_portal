using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class TimeSheetDto
    {
       
        public int TimeSheetId { get; set; }
        public string TimeSheetNo { get; set; }
        public int WorkOrderId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Remarks { get; set; }
        public bool DelInd { get; set; }
        public string AuthorizedBy { get; set; }
        public string Designation { get; set; }
        public string AuthorizerEmail { get; set; }
        public DateTime AuthorizedOn { get; set; }
        public bool Authorized { get; set; }
        public string ImageId { get; set; }
        public string TimeSheetFileName { get; set; }
        public string UserName { get; set; }
        public string CustomerName { get; set; }
        public string WorkOrderNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AuthorizedSignature { get; set; }
        public string UserSignature { get; set; }
        public string CImagePath { get; set; }
        public string UImagePath { get; set; }
        public int ProvisionalBillId { get; set; }
        public int ButtonValue { get; set; }
        public bool FinalSubmit { get; set; }
        public string ProvisionalBillNo { get; set; }
        public string UserEmail { get; set; }
        public DateTime TillDate { get; set; }
        public DateTime TillFromDate { get; set; }
        public string Email { get; set; }
    }
}