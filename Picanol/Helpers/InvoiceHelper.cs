//Billing and Shipping Details Address Savein tblProvisionallBill_Janesh_21012025

using NLog;
using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.Utils;
using Picanol.ViewModels;
using Picanol.ViewModels.EInvoiceModel.IRNModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.Helpers
{

    public class InvoiceHelper
    {
        private readonly OrderHelper _orderHelper;
        private readonly CustomerHelper _customerHelper;
        PicannolEntities _context = new PicannolEntities();
       //OrderHelper _orderHelper =  new OrderHelper();
        EInvoiceHelper _eInvoiceHelper = new EInvoiceHelper();
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        RespPlGenIRNDec respPlGenIRNDec = new RespPlGenIRNDec();
        Controller mController;

        public InvoiceHelper()
        {
            _orderHelper = new OrderHelper(this);
            _customerHelper = new CustomerHelper(this);
        }


        private InvoiceService _invoiceService;
        protected InvoiceService InvoiceService
        {
            get
            {
                if (_invoiceService == null) _invoiceService = new InvoiceService(entities, validationDictionary);
                return _invoiceService;
            }
        }

        protected iValidation validationDictionary { get; set; }
        #endregion

        #region Ctor
        public InvoiceHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            mController = controller;
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }
        #endregion

        #region Base Methods
        public List<InvoiceDto> GetInvoicesList(InvoiceListViewModel vm)
        {


            return InvoiceService.GetFilteredInvoicesList(vm);
        }


        public List<InvoiceDto> GetCancellInvoiceList(InvoiceListViewModel vm)
        {

            return InvoiceService.GetCancelledInvoiceList(vm);
        }


        //New invoice 170721
        public List<InvoiceDto> GetInvoicesListVersion1(InvoiceListViewModel vm)
        {
            InvoiceItemDto componentCostItem = new InvoiceItemDto();
            InvoiceItemDto labourCostItem = new InvoiceItemDto();
            List<OrderPartDto> parts =new  List<OrderPartDto>();
            CustomerDto customerDetails = new CustomerDto();
            List<InvoiceDto> listt = new List<InvoiceDto>();

              listt = InvoiceService.GetFilteredInvoicesListVersion1(vm);

            foreach( var item in listt)
            {
                if (item != null)
                {
                    var OrderDetails = _context.tblOrders.Where(x => x.OrderGUID == item.OrderGuid).
                                      Select(x => new OrderDto
                                      {
                                          TimeTaken = x.RepairTime,
                                          Discount = x.Discount,
                                          DateCreated = x.DateCreated,
                                          OrderGUID = x.OrderGUID,
                                          PackingType = x.PackingType,
                                          ExemptComponentCost = (x.ExemptComponentCost != null) ? (bool)x.ExemptComponentCost : false,
                                          ExemptLabourCost = (x.ExemptLabourCost != null) ? (bool)x.ExemptLabourCost : false,
                                      }).ToList();


                    var PackingDetails = _context.tblCustomers.Where(x => x.CustomerId == item.CustomerId).
                                  Select(x => new CustomerDto
                                  {
                                      SmallPacking = x.SmallPacking,
                                      SmallForwarding = x.SmallForwarding,
                                      BigPacking = x.BigPacking,
                                      BigForwarding = x.BigForwarding

                                  }).ToList();

                    if (OrderDetails.Count!=0)
                    {
                        string selectAction = "";
                        foreach (var o in OrderDetails)
                        {
                            componentCostItem = GetComponentsCost(parts, o.Discount, o.DateCreated, o.OrderGUID,selectAction);
                            labourCostItem = GetLabourCost(customerDetails.RepairCharges, o.TimeTaken, o.Discount, o.DateCreated, o.OrderGUID, selectAction);

                            item.RepairCharges = labourCostItem.RepairCharges;
                            item.LabourCost = labourCostItem.Amount;
                            item.ComponentsCost = componentCostItem.Amount;
                            
                            var i = _context.tblInvoices.Where(x => x.OrderGuid == item.OrderGuid && (x.InvoiceNo != "" || x.InvoiceNo != null) && x.DelInd == false && x.Cancelled == false).FirstOrDefault();

                            if (i.Packing == null)
                            {
                                if (o.PackingType == PackingTypes.Small.ToString())
                                {
                                   item.PackingCharges = (decimal)PackingDetails.Select(x=>x.SmallPacking).FirstOrDefault();
                                   item.ForwardingCharges = (decimal)PackingDetails.Select(x => x.SmallForwarding).FirstOrDefault();

                                }
                                else if (o.PackingType == PackingTypes.Big.ToString())
                                {
                                    item.PackingCharges = (decimal)PackingDetails.Select(x => x.BigPacking).FirstOrDefault();
                                    item.ForwardingCharges = (decimal)PackingDetails.Select(x => x.BigForwarding).FirstOrDefault();
                                }

                            }
                        }

                    }

                }
              
            }

             return listt;
        }

        public string cancelECreditNote(Guid orderId)
        {
            string response = "";
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            PicannolEntities context = new PicannolEntities();
            RespPlCancelIRN respPlCancelIRN = new RespPlCancelIRN();

            if (context.tblEInvoices.Any(x => x.OrderID == orderId && x.DelInd == false 
             && (x.Cancelled == false || x.Cancelled == null)))
            {
                var invoiceType = ConfigurationManager.AppSettings["CreditNote"];
                var eInv = context.tblEInvoices.Where(x => x.OrderID == orderId && x.DelInd == false 
                                                        && x.Cancelled == false &&
                                                        x.Type == invoiceType).FirstOrDefault();
                respPlCancelIRN = _eInvoiceHelper.CancelEInvoice(eInv);

                if (respPlCancelIRN.CancelDate != null || respPlCancelIRN.Irn != null)
                {
                    if (context.tblCreditNotes.Any(x => x.OrderGuid == orderId && x.DelInd == false 
                                                    && x.Cancelled == false))
                    {
                        var inv = context.tblCreditNotes.Where(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                        inv.Cancelled = true;
                        inv.CancelledDate = DateTime.Now;
                        inv.CancelledBy = userInfo.UserId;
                        context.tblCreditNotes.Attach(inv);
                        context.Entry(inv).State = EntityState.Modified;
                        context.SaveChanges();

                        //record user activity
                        string ActionName = $"CencelCreditNote - {orderId}";
                        string TableName = "tblCreditNote, tblEinvoce";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End

                        response = "E-Credit note cancelled";
                    }
                    else if (context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == true))
                        response = "Invoice already cancelled";
                    else
                        response = "Invoice doesn't exist";
                    return response;
                }
                else
                {
                    if (respPlCancelIRN.ErrorDetails != null)
                    {
                        response = respPlCancelIRN.ErrorDetails[0].ErrorMessage.ToString();
                    }
                    else
                    {
                        response = respPlCancelIRN.errorMsg.ToString();
                    }
                    return response;
                }

            }

            return response;
        }

        public List<ProformaInvoiceDto> GetSearchListProformaInvoice(ProformaViewModel svm)
        {
            try
            {
                List<ProformaInvoiceDto> srList = InvoiceService.GetPIList(svm);

                DateTime enddate = svm.EndDate;
                TimeSpan ts = new TimeSpan(11, 59, 0);
                enddate = enddate.Date + ts;

                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                {
                    srList = srList.Where(x => x.ProformaInvoiceDate >= svm.StartDate && x.ProformaInvoiceDate <= svm.EndDate).ToList();
                    //srList = srList.Where(x => x.ProformaInvoiceDate >= svm.StartDate && x.ProformaInvoiceDate <= enddate).ToList();

                }
                else if (svm.ProformaInvoiceNo != null && svm.ProformaInvoiceNo != "" && svm.ProformaInvoiceNo != "0")
                {
                    srList = srList.Where(x => x.ProformaInvoiceNo == svm.ProformaInvoiceNo).ToList();

                }else if(svm.CustomerId!=0)
                {
                    srList = srList.Where(x => x.CustomerId == svm.CustomerId).ToList();
                }
                srList = srList.Skip((svm.PageNo - 1) * svm.PageSize).Take(svm.PageSize).ToList();
                return srList;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceRequestList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        #region get Proforma Invoice List
        public List<ProformaInvoiceDto> GetProformaInvoiceList(ProformaViewModel model, int PageSize, int PageNo)
        {
            List<ProformaInvoiceDto> srList = InvoiceService.GetProformaInvoiceList(PageSize, PageNo);

            try
            {
                if (model.CustomerId != null & model.CustomerId != 0)
                {
                    srList = srList.Where(x => x.CustomerId == model.CustomerId).ToList();
                }
                if (model.ProformaInvoiceNo != null)
                {
                    srList = srList.Where(x => x.ProformaInvoiceNo.Contains(model.ProformaInvoiceNo)).ToList();

                }
                if (model.StartDate != null && model.StartDate != DateTime.MinValue)
                {
                    srList = srList.Where(x => x.TillFromDate >= model.StartDate).ToList();
                }
                if (model.CustomerId != 0)
                {
                    srList = srList.Where(x => x.CustomerId == model.CustomerId).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetProformaInvoiceList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
            return srList;

        }
        #endregion


        #region Save Credit note
        public string SaveCreditNotes(OrderDto order, InvoiceDto invoice, CustomerDto customer, string selectedAction, string invoiceNumber, List<InvoiceItemDto> items)
        {
            PicannolEntities context = new PicannolEntities();
            string newInvNo = "";
            string lastInvoiceNo = "";
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var roleId = ConfigurationManager.AppSettings["AllowRoleIdForEInvoice"];

            string invType = ConstantsHelper.InvoiceType.CN.ToString();
            lastInvoiceNo = GetLastInvoiceNo(invType);
            if (lastInvoiceNo != null && lastInvoiceNo != "")
            {
                newInvNo = GetNewInvoiceNumber(lastInvoiceNo, invType);
            }
            else
            {
                newInvNo = GetFirstInvoiceNo(invType);
            }
            var cred = new tblCreditNote();
            cred.OrderGuid = order.OrderGUID;
            cred.InvoiceNumber = invoiceNumber;
            cred.CreditNoteDate = DateTime.Now;
            cred.TaxableAmount = invoice.Amount;
            cred.CustomerId = order.CustomerId;
            cred.TaxAmount = invoice.Amount;
            cred.Createdby = UserInfo.UserId;
            cred.TotalAmount = invoice.Amount;
            cred.ComponentCost = invoice.ComponentsCost;
            cred.LabourCost = invoice.LabourCost;
            cred.PackagingForwarding = invoice.ForwardingCharges + invoice.PackingCharges;
            //cred.ComponenTCost

            cred.DelInd = false;
            cred.CreditNoteFileName = customer.CustomerName + "_" + order.TrackingNumber + "_" + ConstantsHelper.InvoiceType.CN.ToString() + ".pdf";
            var result = context.tblCreditNotes.Where(x => x.OrderGuid == order.OrderGUID 
            && x.InvoiceNumber == invoiceNumber && x.DelInd==false && x.Cancelled==false).FirstOrDefault();
            if (result != null)
            {
                //fetch credit note detail

                cred.CreditNoteNo = result.CreditNoteNo;
                newInvNo = cred.CreditNoteNo;

                //End
            }
            else
            {
                //Generate new Credit note

                if (selectedAction != ConstantsHelper.Actions.PrintJobSheet.ToString()
                    && selectedAction != ConstantsHelper.Actions.ReturnLoan.ToString())
                {
                 
                    var invoiceDtl = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID && (x.InvoiceNo != "" || x.InvoiceNo != null) 
                     && x.DelInd == false && x.Cancelled == false).FirstOrDefault();

                    var createCreditNotes = (CreditNoteModel)HttpContext.Current.Session["ReEditableCreditNotes"];

                    if (roleId.Contains(UserInfo.RoleId.ToString()))
                    {
                        if (order.RepairType == ConstantsHelper.RepairType.Chargeable.ToString()
                          || order.RepairType == ConstantsHelper.RepairType.NoRepairWarranty.ToString())
                        {

                            if(createCreditNotes.partialAmount==true && invoiceDtl.Amount != invoice.Amount 
                                || createCreditNotes.partialAmount == false && invoiceDtl.Amount == invoice.Amount)
                            {
                                invoice.InvoiceNo = newInvNo;
                                respPlGenIRNDec = _eInvoiceHelper.GenerateIRN(customer, invoice, items, order);
                                if (respPlGenIRNDec.Irn != null)
                                {
                                    cred.CreditNoteNo = newInvNo;
                                    context.tblCreditNotes.Add(cred);
                                    context.SaveChanges();

                                    //record user activity
                                    string ActionName = $"E-CRN created, CRN No - {newInvNo}"
                                    + $", OrderGUID - {order.OrderGUID}";
                                    string TableName = "tblCrediNote, tblEInvoice";
                                    if (ActionName != null)
                                    {
                                        new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                                    }
                                    //End
                                }
                                else
                                {
                                    if (respPlGenIRNDec.ErrorDetails != null)
                                    {
                                        return invoice.ErrorMessage = respPlGenIRNDec.ErrorDetails[0].ErrorMessage.ToString(); ;
                                    }
                                    else
                                    {
                                        return invoice.ErrorMessage = respPlGenIRNDec.errorMsg; ;
                                    }

                                }
                            
                            }
                            else
                            {
                                return invoice.ErrorMessage = "Credit note invoice has changed. Please check the invoice detal.";
                            }

                            

                        }
                        else
                        {
                            return invoice.ErrorMessage = "Repair Type should be chargeable"; ;
                        }
                    }
                    else
                    {
                        return invoice.ErrorMessage = "You are no allowed to genertae E-Credit Note. Please contact your admin";
                    }
                        
                }
                //End
            }

            return newInvNo;
        }
        #endregion


        #region save debit note
        public string SaveDebitNotes(OrderDto order, InvoiceDto invoice, CustomerDto customer, string selectedAction, string invoiceNumber, List<InvoiceItemDto> items)
        {
            PicannolEntities context = new PicannolEntities();
            string newInvNo = "";
            string lastInvoiceNo = "";
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var roleId = ConfigurationManager.AppSettings["AllowRoleIdForEInvoice"];
            var creditNoteDtl = context.tblCreditNotes.Where(x => x.OrderGuid == order.OrderGUID 
            && x.InvoiceNumber == invoiceNumber 
            && x.DelInd == false && x.Cancelled == false).FirstOrDefault();

            string invType = ConstantsHelper.InvoiceType.DN.ToString();
            lastInvoiceNo = GetLastInvoiceNo(invType);
            if (lastInvoiceNo != null && lastInvoiceNo != "")
            {
                newInvNo = GetNewInvoiceNumber(lastInvoiceNo, invType);
            }
            else
            {
                newInvNo = GetFirstInvoiceNo(invType);
            }
            var debitNote = new tblDebitNote();
            debitNote.OrderGuid = order.OrderGUID;
            debitNote.InvoiceNo = invoiceNumber;
            debitNote.DateCreated = DateTime.Now;
            debitNote.DebitNoteDate = DateTime.Now.Date;
            debitNote.TotalAmount = invoice.Amount;
            debitNote.CustomerId = order.CustomerId;
            debitNote.Amount = invoice.Amount;
            debitNote.UserId = UserInfo.UserId;
            debitNote.TotalAmount = invoice.Amount;
            debitNote.DelInd = false;

            debitNote.DebitNoteFileName = customer.CustomerName + "_" + order.TrackingNumber + "_" + ConstantsHelper.InvoiceType.CN.ToString() + ".pdf";
            var result = context.tblDebitNotes.Where(x => x.OrderGuid == order.OrderGUID && x.InvoiceNo == invoiceNumber && x.DelInd == false).FirstOrDefault();

            
            if (result != null)
            {
                //fetch debit note detail if debit is already generated

                debitNote.DebitNoteNo = result.DebitNoteNo;
                newInvNo = debitNote.DebitNoteNo;
                
                //End
            }
            else
            {
                //Generate new Debit note 
                if (selectedAction != ConstantsHelper.Actions.PrintJobSheet.ToString()
                    && selectedAction != ConstantsHelper.Actions.ReturnLoan.ToString())
                {
                    var crn = context.tblCreditNotes.Where(x => x.OrderGuid == order.OrderGUID 
                    && x.DelInd == false && x.Cancelled==false).FirstOrDefault();
                    if (crn.TotalAmount == invoice.Amount)
                    {
                        if (roleId.Contains(UserInfo.RoleId.ToString()))
                        {
                            if (order.RepairType == ConstantsHelper.RepairType.Chargeable.ToString()
                              || order.RepairType == ConstantsHelper.RepairType.NoRepairWarranty.ToString())
                            {
                                invoice.InvoiceNo = newInvNo;
                                respPlGenIRNDec = _eInvoiceHelper.GenerateIRN(customer, invoice, items, order);
                                if (respPlGenIRNDec.Irn != null)
                                {
                                    //debitNote.CreditNoteNo = "";
                                    debitNote.CreditNoteNo = creditNoteDtl.CreditNoteNo;
                                    debitNote.DebitNoteNo = newInvNo;
                                    context.tblDebitNotes.Add(debitNote);
                                    context.SaveChanges();

                                    //record user activity
                                    string ActionName = $"E-DBN created, DebitNote No - {newInvNo}"
                                    + $", OrderGUID - {order.OrderGUID}";
                                    string TableName = "tblDebitNote, tblEInvoice";
                                    if (ActionName != null)
                                    {
                                        new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                                    }
                                    //End
                                }
                                else
                                {
                                    if (respPlGenIRNDec.ErrorDetails != null)
                                    {
                                        return invoice.ErrorMessage = respPlGenIRNDec.ErrorDetails[0].ErrorMessage.ToString(); ;
                                    }
                                    else
                                    {
                                        return invoice.ErrorMessage = respPlGenIRNDec.errorMsg; ;
                                    }

                                }

                            }
                            else
                            {
                                return invoice.ErrorMessage = "Repair Type should be chargeable"; ;
                            }
                        }
                        else
                        {
                            return invoice.ErrorMessage = "You are no allowed to genertae E-Credit Note. Please contact your admin";
                        }
                    }
                    else
                    {
                        return invoice.ErrorMessage = "Debit note invoice has changed please check invoice amount.";
                    }

                    
                }
                //End
            }

            return newInvNo;
        }


        #endregion


        #region Save new Inovoice
        public string SaveNewInvoice(OrderDto order, decimal amount, string customerName, string selectedAction, InvoiceDto invoice, CustomerDto customer, List<InvoiceItemDto> items)
        {
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var roleId = ConfigurationManager.AppSettings["AllowRoleIdForEInvoice"];

            string newInvNo = "";

            try
            {
                PicannolEntities context = new PicannolEntities();
                if (context.tblInvoices.Any(x => x.OrderGuid == order.OrderGUID && x.Cancelled == false) && selectedAction != ConstantsHelper.Actions.PerformaInvoice.ToString())
                {

                    newInvNo = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false && x.Cancelled == false).Select(x => x.InvoiceNo).FirstOrDefault();
                }
                else
                {
                    string lastInvoiceNo = "";
                    string invType = "";
                    if (order.RepairType == ConstantsHelper.RepairType.FOC.ToString())
                        invType = ConstantsHelper.InvoiceType.FOC.ToString();
                    else if (order.RepairType == ConstantsHelper.RepairType.UnRepairedBoards.ToString())
                        invType = ConstantsHelper.InvoiceType.URD.ToString();
                    else if (order.RepairType == ConstantsHelper.RepairType.RepairWarranty.ToString())
                        invType = ConstantsHelper.InvoiceType.RW.ToString();
                    else if (order.RepairType == ConstantsHelper.RepairType.Loan.ToString())
                        invType = ConstantsHelper.InvoiceType.LN.ToString();
                    else if (selectedAction == ConstantsHelper.Actions.PerformaInvoice.ToString())
                        invType = ConstantsHelper.InvoiceType.PI.ToString();
                    else
                        invType = ConstantsHelper.InvoiceType.RP.ToString();

                    lastInvoiceNo = GetLastInvoiceNo(invType);

                    if (lastInvoiceNo != null && lastInvoiceNo != "")
                    {
                        char[] splitChar = { '/' };
                        string[] strArray = lastInvoiceNo.Split(splitChar);
                        int month = DateTime.Now.Month;
                        var year = DateTime.Now.ToString("yy");
                        int yy = Convert.ToInt32(year);
                        var currYear = year + Convert.ToString(yy + 1);
                        var prevYear = Convert.ToString(yy - 1) + year;
                        if (strArray[0].Contains("DOC"))
                        {
                            strArray[0] = strArray[1];
                            strArray[1] = strArray[2];
                            strArray[2] = strArray[3];
                        }

                        if ((strArray[1] == prevYear && month < 4) || strArray[1] == currYear)
                        {
                            int i = Convert.ToInt32(strArray[0]);
                            i = i + 1;
                            //newInvNo = Convert.ToString(i).PadLeft(4, '0') + "/" + strArray[1] + "/" + strArray[2];
                            newInvNo = Convert.ToString(i) + "/" + strArray[1] + "/" + strArray[2];

                        }
                        else if (strArray[1] == prevYear && month == 4)
                        {
                            newInvNo = GetFirstInvoiceNo(invType);
                        }
                        if ((strArray[1] == prevYear && month > 4) || strArray[1] == currYear)
                        {
                            int i = Convert.ToInt32(strArray[0]);
                            i = i + 1;
                            //newInvNo = Convert.ToString(i).PadLeft(4, '0') + "/" + strArray[1] + "/" + strArray[2];
                            newInvNo = Convert.ToString(i) + "/" + strArray[1] + "/" + strArray[2];
                        }
                    }
                    else
                    {
                        newInvNo = GetFirstInvoiceNo(invType);


                    }
                    if (selectedAction == ConstantsHelper.Actions.PerformaInvoice.ToString())
                    {

                        tblProformaInvoice proformaInvoice = new tblProformaInvoice();
                        proformaInvoice.DateCreated = DateTime.Now;
                        proformaInvoice.Amount = amount;
                        proformaInvoice.CustomerId = order.CustomerId;
                        proformaInvoice.OrderGuid = order.OrderGUID;
                        proformaInvoice.ProformaInvoiceDate = DateTime.Now.Date;
                        proformaInvoice.Status = 1;
                        proformaInvoice.CreatedBy = UserInfo.UserId;
                        proformaInvoice.PackingType = order.PackingType;
                        proformaInvoice.DelInd = false;
                        proformaInvoice.Packing = (invoice.PackingCharges != null) ? invoice.PackingCharges : 0;
                        proformaInvoice.Forwading = (invoice.ForwardingCharges != null) ? invoice.ForwardingCharges : 0;
                        //ti.RepairCharge = (invoice.LabourCost != null) ? invoice.LabourCost : 0;
                        proformaInvoice.ComponentCost = (invoice.ComponentsCost != null) ?
                            Math.Round(invoice.ComponentsCost) : 0;

                        if (invoice.InvoiceItems != null)
                        {
                            if (invoice.InvoiceItems.Count != 0)
                            {
                                if (invoice.InvoiceItems.Count > 1)
                                {
                                    proformaInvoice.RepairCharge = invoice.InvoiceItems[1].RepairCharges;
                                }
                                else
                                {
                                    proformaInvoice.RepairCharge = invoice.InvoiceItems[0].RepairCharges;
                                }
                            }
                        }
                        proformaInvoice.FileName = customerName + "_" + order.TrackingNumber + "_" + invType + ".pdf";

                        var result = context.tblProformaInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.ProformaInvoiceNo == lastInvoiceNo && x.DelInd == false).FirstOrDefault();
                        if (result != null)
                        {
                            proformaInvoice.ProformaInvoiceNo = result.ProformaInvoiceNo;
                            newInvNo = proformaInvoice.ProformaInvoiceNo;
                        }
                        else
                        {
                            proformaInvoice.ProformaInvoiceNo = newInvNo;
                        }
                        //Code By Janesh_15012025
                        //Saving Addresss in Invoice
                        proformaInvoice.BillingAddress = customer.AddressLine1 + "," + customer.AddressLine2 + "," + customer.District + "," + customer.State + "," + customer.PIN;
                        proformaInvoice.StateCode = customer.StateCode;
                        proformaInvoice.ShippingAddress = customer.ShippingAddressLine1 + "," + customer.ShippingAddressLine2 + "," + customer.ShippingDistrict + "," + customer.ShippingPIN;
                        proformaInvoice.ShippingStateCode = customer.ShippingStateCode;
                        context.tblProformaInvoices.Add(proformaInvoice);
                        context.SaveChanges();

                        //record user activity
                        string ActionName = $"PI created, PI No - {newInvNo}"
                        + $"OrderGUID - {order.OrderGUID}";
                        string TableName = "tblProformaInvoice";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        tblInvoice ti = new tblInvoice();
                        ti.InvoiceNo = newInvNo;

                        
                        
                        ti.DateCreated = DateTime.Now;
                        ti.Amount = amount;
                        ti.CustomerId = order.CustomerId;
                        ti.OrderGuid = order.OrderGUID;
                        ti.InvoiceDate = DateTime.Now.Date;
                        ti.Status = 1;
                        ti.GeneratedByUserId = UserInfo.UserId;
                        ti.Cancelled = false;
                        ti.DelInd = false;
                        ti.ComponentCost = Math.Round(invoice.ComponentsCost);
                        ti.Forwading = invoice.ForwardingCharges;
                        ti.Packing = invoice.PackingCharges;

                        if (invoice.InvoiceItems != null)
                        {
                            if (invoice.InvoiceItems.Count != 0)
                            {
                                if (invoice.InvoiceItems.Count > 1)
                                {
                                    ti.RepairCharge = invoice.InvoiceItems[1].RepairCharges;
                                }
                                else
                                {
                                    ti.RepairCharge = invoice.InvoiceItems[0].RepairCharges;
                                }
                            }
                        }
                        ti.InvoiceFileName = customerName + "_" + order.TrackingNumber + "_" + invType + ".pdf";
                        invoice.InvoiceDate = DateTime.Now;
                        invoice.InvoiceNo = newInvNo;
                        

                        if (selectedAction != Actions.PrintJobSheet.ToString()
                            && selectedAction != Actions.ReturnLoan.ToString())
                        {
                            if (order.RepairType == ConstantsHelper.RepairType.Chargeable.ToString()
                                   || order.RepairType == ConstantsHelper.RepairType.NoRepairWarranty.ToString())
                            {
                                if (roleId.Contains(UserInfo.RoleId.ToString()))
                                {
                                    var pInvoice = _context.tblProformaInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false).FirstOrDefault();
                                    if (pInvoice != null)
                                    {
                                        if (pInvoice.Amount == invoice.Amount)
                                        {
                                            respPlGenIRNDec = _eInvoiceHelper.GenerateIRN(customer, invoice, items, order);
                                            if (respPlGenIRNDec.Irn != null)
                                            {
                                                invoice.EInVoiceQrCode = respPlGenIRNDec.EInvoiceQrCodeImg;
                                                invoice.IRN = respPlGenIRNDec.Irn;
                                                //Code By Janesh_15012025
                                                //Saving Addresss in Invoice
                                                ti.BillingAddress = pInvoice.BillingAddress;
                                                ti.StateCode = pInvoice.StateCode;
                                                ti.ShippingAddress = pInvoice.ShippingAddress;
                                                ti.ShippingStateCode = pInvoice.ShippingStateCode;
                                                InvoiceService.AddInvoice(ti);

                                                //record user activity
                                                string ActionName = $"E-Inv created, Inv No - {newInvNo}"
                                                + $", OrderGUID - {order.OrderGUID}";
                                                string TableName = "tblEInvoice, tblInvoices";
                                                if (ActionName != null)
                                                {
                                                    new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                                                }
                                                //End
                                            }
                                            else
                                            {
                                                if (respPlGenIRNDec.ErrorDetails != null)
                                                {
                                                    return invoice.ErrorMessage = respPlGenIRNDec.ErrorDetails[0].ErrorMessage.ToString(); ;
                                                }
                                                else
                                                {
                                                    return invoice.ErrorMessage = respPlGenIRNDec.errorMsg; ;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return invoice.ErrorMessage = "Cannot create invoice. Proforma Invoice amount does not match with current amount.";
                                        }
                                    }
                                    else
                                    {
                                        return invoice.ErrorMessage = "Please generate Proforma Invoice";
                                    }
                                }
                                else
                                {
                                    return invoice.ErrorMessage = "You are not allowed to generate E-Invoice. Please contact your admin.";
                                }
                            }
                            else
                            {
                                ti.BillingAddress = customer.AddressLine1 + "," + customer.AddressLine2 + "," + customer.District + "," + customer.State + "," + customer.PIN;
                                ti.StateCode = customer.StateCode;
                                ti.ShippingAddress = customer.ShippingAddressLine1 + "," + customer.ShippingAddressLine2 + "," + customer.ShippingDistrict + "," + customer.ShippingPIN;
                                ti.ShippingStateCode = customer.ShippingStateCode;
                                InvoiceService.AddInvoice(ti);

                                //record user activity
                                string ActionName = $"Inv created, Inv No - {newInvNo}"
                                + $"OrderGUID - {order.OrderGUID}";
                                string TableName = "tblInvoices";
                                if (ActionName != null)
                                {
                                    new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                                }
                                //End
                            }

                            var op = context.tblOrderParts.Where(x => x.OrderGUID == order.OrderGUID).ToList();
                            foreach (var item in op)
                            {
                                item.DateCreated = DateTime.Now;
                                context.Entry(item).State = EntityState.Modified;
                                context.SaveChanges();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {


                throw ex;
            }

            return newInvNo;

        }
        #endregion


        private string GetNewInvoiceNumber(string lastInvoiceNo, string invType)
        {
            //var lastCreditNote = ConfigurationManager.AppSettings["LastCreditNote"];
            //var skipCreditNo = ConfigurationManager.AppSettings["SkipCreditNo"];

            var lastCreditNote = "";
            var skipCreditNo = "";

            if (invType == "DN")
            {
                lastCreditNote = ConfigurationManager.AppSettings["LastDebitNote"];
                skipCreditNo = ConfigurationManager.AppSettings["SkipDebitNo"];
            }
            else
            {
                lastCreditNote = ConfigurationManager.AppSettings["LastCreditNote"];
                skipCreditNo = ConfigurationManager.AppSettings["SkipCreditNo"];
            }

            string newInvNo = "";
            char[] splitChar = { '/' };
            string[] strArray = lastInvoiceNo.Split(splitChar);
            int month = DateTime.Now.Month;
            var year = DateTime.Now.ToString("yy");
            int yy = Convert.ToInt32(year);
            var currYear = year + Convert.ToString(yy + 1);
            var prevYear = Convert.ToString(yy - 1) + year;

            //change by Sunit - 14022025
            //skip [lastCreditNote] because it is generated from GST prortal by customer 
            //so we skipped [skipCreditNo] number

            if (lastInvoiceNo == lastCreditNote)
            {
                if ((strArray[1] == prevYear && month < 4) || strArray[1] == currYear)
                {
                    int i = Convert.ToInt32(strArray[0]);
                    i = int.Parse(skipCreditNo) + 1;
                    newInvNo = Convert.ToString(i) + "/" + strArray[1] + "/" + strArray[2];
                }
                else if (strArray[1] == prevYear && month == 4)
                {
                    newInvNo = GetFirstInvoiceNo(invType);
                }
                if ((strArray[1] == prevYear && month > 4) || strArray[1] == currYear)
                {
                    int i = Convert.ToInt32(strArray[0]);
                    i = int.Parse(skipCreditNo) + 1;
                    newInvNo = Convert.ToString(i) + "/" + strArray[1] + "/" + strArray[2];
                }
                //End
            }
            else
            {
                if ((strArray[1] == prevYear && month < 4) || strArray[1] == currYear)
                {
                    int i = Convert.ToInt32(strArray[0]);
                    i = i + 1;
                    newInvNo = Convert.ToString(i) + "/" + strArray[1] + "/" + strArray[2];
                }
                else if (strArray[1] == prevYear && month == 4)
                {
                    newInvNo = GetFirstInvoiceNo(invType);
                }
                if ((strArray[1] == prevYear && month > 4) || strArray[1] == currYear)
                {
                    int i = Convert.ToInt32(strArray[0]);
                    i = i + 1;
                    newInvNo = Convert.ToString(i) + "/" + strArray[1] + "/" + strArray[2];
                }
            }


            
            return newInvNo;
        }

        public string GetFirstInvoiceNo(string invType)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            if (month < 4)
                year = year - 1;
            string yy = Convert.ToString(year).Substring(2);
            yy = yy + Convert.ToString(Convert.ToInt32(yy) + 1);
            //return  "0001/" + yy + "/" + invType;
            return "1/" + yy + "/" + invType;
        }

        public string GetLastInvoiceNo(string type)
        {
            return InvoiceService.GetLastInvoiceNumber(type);
        }
        public OrderPaymentDto GetPaymentDetails(Guid id)
        {
            return InvoiceService.GetPaymentDetails(id);
        }
        public InvoiceDto GetInvoiceDetailsbyOrderId(Guid orderId)
        {
            return InvoiceService.GetInvoiceDetailsbyOrderId(orderId);
        }

        public CreditNoteModel GetCreditDetailsbyOrderId(Guid orderId)
        {
            return InvoiceService.GetCreditNoteDetailbyOrderId(orderId);
        }

        public InvoiceDto GetProformaInvoiceDetail(Guid orderId)
        {
            return InvoiceService.GetProformaInvoiceDetail(orderId);
        }

        //new helper for Invoice Print 
        public InvoiceDto GetInvoiceDetailsbyOrderIdV1(Guid orderId)
        {
            return InvoiceService.GetInvoiceDetailsbyOrderIdV1(orderId);
        }
        //End Here

        public List<ProformaInvoiceDto> GetProformaList(int PageSize, int PageNo)
        {
            return InvoiceService.GetProformaList(PageSize, PageNo);
        }
        public List<ProformaInvoiceDto> GetExcelofPI(ProformaViewModel svm)
        {
            try
            {
                List<ProformaInvoiceDto> srList = InvoiceService.ExcelForPIList();

                if (svm.ProformaInvoiceId != null & svm.ProformaInvoiceId != 0)
                {
                    srList = srList.Where(x => x.ProformaInvoiceId == svm.ProformaInvoiceId).ToList();
                }
                if (svm.ProformaInvoiceNo != null)
                {
                    srList = srList.Where(x => x.ProformaInvoiceNo.Contains(svm.ProformaInvoiceNo)).ToList();

                }
                /*if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                {

                    srList = srList.Where(x => x.DateCreated >= svm.StartDate && x.DateCreated <= svm.EndDate).ToList();
                    foreach (var item in srList)
                    {
                        item.EndDate = svm.EndDate;
                        item.TillFromDate = svm.StartDate;
                    }
                }*/

                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                {
                    DateTime enddate = svm.EndDate;
                    TimeSpan ts = new TimeSpan(11, 59, 0);
                    enddate = enddate.Date + ts;
                    srList = srList.Where(x => x.ProformaInvoiceDate >= svm.StartDate && x.ProformaInvoiceDate <= enddate).ToList();
                }

                return srList;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceRequestList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        //Get Cancelled Invoice
        public InvoiceDto GetCancelledInvoicebyOrderId(Guid orderId, int InvoiceId, string invoiceNo)
        {
            return InvoiceService.GetCancelledInvoiceDetailsbyOrderId(orderId, InvoiceId, invoiceNo);
        }
        //End Here

        public int RecordPayment(InvoiceListViewModel vm)
        {
            tblOrderPayment op = new tblOrderPayment();
            int i = 0;

            {
                var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
                PicannolEntities context = new PicannolEntities(); 
                op.DateCreated = DateTime.Now;
                op.DatePaid = vm.OrderPayment.DatePaid.Date;
                op.InvoiceNo = vm.OrderPayment.InvoiceNo;
                op.LastModified = DateTime.Now;
                op.OrderGUID = vm.OrderPayment.OrderGUID;
                op.PaymentType = vm.OrderPayment.SelectedPaymentMethod;
                op.PaymentDetails = vm.OrderPayment.PaymentDetails;
                op.AmountPaid = vm.OrderPayment.AmountPaid;
                op.TDS = vm.OrderPayment.TDS;
                op.PaymentGUID = Guid.NewGuid();
                op.InvoiceAmount = vm.OrderPayment.InvoiceAmount;
                op.CustomerId = vm.OrderPayment.CustomerId;
                op.Paid = true;
                op.ProvisionalBillId = vm.OrderPayment.ProvisionalBillId;
                op.ReceivedAmount = (decimal)(op.AmountPaid + op.TDS);
                decimal tt = 0;
                decimal checkTDSamount = vm.OrderPayment.TDS;
                decimal checkAmountpaid = vm.OrderPayment.AmountPaid;
                
                var pk = context.tblOrderPayments.Where(x => x.InvoiceNo == vm.OrderPayment.InvoiceNo).ToList();
                
                    foreach (var item in pk)
                    {
                        tt += (int) item.ReceivedAmount;
                        checkTDSamount += item.TDS ?? 0;
                        checkAmountpaid += item.AmountPaid;
                    }

                    var tor = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == vm.OrderPayment.ProvisionalBillId).FirstOrDefault();
                    if (tor != null)
                    {
                        if (vm.OrderPayment.InvoiceAmount == (checkTDSamount + checkAmountpaid))
                        {
                            tor.Paid = true;
                            op.Status = "Completed";
                           
                        }
                        else
                        {
                            tor.Paid = false;
                            op.Status = "Pending";

                        }

                        context.tblProvisionalBills.Add(tor);
                        context.Entry(tor).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                context.tblOrderPayments.Add(op);
                context.SaveChanges();
                i = op.OrderPaymentId;
                

                //record user activity

                string ActionName = $"RecordPayment, Invoice No - {vm.OrderPayment.InvoiceNo}"
                + $"OrderGUID - {vm.OrderPayment.OrderGUID}";
                string TableName = "tblOrders, tblOrderPayments";

                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                }
                //End

                //return i;
            }
            return i;
        }

        public int RecordDispatchDetails(OrdersViewModel vm)
        {
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            int i = 0;
            tblDispatchDetail dd = new tblDispatchDetail();
            PicannolEntities context = new PicannolEntities();
            dd.DocketNo = vm.DispatchDetails.DocketNumber;
            dd.DispatchDetails = vm.DispatchDetails.DispatchDetails;
            dd.DispatchDate = vm.DispatchDetails.DispatchDate;
            dd.TrackingNumber = vm.DispatchDetails.TrackingNumber;
            dd.OrderGUID = vm.DispatchDetails.OrderGUID;
            dd.DateCreated = DateTime.Now;
            dd.DelInd = false;
            dd.InvoiceNo = vm.DispatchDetails.InvoiceNo;
            context.tblDispatchDetails.Add(dd);
            context.SaveChanges();
            i = dd.DispatchId;
            var tor = context.tblOrders.Where(x => x.OrderGUID == vm.DispatchDetails.OrderGUID).FirstOrDefault();
            tor.Dispatched = true;
            tor.Status = ConstantsHelper.OrderStatus.Dispatched.ToString();
            context.tblOrders.Attach(tor);
            context.Entry(tor).State = EntityState.Modified;
            context.SaveChanges();

            //record user activity
            string ActionName = $"RecordDispatchDetails, Inv No - {vm.DispatchDetails.InvoiceNo}"
            + $"OrderGUID - {vm.DispatchDetails.OrderGUID}";
            string TableName = "tblOrders, tblDispatchDetails";
            if (ActionName != null)
            {
                new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
            }
            //End

            return i;
        }

        public string EmailInvoiceToCustomer(InvoiceDto invoice, OrderDto order, string email, string cc)
        {

            string response = "";
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var ccEmail = ConfigurationManager.AppSettings["ccEmail"];
            var root = HttpContext.Current.Server.MapPath("~/Content/PDF/Invoices/");
            try
            {
                var path = System.IO.Path.Combine(root, invoice.InvoiceFileName);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Dear Sir,");
                stringBuilder.Append("<br /><br />Please find attached hereto our Retail Invoice No- <b>" + invoice.InvoiceNo + "</b> for the repairs of PCBs   sent to our office.<br/>");
                stringBuilder.Append("<br /><br /><b> Note:- </b> <br/>");
                stringBuilder.Append("<br /> •	For any reason if you need any changes in our invoice, kindly inform us in the same month only, after that we cannot change anything.<br/>");
                stringBuilder.Append("•	ALSO WE REQUEST YOU TO KINDLY CLEARLY MENTION TDS AMOUNT AND PERCENTAGE OF TDS TO AVOID ANY CONFUSION IN FUTURE.");
                stringBuilder.Append(" IF YOU ARE DEDUCTING ANY  TDS KINDLY INFORM US BY EMAIL.<br/>");
                stringBuilder.Append("<b> •	ALWAYS SEND PAYMENT DETAILS ON sreekala.balakrishnan@picanol.be so that we can dispatch your print immediately without any further delay. </b> <br/>");
                stringBuilder.Append("<br/>Thanks And Regards.<br/>");
                stringBuilder.Append("Sreekala Balakrishan.<br/>");
                stringBuilder.Append("<br /><br /><font color='red'>+++ If you receive any mail/email informing you of a change of our bank account, please call and inform us immediately and do not act based on such emails +++</font><br/>");
                stringBuilder.Append("<br /><br /><b><font color='red'>+++ This mailbox is not monitored. Please do not reply to this email. In case of any queries contact at sreekala.balakrishnan@picanol.be +++</font></b><br/>");
                GMailer mailer = new GMailer();
                mailer.AttachmentPath = path;
                mailer.ToEmail = email;
                mailer.CcEmail = ccEmail;
                mailer.Subject = "Invoice #" + invoice.InvoiceNo + " from Picanol India";
                mailer.Body = stringBuilder.ToString();
                mailer.IsHtml = true;
                mailer.Send();

                //record user activity
                string ActionName = $"EmailInvoiceToCustomer, Inv No - {invoice.InvoiceNo}";
                string TableName = "";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(UserInfo.UserId, ActionName, TableName);
                }
                //End

                response = "success";
            }
            catch (FileNotFoundException ex)
            {

                response = "Invoice not generated. Please generate e-invoice";
            }
            catch (Exception ex1)
            {
                response = ex1.Message;
            }
            return response;
        }
        public InvoiceDto GetInvoiceDetails(List<OrderPartDto> o, int? check)
        {
            InvoiceDto vm = new InvoiceDto();
            foreach (var item in o)
            {

            }

            return vm;
        }

        #region Get Invoice Items
        public InvoiceDto GetInvoiceItems(List<OrderPartDto> parts, CustomerDto customer, OrderDto order, string selectedAction, string invoiceNumber, string InvoiceGenerated)
        {
            var invoiceType = "";
            PicannolEntities context = new PicannolEntities();
            var piDetail = context.tblProformaInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false).FirstOrDefault();
            DateTime creditNoteDate = Convert.ToDateTime("2023-06-28");

            if (selectedAction == SelectAction.CreateCreditNote.ToString())
                invoiceType = ConfigurationManager.AppSettings["CreditNote"];
            else if (selectedAction == SelectAction.CreateDebitNote.ToString())
                invoiceType = ConfigurationManager.AppSettings["DebitNote"];
            else
                invoiceType = ConfigurationManager.AppSettings["Invoice"];

            try
            {
                
                DateTime ComponentDate = Convert.ToDateTime("2021-11-10");
                PicanolUtils utils = new PicanolUtils();
                InvoiceDto invoice = new InvoiceDto();
                invoice.ErrorMessage = "";
                List<InvoiceItemDto> items = new List<InvoiceItemDto>();
                if (order.PartName.Contains("TOOLS") || order.PartName.Contains("PCBs"))
                {
                    var createCreditNotes = (CreditNoteModel)HttpContext.Current.Session["ReEditableCreditNotes"];
                    var result = context.tblCreditNotes.Where(x => x.OrderGuid == order.OrderGUID
                    && x.InvoiceNumber == invoiceNumber && x.DelInd == false && x.Cancelled == false).FirstOrDefault();

                    if (createCreditNotes != null && createCreditNotes.partialAmount == true)
                    {
                        InvoiceItemDto item = new InvoiceItemDto();
                        item.Name = ConfigurationManager.AppSettings["ComponentName"];
                        item.Qty = 1;
                        item.HSNCode = ConfigurationManager.AppSettings["ComponentHSN"];
                        item.IsService = "N";
                        item.Rate = 0;
                        item.Rate = 0;
                        item.UnitOfMeasurement = "NOS";
                        item.ComponentsCost = createCreditNotes.ComponentCost;
                        invoice.ComponentQty = 1;
                        invoice.OrgComponentCost += createCreditNotes.ComponentCost;
                        item.SGSTAmount = 0;
                        item.CGSTAmount = 0;
                        item.IGSTAmount = 0;
                        item.Amount = Math.Round(item.ComponentsCost, 0);
                        item.GSTRate = 18;
                        item.GSTAmount = Math.Round((item.Amount * (item.GSTRate / 100)), 0);
                        item.Total = Math.Round(item.Amount + item.GSTAmount, 0);
                        item.AmountBTax = item.Amount;
                        items.Add(item);
                        invoice.ComponentsCost = Math.Round(createCreditNotes.ComponentCost, 0);
                    } else if(createCreditNotes != null && createCreditNotes.partialAmount == false && result!=null)
                    {
                        InvoiceItemDto item = new InvoiceItemDto();
                        item.Name = ConfigurationManager.AppSettings["ComponentName"];
                        item.Qty = 1;
                        item.HSNCode = ConfigurationManager.AppSettings["ComponentHSN"];
                        item.IsService = "N";
                        item.Rate = 0;
                        item.Rate = 0;
                        item.UnitOfMeasurement = "NOS";
                        item.ComponentsCost = result.ComponentCost;
                        invoice.ComponentQty = 1;
                        invoice.OrgComponentCost += result.ComponentCost;
                        item.SGSTAmount = 0;
                        item.CGSTAmount = 0;
                        item.IGSTAmount = 0;
                        item.Amount = Math.Round(item.ComponentsCost, 0);
                        item.GSTRate = 18;
                        item.GSTAmount = Math.Round((item.Amount * (item.GSTRate / 100)), 0);
                        item.Total = Math.Round(item.Amount + item.GSTAmount, 0);
                        item.AmountBTax = item.Amount;
                        items.Add(item);
                        invoice.ComponentsCost = Math.Round(result.ComponentCost, 0);
                    } else {
                        for (int i = 0; i < parts.Count; i++)
                        {
                            InvoiceItemDto item = new InvoiceItemDto();
                            item.Name = parts[i].PartName;
                            item.InvoiceItemID = i + 1;
                            item.Qty = parts[i].Qty;
                            //item.HSNCode = "84484990";
                            item.HSNCode = ConfigurationManager.AppSettings["ComponentHSN"];
                            item.IsService = "N";
                            item.Rate = (decimal)(2 * (parts[i].Price));
                            item.Rate = (int)Math.Round(item.Rate, 0);
                            item.UnitOfMeasurement = "NOS";
                            item.ComponentsCost = (decimal)(parts[i].Qty * item.Rate);
                            invoice.ComponentQty += parts[i].Qty;
                            invoice.OrgComponentCost += Math.Round((decimal)(parts[i].Qty * parts[i].Price), 0);
                            item.SGSTAmount = 0;
                            item.CGSTAmount = 0;
                            item.IGSTAmount = 0;
                            item.Amount = Math.Round(item.ComponentsCost, 0);
                            item.GSTRate = 18;
                            item.GSTAmount = Math.Round((item.Amount * (item.GSTRate / 100)), 0);
                            //}
                            item.Total = Math.Round(item.Amount + item.GSTAmount, 0);
                            item.AmountBTax = item.Amount;
                            items.Add(item);
                            //invoice.ComponentsCost += (invoice.OrgComponentCost * 2) >= 3000 ? (invoice.OrgComponentCost * 2) : 3000;
                            invoice.ComponentsCost = Math.Round((invoice.OrgComponentCost * 2), 0);
                        }
                    }
                    var eInvoice = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID 
                    && x.DelInd == false && x.Cancelled == false && x.Type== invoiceType).FirstOrDefault();
                    if (eInvoice != null)
                    {
                        invoice.IRN = eInvoice.IRN;
                        invoice.EInVoiceQrCode = eInvoice.QRCode;
                    }
                }
                else
                {
                    if (order.RepairType != RepairType.Loan.ToString())
                    {
                        int invoiceItemId = 0;
                        int invoiceItemQty = 0;
                        InvoiceItemDto componentCostItem = new InvoiceItemDto();
                        InvoiceItemDto labourCostItem = new InvoiceItemDto();

                        if (InvoiceGenerated == "InvoiceGenerated")
                        {
                            //Download E-Invoice if Invoice is already generated
                            var eInvoice = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID && x.DelInd == false && x.Cancelled == false && x.Type == invoiceType).FirstOrDefault();
                            if (eInvoice != null)
                            {
                                invoice.IRN = eInvoice.IRN;
                                invoice.EInVoiceQrCode = eInvoice.QRCode;
                            }
                            //End

                            var invoiceDtl = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID && (x.InvoiceNo != "" || x.InvoiceNo != null) && x.DelInd == false && x.Cancelled==false).FirstOrDefault();
                            
                            //fetch packaging forwarding charges for if credit note is already generated
                            var crndetail = _context.tblCreditNotes.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                            if (crndetail != null && selectedAction == SelectAction.CreateCreditNote.ToString() 
                                || selectedAction == SelectAction.CreateDebitNote.ToString())
                            {
                                if(crndetail.CreditNoteDate < creditNoteDate)
                                {
                                    invoice.PackingCharges = (invoiceDtl.Packing != null) ? (decimal)invoiceDtl.Packing : 0;
                                    invoice.ForwardingCharges = (invoiceDtl.Forwading != null) ? (decimal)invoiceDtl.Forwading : 0;
                                }
                                else
                                {
                                    invoice.PackingCharges = crndetail.PackagingForwarding;
                                    invoice.ForwardingCharges = 0;
                                }  
                            }
                            //end
                            else {
                                invoice.PackingCharges = (invoiceDtl.Packing != null) ? (decimal)invoiceDtl.Packing : 0;
                                invoice.ForwardingCharges = (invoiceDtl.Forwading != null) ? (decimal)invoiceDtl.Forwading : 0;
                            }

                            //invoice.PackingCharges = (invoiceDtl.Packing!=null)?(decimal)invoiceDtl.Packing:0;
                            //invoice.ForwardingCharges = (invoiceDtl.Forwading!=null)?(decimal)invoiceDtl.Forwading:0;
                            componentCostItem = GetComponentsCost(parts, order.Discount, order.DateCreated, order.OrderGUID, selectedAction);
                            labourCostItem = GetLabourCost(invoiceDtl.RepairCharge, order.TimeTaken, order.Discount, order.DateCreated, order.OrderGUID, selectedAction);

                            //fetch labour and component cost for credit note
                            
                            if (selectedAction == SelectAction.CreateCreditNote.ToString())
                            {
                                var createCreditNotes = (CreditNoteModel)HttpContext.Current.Session["ReEditableCreditNotes"];
                                
                                if (createCreditNotes != null)
                                {
                                    /*if (createCreditNotes.partialAmount == true)
                                    {
                                        invoice.PackingCharges = createCreditNotes.PckageForwarding;
                                        invoice.ForwardingCharges = 0;
                                    }*/

                                    if (createCreditNotes.partialAmount==true)
                                    {
                                        componentCostItem.ComponentsCost = createCreditNotes.ComponentCost;
                                        componentCostItem.Amount = createCreditNotes.ComponentCost;
                                        componentCostItem.Rate = createCreditNotes.ComponentCost;
                                        componentCostItem.AmountBTax = createCreditNotes.ComponentCost;
                                        invoice.PackingCharges = createCreditNotes.PckageForwarding;
                                        invoice.ForwardingCharges = 0;
                                    }
                                    if (createCreditNotes.partialAmount == true)
                                    {
                                        labourCostItem.Amount = createCreditNotes.LabourCost;
                                        labourCostItem.Rate = createCreditNotes.LabourCost;
                                        labourCostItem.AmountBTax = createCreditNotes.LabourCost;
                                        labourCostItem.GSTAmount = Math.Round((createCreditNotes.LabourCost) * 18 / 100);
                                        labourCostItem.OrgLabourCost = createCreditNotes.LabourCost;
                                        labourCostItem.Total = labourCostItem.GSTAmount + createCreditNotes.LabourCost;
                                    }
                                }
                                //End
                            }

                        }
                        else
                        {
                            string selectAction = "";
                            // fetch packing, forwording and Repair charge from tblCustomer if invoice is not generated

                            if (order.PackingType == PackingTypes.Small.ToString())
                            {
                                invoice.PackingCharges = (decimal)customer.SmallPacking;
                                invoice.ForwardingCharges = (decimal)customer.SmallForwarding;
                            }
                            else if (order.PackingType == PackingTypes.Big.ToString())
                            {
                                invoice.PackingCharges = (decimal)customer.BigPacking;
                                invoice.ForwardingCharges = (decimal)customer.BigForwarding;
                            }
                            componentCostItem = GetComponentsCost(parts, order.Discount, order.DateCreated, order.OrderGUID, selectAction);
                            labourCostItem = GetLabourCost(customer.RepairCharges, order.TimeTaken, order.Discount, order.DateCreated, order.OrderGUID, selectAction);
                            //End


                            //14-07-2023
                            //fetch invoice detail if pi is generated is laready generated
                            //var piDetail = context.tblProformaInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false).FirstOrDefault();
                            if (piDetail != null)
                            {
                                if (piDetail.Packing != 0 && piDetail.Forwading != 0)
                                {
                                    invoice.PackingCharges = (decimal)piDetail.Packing;
                                    invoice.ForwardingCharges = (decimal)piDetail.Forwading;
                                    invoice.RepairCharges = piDetail.RepairCharge * order.TimeTaken;
                                    //invoice.ComponentsCost = piDetail.ComponentCost;
                                }
                            }
                            //End

                            /*componentCostItem = GetComponentsCost(parts, order.Discount, order.DateCreated, order.OrderGUID,selectAction);
                            labourCostItem = GetLabourCost(customer.RepairCharges, order.TimeTaken, order.Discount, order.DateCreated, order.OrderGUID,selectAction);*/

                        }

                        labourCostItem.InvoiceItemID = invoiceItemId + 1;
                        labourCostItem.Qty = invoiceItemQty + 1;
                        //Newly Added
                        // labourCostItem.TotalComponentQty = componentCostItem.TotalComponentQty;
                        
                        /*if (order.Discount != null)*/
                        if (order.Discount != null)
                        {
                            //var invGen = context.tblInvoices.Any(x => x.OrderGuid == order.OrderGUID && x.Cancelled == false && x.DelInd==false);

                            var invGen = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID
                            && x.DelInd == false
                            && x.Cancelled == false).FirstOrDefault();

                            if (invGen != null)
                            {
                                invoice.PackingCharges = (decimal)invGen.Packing;
                                invoice.PackingCharges = Math.Round(invoice.PackingCharges, MidpointRounding.AwayFromZero);
                                invoice.ForwardingCharges = (decimal)invGen.Forwading;
                                invoice.ForwardingCharges = Math.Round(invoice.ForwardingCharges, MidpointRounding.AwayFromZero);
                            }
                            else
                            {

                                //Calculate Packing and forwarding after discount
                                if (piDetail !=null)
                                {
                                    
                                    if(piDetail.Packing==0 && piDetail.Forwading == 0)
                                    {
                                        //calculate packing and fording charges at run time after Proforma Invoice generated
                                        invoice.PackingCharges = invoice.PackingCharges - ((invoice.PackingCharges * (decimal)order.Discount) / 100);
                                        invoice.PackingCharges = Math.Round(invoice.PackingCharges, MidpointRounding.AwayFromZero);
                                        invoice.ForwardingCharges = invoice.ForwardingCharges - ((invoice.ForwardingCharges * (decimal)order.Discount) / 100);
                                        invoice.ForwardingCharges = Math.Round(invoice.ForwardingCharges, MidpointRounding.AwayFromZero);
                                        //end
                                    }
                                    else
                                    {
                                        // Fetch saving packing and forwading detail from proforma invoice
                                        invoice.PackingCharges = piDetail.Packing;
                                        invoice.PackingCharges = Math.Round(invoice.PackingCharges, MidpointRounding.AwayFromZero);
                                        invoice.ForwardingCharges = piDetail.Forwading;
                                        invoice.ForwardingCharges = Math.Round(invoice.ForwardingCharges, MidpointRounding.AwayFromZero);
                                        //end
                                    }
                                    
                                }
                                else
                                {
                                    //Calculate discount in Packing and Forwading charges
                                    invoice.PackingCharges = invoice.PackingCharges - ((invoice.PackingCharges * (decimal)order.Discount) / 100);
                                    invoice.PackingCharges = Math.Round(invoice.PackingCharges, MidpointRounding.AwayFromZero);
                                    invoice.ForwardingCharges = invoice.ForwardingCharges - ((invoice.ForwardingCharges * (decimal)order.Discount) / 100);
                                    invoice.ForwardingCharges = Math.Round(invoice.ForwardingCharges, MidpointRounding.AwayFromZero);
                                    //End
                                }
                                //End
                            }
                            
                        }


                        if (!order.ExemptComponentCost)
                        {
                            componentCostItem.AmountBTax = Math.Round(componentCostItem.Amount + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            
                            componentCostItem.Rate = componentCostItem.AmountBTax;
                            componentCostItem.Amount = componentCostItem.AmountBTax;
                            componentCostItem.TaxableValue = componentCostItem.AmountBTax;
                            componentCostItem.InvoiceItemID = invoiceItemId + 1;
                            componentCostItem.Qty = invoiceItemQty + 1;
                            invoiceItemId++;
                            invoiceItemQty++;
                            componentCostItem.GSTRate = 18;
                            componentCostItem.GSTAmount = Math.Round((componentCostItem.AmountBTax * (componentCostItem.GSTRate / 100)), 0);
                            componentCostItem.Total = componentCostItem.AmountBTax + componentCostItem.GSTAmount;
                            items.Add(componentCostItem);
                        }
                        else
                        {
                            componentCostItem.ComponentsCost = 0;
                            labourCostItem.AmountBTax = Math.Round(labourCostItem.Amount + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            //Newly Added

                            //labourCostItem.Rate = Math.Round(labourCostItem.Rate + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            labourCostItem.Rate = labourCostItem.AmountBTax;
                            labourCostItem.Amount = labourCostItem.AmountBTax;
                            labourCostItem.TaxableValue = labourCostItem.AmountBTax;
                            labourCostItem.RepairCharges = labourCostItem.RepairCharges;
                            labourCostItem.GSTRate = 18;
                            labourCostItem.GSTAmount = Math.Round((labourCostItem.AmountBTax * (labourCostItem.GSTRate / 100)), 0);
                            labourCostItem.Total = labourCostItem.AmountBTax + labourCostItem.GSTAmount;
                        }
                        items.Add(labourCostItem);


                        invoice.OrgComponentCost = componentCostItem.OrgComponentsCost;
                        invoice.ComponentQty = componentCostItem.TotalComponentQty;
                        invoice.ComponentsCost = componentCostItem.ComponentsCost;
                        invoice.LabourCost = labourCostItem.OrgLabourCost;

                        invoice.OrgAmount = Math.Round(invoice.OrgComponentCost + invoice.PackingCharges + invoice.ForwardingCharges + invoice.LabourCost, 2);
                        decimal gst = 18;

                        invoice.OrgTotalTax = Math.Round(((gst / 100) * invoice.OrgAmount), 2);
                        invoice.OrgAmount += Math.Round(invoice.OrgTotalTax, 2);
                    }
                }
                invoice.InvoiceItems = items;
                foreach (var a in items)
                {
                    invoice.Amount += a.AmountBTax;
                    invoice.TotalTax += a.GSTAmount;
                }

                invoice.AmountBTax = invoice.Amount;
                invoice.Amount = invoice.AmountBTax + invoice.TotalTax;
                invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";

               

                //Generate Credit note
                
                if (selectedAction == SelectAction.CreateCreditNote.ToString())
                {
                    var creditNote = ConfigurationManager.AppSettings["CreditNote"];
                    var result = context.tblCreditNotes.Where(x => x.OrderGuid == order.OrderGUID 
                    && x.InvoiceNumber == invoiceNumber && x.DelInd==false && x.Cancelled==false).FirstOrDefault();
                    if (result != null)
                    {
                        // fetch credit note detail
                        // if Credit note already generated

                        var eCredNote = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID && x.InvoiceNo == 
                        result.CreditNoteNo && x.Cancelled==false && x.Type== creditNote).FirstOrDefault();

                        if (eCredNote != null)
                        {
                            invoice.EInVoiceQrCode = eCredNote.QRCode;
                            invoice.IRN = eCredNote.IRN;
                            invoice.CreditNoteNo = result.CreditNoteNo;
                        }
                        else
                        {
                            invoice.CreditNoteNo = result.CreditNoteNo;
                        }
                        //End
                    }
                    else
                    {
                        invoice.CreditNoteNo = SaveCreditNotes(order, invoice, customer, selectedAction, invoiceNumber, items);

                        //Download new Credit note after IRN generation

                        if (invoice.CreditNoteNo!=null)
                        {
                            var creditNoteType = ConfigurationManager.AppSettings["CreditNote"];
                            var ecreditNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.CreditNoteNo 
                                                    && x.DelInd == false && x.Cancelled==false
                                                    && x.Type==creditNoteType).FirstOrDefault();
                            if (ecreditNote != null)
                            {
                                invoice.EInVoiceQrCode = ecreditNote.QRCode;
                                invoice.IRN = ecreditNote.IRN;
                                invoice.CreditNoteNo = invoice.CreditNoteNo;
                            }

                        }
                        //End

                    }
                    //End
                    invoice.CreditNoteDate = DateTime.Now.Date;
                    return invoice;
                }
                //End

                //Generate Debit Note
                
                if (selectedAction == ConstantsHelper.SelectAction.CreateDebitNote.ToString())
                {
                    var debitnote = ConfigurationManager.AppSettings["DebitNote"];
                    var result = context.tblDebitNotes.Where(x => x.OrderGuid == order.OrderGUID
                    && x.InvoiceNo == invoiceNumber && x.DelInd == false).FirstOrDefault();
                    if (result != null)
                    {
                        // fetch debit note detail
                        // if Debit note already generated

                        var eDebitNote = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID && x.InvoiceNo ==
                        result.DebitNoteNo && x.Cancelled == false && x.Type == debitnote).FirstOrDefault();

                        if (eDebitNote != null)
                        {
                            invoice.EInVoiceQrCode = eDebitNote.QRCode;
                            invoice.IRN = eDebitNote.IRN;
                            invoice.DebitNoteNo = result.DebitNoteNo;
                            invoice.DebitNoteDate = result.DebitNoteDate.ToString("dd-MM-yyyy");
                        }
                        else
                        {
                            invoice.DebitNoteNo = result.DebitNoteNo;
                        }
                        //End
                    }
                    else
                    {

                        invoice.DebitNoteNo = SaveDebitNotes(order, invoice, customer, selectedAction, invoiceNumber, items);

                        //Download new load debit note
                        //After IRN generation
                        if (invoice.DebitNoteNo != null)
                        {
                            var debitNoteresult = context.tblDebitNotes.Where(x => x.OrderGuid == order.OrderGUID
                            && x.InvoiceNo == invoiceNumber && x.DelInd == false).FirstOrDefault();
                            var debitNoteType = ConfigurationManager.AppSettings["DebitNote"];
                            var edebitNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.DebitNoteNo
                                                    && x.DelInd == false && x.Cancelled == false
                                                    && x.Type == debitNoteType).FirstOrDefault();
                            if (edebitNote != null)
                            {
                                invoice.EInVoiceQrCode = edebitNote.QRCode;
                                invoice.IRN = edebitNote.IRN;
                                invoice.CreditNoteNo = invoice.CreditNoteNo;
                                invoice.DebitNoteDate = debitNoteresult.DebitNoteDate.ToString("dd-MM-yyyy");
                            }

                        }
                        //End

                    }

                    invoice.CreditNoteDate = DateTime.Now.Date;
                    return invoice;
                }
                //End

                //View Invoice befor generate e-Invoice
                if (selectedAction != Actions.PreViewInvoice.ToString())
                {
                    var found = false;
                    if (selectedAction != Actions.PerformaInvoice.ToString())
                    {
                        if (context.tblInvoices.Any(x => x.OrderGuid == order.OrderGUID && x.Cancelled == false))
                        {
                            
                            var i = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.Cancelled == false).FirstOrDefault();

                            //28-05-2023
                            //Check current invoice amount and generated amount
                            //To fix amount difference between generated amount and downloaded inovice amount reported by Picanol
                            found = true;
                            if (i.Amount == invoice.Amount)
                            {
                                invoice.InvoiceNo = i.InvoiceNo;
                                invoice.InvoiceDate = i.InvoiceDate;
                                var a = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.Cancelled == false).FirstOrDefault();
                                a.Amount = invoice.Amount;
                                context.Entry(a).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                            else
                            {
                                invoice.ErrorMessage = "Invoice amount has been changed. Please check inovoice detail".ToString();
                            }
                            
                        }
                    }
                    else
                    {
                        //Check Total invoice amount PI total Amount
                        //Update PI total Amount if total amount has changed while generating PI
                        if (context.tblProformaInvoices.Any(x => x.OrderGuid == order.OrderGUID && x.DelInd == false))
                        {
                            var i = context.tblProformaInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false).FirstOrDefault();
                            invoice.InvoiceNo = i.ProformaInvoiceNo;
                            invoice.InvoiceDate = i.ProformaInvoiceDate;
                            if (invoice.Amount != i.Amount)
                            {
                                i.Amount = invoice.Amount;
                                i.ComponentCost = Math.Round(invoice.ComponentsCost);
                                context.tblProformaInvoices.Attach(i);
                                context.Entry(i).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                            found = true;
                        }
                        //End
                    }
                    if (!found)
                    {
                        invoice.InvoiceNo = SaveNewInvoice(order, invoice.Amount, customer.CustomerName, selectedAction, invoice, customer, items);
                        invoice.InvoiceDate = DateTime.Now.Date;
                            
                    }
                }
                return invoice;

                //End
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region get cancelled invoice items
        public InvoiceDto GetCancelledInvoiceItems(List<OrderPartDto> parts, CustomerDto customer, OrderDto order, string selectedAction, string invoiceNumber, string InvoiceGenerated)
        {
            try
            {
                PicannolEntities context = new PicannolEntities();
                DateTime ComponentDate = Convert.ToDateTime("2021-11-10");
                PicanolUtils utils = new PicanolUtils();
                InvoiceDto invoice = new InvoiceDto();
                invoice.ErrorMessage = "";
                List<InvoiceItemDto> items = new List<InvoiceItemDto>();
                if (order.PartName.Contains("TOOLS") || order.PartName.Contains("PCBs"))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        InvoiceItemDto item = new InvoiceItemDto();
                        item.Name = parts[i].PartName;
                        item.InvoiceItemID = i + 1;
                        item.Qty = parts[i].Qty;
                        //item.HSNCode = "84484990";
                        item.HSNCode = ConfigurationManager.AppSettings["ComponentHSN"];
                        item.IsService = "N";
                        item.Rate = (decimal)(2 * (parts[i].Price));
                        item.Rate = (int)Math.Round(item.Rate, 0);
                        item.UnitOfMeasurement = "NOS";
                        item.ComponentsCost = (decimal)(parts[i].Qty * item.Rate);
                        invoice.ComponentQty += parts[i].Qty;
                        invoice.OrgComponentCost += Math.Round((decimal)(parts[i].Qty * parts[i].Price), 0);
                        item.SGSTAmount = 0;
                        item.CGSTAmount = 0;
                        item.IGSTAmount = 0;
                        item.Amount = Math.Round(item.ComponentsCost, 0);
                        item.GSTRate = 18;
                        item.GSTAmount = Math.Round((item.Amount * (item.GSTRate / 100)), 0);
                        //}
                        item.Total = Math.Round(item.Amount + item.GSTAmount, 0);
                        item.AmountBTax = item.Amount;
                        items.Add(item);
                        //invoice.ComponentsCost += (invoice.OrgComponentCost * 2) >= 3000 ? (invoice.OrgComponentCost * 2) : 3000;
                        invoice.ComponentsCost = Math.Round((invoice.OrgComponentCost * 2), 0);
                    }

                    var eInvoice = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID && x.DelInd == false && x.Cancelled == true).FirstOrDefault();
                    if (eInvoice != null)
                    {
                        invoice.IRN = eInvoice.IRN;
                        invoice.EInVoiceQrCode = eInvoice.QRCode;
                    }
                }
                else
                {
                    if (order.RepairType != ConstantsHelper.RepairType.Loan.ToString())
                    {
                        int invoiceItemId = 0;
                        int invoiceItemQty = 0;
                        InvoiceItemDto componentCostItem = new InvoiceItemDto();
                        InvoiceItemDto labourCostItem = new InvoiceItemDto();
                        var invoiceDtl = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.InvoiceNo==invoiceNumber && x.DelInd == false && x.Cancelled == true).FirstOrDefault();
                        invoice.InvoiceNo = invoiceDtl.InvoiceNo;
                        invoice.InvoiceDate = invoiceDtl.InvoiceDate;
                        
                        // fetch packing, forwording and Repair charge from tblInvoice if invoice is already generated

                        var eInvoice = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID && x.DelInd == false && x.Cancelled == true && x.InvoiceNo==invoiceNumber).FirstOrDefault();
                        if (eInvoice != null)
                        {
                            invoice.IRN = eInvoice.IRN;
                            //invoice.DateCreated = eInvoice.CancelledDate.Value;
                            invoice.EInVoiceQrCode = eInvoice.QRCode;
                        }

                        invoice.PackingCharges = (invoiceDtl.Packing!=null)?(decimal)invoiceDtl.Packing:0;
                        invoice.ForwardingCharges = (invoiceDtl.Forwading!=null)? (decimal)invoiceDtl.Forwading:0;
                        invoice.Cancelled = invoiceDtl.Cancelled;
                        invoiceDtl.CancelledDate = invoiceDtl.CancelledDate.Value;
                        componentCostItem = GetCancelledComponentsCost(parts, order.Discount, order.DateCreated, order.OrderGUID, invoiceDtl.InvoiceNo);
                        labourCostItem = GetCancelledLabourCost(invoiceDtl.RepairCharge, order.TimeTaken, order.Discount, order.DateCreated, order.OrderGUID, invoiceDtl.InvoiceNo);

                        //End

                        labourCostItem.InvoiceItemID = invoiceItemId + 1;
                        labourCostItem.Qty = invoiceItemQty + 1;
                        //Newly Added
                        // labourCostItem.TotalComponentQty = componentCostItem.TotalComponentQty;
                        if (order.Discount != null)
                        {
                            invoice.PackingCharges = invoice.PackingCharges - ((invoice.PackingCharges * (decimal)order.Discount) / 100);
                            invoice.PackingCharges = Math.Round(invoice.PackingCharges, MidpointRounding.AwayFromZero);

                            invoice.ForwardingCharges = invoice.ForwardingCharges - ((invoice.ForwardingCharges * (decimal)order.Discount) / 100);
                            invoice.ForwardingCharges = Math.Round(invoice.ForwardingCharges, MidpointRounding.AwayFromZero);
                        }
                        if (!order.ExemptComponentCost)
                        {
                            componentCostItem.AmountBTax = Math.Round(componentCostItem.Amount + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            //Newly Added
                            //  componentCostItem.Rate = Math.Round(componentCostItem.Rate + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            componentCostItem.Rate = componentCostItem.AmountBTax;
                            componentCostItem.Amount = componentCostItem.AmountBTax;
                            componentCostItem.TaxableValue = componentCostItem.AmountBTax;
                            componentCostItem.InvoiceItemID = invoiceItemId + 1;
                            componentCostItem.Qty = invoiceItemQty + 1;
                            invoiceItemId++;
                            invoiceItemQty++;
                            componentCostItem.GSTRate = 18;
                            componentCostItem.GSTAmount = Math.Round((componentCostItem.AmountBTax * (componentCostItem.GSTRate / 100)), 0);
                            componentCostItem.Total = componentCostItem.AmountBTax + componentCostItem.GSTAmount;
                            items.Add(componentCostItem);
                        }
                        else
                        {
                            componentCostItem.ComponentsCost = 0;
                            labourCostItem.AmountBTax = Math.Round(labourCostItem.Amount + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            //Newly Added

                            //labourCostItem.Rate = Math.Round(labourCostItem.Rate + invoice.PackingCharges + invoice.ForwardingCharges, 0);
                            labourCostItem.Rate = labourCostItem.AmountBTax;
                            labourCostItem.Amount = labourCostItem.AmountBTax;
                            labourCostItem.TaxableValue = labourCostItem.AmountBTax;
                            labourCostItem.RepairCharges = labourCostItem.RepairCharges;
                            labourCostItem.GSTRate = 18;
                            labourCostItem.GSTAmount = Math.Round((labourCostItem.AmountBTax * (labourCostItem.GSTRate / 100)), 0);
                            labourCostItem.Total = labourCostItem.AmountBTax + labourCostItem.GSTAmount;
                        }
                        items.Add(labourCostItem);

                        invoice.OrgComponentCost = componentCostItem.OrgComponentsCost;
                        invoice.ComponentQty = componentCostItem.TotalComponentQty;
                        invoice.ComponentsCost = componentCostItem.ComponentsCost;
                        invoice.LabourCost = labourCostItem.OrgLabourCost;

                        invoice.OrgAmount = Math.Round(invoice.OrgComponentCost + invoice.PackingCharges + invoice.ForwardingCharges + invoice.LabourCost, 2);
                        decimal gst = 18;
                        invoice.OrgTotalTax = Math.Round(((gst / 100) * invoice.OrgAmount), 2);
                        invoice.OrgAmount += Math.Round(invoice.OrgTotalTax, 2);

                    }
                }
                invoice.InvoiceItems = items;
                foreach (var a in items)
                {
                    invoice.Amount += a.AmountBTax;
                    invoice.TotalTax += a.GSTAmount;
                }

                invoice.AmountBTax = invoice.Amount;
                invoice.Amount = invoice.AmountBTax + invoice.TotalTax;
                invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";

                return invoice;

                //return invoice;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private InvoiceItemDto GetLabourCost(decimal? repairCharges, decimal? timeTaken, decimal? discount, DateTime OrderDate, Guid orderguid,string selectedAction)
        {
            var invoiceDtl = _context.tblInvoices.Where(x => x.OrderGuid == orderguid && x.Cancelled == false && x.DelInd == false).FirstOrDefault();
            if (invoiceDtl != null)
            {
                repairCharges = (invoiceDtl.RepairCharge!=null)? invoiceDtl.RepairCharge:0;
            }
            InvoiceItemDto item1 = new InvoiceItemDto();
            item1.Name = ConfigurationManager.AppSettings["LabourAndTestingName"];
            item1.HSNCode = ConfigurationManager.AppSettings["LabourAndTestingHSN"];
            item1.IsService = "Y";
            item1.UnitOfMeasurement = "";
            var crnlabourCostAmt = false;

            if (crnlabourCostAmt == false)
            {
                PicannolEntities context = new PicannolEntities();
                var invGen = context.tblInvoices.Where(x => x.OrderGuid == orderguid
                            && x.DelInd == false
                            && x.Cancelled == false).FirstOrDefault();

                //fetch labour cost from tblInvoice
                //if invoice is generated else from repair charge for new invoice

                if (invGen != null)
                {
                    repairCharges = (invGen.RepairCharge!=null)?invGen.RepairCharge:0;
                }
                if (timeTaken == null)
                {
                    timeTaken = 0;
                }

                /*item1.Amount = (invGen != null)?Math.Round((decimal)(invGen.RepairCharge * timeTaken), 0)
                    : Math.Round((decimal)(repairCharges * timeTaken), 0);*/

                item1.Amount = Math.Round((decimal)(repairCharges * timeTaken), 0);
                //end

            }
            if (discount != null)
            {
                item1.Amount = item1.Amount - ((item1.Amount * (decimal)discount) / 100);
                item1.Amount = Math.Round(item1.Amount, MidpointRounding.AwayFromZero);
            }

            //fetch Labour cost from credit note if credit note has been already generated
            if (selectedAction == SelectAction.CreateCreditNote.ToString()
                || selectedAction == SelectAction.CreateDebitNote.ToString()){
                var checkCreditNote = _context.tblCreditNotes.Where(x => x.OrderGuid == orderguid && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                if (checkCreditNote != null)
                {
                    item1.Amount = (int)checkCreditNote.LabourCost;
                    crnlabourCostAmt = true;
                }
            }
            //End

            item1.Rate = item1.Amount;
            item1.AmountBTax = item1.Amount;
            item1.GSTRate = 18;
            item1.GSTAmount = Math.Round((item1.AmountBTax * (item1.GSTRate / 100)), 0);
            item1.Total = item1.AmountBTax + item1.GSTAmount;
            item1.OrgLabourCost = item1.AmountBTax;
            item1.RepairCharges = (decimal)repairCharges;
            return item1;
        }



        private InvoiceItemDto GetCancelledLabourCost(decimal? repairCharges, decimal? timeTaken, decimal? discount, DateTime OrderDate, Guid orderguid, string invoiceNo)
        {
            var rcharge = _context.tblInvoices.Where(x => x.OrderGuid == orderguid 
            && x.Cancelled == true && x.DelInd == false && x.InvoiceNo== invoiceNo).FirstOrDefault();
            if (rcharge != null)
            {
                repairCharges = rcharge.RepairCharge;
            }
            InvoiceItemDto item1 = new InvoiceItemDto();

            //item1.Name = "LABOUR & TESTING";
            //item1.HSNCode = "998729";

            item1.Name = ConfigurationManager.AppSettings["LabourAndTestingName"];
            item1.HSNCode = ConfigurationManager.AppSettings["LabourAndTestingHSN"];
            item1.IsService = "Y";
            item1.UnitOfMeasurement = "";
            item1.Amount = Math.Round((decimal)(repairCharges * timeTaken), 0);
            if (discount != null)
            {
                item1.Amount = item1.Amount - ((item1.Amount * (decimal)discount) / 100);
                item1.Amount = Math.Round(item1.Amount, MidpointRounding.AwayFromZero);
            }
            item1.Rate = item1.Amount;
            item1.AmountBTax = item1.Amount;
            item1.GSTRate = 18;
            item1.GSTAmount = Math.Round((item1.AmountBTax * (item1.GSTRate / 100)), 0);
            item1.Total = item1.AmountBTax + item1.GSTAmount;
            item1.OrgLabourCost = item1.AmountBTax;
            item1.RepairCharges = (decimal)repairCharges;
            return item1;
        }

        private InvoiceItemDto GetComponentsCost(List<OrderPartDto> parts, decimal? discount, DateTime OrderDate, Guid orderguid, string selectAction)
        {
            int Ccost = Convert.ToInt32(ConfigurationManager.AppSettings["ComponentCost"].ToString());
            var invDetail = _context.tblInvoices.Where(x => x.OrderGuid == orderguid && x.Cancelled == false && x.DelInd == false).FirstOrDefault();
            var creditNote = _context.tblCreditNotes.Where(x => x.OrderGuid == orderguid && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
            var crnfound = false;
            
            InvoiceItemDto item = new InvoiceItemDto();
            DateTime ComponentDate = Convert.ToDateTime("2021-11-10");
            item.Name = ConfigurationManager.AppSettings["ComponentName"];
            item.HSNCode = ConfigurationManager.AppSettings["ComponentHSN"];
            item.IsService = "N";
            item.UnitOfMeasurement = "NOS";

            //calculted original component total price 
            foreach (var p in parts)
            {
                item.ComponentsCost += (decimal)(p.Qty * p.Price);
                item.TotalComponentQty += p.Qty;
            }
            //end


            //Fetch Component cost from credit note and debit note generation
            if (selectAction == SelectAction.CreateCreditNote.ToString() || selectAction== SelectAction.CreateDebitNote.ToString()){
                if (creditNote != null)
                {
                    Ccost = (int)creditNote.ComponentCost;
                    item.ComponentsCost = Ccost;
                    //item.ComponentsCost = (item.ComponentsCost * 2) >= Ccost ? (item.ComponentsCost * 2) : Ccost;
                    crnfound = true;
                }
            }
            //End

            item.OrgComponentsCost = item.ComponentsCost;

            //total calculate component cost for e-invoice
            if (invDetail != null && crnfound == false)
            {
                item.ComponentsCost = (invDetail.ComponentCost==null)?
                    (item.ComponentsCost * 2) >= 3000 ? (item.ComponentsCost * 2) : 3000:
                    item.ComponentsCost = (item.ComponentsCost * 2) >= Ccost ? (item.ComponentsCost * 2) : Math.Round((decimal)invDetail.ComponentCost);
            } else if(invDetail == null && crnfound == false){
                item.ComponentsCost = (item.ComponentsCost * 2) >= Ccost ? (item.ComponentsCost * 2) : Ccost;
            }
            //End

            //calculate discount on component cost if discount is not null
            if (discount != null)
            {
                PicannolEntities context = new PicannolEntities();

                //fetch component cost from credit note if credi note/debit has been generated
                if (creditNote != null && selectAction == SelectAction.CreateCreditNote.ToString() ||
                    selectAction == SelectAction.CreateDebitNote.ToString())
                {
                    item.ComponentsCost = (decimal)creditNote.ComponentCost;
                    item.ComponentsCost = Math.Round(item.ComponentsCost, MidpointRounding.AwayFromZero);
                }
                //end
                else
                {
                    if (invDetail != null)
                    {
                        item.ComponentsCost = item.ComponentsCost;
                        item.ComponentsCost = Math.Round(item.ComponentsCost, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        item.ComponentsCost = item.ComponentsCost - ((item.ComponentsCost * (decimal)discount) / 100);
                        item.ComponentsCost = Math.Round(item.ComponentsCost, MidpointRounding.AwayFromZero);
                    }
                    
                }  
            }
            item.SGSTAmount = 0;
            item.CGSTAmount = 0;
            item.IGSTAmount = 0;
            item.Amount = Math.Round(item.ComponentsCost, 0);
            item.Rate = item.Amount;
            item.AmountBTax = item.Amount;
            return item;
        }


        private InvoiceItemDto GetCancelledComponentsCost(List<OrderPartDto> parts, decimal? discount, DateTime OrderDate, Guid orderguid, string invoiceNo)
        {
            var Ivalue = _context.tblInvoices.Where(x => x.OrderGuid == orderguid && x.Cancelled == true && x.DelInd == false && x.InvoiceNo== invoiceNo).FirstOrDefault();
            InvoiceItemDto item = new InvoiceItemDto();
            DateTime ComponentDate = Convert.ToDateTime("2021-11-10");
            item.Name = ConfigurationManager.AppSettings["ComponentName"];
            item.HSNCode = ConfigurationManager.AppSettings["ComponentHSN"];
            item.IsService = "N";
            item.UnitOfMeasurement = "NOS";
            int Ccost = Convert.ToInt32(ConfigurationManager.AppSettings["ComponentCost"].ToString());
            foreach (var p in parts)
            {
                item.ComponentsCost += (decimal)(p.Qty * p.Price);
                item.TotalComponentQty += p.Qty;
            }
            item.OrgComponentsCost = item.ComponentsCost;
            if (Ivalue != null)
            {
                if (Ivalue.ComponentCost == null)
                {
                    item.ComponentsCost = (item.ComponentsCost * 2) >= 3000 ? (item.ComponentsCost * 2) : 3000;
                }
                else
                {
                    item.ComponentsCost = (item.ComponentsCost * 2) >= Ccost ? (item.ComponentsCost * 2) : Ccost;
                }
            }
            else
            {
                item.ComponentsCost = (item.ComponentsCost * 2) >= Ccost ? (item.ComponentsCost * 2) : Ccost;
            }
            if (discount != null)
            {
                item.ComponentsCost = item.ComponentsCost - ((item.ComponentsCost * (decimal)discount) / 100);
                item.ComponentsCost = Math.Round(item.ComponentsCost, MidpointRounding.AwayFromZero);
            }
            item.SGSTAmount = 0;
            item.CGSTAmount = 0;
            item.IGSTAmount = 0;
            item.Amount = Math.Round(item.ComponentsCost, 0);
            //item.Amount = Math.Round(item.ComponentsCost * item.TotalComponentQty, 0);
            item.Rate = item.Amount;
            //item.Rate = Math.Round(item.ComponentsCost, 0);
            item.AmountBTax = item.Amount;
            return item;
        }

        public string ReopenOrder(Guid orderId)
        {
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            string response = "";
            PicannolEntities context = new PicannolEntities();
            var order = context.tblOrders.Where(x => x.OrderGUID == orderId).FirstOrDefault();
            
            order.Status = ConstantsHelper.OrderStatus.Open.ToString();
            try
            {
                context.tblOrders.Attach(order);
                context.Entry(order).State = EntityState.Modified;
                context.SaveChanges();
                
                //record user activity
                string ActionName = $"ReopenOrder, OrderGUID- {orderId}";
                string TableName = "tblOrders";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                //End

                response = "Invoice opened for editing";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return response;
        }


        public string CancelInvoice(Guid orderId)
        {
            string response = "";
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            PicannolEntities context = new PicannolEntities();
            if (context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false))
            {

                var inv = context.tblInvoices.Where(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                inv.Cancelled = true;
                inv.CancelledDate = DateTime.Now;
                inv.CancelledBy = userInfo.UserId;
                context.tblInvoices.Attach(inv);
                context.Entry(inv).State = EntityState.Modified;
                context.SaveChanges();
                string res = ReopenOrder(orderId);
                var op = context.tblOrderParts.Where(x => x.OrderGUID == orderId).ToList();
                foreach (var item in op)
                {
                    item.DelInd = true;
                    context.Entry(item).State = EntityState.Modified;
                    context.SaveChanges();
                }

                //record user activity
                string ActionName = $"CancelInvoice, OrderGUID - {orderId}";
                string TableName = "tblInvoices";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                //End

                response = "Invoice cancelled";
            }
            else if (context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == true))
                response = "Invoice already cancelled";
            else
                response = "Invoice doesn't exist";
            return response;
        }

        #region Cancel E-Invoice
        public string CancelEInvoice(Guid orderId)
        {
            string response = "";
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            PicannolEntities context = new PicannolEntities();
            RespPlCancelIRN respPlCancelIRN = new RespPlCancelIRN();

            if (context.tblEInvoices.Any(x => x.OrderID == orderId && x.DelInd == false && (x.Cancelled == false || x.Cancelled == null)))
            {
                var invoiceType = ConfigurationManager.AppSettings["Invoice"];
                var eInv = context.tblEInvoices.Where(x => x.OrderID == orderId && x.DelInd == false && x.Cancelled == false &&
                x.Type== invoiceType).FirstOrDefault();
                respPlCancelIRN = _eInvoiceHelper.CancelEInvoice(eInv);

                if (respPlCancelIRN.CancelDate != null || respPlCancelIRN.Irn != null)
                {
                    if (context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false))
                    {

                        var inv = context.tblInvoices.Where(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                        inv.Cancelled = true;
                        inv.CancelledDate = DateTime.Now;
                        inv.CancelledBy = userInfo.UserId;
                        context.tblInvoices.Attach(inv);
                        context.Entry(inv).State = EntityState.Modified;
                        context.SaveChanges();
                        string res = ReopenOrder(orderId);
                        var op = context.tblOrderParts.Where(x => x.OrderGUID == orderId).ToList();
                        foreach (var item in op)
                        {
                            item.DelInd = true;
                            context.Entry(item).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        //record user activity
                        string ActionName = $"CancelEInvoice - {orderId}";
                        string TableName = "tblInvoices, tblEinvoce";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End

                        response = "Invoice cancelled";
                    }
                    else if (context.tblInvoices.Any(x => x.OrderGuid == orderId && x.DelInd == false && x.Cancelled == true))
                        response = "Invoice already cancelled";
                    else
                        response = "Invoice doesn't exist";
                    return response;
                }
                else
                {
                    if (respPlGenIRNDec.ErrorDetails != null)
                    {
                        response = respPlCancelIRN.ErrorDetails[0].ErrorMessage.ToString();
                    }
                    else
                    {
                        response = respPlCancelIRN.errorMsg.ToString();
                    }
                    return response;
                }

            }

            return response;
        }
        #endregion

        public string UpdateInvoice(OrderDto order)
        {
            PicannolEntities context = new PicannolEntities();
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var CustomerDetail = context.tblCustomers.Where(x => x.CustomerId == order.CustomerId).FirstOrDefault();
            var isInvoiceExist = context.tblInvoices.Where(x => x.OrderGuid == order.OrderGUID).FirstOrDefault();
            if (isInvoiceExist != null)
            {
                string invType = "";
                if (order.RepairType == RepairType.FOC.ToString())
                    invType = InvoiceType.FOC.ToString();
                else if (order.RepairType == RepairType.UnRepairedBoards.ToString())
                    invType = InvoiceType.URD.ToString();
                else if (order.RepairType == RepairType.RepairWarranty.ToString())
                    invType = InvoiceType.RW.ToString();
                else if (order.RepairType == RepairType.Loan.ToString())
                    invType = InvoiceType.LN.ToString();
                else
                    invType = InvoiceType.RP.ToString();

                tblInvoice ti = new tblInvoice();

                isInvoiceExist.DateCreated = DateTime.Now;
                isInvoiceExist.CustomerId = order.CustomerId;
                isInvoiceExist.OrderGuid = order.OrderGUID;
                isInvoiceExist.Status = 1;
                isInvoiceExist.InvoiceFileName = CustomerDetail.CustomerName + "_" + order.TrackingNumber + "_" + invType + ".pdf";
                context.Entry(isInvoiceExist).State = EntityState.Modified;
                context.SaveChanges();

                //record user activity
                string ActionName = $"UpdateInvoice - OrderGUID {order.OrderGUID}" +
                    $"Tracking Number - {order.TrackingNumber}";
                string TableName = "tblInvoices";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                //End

                return "success";
            }
            else
            {
                return "not found";
            }

        }
        #endregion
    }
}