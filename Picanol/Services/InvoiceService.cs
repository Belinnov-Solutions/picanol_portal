using Picanol.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Picanol.Models;
//using System.Data.Objects;
using Picanol.ViewModels;
using System.Data.Entity;
using System.Configuration;
using System.Data.Objects.SqlClient;
using Picanol.Helpers;

namespace Picanol.Services
{
    public class InvoiceService : BaseService<PicannolEntities, tblInvoice>
    {
        #region Constructors
        internal InvoiceService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }


        //private readonly InvoiceHelper _invoiceHelper;
        #endregion

        #region BaseMethods
        public void SaveInvoice(tblInvoice cl)
        {
            AddInvoice(cl);
        }
        public void AddInvoice(tblInvoice cls)
        {
            if (cls == null)
                throw new ArgumentNullException("challan", "Null Parameter");
            Add(cls);
        }
        internal string GetLastAC1FOC(string invtype)
        {
            string i = "";
            if (invtype == "AC1FOC")
            {
                var inv = (from a in Context.tblInvoices
                           where a.DelInd == false && a.InvoiceNo.Contains("AC1/FOC")
                           select a).ToList();

                inv = inv.OrderByDescending(x => x.InvoiceId).ToList();
                i = inv.Select(x => x.InvoiceNo).FirstOrDefault();
            }
            return i;
        }
        internal string GetLastInvoiceNumber(string invoiceType)
        {
            string i = "";

            if (invoiceType == ConstantsHelper.InvoiceType.DN.ToString())
            {
                var inv = (from a in Context.tblDebitNotes
                           where a.DelInd == false && a.DebitNoteNo.Contains(invoiceType)
                           select a).ToList();

                inv = inv.OrderByDescending(x => x.DebitNoteId).ToList();
                i = inv.Select(x => x.DebitNoteNo).FirstOrDefault();

            }
            if (invoiceType == ConstantsHelper.InvoiceType.CN.ToString())
            {
                var inv = (from a in Context.tblCreditNotes
                           where a.DelInd == false && a.CreditNoteNo.Contains(invoiceType)
                           select a).ToList();

                inv = inv.OrderByDescending(x => x.CreditNoteId).ToList();
                i = inv.Select(x => x.CreditNoteNo).FirstOrDefault();
            }

            if (invoiceType != ConstantsHelper.InvoiceType.PI.ToString() 
                && invoiceType != ConstantsHelper.InvoiceType.CN.ToString()
                && invoiceType != ConstantsHelper.InvoiceType.DN.ToString())
            {
                var inv = (from a in Context.tblInvoices
                           where a.DelInd == false && a.InvoiceNo.Contains(invoiceType)
                           select a).ToList();

                inv = inv.OrderByDescending(x => x.InvoiceId).ToList();

                if (invoiceType == ConstantsHelper.InvoiceType.AC1.ToString())
                {
                    inv.RemoveAll(x => x.InvoiceNo.Contains("AC1/FOC"));
                }

                if (invoiceType == ConstantsHelper.InvoiceType.AC.ToString())
                {
                    inv.RemoveAll(x => x.InvoiceNo.Contains("AC1/FOC"));
                }
                if (invoiceType == ConstantsHelper.InvoiceType.FOC.ToString())
                {
                    inv.RemoveAll(x => x.InvoiceNo.Contains("AC1/FOC"));
                }
                i = inv.Select(x => x.InvoiceNo).FirstOrDefault();
            }

            else if (invoiceType != ConstantsHelper.InvoiceType.CN.ToString() 
                && invoiceType != ConstantsHelper.InvoiceType.DN.ToString())
            {
                var inv = (from a in Context.tblProformaInvoices
                           where a.ProformaInvoiceNo.Contains(invoiceType)
                           select a).ToList();

                inv = inv.OrderByDescending(x => x.ProformaInvoiceId).ToList();
                i = inv.Select(x => x.ProformaInvoiceNo).FirstOrDefault();

            }
            return i;
        }

        public List<InvoiceDto> GetFilteredInvoicesList(InvoiceListViewModel vm)
        {
            vm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            PicannolEntities context = new PicannolEntities();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (!string.IsNullOrEmpty(vm.fDate) && !string.IsNullOrEmpty(vm.fDate))
            {
                fdate = Convert.ToDateTime(vm.fDate);
                DateTime? fDate = Convert.ToDateTime(vm.tDate);
                tdate = fDate.Value.AddDays(1);
            }
            var result = new List<InvoiceDto>();
            if (fdate != null && tdate != null && vm.SelectedCustomer == 0)
            {
                result = context.tblInvoices.Where(x => x.DelInd == false 
                && x.Cancelled == false && x.DateCreated >= fdate 
                && x.DateCreated <= tdate).
                    Select(x => new InvoiceDto
                    {
                        InvoiceId = x.InvoiceId,
                        InvoiceNo = x.InvoiceNo,
                        DateCreated = x.DateCreated,
                        OrderGuid = x.OrderGuid,
                        Amount = x.Amount,
                        Status = x.Status,
                        InvoiceDate = x.InvoiceDate,
                        DueDate = x.DueDate,
                        CustomerId = x.CustomerId,
                        ProvisionalBillId = (x.ProvisionalBillId != null) ? (int)x.ProvisionalBillId : 0,
                        WorkOrderId = context.tblWorkOrders.Where(z => z.WorkOrderGUID == x.OrderGuid).Select(z => z.WorkOrderId).FirstOrDefault(),
                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
            }
            else if (fdate != null && tdate != null && vm.SelectedCustomer != 0)
            {
                result = context.tblInvoices.Where(x => x.DelInd == false 
                && x.Cancelled == false && x.DateCreated >= fdate && x.DateCreated <= tdate 
                && x.CustomerId == vm.SelectedCustomer).
                    Select(x => new InvoiceDto
                    {
                        InvoiceId = x.InvoiceId,
                        InvoiceNo = x.InvoiceNo,
                        DateCreated = x.DateCreated,
                        OrderGuid = x.OrderGuid,
                        Amount = x.Amount,
                        Status = x.Status,
                        InvoiceDate = x.InvoiceDate,
                        DueDate = x.DueDate,
                        CustomerId = x.CustomerId,
                        ProvisionalBillId = (x.ProvisionalBillId != null) ? (int)x.ProvisionalBillId : 0,
                        WorkOrderId = context.tblWorkOrders.Where(z => z.WorkOrderGUID == x.OrderGuid).Select(z => z.WorkOrderId).FirstOrDefault(),
                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
            }
            else if (fdate == null && tdate == null && vm.SelectedCustomer != 0)
            {
                result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false && vm.fDate == null && vm.tDate == null && x.CustomerId == vm.SelectedCustomer).
                                    Select(x => new InvoiceDto
                                    {
                                        InvoiceId = x.InvoiceId,
                                        InvoiceNo = x.InvoiceNo,
                                        DateCreated = x.DateCreated,
                                        OrderGuid = x.OrderGuid,
                                        Amount = x.Amount,
                                        Status = x.Status,
                                        InvoiceDate = x.InvoiceDate,
                                        DueDate = x.DueDate,
                                        CustomerId = x.CustomerId,
                                        ProvisionalBillId = (x.ProvisionalBillId != null) ? (int)x.ProvisionalBillId : 0,
                                        WorkOrderId = context.tblWorkOrders.Where(z => z.WorkOrderGUID == x.OrderGuid).Select(z => z.WorkOrderId).FirstOrDefault(),
                                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
            }
            /*result.RemoveAll(x => x.InvoiceNo.Contains("AC1"));*/
            result.RemoveAll(x => x.InvoiceNo.Contains(ConstantsHelper.InvoiceType.AC1.ToString()));

            return result;
        }

        public List<InvoiceDto> GetCancelledInvoiceList(InvoiceListViewModel vm)
        {
            vm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            PicannolEntities context = new PicannolEntities();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (!string.IsNullOrEmpty(vm.fDate) && !string.IsNullOrEmpty(vm.fDate))
            {
                fdate = Convert.ToDateTime(vm.fDate);
                DateTime? fDate = Convert.ToDateTime(vm.tDate);
                tdate = fDate.Value.AddDays(1);


            }
            var result = new List<InvoiceDto>();
            if (fdate != null && tdate != null && vm.SelectedCustomer == 0)
            {
                result = context.tblInvoices.Where(x => x.DelInd == false 
                && x.Cancelled == true && x.DateCreated >= fdate && x.DateCreated <= tdate).
                    Select(x => new InvoiceDto
                    {
                        InvoiceId = x.InvoiceId,
                        InvoiceNo = x.InvoiceNo,
                        DateCreated = x.DateCreated,
                        OrderGuid = x.OrderGuid,
                        Amount = x.Amount,
                        Status = x.Status,
                        InvoiceDate = x.InvoiceDate,
                        DueDate = x.DueDate,
                        CustomerId = x.CustomerId,
                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
            }
            else if (fdate != null && tdate != null && vm.SelectedCustomer != 0)
            {
                result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == true && x.DateCreated >= fdate && x.DateCreated <= tdate && x.CustomerId == vm.SelectedCustomer).
                    Select(x => new InvoiceDto
                    {
                        InvoiceId = x.InvoiceId,
                        InvoiceNo = x.InvoiceNo,
                        DateCreated = x.DateCreated,
                        OrderGuid = x.OrderGuid,
                        Amount = x.Amount,
                        Status = x.Status,
                        InvoiceDate = x.InvoiceDate,
                        DueDate = x.DueDate,
                        CustomerId = x.CustomerId,
                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
            }
            else if (fdate == null && tdate == null && vm.SelectedCustomer != 0)
            {
                result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == true && vm.fDate == null && vm.tDate == null && x.CustomerId == vm.SelectedCustomer).
                                    Select(x => new InvoiceDto
                                    {
                                        InvoiceId = x.InvoiceId,
                                        InvoiceNo = x.InvoiceNo,
                                        DateCreated = x.DateCreated,
                                        OrderGuid = x.OrderGuid,
                                        Amount = x.Amount,
                                        Status = x.Status,
                                        InvoiceDate = x.InvoiceDate,
                                        DueDate = x.DueDate,
                                        CustomerId = x.CustomerId,
                                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
            }
            else
            {
                result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == true && vm.fDate == null && vm.tDate == null && x.InvoiceNo.Contains(vm.InvoiceNo)).
                                                    Select(x => new InvoiceDto
                                                    {
                                                        InvoiceId = x.InvoiceId,
                                                        InvoiceNo = x.InvoiceNo,
                                                        DateCreated = x.DateCreated,
                                                        OrderGuid = x.OrderGuid,
                                                        Amount = x.Amount,
                                                        Status = x.Status,
                                                        InvoiceDate = x.InvoiceDate,
                                                        DueDate = x.DueDate,
                                                        CustomerId = x.CustomerId,
                                                        CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                                        CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                                        TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                                    }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();

            }

            /*result.RemoveAll(x => x.InvoiceNo.Contains("AC1"));*/
            result.RemoveAll(x => x.InvoiceNo.Contains(ConstantsHelper.InvoiceType.AC1.ToString()));
            return result;
        }

        //new invoive pagination

        public List<InvoiceDto> GetFilteredInvoicesListVersion1(InvoiceListViewModel vm)
        {
            vm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());

            try
            {
                PicannolEntities context = new PicannolEntities();
                DateTime? fdate = null;
                DateTime? tdate = null;
                if (!string.IsNullOrEmpty(vm.fDate) && !string.IsNullOrEmpty(vm.fDate))
                {
                    fdate = Convert.ToDateTime(vm.fDate);
                    DateTime? fDate = Convert.ToDateTime(vm.tDate);
                    tdate = fDate.Value.AddDays(1);
                }
                var result = new List<InvoiceDto>();
                if (fdate != null && tdate != null && vm.SelectedCustomer == 0)
                {

                    result = context.tblInvoices.Where(x => x.InvoiceNo.Contains("AC1") == false 
                    && x.InvoiceNo.Contains("AC") == false && x.InvoiceNo.Contains("AC1/FOC") == false
                    && x.DelInd == false && x.Cancelled == false && x.DateCreated >= fdate 
                    && x.DateCreated <= tdate).
                        Select(x => new InvoiceDto
                        {
                            InvoiceId = x.InvoiceId,
                            InvoiceNo = x.InvoiceNo,
                            DateCreated = x.DateCreated,
                            OrderGuid = x.OrderGuid,
                            Amount = x.Amount,
                            Status = x.Status,
                            InvoiceDate = x.InvoiceDate,
                            DueDate = x.DueDate,
                            CustomerId = x.CustomerId,
                            CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                            DebitNoteNo = context.tblDebitNotes.Where(y => y.InvoiceNo == x.InvoiceNo).Select(y => y.DebitNoteNo).FirstOrDefault(),
                            CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                            TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                            PackingCharges = (x.Packing != null ? (decimal)x.Packing : 0),
                            ForwardingCharges = (x.Forwading != null ? (decimal)x.Forwading : 0),
                        }).OrderByDescending(x => x.DateCreated).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
                }
                else if (fdate != null && tdate != null && vm.SelectedCustomer != 0)
                {
                    result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false && x.DateCreated >= fdate && x.DateCreated <= tdate && x.CustomerId == vm.SelectedCustomer).
                        Select(x => new InvoiceDto
                        {
                            InvoiceId = x.InvoiceId,
                            InvoiceNo = x.InvoiceNo,
                            DateCreated = x.DateCreated,
                            OrderGuid = x.OrderGuid,
                            Amount = x.Amount,
                            Status = x.Status,
                            InvoiceDate = x.InvoiceDate,
                            DueDate = x.DueDate,
                            CustomerId = x.CustomerId,
                            CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                            CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                            DebitNoteNo = context.tblDebitNotes.Where(y => y.InvoiceNo == x.InvoiceNo).Select(y => y.DebitNoteNo).FirstOrDefault(),
                            PackingCharges = (x.Packing != null ? (decimal)x.Packing : 0),
                            ForwardingCharges = (x.Forwading != null ? (decimal)x.Forwading : 0),
                            TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                        }).OrderByDescending(x => x.DateCreated)/*.Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();*/
                            .Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
                }
                else if (fdate == null && tdate == null && vm.SelectedCustomer != 0)
                {
                    result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false
                    && vm.fDate == null && vm.tDate == null && x.CustomerId == vm.SelectedCustomer).
                                        Select(x => new InvoiceDto
                                        {
                                            InvoiceId = x.InvoiceId,
                                            InvoiceNo = x.InvoiceNo,
                                            DateCreated = x.DateCreated,
                                            OrderGuid = x.OrderGuid,
                                            Amount = x.Amount,
                                            Status = x.Status,
                                            InvoiceDate = x.InvoiceDate,
                                            DueDate = x.DueDate,
                                            CustomerId = x.CustomerId,
                                            CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                            CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                            DebitNoteNo = context.tblDebitNotes.Where(y => y.InvoiceNo == x.InvoiceNo).Select(y => y.DebitNoteNo).FirstOrDefault(),
                                            TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                            PackingCharges = (x.Packing != null ? (decimal)x.Packing : 0),
                                            ForwardingCharges = (x.Forwading != null ? (decimal)x.Forwading : 0),
                                        }).OrderByDescending(x => x.DateCreated)/*.Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();*/
                                         .Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();

                }
                else if (fdate == null && tdate == null 
                    && vm.SelectedCustomer == 0 
                    && vm.InvoiceNo != null)
                {
                    result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false
                    && vm.fDate == null && vm.tDate == null && x.InvoiceNo == vm.InvoiceNo).
                                        Select(x => new InvoiceDto
                                        {
                                            InvoiceId = x.InvoiceId,
                                            InvoiceNo = x.InvoiceNo,
                                            DateCreated = x.DateCreated,
                                            OrderGuid = x.OrderGuid,
                                            Amount = x.Amount,
                                            Status = x.Status,
                                            InvoiceDate = x.InvoiceDate,
                                            DueDate = x.DueDate,
                                            CustomerId = x.CustomerId,
                                            CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                            CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                            DebitNoteNo = context.tblDebitNotes.Where(y => y.InvoiceNo == x.InvoiceNo).Select(y => y.DebitNoteNo).FirstOrDefault(),
                                            TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                            PackingCharges = (x.Packing != null ? (decimal)x.Packing : 0),
                                            ForwardingCharges = (x.Forwading != null ? (decimal)x.Forwading : 0),
                                        }).OrderByDescending(x => x.DateCreated)/*.Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();*/
                                         .Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();

                }
                else if (fdate == null && tdate == null && vm.SelectedCustomer != 0 && vm.InvoiceNo != null)
                {
                    result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false
                   && vm.fDate == null && vm.tDate == null && x.InvoiceNo == vm.InvoiceNo && x.CustomerId == vm.SelectedCustomer).
                                       Select(x => new InvoiceDto
                                       {
                                           InvoiceId = x.InvoiceId,
                                           InvoiceNo = x.InvoiceNo,
                                           DateCreated = x.DateCreated,
                                           OrderGuid = x.OrderGuid,
                                           Amount = x.Amount,
                                           Status = x.Status,
                                           InvoiceDate = x.InvoiceDate,
                                           DueDate = x.DueDate,
                                           CustomerId = x.CustomerId,
                                           CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                           CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                           DebitNoteNo = context.tblDebitNotes.Where(y => y.InvoiceNo == x.InvoiceNo).Select(y => y.DebitNoteNo).FirstOrDefault(),
                                           TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                           PackingCharges = (x.Packing != null ? (decimal)x.Packing : 0),
                                           ForwardingCharges = (x.Forwading != null ? (decimal)x.Forwading : 0),
                                       }).OrderByDescending(x => x.DateCreated)/*.Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();*/
                                        .Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();

                }
                else if (fdate != null && tdate != null && vm.SelectedCustomer != 0 && vm.InvoiceNo != null)
                {
                    result = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false
                                       && vm.fDate == null && vm.tDate == null
                                       && x.InvoiceNo == vm.InvoiceNo && x.CustomerId == vm.SelectedCustomer
                                       && x.DateCreated >= fdate && x.DateCreated <= tdate).
                                                           Select(x => new InvoiceDto
                                                           {
                                                               InvoiceId = x.InvoiceId,
                                                               InvoiceNo = x.InvoiceNo,
                                                               DateCreated = x.DateCreated,
                                                               OrderGuid = x.OrderGuid,
                                                               Amount = x.Amount,
                                                               Status = x.Status,
                                                               InvoiceDate = x.InvoiceDate,
                                                               DueDate = x.DueDate,
                                                               CustomerId = x.CustomerId,
                                                               CustomerName = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                                                               CustomerEmail = context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.Email).FirstOrDefault(),
                                                               DebitNoteNo = context.tblDebitNotes.Where(y => y.InvoiceNo == x.InvoiceNo).Select(y => y.DebitNoteNo).FirstOrDefault(),
                                                               TrackingNo = context.tblOrders.Where(y => y.OrderGUID == x.OrderGuid).Select(y => y.TrackingNumber).FirstOrDefault(),
                                                               PackingCharges = (x.Packing != null ? (decimal)x.Packing : 0),
                                                               ForwardingCharges = (x.Forwading != null ? (decimal)x.Forwading : 0),
                                                           }).OrderByDescending(x => x.DateCreated)
                                                            .Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList();
                }


                /* result.RemoveAll(x => x.InvoiceNo.Contains("AC1"));
                 result.RemoveAll(x => x.InvoiceNo.Contains("AC"));*/

                result.RemoveAll(x => x.InvoiceNo.Contains(ConstantsHelper.InvoiceType.AC1.ToString()));
                result.RemoveAll(x => x.InvoiceNo.Contains(ConstantsHelper.InvoiceType.AC.ToString()));

                var cr = context.tblCreditNotes.Where(x => x.DelInd == false && x.Cancelled==false).
                                        Select(x => new CreditNoteModel
                                        {
                                            CreditNoteNo = x.CreditNoteNo,
                                            CreditNoteId = x.CreditNoteId,
                                            InvoiceNumber = x.InvoiceNumber,
                                            OrderGuid = x.OrderGuid,
                                            TotalAmount = x.TotalAmount
                                        }).ToList();

                foreach (var r in result)
                {
                    foreach (var cnr in cr)
                    {
                        if (r.OrderGuid == cnr.OrderGuid && r.InvoiceNo == cnr.InvoiceNumber)
                        {
                            r.creditNote = cnr.CreditNoteNo;
                            r.CreditNoteAmount = cnr.TotalAmount ?? 0;
                        }
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //End here

        public List<ProformaInvoiceDto> GetProformaInvoiceList(int PageSize = 10, int PageNo = 1)
        {
            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            var st = (from a in Context.tblProformaInvoices
                      join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                      join c in Context.tblOrders on a.OrderGuid equals c.OrderGUID
                      where a.DelInd == false && c.DelInd == false
                      select new ProformaInvoiceDto
                      {
                          ProformaInvoiceId = a.ProformaInvoiceId,
                          ProformaInvoiceNo = a.ProformaInvoiceNo,
                          DateCreated = a.DateCreated,
                          Amount = a.Amount,
                          ProformaInvoiceDate = a.ProformaInvoiceDate,
                          CustomerName = b.CustomerName,
                          CustomerId = b.CustomerId,
                          Zone = b.Zone,
                          OrderGuid = a.OrderGuid,
                          TrackingNumber = c.TrackingNumber,
                      }).OrderByDescending(x => x.ProformaInvoiceId).Skip((PageNo - 1) * PageSize).Take(PageSize).ToList();


            var invoiceList = (from a in Context.tblInvoices
                      join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                      join c in Context.tblOrders on a.OrderGuid equals c.OrderGUID
                      where a.DelInd == false && c.DelInd == false && a.Cancelled ==false
                      select new InvoiceDto
                      {
                          InvoiceId = a.InvoiceId,
                          CustomerId = b.CustomerId,
                          OrderGuid = a.OrderGuid,
                      }).ToList();


           var piList = st.Where(f => !invoiceList.Any(t => t.OrderGuid == f.OrderGuid)).ToList();

            return piList;
        }

        public OrderPaymentDto GetPaymentDetails(Guid id)
        {
            var d = (from a in Context.tblOrderPayments
                     where a.OrderGUID == id
                     select new OrderPaymentDto
                     {
                         DatePaid = (DateTime)a.DatePaid,
                         PaymentDetails = a.PaymentDetails,
                         PaymentType = a.PaymentType
                     }).FirstOrDefault();
            return d;
        }

        public InvoiceDto GetInvoiceDetailsbyOrderId(Guid id)
        {
            var i = (from a in Context.tblInvoices
                     where a.OrderGuid == id && a.DelInd == false && a.Cancelled == false
                     select new InvoiceDto
                     {
                         InvoiceDate = a.InvoiceDate,
                         InvoiceNo = a.InvoiceNo,
                         InvoiceFileName = a.InvoiceFileName
                     }).FirstOrDefault();
            return i;
        }


        public CreditNoteModel GetCreditNoteDetailbyOrderId(Guid id)
        {
            var i = (from creditNote in Context.tblCreditNotes
                     where creditNote.OrderGuid == id && creditNote.DelInd == false 
                     && creditNote.Cancelled == false
                     select new CreditNoteModel
                     {
                         CreditNoteDate = creditNote.CreditNoteDate,
                         CreditNoteNo = creditNote.CreditNoteNo,
                         CreditNoteFileName = creditNote.CreditNoteFileName
                     }).FirstOrDefault();
            return i;
        }

        public InvoiceDto GetProformaInvoiceDetail(Guid id)
        {
            var i = (from a in Context.tblProformaInvoices
                     where a.OrderGuid == id && a.DelInd == false
                     select new InvoiceDto
                     {
                         InvoiceDate = a.ProformaInvoiceDate,
                         InvoiceNo = a.ProformaInvoiceNo,
                         InvoiceFileName = a.FileName
                     }).FirstOrDefault();
            return i;
        }
        public InvoiceDto GetInvoiceDetailsbyOrderIdV1(Guid id)
        {
            var i = (from a in Context.tblInvoices
                     join b in Context.tblOrders on a.OrderGuid equals b.OrderGUID
                     where a.OrderGuid == id && a.DelInd == false && a.Cancelled == false && b.DelInd == false
                     select new InvoiceDto
                     {
                         InvoiceDate = a.InvoiceDate,
                         InvoiceNo = a.InvoiceNo,
                         InvoiceFileName = a.InvoiceFileName,
                         RepairType = b.RepairType,
                         CustomerId = b.CustomerId,
                         OrderGuid = b.OrderGUID

                     }).FirstOrDefault();
            return i;
        }


        public InvoiceDto GetCancelledInvoiceDetailsbyOrderId(Guid id, int InvoiceId, string invoiceNo)
        {
            InvoiceDto invoiceDto = new InvoiceDto();
            try
            {
                invoiceDto = (from a in Context.tblInvoices
                              join b in Context.tblOrders on a.OrderGuid equals b.OrderGUID
                              where a.OrderGuid == id && a.InvoiceNo == invoiceNo
                              && a.DelInd == false
                              && a.Cancelled == true
                              && b.DelInd == false
                              && a.InvoiceId == InvoiceId
                              select new InvoiceDto
                              {
                                  InvoiceId = a.InvoiceId,
                                  InvoiceDate = a.InvoiceDate,
                                  InvoiceNo = a.InvoiceNo,
                                  InvoiceFileName = a.InvoiceFileName,
                                  RepairType = b.RepairType,
                                  CustomerId = b.CustomerId,
                                  OrderGuid = b.OrderGUID,

                              }).FirstOrDefault();

            }
            catch (Exception)
            {

                //throw;
            }

            return invoiceDto;
        }
        public List<ProformaInvoiceDto> GetProformaList(int PageSize = 10, int PageNo = 1)
        {
            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            var st = (from a in Context.tblProformaInvoices
                      join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                      join c in Context.tblOrders on a.OrderGuid equals c.OrderGUID
                      where a.DelInd == false
                      select new ProformaInvoiceDto
                      {
                          ProformaInvoiceNo = a.ProformaInvoiceNo,
                          DateCreated = a.DateCreated,
                          OrderGuid = a.OrderGuid,
                          Amount = a.Amount,
                          ProformaInvoiceDate = a.ProformaInvoiceDate,
                          CustomerName = b.CustomerName,
                          TrackingNumber = c.TrackingNumber

                      }).OrderByDescending(x => x.ProformaInvoiceNo).Skip((PageNo - 1) * PageSize).Take(PageSize).OrderByDescending(x => x.ProformaInvoiceNo).ToList();
            return st;
        }
        public List<ProformaInvoiceDto> ExcelForPIList()
        {
            var s = (from a in Context.tblProformaInvoices
                     join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                     where (a.DelInd == false)
                     select new ProformaInvoiceDto
                     {
                         OrderGuid = a.OrderGuid,
                         ProformaInvoiceId = a.ProformaInvoiceId,
                         ProformaInvoiceNo = a.ProformaInvoiceNo,
                         CustomerName = b.CustomerName,
                         DateCreated = a.DateCreated,
                         ProformaInvoiceDate = a.ProformaInvoiceDate,
                         FromDate = a.DateCreated,
                         Zone = b.Zone,
                         FDate = SqlFunctions.DateName("day", a.DateCreated) + "/" + SqlFunctions.DateName("month", a.DateCreated) + "/" + SqlFunctions.DateName("year", a.DateCreated),
                         ToDate = (DateTime)a.DateCreated,
                         TDate = SqlFunctions.DateName("day", a.DateCreated) + "/" + SqlFunctions.DateName("month", a.DateCreated) + "/" + SqlFunctions.DateName("year", a.DateCreated),
                         Amount = a.Amount,

                     }).OrderByDescending(x => x.ProformaInvoiceId).ToList();

            var invoiceList = (from a in Context.tblInvoices
                               join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                               join c in Context.tblOrders on a.OrderGuid equals c.OrderGUID
                               where a.DelInd == false && c.DelInd == false && a.Cancelled==false
                               select new InvoiceDto
                               {
                                   InvoiceId = a.InvoiceId,
                                   CustomerId = b.CustomerId,
                                   OrderGuid = a.OrderGuid,
                               }).OrderByDescending(x => x.InvoiceId).ToList();


            var downloadPiList = s.Where(f => !invoiceList.Any(t => t.OrderGuid == f.OrderGuid)).ToList();

            return downloadPiList;
        }

        public List<ProformaInvoiceDto> GetPIList(ProformaViewModel svm)
        {
            svm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            List<ProformaInvoiceDto> piList = new List<ProformaInvoiceDto>();

            piList = (from a in Context.tblProformaInvoices
                      join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                      join c in Context.tblOrders on a.OrderGuid equals c.OrderGUID
                      where a.DelInd == false
                      select new ProformaInvoiceDto
                      {
                          ProformaInvoiceId = a.ProformaInvoiceId,
                          ProformaInvoiceNo = a.ProformaInvoiceNo,
                          OrderGuid = a.OrderGuid,
                          CustomerId = b.CustomerId,
                          DateCreated = a.DateCreated,
                          Amount = a.Amount,
                          ProformaInvoiceDate = a.ProformaInvoiceDate,
                          CustomerName = b.CustomerName,
                          Zone = b.Zone,
                          TrackingNumber = c.TrackingNumber,
                      }).OrderByDescending(x => x.ProformaInvoiceId).ToList();

            var invoiceList = (from a in Context.tblInvoices
                               join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                               join c in Context.tblOrders on a.OrderGuid equals c.OrderGUID
                               where a.DelInd == false && c.DelInd == false
                               select new InvoiceDto
                               {
                                   InvoiceId = a.InvoiceId,
                                   CustomerId = b.CustomerId,
                                   OrderGuid = a.OrderGuid,
                               }).ToList();


            var searchPiList = piList.Where(f => !invoiceList.Any(t => t.OrderGuid == f.OrderGuid)).ToList();

            return searchPiList;

        }

        #endregion

        #region Overrides
        public override void Add(tblInvoice ch)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(ch);
            }
        }

        #endregion
    }
}