using NLog;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects.SqlClient;
using Picanol.ViewModels;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.UI;
using System.Windows;

namespace Picanol.Services
{
    public class ServiceRequestService : BaseService<PicannolEntities, tblProvisionalBill>
    {
        #region Constructors
        internal ServiceRequestService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion
        PicannolEntities _context = new PicannolEntities();

        #region BaseMethods
        public int SaveServiceRequest(tblProvisionalBill sr)
        {
            if (sr == null)
                throw new ArgumentNullException("student", "Null Parameter");
            Add(sr);
            return sr.ProvisionalBillId;
        }
        public void AddServiceRequest(tblProvisionalBill cls)
        {
            if (cls == null)
                throw new ArgumentNullException("student", "Null Parameter");
            Add(cls);
        }

        public void UpdateServiceRequest(tblProvisionalBill service)
        {
            if (service == null)
                throw new ArgumentNullException("news", "Null Parameter");
            Update(service);
        }

        public List<ServiceRequestDto> ProvisionalBillListByWorkOrderId(int WorkOrderId)
        {
            var l = (from a in Context.tblProvisionalBills
                     join b in Context.tblUsers on a.UserId equals b.UserId

                     where a.WorkOrderId == WorkOrderId && a.DelInd == false
                     select new ServiceRequestDto
                     {
                         ProvisionalBillId = a.ProvisionalBillId,
                         ProvisionalBillNo = a.ServiceBillNo,
                         DateCreated = a.DateCreated,
                         UserName = b.UserName,
                         Authorized = a.Authorized,
                         CustomerId = a.CustomerId,
                         CallType = a.CallType,
                         WorkOrderId = (int)a.WorkOrderId,
                         FinalSubmit = (bool)a.FinalSubmit,


                     }).ToList();

            foreach (var item in l)
            {
                if (Context.tblTimeSheets.Any(x => x.ProvisionalBillId == item.ProvisionalBillId))
                    item.IsTimeSheetExist = true;
                else
                    item.IsTimeSheetExist = false;
            }
            return l;
        }

        public ServiceRequestDto getWorkOrderDetails(int id)
        {
            var d = (from a in Context.tblWorkOrders
                     join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                     where a.WorkOrderId == id
                     select new ServiceRequestDto
                     {
                         WorkOrderId = a.WorkOrderId,
                         CallType = a.CallType,
                         CustomerId = a.CustomerId,
                         CustomerName = b.CustomerName
                     }).FirstOrDefault();

            return d;
        }


        public List<ServiceRequestDto> GetServiceRequestsList()
        {
            var s = (from a in Context.tblProvisionalBills
                     join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                     join c in Context.tblUsers on a.UserId equals c.UserId
                     where (a.DelInd == false)
                     select new ServiceRequestDto
                     {
                         ProvisionalBillId = a.ProvisionalBillId,
                         ProvisionalBillNo = a.ServiceBillNo,
                         WorkOrderId = (int)a.WorkOrderId,
                         CustomerName = b.CustomerName,
                         Zone = (b.Zone!=null)? b.Zone:"",
                         //FromDate = (DateTime) EntityFunctions.TruncateTime(a.FromDate),
                         FromDate = (DateTime)a.FromDate,
                         FDate = SqlFunctions.DateName("day", a.FromDate) + "/" + SqlFunctions.DateName("month", a.FromDate) + "/" + SqlFunctions.DateName("year", a.FromDate),
                         //ToDate = (DateTime)EntityFunctions.TruncateTime(a.ToDate),
                         ToDate = (DateTime)a.ToDate,
                         TDate = SqlFunctions.DateName("day", a.ToDate) + "/" + SqlFunctions.DateName("month", a.ToDate) + "/" + SqlFunctions.DateName("year", a.ToDate),
                         FinalAmount = a.FinalAmount,
                         UserName = c.UserName,
                         BoardingCharges = (int)a.BoardingCharges,
                         BoardingDays = (int)a.BoardingDays,
                         ServiceDays = a.ServiceDays,
                         ServiceCharge = a.ServiceCharge,
                         //Fare = a.Fare,
                         MachineName = a.MachineName,
                         CallType = a.CallType,
                         PocketExpenses = a.PocketExpenses,
                         //ConveyanceExpenses = a.ConveyanceExpenses,
                         UserId = a.UserId,
                         CustomerId = a.CustomerId,
                         AuthorizerEmail = a.EmailId,
                         Authorized = a.Authorized,
                         ConveyanceExpensesDays = (int)a.ConveyanceExpenseDays,
                         PocketExpensesDays = (int)a.PocketExpenseDays,
                         OvertimeCharges = (int)a.OvertimeCharges,
                         OvertimeHours = (int)a.OvertimeHours,
                         ServiceChargeAmount = (int)(a.BoardingDays * a.ServiceCharge),
                         BoardingAmount = (int)(a.BoardingDays * a.BoardingCharges),
                         PocketExpensesAmount = (int)(a.PocketExpenseDays * a.PocketExpenses),
                         OvertimeAmount = (int)(a.OvertimeHours * a.OvertimeCharges),
                         Cancelled = a.Cancelled == null ? false : a.Cancelled,
                         Remark = (a.Cancelled == null) || (a.Cancelled == false) ? "" : "Cancelled",
                         GST = a.GST,
                         //adding sub customer id - 26/07/2024
                         //changes by Janesh
                         //SubCustomerId = (int)((a.SubCustomerId == null || a.SubCustomerId == 0) ? 0 : a.SubCustomerId),

                     }).OrderByDescending(x => x.ProvisionalBillNo).ToList();
            //chnges for serviceTill from
            foreach (var item in s)
            {
                

                var a = GetFareAndConveyanceDetails(item);
                item.ServiceChargeAmount = (int)(item.ServiceDays * item.ServiceCharge);
                item.BoardingAmount = (int)(item.BoardingDays * item.BoardingCharges);
                item.PocketExpensesAmount = (int)(item.PocketExpensesDays * item.PocketExpenses);
                item.OvertimeAmount = (int)(item.OvertimeHours * item.OvertimeCharges);
                item.AmountBeforeTax = (int)(item.ServiceChargeAmount + item.BoardingAmount +
                                                    item.PocketExpensesAmount + a.ConveyanceExpenses +
                                                    a.Fare + item.OvertimeAmount);
                item.TaxAmount = item.AmountBeforeTax * item.GST / 100;
                int amount = item.AmountBeforeTax + item.TaxAmount;
                item.FinalAmount = amount;
                item.TimeSheetAuthorized = _context.tblTimeSheets.Where(x => x.ProvisionalBillId == item.ProvisionalBillId).Select(x => x.Authorized == null ? false : x.Authorized).FirstOrDefault();
                var servicedaylist = GetServiceBillDays(item.ProvisionalBillId);
                if (servicedaylist.Count <= 0)
                {
                    servicedaylist.Add(new ServiceDaysDto
                    {
                        FromDate = item.FromDate,
                        ToDate = item.ToDate,

                    });
                }

                var lastitem = servicedaylist[servicedaylist.Count - 1];
                item.EndDate = lastitem.ToDate;
                var firstitem = servicedaylist.FirstOrDefault();
                item.TillFromDate = firstitem.FromDate;
                //new
                var orderId = _context.tblWorkOrders.Where(x => x.WorkOrderId == item.WorkOrderId && x.DelInd == false).Select(x => x.WorkOrderGUID).FirstOrDefault();

                if (_context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false))
                {

                    var invoice = _context.tblInvoices.Where(x => x.OrderGuid == orderId && x.ProvisionalBillId == item.ProvisionalBillId && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    var invoiceWithoutPId = _context.tblInvoices.Where(x => x.OrderGuid == orderId && x.ProvisionalBillId == null && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    if (invoice != null)
                    {
                        item.InvoiceNumber = invoice.InvoiceNo;
                        item.LastEmailSendOn = invoice.SendOnDate != null ? invoice.SendOnDate : null;
                    }

                    else if (invoiceWithoutPId != null)
                    {
                        item.InvoiceNumber = invoiceWithoutPId.InvoiceNo;
                        item.LastEmailSendOn = invoiceWithoutPId.SendOnDate != null ? invoiceWithoutPId.SendOnDate : null;
                    }
                    if (invoice == null && invoiceWithoutPId == null)
                    {
                        item.InvoiceNumber = "";

                    }
                }
                else
                {
                    item.InvoiceNumber = "";

                }
                /*if (item.SubCustomerId > 0 && item.SubCustomerId != null)
                    item.CustomerName = _context.tblSubCustomers.Where(x => x.SubCustomerId == item.SubCustomerId).Select(x => x.SubCustomerName).FirstOrDefault();*/
            }
          

            return s;
        }

       

        public List<ServiceRequestDto> GetServiceRequestsListVersion6(ServiceRequestViewModel svm)
        {
            List<ServiceRequestDto> searchList = new List<ServiceRequestDto>();
            
            try
            {
                svm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
                char[] splitchar = { '/' };
                string fdate = "";
                string tdate = "";
                if (svm.FDate != null && svm.FDate != "")
                {
                    string[] arr = svm.FDate.Split(splitchar);

                    fdate = arr[2] + "-" + arr[1] + "-" + arr[0];
                }
                if (svm.TDate != null && svm.TDate != "")
                {
                    string[] arr1 = svm.TDate.Split(splitchar);
                    tdate = arr1[2] + "-" + arr1[1] + "-" + arr1[0];
                }

                string MutipleZone = "";
                //MultipleZone 
                if (svm.MultipleZone != null)
                    MutipleZone = string.Join("','", (string[])svm.MultipleZone);

                int subcustomerId = _context.tblSubCustomers.Where(x => x.SubCustomerName == svm.CustomerName && x.DelInd == false).Select(x => x.SubCustomerId).FirstOrDefault();


                string query =
                " select Distinct a.ProvisionalBillId, a.Paid as Paid, a.ServiceBillNo as ProvisionalBillNo,a.WorkOrderId as WorkOrderId,b.CustomerName as CustomerName,b.Zone as Zone," +
                "a.FromDate as FromDate,a.ToDate as ToDate,a.FinalAmount as FinalAmount,c.UserName as UserName,a.BoardingCharges as BoardingCharges,a.BoardingDays as BoardingDays," +
                "a.ServiceDays as ServiceDays,a.ServiceCharge as ServiceCharge," +
                "a.MachineName as MachineName,a.CallType as CallType,a.PocketExpenses as PocketExpenses,a.UserId as UserId," +
                "a.CustomerId as CustomerId, a.EmailId as EmailId,a.Authorized as Authorized," +
                "a.ConveyanceExpenseDays as ConveyanceExpenseDays,a.PocketExpenseDays as PocketExpensesDays,a.OvertimeCharges as OvertimeCharges,a.OvertimeHours as OvertimeHours," +
                "a.BoardingDays* a.ServiceCharge as ServiceChargeAmount," +
                "a.BoardingDays* a.BoardingCharges as BoardingAmount,a.PocketExpenseDays* a.PocketExpenses as PocketExpensesAmount,a.OvertimeHours* a.OvertimeCharges as OvertimeAmount," +
                //"IIF(a.Cancelled <> null, a.Cancelled, '') as Cancelled," +
                "isnull(a.SubCustomerId,0) as SubCustomerId," +
                "isnull(a.Cancelled,0) as Cancelled," +
                "a.GST as GST from tblProvisionalBill as a " +
                "inner join tblCustomer as b on a.CustomerID = b.CustomerID " +
                "inner join tblUser as c on c.UserId = a.UserId " +
                 "where a.DelInd=0 ";

                //zone proforma invice list
                if (svm.MultipleZone != null && svm.MultipleZone.Length != 0 && svm.FDate == null)
                    query += "and b.Zone in ('" + MutipleZone + "')";

                if (svm.MultipleZone != null && svm.MultipleZone.Length != 0 && svm.FDate != null && svm.FDate != "")
                    query += " and CAST(a.FromDate as date) >= '" + fdate + "' and CAST(a.ToDate as date) <= '" + tdate + "' " + "and b.Zone in ('" + MutipleZone + "')";

                //when there is from date and to date
                if (svm.FDate != null && svm.FDate != "" && svm.TDate != null && svm.TDate != "")
                    query += " and CAST(a.FromDate as date) >= '" + fdate + "' and CAST(a.ToDate as date) <= '" + tdate + "' ";

                //Provisional bill no only
                if (svm.ProvisionalNo != "" && svm.ProvisionalNo != null)
                    query += "and  a.ServiceBillNo like '%" + svm.ProvisionalNo + "%'";

                //where Provisional bill No and both dates
                if (svm.CustomerId != null && subcustomerId == 0)
                    query += "and a.CustomerId =" + svm.CustomerId;


                if (subcustomerId != null && subcustomerId != 0)
                    query += "and a.SubCustomerId =" + subcustomerId;

                if (svm.SelectedUserId != null && svm.SelectedUserId != 0)
                    query += "and a.UserId=" + svm.SelectedUserId;

                if (svm.SelectedUserId != null && svm.SelectedUserId != 0 && svm.FDate != null && svm.FDate != "" && svm.TDate != null && svm.TDate != "")
                    query += "and a.UserId=" + svm.SelectedUserId +
                        " and CAST(a.FromDate as date) >= '" + fdate
                        + "' and CAST(a.ToDate as date) <= '" + tdate + "' ";

                //when there is no filter ---> where Delind = 0
                //chnges for serviceTill from
                query += " order by a.ServiceBillNo desc";
                PicannolEntities context = new PicannolEntities();

                searchList = context.Database.SqlQuery<ServiceRequestDto>(query).Skip((svm.PageNo - 1) * svm.PageSize).Take(svm.PageSize).ToList<ServiceRequestDto>();

                if (subcustomerId == 0 && svm.SelectedUserId == 0 && (svm.CustomerId != null && svm.CustomerId != 0)
                    && (svm.ProvisionalNo == null || svm.ProvisionalNo == "")
                    && (svm.FDate == null || svm.FDate == "") && (svm.TDate == null || svm.TDate == "") && svm.PageNo == 1)
                {
                    searchList = searchList.Where(x => x.CustomerId == svm.CustomerId && x.SubCustomerId == 0).ToList();

                }


                foreach (var item in searchList)
                {
                    var a = GetFareAndConveyanceDetails(item);
                    item.ServiceChargeAmount = (int)(item.ServiceDays * item.ServiceCharge);
                    item.BoardingAmount = (int)(item.BoardingDays * item.BoardingCharges);
                    item.PocketExpensesAmount = (int)(item.PocketExpensesDays * item.PocketExpenses);
                    item.OvertimeAmount = (int)(item.OvertimeHours * item.OvertimeCharges);

                    item.AmountBeforeTax = (int)(item.ServiceChargeAmount + item.BoardingAmount +
                                                        item.PocketExpensesAmount + a.ConveyanceExpenses +
                                                        a.Fare + item.OvertimeAmount);
                    // int val = (int)(Math.Round(Convert.ToDecimal(item.AmountBeforeTax * item.GST / 100)));
                    item.TaxAmount = item.AmountBeforeTax * item.GST / 100;
                    int amount = item.AmountBeforeTax + item.TaxAmount;
                    item.FinalAmount = amount;
                    item.TimeSheetAuthorized = _context.tblTimeSheets.Where(x => x.ProvisionalBillId == item.ProvisionalBillId).Select(x => x.Authorized == null ? false : x.Authorized).FirstOrDefault();
                    var servicedaylist = GetServiceBillDays(item.ProvisionalBillId);
                    if (servicedaylist.Count <= 0)
                    {
                        servicedaylist.Add(new ServiceDaysDto
                        {
                            FromDate = item.FromDate,
                            ToDate = item.ToDate,

                        });
                    }

                    var lastitem = servicedaylist[servicedaylist.Count - 1];
                    item.EndDate = lastitem.ToDate;
                    var firstitem = servicedaylist.FirstOrDefault();
                    item.TillFromDate = firstitem.FromDate;
                    //new
                    var orderId = _context.tblWorkOrders.Where(x => x.WorkOrderId == item.WorkOrderId && x.DelInd == false).Select(x => x.WorkOrderGUID).FirstOrDefault();

                    if (_context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false))
                    {

                        var invoice = _context.tblInvoices.Where(x => x.OrderGuid == orderId && x.ProvisionalBillId == item.ProvisionalBillId && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                        var invoiceWithoutPId = _context.tblInvoices.Where(x => x.OrderGuid == orderId && x.ProvisionalBillId == null && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                        if (invoice != null)
                        {
                            item.InvoiceNumber = invoice.InvoiceNo;
                            item.LastEmailSendOn = invoice.SendOnDate != null ? invoice.SendOnDate : null;
                        }

                        else if (invoiceWithoutPId != null)
                        {
                            item.InvoiceNumber = invoiceWithoutPId.InvoiceNo;
                            item.LastEmailSendOn = invoiceWithoutPId.SendOnDate != null ? invoiceWithoutPId.SendOnDate : null;
                        }

                        var cr = context.tblCreditNotes.Where(x => x.DelInd == false && x.Cancelled == false).
                                    Select(x => new CreditNoteModel
                                    {
                                        CreditNoteNo = x.CreditNoteNo,
                                        CreditNoteId = x.CreditNoteId,
                                        InvoiceNumber = x.InvoiceNumber,
                                        OrderGuid = x.OrderGuid,
                                        ProvisionalBillId = x.ProvisionalBillId,
                                        TotalAmount = x.TotalAmount
                                    }).ToList();


                        var debitNoteList = context.tblDebitNotes.Where(x => x.DelInd == false).
                                           Select(x => new DebitNoteModel
                                           {
                                               DebitNoteNo = x.DebitNoteNo,
                                               DebitNoteId = x.DebitNoteId,
                                               InvoiceNumber = x.InvoiceNo,
                                               OrderGuid = x.OrderGuid,
                                               ProvisionalBillId = x.ProvisionalBillId,
                                           }).ToList();


                        if (cr != null)
                        {
                            foreach (var r in searchList)
                            {
                                foreach (var cnr in cr)
                                {
                                    if (orderId == cnr.OrderGuid && r.InvoiceNumber == cnr.InvoiceNumber)
                                    {
                                        r.creditNoteNumber = cnr.CreditNoteNo;
                                        r.CreditNoteTotalamount = (int)cnr.TotalAmount;
                                        r.ProvisionalBillId = cnr.ProvisionalBillId;
                                    }
                                }
                            }
                        }

                        if (debitNoteList != null)
                        {
                            foreach (var debitNote in searchList)
                            {
                                foreach (var dbn in debitNoteList)
                                {
                                    if (orderId == dbn.OrderGuid && debitNote.InvoiceNumber == dbn.InvoiceNumber)
                                    {
                                        debitNote.DebitNoteNumber = dbn.DebitNoteNo;
                                        debitNote.ProvisionalBillId = dbn.ProvisionalBillId;
                                    }
                                }
                            }
                        }
                        var pkk = 0;
                        var k = context.tblOrderPayments.OrderByDescending(x => x.OrderPaymentId).ToList();
                        pkk = k.Select(x => x.OrderPaymentId).FirstOrDefault();
                        pkk = pkk++;
                        item.PaymentNumber = pkk;


                        var RecordPaymentList = context.tblOrderPayments.Where(x => x.DelInd == false && x.ProvisionalBillId == item.ProvisionalBillId).ToList();
                        RecordPaymentList = RecordPaymentList.OrderByDescending(x => x.OrderPaymentId).ToList();
                        foreach (var rcp in RecordPaymentList)
                        {
                            item.RemainOrderPayment = true;
                            item.BalanceAmount = rcp.BalanceAmount;
                            break;
                        }

                        if (invoice == null && invoiceWithoutPId == null)
                        {
                            item.InvoiceNumber = "";

                        }

                        // item.LastEmailSendOn= invoice
                    }
                    else
                    {
                        item.InvoiceNumber = "";

                    }
                }
               
                
            }
            catch (Exception)
            {

            }

            return searchList;
        }
        //End 



        //new 15/7/21
        //New Method for Provisional bill pagination By Himanshu//
        public List<ServiceRequestDto> GetServiceRequestsListVersion2(int PageSize = 10, int PageNo = 1)
        {

            PicannolEntities context = new PicannolEntities();

            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());

            var s = (from a in Context.tblProvisionalBills
                     join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                     join c in Context.tblUsers on a.UserId equals c.UserId
                     where (a.DelInd == false && b.Delind == false)
                     select new ServiceRequestDto
                     {
                         ProvisionalBillId = a.ProvisionalBillId,
                         ProvisionalBillNo = a.ServiceBillNo,
                         WorkOrderId = (int)a.WorkOrderId,
                         CustomerName = b.CustomerName,
                         FromDate = (DateTime)a.FromDate,
                         FDate = SqlFunctions.DateName("day", a.FromDate) + "/" + SqlFunctions.DateName("month", a.FromDate) + "/" + SqlFunctions.DateName("year", a.FromDate),
                         ToDate = (DateTime)a.ToDate,
                         TDate = SqlFunctions.DateName("day", a.ToDate) + "/" + SqlFunctions.DateName("month", a.ToDate) + "/" + SqlFunctions.DateName("year", a.ToDate),
                         FinalAmount = a.FinalAmount,
                         UserName = c.UserName,
                         BoardingCharges = (int)a.BoardingCharges,
                         BoardingDays = (int)a.BoardingDays,
                         ServiceDays = a.ServiceDays,
                         ServiceCharge = a.ServiceCharge,
                         //Fare = a.Fare,
                         MachineName = a.MachineName,
                         CallType = a.CallType,
                         PocketExpenses = a.PocketExpenses,
                         //ConveyanceExpenses = a.ConveyanceExpenses,
                         UserId = a.UserId,
                         CustomerId = a.CustomerId,
                         AuthorizerEmail = a.EmailId,
                         Authorized = a.Authorized,
                         Zone = b.Zone,
                         ConveyanceExpensesDays = (int)a.ConveyanceExpenseDays,
                         PocketExpensesDays = (int)a.PocketExpenseDays,
                         OvertimeCharges = (int)a.OvertimeCharges,
                         OvertimeHours = (int)a.OvertimeHours,
                         ServiceChargeAmount = (int)(a.BoardingDays * a.ServiceCharge),
                         BoardingAmount = (int)(a.BoardingDays * a.BoardingCharges),
                         PocketExpensesAmount = (int)(a.PocketExpenseDays * a.PocketExpenses),
                         OvertimeAmount = (int)(a.OvertimeHours * a.OvertimeCharges),
                         Cancelled = a.Cancelled == null ? false : a.Cancelled,
                         GST = a.GST,
                         Paid = a.Paid
                     }).OrderByDescending(x => x.ProvisionalBillId).Skip((PageNo - 1) * PageSize).Take(PageSize).OrderByDescending(x => x.ProvisionalBillNo).ToList();

            //changes for serviceBill form     
            foreach (var item in s)
            {
                var a = GetFareAndConveyanceDetails(item);

                item.ServiceChargeAmount = (item.ServiceDays * item.ServiceCharge);
                item.BoardingAmount = (item.BoardingDays * item.BoardingCharges);
                item.PocketExpensesAmount = (item.PocketExpensesDays * item.PocketExpenses);
                item.OvertimeAmount = (item.OvertimeHours * item.OvertimeCharges);
                item.AmountBeforeTax = (item.ServiceChargeAmount + item.BoardingAmount +
                                                     item.PocketExpensesAmount + a.ConveyanceExpenses +
                                                     a.Fare + item.OvertimeAmount);
                int val = (int)(Math.Round(Convert.ToDecimal(item.AmountBeforeTax * item.GST / 100)));
                int amount = item.AmountBeforeTax + val;
                item.FinalAmount = amount;
                item.TimeSheetAuthorized = _context.tblTimeSheets.Where(x => x.ProvisionalBillId == item.ProvisionalBillId).Select(x => x.Authorized == null ? false : x.Authorized).FirstOrDefault();
                var servicedaylist = GetServiceBillDays(item.ProvisionalBillId);
                if (servicedaylist.Count <= 0)
                {
                    servicedaylist.Add(new ServiceDaysDto
                    {
                        FromDate = item.FromDate,
                        ToDate = item.ToDate,
                    });
                }

                var lastitem = servicedaylist[servicedaylist.Count - 1];
                item.EndDate = lastitem.ToDate;
                var firstitem = servicedaylist.FirstOrDefault();
                item.TillFromDate = firstitem.FromDate;
                var orderId = _context.tblWorkOrders.Where(x => x.WorkOrderId == item.WorkOrderId && x.DelInd == false).Select(x => x.WorkOrderGUID).FirstOrDefault();
                if (_context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false))
                {
                    var invoice = _context.tblInvoices.Where(x => x.OrderGuid == orderId && x.ProvisionalBillId == item.ProvisionalBillId && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    var invoiceWithoutPId = _context.tblInvoices.Where(x => x.OrderGuid == orderId && x.ProvisionalBillId == null && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    if (invoice != null)
                    {
                        item.InvoiceNumber = invoice.InvoiceNo;
                        item.LastEmailSendOn = invoice.SendOnDate != null ? invoice.SendOnDate : null;
                        item.OrderGuid = invoice.OrderGuid;
                    }
                    else if (invoiceWithoutPId != null)
                    {
                        item.InvoiceNumber = invoiceWithoutPId.InvoiceNo;
                        item.LastEmailSendOn = invoiceWithoutPId.SendOnDate != null ? invoiceWithoutPId.SendOnDate : null;
                        item.OrderGuid = invoice.OrderGuid;
                    }
                    if (invoice == null && invoiceWithoutPId == null)
                    {
                        item.InvoiceNumber = "";
                    }
                    var cr = context.tblCreditNotes.Where(x => x.DelInd == false && x.Cancelled == false).
                                       Select(x => new CreditNoteModel
                                       {
                                           CreditNoteNo = x.CreditNoteNo,
                                           CreditNoteId = x.CreditNoteId,
                                           InvoiceNumber = x.InvoiceNumber,
                                           OrderGuid = x.OrderGuid,
                                           ProvisionalBillId = x.ProvisionalBillId,
                                           TotalAmount = x.TotalAmount
                                       }).ToList();
                    if (cr != null)
                    {
                        foreach (var r in s)
                        {
                            foreach (var cnr in cr)
                            {
                                if (orderId == cnr.OrderGuid && r.InvoiceNumber == cnr.InvoiceNumber/* && cnr.ProvisionalBillId==r.ProvisionalBillId*/)
                                {
                                    r.creditNoteNumber = cnr.CreditNoteNo;
                                    r.CreditNoteTotalamount = cnr.TotalAmount ?? 0;
                                }
                            }
                        }
                    }
                    var debitNoteinfo = context.tblDebitNotes.Where(x => x.DelInd == false).
                                       Select(x => new DebitNoteModel
                                       {
                                           DebitNoteNo = x.DebitNoteNo,
                                           DebitNoteId = x.DebitNoteId,
                                           InvoiceNumber = x.InvoiceNo,
                                           OrderGuid = x.OrderGuid,
                                           ProvisionalBillId = x.ProvisionalBillId,
                                           TotalAmount = x.TotalAmount
                                       }).ToList();
                    if (debitNoteinfo != null)
                    {
                        foreach (var r in s)
                        {
                            foreach (var cnr in debitNoteinfo)
                            {
                                if (orderId == cnr.OrderGuid && r.InvoiceNumber == cnr.InvoiceNumber)
                                {
                                    r.DebitNoteNumber = cnr.DebitNoteNo;
                                }
                            }
                        }
                    }
                    var pkk = 0;
                    var k = context.tblOrderPayments.OrderByDescending(x => x.OrderPaymentId).ToList();
                    pkk = k.Select(x => x.OrderPaymentId).FirstOrDefault();
                    pkk = pkk++;
                    item.PaymentNumber = pkk;
                    var RecordPaymentList = context.tblOrderPayments.Where(x => x.DelInd == false && x.ProvisionalBillId == item.ProvisionalBillId).ToList();
                    RecordPaymentList = RecordPaymentList.OrderByDescending(x => x.OrderPaymentId).ToList();
                    foreach (var rcp in RecordPaymentList)
                    {
                        item.RemainOrderPayment = true;
                        item.BalanceAmount = rcp.BalanceAmount;
                        break;
                    }
                }


                else
                {
                    item.InvoiceNumber = "";

                }
            }
            

            return s;
        }


        public List<TimeSheetDetailDto> TimeSheetDetailsList(int id)
        {
            try
            {
                var z = (from a in Context.tblTimeSheetDetails
                         where a.TimeSheetId == id
                         select new TimeSheetDetailDto
                         {
                             WorkDate = a.WorkDate,
                             TotalHours = a.TotalHours,
                             Description = a.Description,
                             WeekNo = SqlFunctions.StringConvert((double)a.WeekNo),

                         }).ToList();
                return z;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in TimeSheetDetails!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        public TimeSheetDto TimeSheetDetails(int id)
        {
            try
            {
                var t = (from a in Context.tblTimeSheets
                         join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                         join c in Context.tblUsers on a.UserId equals c.UserId
                         where a.TimeSheetId == id && a.DelInd == false
                         select new TimeSheetDto
                         {
                             TimeSheetNo = a.TimeSheetNo,
                             CustomerName = b.CustomerName,
                             UserName = c.UserName,
                             UserEmail = c.Email,
                             FromDate = (DateTime)a.FromDate,
                             ToDate = (DateTime)a.ToDate,
                             UImagePath = a.UserSignatureId,
                             CImagePath = a.ImageId,
                             AuthorizedBy = a.AuthorizedBy,
                             AuthorizerEmail = a.EmailId,
                             Designation = a.Designation,
                             Remarks = a.Remarks
                         }).FirstOrDefault();
                return t;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in TimeSheetDetail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public List<ServiceDaysDto> GetServiceBillDays(int id)
        {
            try
            {
                var billDays = Context.tblServiceDays.Where(x => x.ProvisionalBillId == id).
                    Select(x => new ServiceDaysDto
                    {
                        FromDate = x.FromDate,
                        ToDate = x.ToDate
                    }).ToList();
                return billDays;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceBillDays!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ServiceRequestDto GetServiceRequestDetails(int id)
        {
            try
            {
                var s = (from a in Context.tblProvisionalBills
                         join b in Context.tblUsers on a.UserId equals b.UserId
                         where a.ProvisionalBillId == id && a.DelInd == false
                         select new ServiceRequestDto
                         {
                             ProvisionalBillNo = a.ServiceBillNo,
                             WorkOrderId = (int)a.WorkOrderId,
                             ProvisionalBillId = a.ProvisionalBillId,
                             CustomerId = a.CustomerId,
                             SubCustomerId = a.SubCustomerId != null ? (int)a.SubCustomerId.Value : 0,
                             MachineName = a.MachineName,
                             UserId = a.UserId,
                             CallType = a.CallType,
                             FromDate = a.FromDate,
                             ToDate = a.ToDate,
                             ServiceCharge = (int)a.ServiceCharge,
                             PocketExpenses = a.PocketExpenses,
                             BoardingCharges = (int)a.BoardingCharges,
                             BoardingDays = (int)a.BoardingDays,
                             OtherCharges = a.OtherCharges,
                             DateCreated = a.DateCreated,
                             GST = a.GST,
                             FinalAmount = a.FinalAmount,
                             ServiceDays = a.ServiceDays,
                             PocketExpensesDays = (int)a.PocketExpenseDays,
                             ConveyanceExpensesDays = (int)a.ConveyanceExpenseDays,
                             AuthorizedBy = a.AuthorizedBy,
                             AuthorizerEmail = a.EmailId,
                             Designation = a.Designation,
                             AuthorizedOn = SqlFunctions.DateName("day", a.AuthorizedOn) + "/" + SqlFunctions.DateName("month", a.AuthorizedOn) + "/" + SqlFunctions.DateName("year", a.AuthorizedOn),
                             UserName = b.UserName,
                             OvertimeCharges = (int)a.OvertimeCharges,
                             OvertimeHours = (int)a.OvertimeHours,
                             TotalDays = (int)a.TotalDays,
                             FinalSubmit = (bool)a.FinalSubmit,
                             Authorized = a.Authorized
                         }).FirstOrDefault();
                s = GetFareAndConveyanceDetails(s);
                return s;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceRequestDetails!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        public ServiceRequestDto GetFareAndConveyanceDetails(ServiceRequestDto sr)
        {
            try
            {
                ServiceRequestDto srd = new ServiceRequestDto();
                srd = sr;
                var e = (from a in Context.tblDetailedExpenses
                         where a.ProvisionalBillId == sr.ProvisionalBillId
                         select new DetailedExpenseDto
                         {
                             Type = a.Type,
                             ExpenseFrom = a.ExpenseFrom,
                             ExpenseTo = a.ExpenseTo,
                             Remarks = a.Remarks,
                             Amount = (int)a.Amount,
                         }).ToList();
                srd.ConeyanceExpenseDetails = new List<DetailedExpenseDto>();
                srd.FareExpenseDetails = new List<DetailedExpenseDto>();
                foreach (var item in e)
                {
                    if (item.Type == ConstantsHelper.ExpenseType.ConveyanceAndIncidental.ToString())
                    {
                        srd.ConeyanceExpenseDetails.Add(item);
                        srd.ConveyanceExpenses += (int)item.Amount;
                        srd.newConveyaceExpances = (int)item.Amount;
                    }
                    else if (item.Type == ConstantsHelper.ExpenseType.Fare.ToString())
                    {
                        srd.FareExpenseDetails.Add(item);
                        srd.Fare += (int)item.Amount;
                        srd.NewFare = (int)item.Amount;
                    }
                }
                return srd;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetFareAndConveyanceDetails!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }


        public ServiceRequestDto GetServiceRequestDetailsByWorkOrder(int id, string initiator)
        {
            try
            {
                ServiceRequestDto s = new ServiceRequestDto();

                if (Context.tblCreditNotes.Any(x => x.ProvisionalBillId == id && (x.CRNCreated == true && x.DelInd == false && x.Cancelled == false))
                    && (initiator == ConstantsHelper.SelectAction.CreateCreditNote.ToString()
                    || initiator == ConstantsHelper.SelectAction.CreateDebitNote.ToString()))
                {

                    s = (from a in Context.tblCreditNotes
                         join b in Context.tblProvisionalBills on a.ProvisionalBillId equals b.ProvisionalBillId
                         join c in Context.tblUsers on b.UserId equals c.UserId
                         where a.ProvisionalBillId == id && a.Cancelled == false
                         select new ServiceRequestDto
                         {
                             ProvisionalBillNo = b.ServiceBillNo,
                             ProvisionalBillId = a.ProvisionalBillId,
                             CustomerId = a.CustomerId,
                             SubCustomerId = b.SubCustomerId != null ? (int)b.SubCustomerId : 0,
                             MachineName = b.MachineName,
                             UserId = b.UserId,
                             CallType = b.CallType,
                             FromDate = b.FromDate,
                             ToDate = b.ToDate,
                             ServiceCharge = (int?)a.ServiceCharge ?? 0,
                             Fare = (int?)a.Fare ?? 0,
                             PocketExpenses = (int)a.PocketExpeses,
                             BoardingCharges = (int?)a.BoardingCharges ?? 0,
                             BoardingDays = (int?)a.BoardingDays ?? 0,
                             ConveyanceExpenses = (int?)a.ConveyanceExpenses ?? 0,
                             OtherCharges = b.OtherCharges,
                             DateCreated = a.CreditNoteDate,
                             GST = b.GST,
                             FinalAmount = (int?)a.TotalAmount ?? 0,
                             ServiceDays = (int?)a.ServiceDays ?? 0,
                             PocketExpensesDays = (int?)a.PocketExpenseDays ?? 0,
                             ConveyanceExpensesDays = (int?)a.ConveyanceExpenseDays ?? 0,
                             AuthorizedBy = b.AuthorizedBy,
                             AuthorizerEmail = b.EmailId,
                             Designation = b.Designation,
                             AuthorizedOn = SqlFunctions.DateName("day", b.AuthorizedOn) + "/" + SqlFunctions.DateName("month", b.AuthorizedOn) + "/" + SqlFunctions.DateName("year", b.AuthorizedOn),
                             UserName = c.UserName,
                             OvertimeCharges = (int?)a.OvertimeCharges ?? 0,
                             OvertimeHours = (int?)a.OvertimeHours ?? 0,
                             WorkOrderId = (int)b.WorkOrderId,
                             CRNCreated = a.CRNCreated
                         }).FirstOrDefault();

                }
                else
                {

                    s = (from a in Context.tblProvisionalBills
                         join b in Context.tblUsers on a.UserId equals b.UserId
                         where a.ProvisionalBillId == id
                         select new ServiceRequestDto
                         {
                             ProvisionalBillNo = a.ServiceBillNo,
                             ProvisionalBillId = a.ProvisionalBillId,
                             CustomerId = a.CustomerId,
                             SubCustomerId = a.SubCustomerId != null ? (int)a.SubCustomerId : 0,
                             MachineName = a.MachineName,
                             UserId = a.UserId,
                             CallType = a.CallType,
                             FromDate = a.FromDate,
                             ToDate = a.ToDate,
                             ServiceCharge = (int?)a.ServiceCharge ?? 0,
                             //Fare = (int?) a.Fare ?? 0,
                             PocketExpenses = a.PocketExpenses,
                             BoardingCharges = (int?)a.BoardingCharges ?? 0,
                             BoardingDays = (int?)a.BoardingDays ?? 0,
                             //ConveyanceExpenses = (int?)a.ConveyanceExpenses ?? 0,
                             OtherCharges = a.OtherCharges,
                             DateCreated = a.DateCreated,
                             GST = a.GST,
                             FinalAmount = (int?)a.FinalAmount ?? 0,
                             ServiceDays = (int?)a.ServiceDays ?? 0,
                             PocketExpensesDays = (int?)a.PocketExpenseDays ?? 0,
                             ConveyanceExpensesDays = (int?)a.ConveyanceExpenseDays ?? 0,
                             AuthorizedBy = a.AuthorizedBy,
                             AuthorizerEmail = a.EmailId,
                             Designation = a.Designation,
                             AuthorizedOn = SqlFunctions.DateName("day", a.AuthorizedOn) + "/" + SqlFunctions.DateName("month", a.AuthorizedOn) + "/" + SqlFunctions.DateName("year", a.AuthorizedOn),
                             UserName = b.UserName,
                             OvertimeCharges = (int?)a.OvertimeCharges ?? 0,
                             OvertimeHours = (int?)a.OvertimeHours ?? 0,
                             WorkOrderId = (int)a.WorkOrderId,
                             CRNCreated = false
                         }).FirstOrDefault();
                }
                if (s.CRNCreated == false)
                {
                    s = GetFareAndConveyanceDetails(s);
                }

                var provisionalServiceDays = GetServiceBillDays(s.ProvisionalBillId);
                if (provisionalServiceDays.Count <= 0)
                {
                    provisionalServiceDays.Add(new ServiceDaysDto
                    {
                        FromDate = s.FromDate,
                        ToDate = s.ToDate,
                    });
                }
                var lastitem = provisionalServiceDays[provisionalServiceDays.Count - 1];
                s.ToDate = lastitem.ToDate;
                var firstitem = provisionalServiceDays.FirstOrDefault();
                s.FromDate = firstitem.FromDate;

                return s;


            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceRequestDetailsByWorkOrder!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }



        #region get Service request detail version v1
        public ServiceRequestDto GetServiceRequestDetailsByWorkOrderV1(int id, string initiator)
        {
            try
            {
                ServiceRequestDto s = new ServiceRequestDto();
                var creditNote = Context.tblCreditNotes.Where(c => c.ProvisionalBillId == id && c.DelInd == false).FirstOrDefault();
                var provisionalBillDtl = Context.tblProvisionalBills.Where(p => p.ProvisionalBillId == id).FirstOrDefault();

                if (creditNote != null)
                {
                    var creditNoteAmt = (int)creditNote.TotalAmount;
                    if (creditNote.ProvisionalBillId == provisionalBillDtl.ProvisionalBillId && creditNote.CRNCreated == true
                        && creditNote.DelInd == false && creditNoteAmt != provisionalBillDtl.FinalAmount && initiator == "")
                    {

                        s = (from a in Context.tblCreditNotes
                             join b in Context.tblProvisionalBills on a.ProvisionalBillId equals b.ProvisionalBillId
                             join c in Context.tblUsers on b.UserId equals c.UserId
                             where a.ProvisionalBillId == id
                             select new ServiceRequestDto
                             {
                                 ProvisionalBillNo = b.ServiceBillNo,
                                 ProvisionalBillId = a.ProvisionalBillId,
                                 CustomerId = a.CustomerId,
                                 SubCustomerId = b.SubCustomerId != null ? (int)b.SubCustomerId : 0,
                                 MachineName = b.MachineName,
                                 UserId = b.UserId,
                                 CallType = b.CallType,
                                 FromDate = b.FromDate,
                                 ToDate = b.ToDate,
                                 ServiceCharge = (int?)a.ServiceCharge ?? 0,
                                 Fare = (int?)a.Fare ?? 0,
                                 PocketExpenses = (int)a.PocketExpeses,
                                 BoardingCharges = (int?)a.BoardingCharges ?? 0,
                                 BoardingDays = (int?)a.BoardingDays ?? 0,
                                 ConveyanceExpenses = (int?)a.ConveyanceExpenses ?? 0,
                                 OtherCharges = b.OtherCharges,
                                 DateCreated = a.CreditNoteDate,
                                 GST = b.GST,
                                 FinalAmount = (int?)a.TotalAmount ?? 0,
                                 ServiceDays = (int?)a.ServiceDays ?? 0,
                                 PocketExpensesDays = (int?)a.PocketExpenseDays ?? 0,
                                 ConveyanceExpensesDays = (int?)a.ConveyanceExpenseDays ?? 0,
                                 AuthorizedBy = b.AuthorizedBy,
                                 AuthorizerEmail = b.EmailId,
                                 Designation = b.Designation,
                                 AuthorizedOn = SqlFunctions.DateName("day", b.AuthorizedOn) + "/" + SqlFunctions.DateName("month", b.AuthorizedOn) + "/" + SqlFunctions.DateName("year", b.AuthorizedOn),
                                 UserName = c.UserName,
                                 OvertimeCharges = (int?)a.OvertimeCharges ?? 0,
                                 OvertimeHours = (int?)a.OvertimeHours ?? 0,
                                 WorkOrderId = (int)b.WorkOrderId,
                                 CRNCreated = a.CRNCreated
                             }).FirstOrDefault();

                    }
                    else
                    {

                        s = (from a in Context.tblProvisionalBills
                             join b in Context.tblUsers on a.UserId equals b.UserId
                             where a.ProvisionalBillId == id
                             select new ServiceRequestDto
                             {
                                 ProvisionalBillNo = a.ServiceBillNo,
                                 ProvisionalBillId = a.ProvisionalBillId,
                                 CustomerId = a.CustomerId,
                                 SubCustomerId = a.SubCustomerId != null ? (int)a.SubCustomerId : 0,
                                 MachineName = a.MachineName,
                                 UserId = a.UserId,
                                 CallType = a.CallType,
                                 FromDate = a.FromDate,
                                 ToDate = a.ToDate,
                                 ServiceCharge = (int?)a.ServiceCharge ?? 0,
                                 //Fare = (int?) a.Fare ?? 0,
                                 PocketExpenses = a.PocketExpenses,
                                 BoardingCharges = (int?)a.BoardingCharges ?? 0,
                                 BoardingDays = (int?)a.BoardingDays ?? 0,
                                 //ConveyanceExpenses = (int?)a.ConveyanceExpenses ?? 0,
                                 OtherCharges = a.OtherCharges,
                                 DateCreated = a.DateCreated,
                                 GST = a.GST,
                                 FinalAmount = (int?)a.FinalAmount ?? 0,
                                 ServiceDays = (int?)a.ServiceDays ?? 0,
                                 PocketExpensesDays = (int?)a.PocketExpenseDays ?? 0,
                                 ConveyanceExpensesDays = (int?)a.ConveyanceExpenseDays ?? 0,
                                 AuthorizedBy = a.AuthorizedBy,
                                 AuthorizerEmail = a.EmailId,
                                 Designation = a.Designation,
                                 AuthorizedOn = SqlFunctions.DateName("day", a.AuthorizedOn) + "/" + SqlFunctions.DateName("month", a.AuthorizedOn) + "/" + SqlFunctions.DateName("year", a.AuthorizedOn),
                                 UserName = b.UserName,
                                 OvertimeCharges = (int?)a.OvertimeCharges ?? 0,
                                 OvertimeHours = (int?)a.OvertimeHours ?? 0,
                                 WorkOrderId = (int)a.WorkOrderId,
                                 CRNCreated = false
                             }).FirstOrDefault();
                    }
                    if (s.CRNCreated == false)
                    {
                        s = GetFareAndConveyanceDetails(s);
                    }
                    var provisionalServiceDays = GetServiceBillDays(s.ProvisionalBillId);
                    if (provisionalServiceDays.Count <= 0)
                    {
                        provisionalServiceDays.Add(new ServiceDaysDto
                        {
                            FromDate = s.FromDate,
                            ToDate = s.ToDate,
                        });
                    }
                    var lastitem = provisionalServiceDays[provisionalServiceDays.Count - 1];
                    s.ToDate = lastitem.ToDate;
                    var firstitem = provisionalServiceDays.FirstOrDefault();
                    s.FromDate = firstitem.FromDate;
                }

                return s;

            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceRequestDetailsByWorkOrder!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        #endregion

        public string GetLastServiceBillNumber()
        {
            try
            {
                string i = "";
                var inv = (from a in Context.tblProvisionalBills
                           where a.DelInd == false
                           select a).ToList();
                inv = inv.OrderByDescending(x => x.ProvisionalBillId).ToList();
                i = inv.Select(x => x.ServiceBillNo).FirstOrDefault();
                return i;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public OrderPaymentDto GetPaymentDetails(string InvoiceNo)
        {
            var d = (from a in Context.tblOrderPayments
                     where a.InvoiceNo == InvoiceNo && a.DelInd == false
                     select new OrderPaymentDto
                     {
                         DatePaid = (DateTime)a.DatePaid,
                         OrderPaymentId = a.OrderPaymentId,
                         PaymentType = a.PaymentType,
                         SelectedPaymentMethod = a.PaymentType,
                         PaymentGUID = a.PaymentGUID,
                         AmountPaid = (a.AmountPaid != null) ? (decimal)a.AmountPaid : 0,
                         TDS = (a.TDS != null) ? (decimal)a.TDS : 0,
                     }).FirstOrDefault();

            decimal tt = 0;
            var pk = Context.tblOrderPayments.Where(x => x.InvoiceNo == InvoiceNo).ToList();

            foreach (var item in pk)
            {
                tt += (decimal)item.ReceivedAmount;

            }
            if (d == null)
            {
            }
            else { d.ReceivedAmount = tt; }


            return d;
        }

        #endregion

        #region Overrides
        public override void Add(tblProvisionalBill student)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(student);
            }
        }
        public override void Update(tblProvisionalBill Service)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                tblProvisionalBill update = Context.tblProvisionalBills.Where(p => p.ProvisionalBillId == Service.ProvisionalBillId).FirstOrDefault();
                //update.PocketExpenses = Service.PocketExpenses;
                update.CallType = Service.CallType;
                update.ServiceCharge = Service.ServiceCharge;
                update.CustomerId = Service.CustomerId;
                update.DateCreated = DateTime.Now;
                update.Fare = Service.Fare;
                update.FromDate = Convert.ToDateTime(Service.FromDate);
                update.ToDate = Convert.ToDateTime(Service.ToDate);
                update.MachineName = Service.MachineName;
                update.BoardingCharges = Service.BoardingCharges;
                update.BoardingDays = Service.BoardingDays;
                update.ConveyanceExpenses = Service.ConveyanceExpenses;
                update.PocketExpenses = Service.PocketExpenses;
                update.GST = Service.GST;
                update.FinalAmount = Service.FinalAmount;
                update.ServiceDays = Service.ServiceDays;
                update.UserId = Service.UserId;
                base.Update(update);
            }
        }

        #endregion
    }
}