using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class ServiceRequestDto
    {
        public ServiceRequestDto()
        {
            SendON = DateTime.Now;
            TDate = null;
        }

        public int ProvisionalBillId { get; set; }
        public string ProvisionalBillNo { get; set; }
        public int CustomerId { get; set; }
        public System.Guid OrderGuid { get; set; }
        public int SubCustomerId { get; set; }
        public string MachineName { get; set; }
        public int UserId { get; set; }
        public string CallType { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int ServiceCharge { get; set; }
        public int Fare { get; set; }

        public int NewFare { get; set; }
        public int PocketExpenses { get; set; }
        public int PocketExpensesDays { get; set; }
        public int BoardingCharges { get; set; }
        public int BoardingDays { get; set; }
        public int ConveyanceExpenses { get; set; }

        public int newConveyaceExpances { get; set; }
        public int ConveyanceExpensesDays { get; set; }
        public int OtherCharges { get; set; }
        public System.DateTime? DateCreated { get; set; }
        public bool DelInd { get; set; }
        public int GST { get; set; }
        public int FinalAmount { get; set; }
        public int ServiceDays { get; set; }



        public string CustomerName { get; set; }

        public int Type { get; set; }

        public string UserName { get; set; }
        public int ServiceChargeAmount { get; set; }
        public int PocketExpensesAmount { get; set; }
        public int BoardingAmount { get; set; }
        public string Date { get; set; }
        public string FDate { get; set; }
        public string TDate { get; set; }
        public int TaxAmount { get; set; }
        public int AmountBeforeTax { get; set; }
        public string AmountInWords { get; set; }
        public string AuthorizedSignature { get; set; }
        public string UserSignature { get; set; }
        public string AuthorizedBy { get; set; }
        public string Designation { get; set; }
        public string AuthorizerEmail { get; set; }
        public string AuthorizedOn { get; set; }
        //public System.DateTime StartDate { get; set; }
        //public System.DateTime EndDate { get; set; }
        public int WorkOrderId { get; set; }
        public bool? Authorized { get; set; }
        public string Imagepath { get; set; }
        public string UImagepath { get; set; }
        public List<DetailedExpenseDto> FareExpenseDetails { get; set; }
        public List<DetailedExpenseDto> ConeyanceExpenseDetails { get; set; }
        public List<ServiceDaysDto> ServiceDaysList { get; set; }
        public int OvertimeHours { get; set; }
        public int OvertimeCharges { get; set; }
        public int OvertimeAmount { get; set; }
        public int TotalDays { get; set; }
        public bool IsTimeSheetExist { get; set; }
        public int ButtonValue { get; set; }
        public bool FinalSubmit { get; set; }
        public int TimeSheetId { get; set; }
        public System.DateTime TillFromDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? LastEmailSendOn { get; set; }
        public DateTime SendON { get; set; }
        public Boolean? Cancelled { get; set; }

        public string Message { get; set; }

        public bool TimeSheetAuthorized { get; set; }

        public string creditNoteNumber { get; set; }
        public decimal CreditNoteTotalamount { get; set; }
        public bool CRNCreated { get; set; }
        public bool Paid { get; set; }
        public string DebitNoteNumber { get; set; }
        public int PaymentNumber { get; set; }
        public bool RemainOrderPayment { get; set; }
        public string SelectedPaymentMethod { get; set; }
        public decimal BalanceAmount { get; set; }

        public string Remark { get; set; }

        public string Zone { get; set; }

        public string BillingAddress { get; set; }
        public string StateCode { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingStateCode { get; set; }
        public string SubCustomerBillingAddress { get; set; }
        public string SubCustomerStateCode { get; set; }
        public string SubCustomerShippingAddress { get; set; }
        public string SubCustomerShippingStateCode { get; set; }
    }
}