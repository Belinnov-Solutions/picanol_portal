using Picanol.Helpers;
using Picanol.Models;
using Picanol.ViewModels;
using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using NLog;
using Picanol.DataModel;
using static Picanol.Helpers.ConstantsHelper;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Configuration;

namespace Picanol.Controllers
{
    [RedirectingAction]
    public class InvoiceController : BaseController
    {
        private readonly InvoiceHelper _invoiceHelper;
        private readonly CustomerHelper _customerHelper;
        private readonly OrderHelper _orderHelper;
        private readonly BusinessHelper _businessHelper;
        private readonly UserHelper _userHelper;
        private readonly EInvoiceHelper _eInvoiceHelper;
        public InvoiceController()
        {
            _userHelper = new UserHelper(this);
            _invoiceHelper = new InvoiceHelper(this);
            _customerHelper = new CustomerHelper(this);
            _orderHelper = new OrderHelper(this);
            _businessHelper = new BusinessHelper(this);
        }
        // GET: Invoice
        public ActionResult Index()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            return View(vm);
        }
        Logger logger = LogManager.GetLogger("databaseLogger");

        public ActionResult ListInvoices()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            vm.CustomersList = _customerHelper.GetCustomersList();
            vm.LastInvoiceNumber = _invoiceHelper.GetLastInvoiceNo(ConstantsHelper.InvoiceType.RP.ToString());
            vm.LastTrackingNumber = _orderHelper.GetLastTrackingNumber();
            vm.CurrentPage = vm.PageNo;
            return View(vm);
        }
        public ActionResult RecordPayment()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            return View(vm);
        }
        public ActionResult GetPaymentDetails(Guid orderId)
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            vm.OrderPayment = new OrderPaymentDto();
            vm.OrderPayment = _invoiceHelper.GetPaymentDetails(orderId);
            return Json(vm);
        }


        public ActionResult Dispatch()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            return View(vm);
        }

        [HttpPost]
        public ActionResult GetInvoiceList(InvoiceListViewModel vm)
        {
            try
            {
               
                // vm.InvoicesList = _invoiceHelper.GetInvoicesList(vm);
                vm.InvoicesList = _invoiceHelper.GetInvoicesListVersion1(vm);
                vm.CountList = vm.InvoicesList.Count;
                vm.CurrentPage = vm.PageNo;
                return PartialView("_InvoicesList", vm);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost]
        public ActionResult SaveOrderPayments(InvoiceListViewModel vm, List<HttpPostedFileBase> images)
        {
            int i = _invoiceHelper.RecordPayment(vm);
            if (i > 0)
            {
                return Json("success");

                var userInfo = (UserSession)Session["UserInfo"];
                string ActionName = $"Record Payment, OrderPaymentId- {vm.OrderPayment.OrderPaymentId}";
                string TableName = "tblOrderPayments and tblOrders";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }

            else
            {
                return Json("failure");

            }

        }

        #region Created credit note
        public ActionResult CreateCreditNote(CreditNoteModel CN)
        {
            Session["ReEditableCreditNotes"] = CN;
           
            InvoiceViewModel vm = new InvoiceViewModel();
            string response = "";
            string file1 = "";

            string SelectedAction = SelectAction.CreateCreditNote.ToString();
            try
            {
                vm = GenerateInvoiceDetails(CN.OrderGuid, CN.CustomerId, SelectedAction, CN.InvoiceNumber);
                vm.InvoiceDetails.InvoiceNo = CN.InvoiceNumber;
                
                if (vm.InvoiceDetails.ErrorMessage == "")
                {
                    response = "success";
                    string ActionName = $"download credit note  - {CN.InvoiceNumber}";
                    string TableName = "tblCreditNote, tblEinvoice";
                    if (ActionName != null)
                    {
                        var userInfo = (UserSession)Session["UserInfo"];
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
                else
                {
                    response = vm.InvoiceDetails.ErrorMessage;
                }
                    
            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            Session["Model"] = vm;
            Session["SelectedUserAction"] = SelectedAction;
            return Json(response);

        }
        #endregion




        #region Cancel Credit Note
        public ActionResult CancelCreditNote(CreditNoteModel cn)
        {
            CreditNoteModel cnm = new CreditNoteModel();
            string response = "";
            string file1 = "";

            string SelectedAction = SelectAction.CancelCreditNote.ToString();
            try
            {
                var crediNoteDtail = _invoiceHelper.GetCreditDetailsbyOrderId(cn.OrderGuid);
                CustomerDto c = _customerHelper.GetCustomerDetails(cn.CustomerId);
                cnm.CreditNoteNo = crediNoteDtail.CreditNoteNo;
                response = _invoiceHelper.cancelECreditNote(cn.OrderGuid);

            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            Session["SelectedUserAction"] = SelectedAction;
            return Json(response);

        }

        #endregion


        #region Created debit note
        public ActionResult CreateDebitNote(DebitNoteModel DN)
        {

            CreditNoteModel Cn = new CreditNoteModel();
            Session["ReEditableCreditNotes"] = Cn;

            InvoiceViewModel vm = new InvoiceViewModel();
            string response = "";
            string file1 = "";

            string SelectedAction = SelectAction.CreateDebitNote.ToString();
            try
            {
                vm = GenerateInvoiceDetails(DN.OrderGuid, DN.CustomerId, SelectedAction, DN.InvoiceNumber);
                vm.InvoiceDetails.InvoiceNo = DN.InvoiceNumber;

                if (vm.InvoiceDetails.ErrorMessage == "")
                {
                    response = "success";
                    string ActionName = $"download DBN  - {DN.InvoiceNumber}";
                    string TableName = "tblDebitNote, tblEinvoice";
                    if (ActionName != null)
                    {
                        var userInfo = (UserSession)Session["UserInfo"];
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
                else
                {
                    response = vm.InvoiceDetails.ErrorMessage;
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            Session["Model"] = vm;
            Session["SelectedUserAction"] = SelectedAction;
            return Json(response);

        }

        #endregion


        public ActionResult PrintInvoice(Guid orderId)
        {
            try
            {
                var userInfo = (UserSession)Session["UserInfo"];
                HttpContext.Session["CreditNoteEditable"] = null; 

                InvoiceDto invoice = _invoiceHelper.GetInvoiceDetailsbyOrderId(orderId);
                logger.Info("InvoiceController --> PrintInvoice --> GetInvoiceDetailsbyOrderId-->" + orderId);
                string fileName = string.Empty;
                
                if (invoice == null)
                {
                    if (userInfo.RoleId == 8)
                    {
                        Response.Write("<script>alert('File Doesnot Exists!');window.location.href='http://picanol.belinnov.in/Invoice/Allinvoices'</script>");
                    }
                    else
                        Response.Write("<script>alert('File Doesnot Exists!');window.location.href='http://picanol.belinnov.in/Invoice/ListInvoices'</script>");

                }
                else
                {
                    fileName = invoice.InvoiceFileName;
                }
                string[] strArray = fileName.Split(new string[] { ".pdf" }, StringSplitOptions.None);
                string FileName = "";
                FileName = strArray[0] + "_Original.pdf";
                var root = Server.MapPath("~/Content/PDF/Invoices/");
                DirectoryInfo di = new DirectoryInfo(root);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                var path = Path.Combine(root, FileName);
                path = Path.GetFullPath(path);
                if (System.IO.File.Exists(path))
                {
                    logger.Info("Invoice PDF Generation --> InvoiceController  -->155-->" + orderId);
                    return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
                }
                else
                {
                    if (userInfo.RoleId == 8)
                    {

                        Response.Write("<script>alert('File Doesnot Exists!');window.location.href='http://picanol.belinnov.in/Invoice/Allinvoices'</script>");

                    }
                    else
                    {
                        //new for pdf print by himanshu
                        path = Path.GetFullPath(path);
                        //    logger.Info("JobSheetPDF" + vm.OrderDetails.TrackingNumber);
                        return new ViewAsPdf("ListInvoice", invoice) { FileName = fileName };
                        //end here
                    }


                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            }


        #region print Invoice
        public ActionResult PrintInvoiceV1(Guid orderId)
        {
            PicannolEntities context = new PicannolEntities();
            var userInfo = (UserSession)Session["UserInfo"];
            HttpContext.Session["ReEditableCreditNotes"] = null;

            string response = "";
            InvoiceViewModel vm = new InvoiceViewModel();
            InvoiceDto invoice = _invoiceHelper.GetInvoiceDetailsbyOrderIdV1(orderId);
            logger.Info("InvoiceController --> PrintInvoice --> GetInvoiceDetailsbyOrderId-->" + orderId);
            
            try
            {
                var InvDetails = context.tblInvoices.Where(x => x.InvoiceNo == invoice.InvoiceNo).FirstOrDefault();
                if (InvDetails != null)
                {
                    vm.ShippingAddress = InvDetails.ShippingAddress;
                    vm.ShippingStateCode = InvDetails.ShippingStateCode;
                    vm.BillingAddresss = InvDetails.BillingAddress;
                    vm.BillingStateCode = InvDetails.StateCode;
                }
                vm = GenerateInvoiceDetails(invoice.OrderGuid, invoice.CustomerId, invoice.RepairType, null);
                logger.Info("GenerateInvoiceDetails" + invoice.TrackingNo);
                if (vm.InvoiceDetails.ErrorMessage == "")
                    response = "success";
                if (response == "success")
                {
                    string ActionName = $"PrintInvoiceV1, InvoiceNo - {invoice.InvoiceNo}";
                    string TableName = "tbInvoice";
                    if (ActionName != null)
                    {
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
                else
                    response = vm.InvoiceDetails.ErrorMessage;
            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            Session["Model"] = vm;
            Session["SelectedUserAction"] = "PrintOriginalInvoice";

            return Json(response);
            }
            //end here
            public ActionResult QuotationPreview()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            vm.CustomersList = _customerHelper.GetCustomersList();
            
            return View(vm);
        }
        #endregion



        #region print cancelled Invoice
        public ActionResult PrintCancelledInvoice(Guid orderId, int InvoiceId, string invoiceNo)
        {
            var userInfo = (UserSession)Session["UserInfo"];
            string response = "";
            PicannolEntities context = new PicannolEntities();
            var result = context.tblInvoices.Where(x => x.OrderGuid == orderId && x.Cancelled==true  && x.InvoiceNo==invoiceNo).FirstOrDefault();
            if (result.InvoiceNo.Contains("AC"))
            {
                var pb = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == result.ProvisionalBillId).FirstOrDefault();
                var wo = context.tblWorkOrders.Where(x => x.WorkOrderGUID == result.OrderGuid).FirstOrDefault();
                
                return RedirectToAction("CancelledServiceBill", "ServiceRequest",
                                new { ProvisionalBillId = pb.ProvisionalBillId, workOrderId = pb.WorkOrderId, Initiator = "ViewInvoice" });
            }
            else
            {
                InvoiceViewModel vm = new InvoiceViewModel();
                InvoiceDto invoice = _invoiceHelper.GetCancelledInvoicebyOrderId(orderId, InvoiceId, invoiceNo);
                string fileName = string.Empty;
                try
                {

                    vm = GenerateCancelInvoiceDetails(invoice.OrderGuid, invoice.CustomerId, invoice.RepairType, invoice.InvoiceNo, InvoiceId);
                    logger.Info("GenerateInvoiceDetails" + invoice.TrackingNo);
                    if (vm.InvoiceDetails.ErrorMessage == "")
                        response = "success";
                    else
                        response = vm.InvoiceDetails.ErrorMessage;
                }
                catch (Exception ex)
                {

                    response = ex.Message;
                    //throw;
                }
                if (invoice.InvoiceNo.Contains("RP"))
                {
                    vm.OrderDetails.RepairType = ConstantsHelper.RepairType.Chargeable.ToString();
                }
                Session["Model"] = vm;
                Session["SelectedUserAction"] = "PrintOriginalInvoice";
                return Json(response);
            }

            
        }
        #endregion


        public ActionResult GetCustomerAddress(int CustomerId)
        {

            string PartNumber = _customerHelper.GetCustomerAddress(CustomerId);
            
            //return View(PartNumber);
            return Json(PartNumber);
        }


        #region Preview Invoice
        public ActionResult PreviewOrigionalInvoice(OrdersViewModel ov)
        {
            ov.SelectedAction = ConstantsHelper.Actions.PreViewInvoice.ToString();
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = new InvoiceViewModel();
            string response = "";
            try
            {
                vm = GenerateInvoiceDetails(ov.SelectedOrderId, ov.SelecetCustomerId, ov.SelectedAction, null);
                logger.Info("GenerateInvoiceDetails" + ov.TrackingNo);
                if (vm.InvoiceDetails.ErrorMessage == "")
                    response = "success";
                else
                    response = vm.InvoiceDetails.ErrorMessage;
            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            
            Session["Model"] = vm;
            Session["SelectedUserAction"] = ov.SelectedAction;
            Session["prviewOrgnlInv"] = ov.previewOrigionalInv;

            return Json(response);

        }
        #endregion

        #region fetch all PI List

        public ActionResult GetAllProformaLIst(int PageSize = 10, int PageNo = 1)
        {
            ProformaViewModel model = new ProformaViewModel();
            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            model.CurrentPageNo = PageNo;
            model.List = _invoiceHelper.GetProformaList(PageSize, PageNo);
            model.LastTrackingNumber = _orderHelper.GetLastTrackingNumber();

            return View(model);
        }
        public ActionResult GetListofPI(int PageSize = 10, int PageNo = 1)
        {
            ProformaViewModel model = new ProformaViewModel();
            model.CurrentPageNo = PageNo;
            model.List = _invoiceHelper.GetProformaInvoiceList(model, PageSize, PageNo);
            model.TotalNumberCounts = model.List.Count;
            model.LastTrackingNumber = _orderHelper.GetLastTrackingNumber();
            model.CustomersList = _customerHelper.GetCustomersList();
            return View(model);
        }

        #endregion

        #region Initiate Action Methods
        public ActionResult ProcessAction(OrdersViewModel ov)
        {
            try
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                var userInfo = (UserSession)Session["UserInfo"];

                InvoiceViewModel vm = new InvoiceViewModel();
                string response = "";
                string file1 = "";

                if (ov.SelectedAction == ConstantsHelper.Actions.CancelInvoice.ToString())
                {
                    var file = _invoiceHelper.GetInvoiceDetailsbyOrderId(ov.SelectedOrderId);
                    string fileName = file.InvoiceFileName;
                    CustomerDto c = _customerHelper.GetCustomerDetails(ov.SelecetCustomerId);
                    string jobsheet = c.CustomerName + "_" + ov.TrackingNo + "_JobSheet.pdf";
                    string[] strArray = fileName.Split(new string[] { ".pdf" }, StringSplitOptions.None);
                    string FileName = "";
                    string FileDuplicate = "";
                    string FileTriplicate = "";
                    var invoices = Server.MapPath("~/Content/PDF/Invoices/");
                    var invoices1 = Server.MapPath("~/Content/PDF/ProvisionalBill/");
                    var cancelledInvoices = Server.MapPath("~/Content/PDF/CancelledInvoice/");
                    var pa = Path.Combine(invoices1, jobsheet);

                    if (System.IO.File.Exists(pa))
                    {
                        System.IO.File.Delete(pa);
                    }
                    if (strArray[0].Contains(InvoiceType.FOC.ToString())
                        || strArray[0].Contains(InvoiceType.RW.ToString())
                        || strArray[0].Contains(InvoiceType.URD.ToString()))
                    {
                        FileName = strArray[0] + ".pdf";

                    }
                    else
                    {
                        FileName = strArray[0] + "_Original.pdf";
                        FileDuplicate = strArray[0] + "_Duplicate.pdf";
                        FileTriplicate = strArray[0] + "_Triplicate.pdf";
                    }
                    var source = Path.Combine(invoices, FileName);
                    var destination = Path.Combine(cancelledInvoices, FileName);

                    if (System.IO.File.Exists(source))
                    {
                        System.IO.File.Copy(source, destination, true);
                        var path = Path.Combine(invoices, FileName);
                        System.IO.File.Delete(path);
                    }
                    if (!strArray[0].Contains(InvoiceType.FOC.ToString())
                        && !strArray[0].Contains(InvoiceType.RW.ToString())
                        && !strArray[0].Contains(InvoiceType.URD.ToString()))
                    {
                        var path1 = Path.Combine(invoices, FileDuplicate);
                        if (System.IO.File.Exists(path1))
                            System.IO.File.Delete(path1);

                        var path2 = Path.Combine(invoices, FileTriplicate);
                        if (System.IO.File.Exists(path2))

                            System.IO.File.Delete(path2);
                    }
                    if (ov.SelectedAction == ConstantsHelper.Actions.CancelInvoice.ToString())
                    {
                        if (file.InvoiceNo.Contains(InvoiceType.FOC.ToString())
                            || file.InvoiceNo.Contains(InvoiceType.RW.ToString())
                            || file.InvoiceNo.Contains(InvoiceType.URD.ToString()))
                        {

                            response = _invoiceHelper.CancelInvoice(ov.SelectedOrderId);
                        }
                        else
                        {
                            response = _invoiceHelper.CancelEInvoice(ov.SelectedOrderId);
                        }
                    }
                    else
                    {
                        response = _invoiceHelper.ReopenOrder(ov.SelectedOrderId);
                    }

                }
                else if (ov.SelectedAction == ConstantsHelper.Actions.ReturnLoan.ToString())
                {
                    response = _orderHelper.ReturnLoan(ov.SelectedOrderId);

                    //record user activity
                    if (response == "Return Recorded")
                    {
                        string ActionName = $"ReturnLoan - {ov.SelectedOrderId}";
                        string TableName = "tblPartLoans, tblOrders";
                        if (ActionName != null)
                        {
                            _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                    }
                    //End
                }
                else if (ov.SelectedAction == ConstantsHelper.Actions.EditProformaInvoice.ToString())
                {
                    var file = _invoiceHelper.GetProformaInvoiceDetail(ov.SelectedOrderId);
                    CustomerDto c = _customerHelper.GetCustomerDetails(ov.SelecetCustomerId);
                    response = _invoiceHelper.ReopenOrder(ov.SelectedOrderId);

                    //record user activity
                    string ActionName = $"Edit PI, PI No. - {ov.SelectedOrderId}";
                    string TableName = "tblProformaInvoice";
                    if (ActionName != null)
                    {
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                    //End

                }
                else
                {
                    try
                    {
                        vm = GenerateInvoiceDetails(ov.SelectedOrderId, ov.SelecetCustomerId, ov.SelectedAction, null);
                        logger.Info("GenerateInvoiceDetails" + ov.TrackingNo);

                        if (vm.InvoiceDetails.ErrorMessage == "")
                        {
                            //record user activity on selected action
                            response = "success";
                            /*if (ov.SelectedAction == "PerformaInvoice")
                            {*/
                            if (ov.SelectedAction == Actions.PerformaInvoice.ToString())
                            {
                                string ActionName = $"Download PI, PI No. - {vm.InvoiceDetails.InvoiceNo}";
                                string TableName = "tblProformaInvoice";
                                if (ActionName != null)
                                {
                                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                                }
                            }

                            /*if (ov.SelectedAction == "PrintOriginalInvoice")
                            {*/
                            if (ov.SelectedAction == Actions.PrintOriginalInvoice.ToString())
                            {
                                string ActionName = $"Download Original Inv, Invoice No - {vm.InvoiceDetails.InvoiceNo}"
                                    + $"OrderGUID - {ov.SelectedOrderId}";
                                string TableName = "tblInvoices, TblEInvoice";
                                if (ActionName != null)
                                {
                                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                                }
                            }
                            /*if (ov.SelectedAction == "PrintDuplicateInvoice")
                            {*/
                            if (ov.SelectedAction == Actions.PrintDuplicateInvoice.ToString())
                            {

                                string ActionName = $"Download Duplicate E-Invoice, Invoice No - {vm.InvoiceDetails.InvoiceNo}";
                                string TableName = "tblInvoices, tblEInvoice";
                                if (ActionName != null)
                                {
                                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                                }
                            }
                            /* if (ov.SelectedAction == "PrintTriplicateInvoice")
                             {*/
                            if (ov.SelectedAction == Actions.PrintTriplicateInvoice.ToString())
                            {
                                string ActionName = $"Download Triplicate E-Invoice, Invoice No - {vm.InvoiceDetails.InvoiceNo}";
                                string TableName = "TblOrder";
                                if (ActionName != null)
                                {
                                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                                }
                            }
                        }
                        else
                        {
                            response = vm.InvoiceDetails.ErrorMessage;
                        }
                        //End

                    }
                    catch (Exception ex)
                    {
                        response = ex.Message;
                        //throw;
                    }
                    Session["Model"] = vm;
                    Session["SelectedUserAction"] = ov.SelectedAction;
                }
                return Json(response);
            }
            catch (Exception ex)
            {
                string response = "OOPS! Something went wrong. Please try again";
                return Json(response);

                //throw;
            }
            
        }

        #region Get invoice detail
        public InvoiceViewModel GenerateInvoiceDetails(Guid OrderId, int ? CustomerId, string SelectedAction, string invoiceNumber)
        {
            InvoiceViewModel vm = new InvoiceViewModel();
            vm.BusinessDetails = _businessHelper.GetBusinessDetails();
            vm.CustomerDetails = _customerHelper.GetCustomerDetails((int)CustomerId);
            vm.OrderDetails = _orderHelper.GetOrderDetails(OrderId);
            vm.OrderDetails.PrintableRepairType = "CHARGEABLE";
            //vm.OrderDetails.WarrantyTime = "3 months except PB Board.";
            vm.OrderDetails.WarrantyTime = "3 months except PB & Tupulo Board.";
            vm.OrderDetails.WarrantyTimeNotes = "PB & Tupulo Board warranty,1 month only"; //commented by ritu 15/09/2020   (From the date of Invoice)";
            if (vm.OrderDetails.RepairType != ConstantsHelper.RepairType.Loan.ToString())
                vm.ConsumedParts = _orderHelper.GetConsumedParts(OrderId);
            else
                vm.ConsumedParts = new List<OrderPartDto>();
            if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.NoRepairWarranty.ToString())
            {
                vm.OrderDetails.PrintableRepairType = "NO REPAIR WARRANTY";
                vm.OrderDetails.WarrantyTime = "0 months";
                vm.OrderDetails.WarrantyTimeNotes = "";
            }

            vm.InvoiceDetails = new InvoiceDto();
            
            //Check if Invoice number is already generated

            PicannolEntities context = new PicannolEntities();
            if (context.tblInvoices.Any(x => x.OrderGuid == OrderId && (x.InvoiceNo != "" || x.InvoiceNo != null) && x.DelInd == false && x.Cancelled == false))
            {
                var i = context.tblInvoices.Where(x => x.OrderGuid == OrderId && (x.InvoiceNo != "" || x.InvoiceNo != null) && x.DelInd == false && x.Cancelled == false).FirstOrDefault();

                // update tblInvoice Packing charges is null

                if (i.Packing == null)
                {

                    if (i.InvoiceNo == "0550/2223/RP")
                    {
                        i.Packing = 0;
                    }
                    else
                    {
                        if (vm.OrderDetails.PackingType == PackingTypes.Small.ToString())
                        {
                            i.Packing = (decimal)vm.CustomerDetails.SmallPacking;
                            //i.Forwading = (decimal)vm.CustomerDetails.SmallForwarding;
                        }
                        else if (vm.OrderDetails.PackingType == PackingTypes.Big.ToString())
                        {
                            i.Packing = (decimal)vm.CustomerDetails.BigPacking;
                            //i.Forwading = (decimal)vm.CustomerDetails.BigForwarding;
                        }
                        context.Entry(i).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                    vm.InvoiceDetails = _invoiceHelper.GetInvoiceItems(vm.ConsumedParts, vm.CustomerDetails, vm.OrderDetails, SelectedAction, invoiceNumber, "InvoiceGenerated");

                    //End
                }
                else
                {
                    vm.InvoiceDetails = _invoiceHelper.GetInvoiceItems(vm.ConsumedParts, vm.CustomerDetails, vm.OrderDetails, SelectedAction, invoiceNumber, "InvoiceGenerated");

                }
            }
            else
            {
                if (vm.CustomerDetails.SmallPacking == null || vm.CustomerDetails.SmallForwarding == null
                                || vm.CustomerDetails.BigPacking == null || vm.CustomerDetails.BigForwarding == null)
                    vm.InvoiceDetails.ErrorMessage = "Packing charges not defined for customer. Please update customer details with packing charges.";
                else if (vm.CustomerDetails.GSTIN == null)
                    vm.InvoiceDetails.ErrorMessage = "Customer GST number missing. Please update GST to generate E-Invoice";
                else
                    vm.InvoiceDetails = _invoiceHelper.GetInvoiceItems(vm.ConsumedParts, vm.CustomerDetails, vm.OrderDetails, SelectedAction, invoiceNumber, "");

            }
                //coment By Janesh _14012024
                //When  ProformaInvoice Generated Allready
            
                if (vm.OrderDetails != null&&SelectedAction== ConstantsHelper.Actions.PerformaInvoice.ToString())
                {
                    if (vm.OrderDetails.PerformaInvoiceNumber != null && vm.OrderDetails.PerformaInvoiceNumber != "")
                    { var PiDetals = context.tblProformaInvoices.Where(x => x.ProformaInvoiceNo == vm.OrderDetails.PerformaInvoiceNumber).FirstOrDefault();
                        if (PiDetals != null)
                        {
                            vm.ShippingAddress = PiDetals.ShippingAddress;
                            vm.ShippingStateCode = PiDetals.ShippingStateCode;
                            vm.BillingAddresss = PiDetals.BillingAddress;
                            vm.BillingStateCode = PiDetals.StateCode;
                        }

                    }

                }
                if (vm.InvoiceDetails != null &&( SelectedAction == ConstantsHelper.Actions.PrintOriginalInvoice.ToString() || SelectedAction == ConstantsHelper.Actions.PrintTriplicateInvoice.ToString() || SelectedAction == ConstantsHelper.Actions.PrintDuplicateInvoice.ToString() || SelectedAction == ConstantsHelper.Actions.PrintAll.ToString() || SelectedAction == "Chargeable"))
                {
                    if (vm.OrderDetails.PerformaInvoiceNumber != null)
                    {
                        var InvDetails = context.tblInvoices.Where(x => x.InvoiceNo == vm.InvoiceDetails.InvoiceNo).FirstOrDefault();
                        if (InvDetails != null)
                        {
                            vm.ShippingAddress = InvDetails.ShippingAddress;
                            vm.ShippingStateCode = InvDetails.ShippingStateCode;
                            vm.BillingAddresss = InvDetails.BillingAddress;
                            vm.BillingStateCode = InvDetails.StateCode;
                        }

                    }

                }
                if(SelectedAction== ConstantsHelper.Actions.PrintJobSheet.ToString()|| SelectedAction == "CreateDebitNote" || SelectedAction == "CreateCreditNote")
                {
                    var OrderDetails = context.tblOrders.Where(x => x.TrackingNumber ==vm.OrderDetails.TrackingNumber ).FirstOrDefault();
                    if (OrderDetails != null)
                    { var InvDetails = context.tblInvoices.Where(x => x.OrderGuid == OrderDetails.OrderGUID && x.Cancelled == false).FirstOrDefault();
                        if (InvDetails != null)
                        {
                            vm.ShippingAddress = InvDetails.ShippingAddress;
                            vm.ShippingStateCode = InvDetails.ShippingStateCode;
                            vm.BillingAddresss = InvDetails.BillingAddress;
                            vm.BillingStateCode = InvDetails.StateCode;
                        }
                    
                    }
                }
            if(SelectedAction== ConstantsHelper.Actions.PreViewInvoice.ToString())
            {
                var OrderDetails = context.tblOrders.Where(x => x.TrackingNumber == vm.OrderDetails.TrackingNumber).FirstOrDefault();
                if (OrderDetails != null)
                {
                    var InvDetails = context.tblInvoices.Where(x => x.OrderGuid == OrderDetails.OrderGUID && x.Cancelled == false).FirstOrDefault();
                    if (InvDetails != null)
                    {
                        vm.ShippingAddress = InvDetails.ShippingAddress;
                        vm.ShippingStateCode = InvDetails.ShippingStateCode;
                        vm.BillingAddresss = InvDetails.BillingAddress;
                        vm.BillingStateCode = InvDetails.StateCode;
                    }

                }
            }

            return vm;
        }
        #endregion



        #region generate cancel Invoice Detail
        public InvoiceViewModel GenerateCancelInvoiceDetails(Guid OrderId, int? CustomerId, string SelectedAction, string invoiceNumber, int InvoiceId)
        {
            InvoiceViewModel vm = new InvoiceViewModel();
            
            try
            {
                vm.BusinessDetails = _businessHelper.GetBusinessDetails();
                vm.CustomerDetails = _customerHelper.GetCustomerDetails((int)CustomerId);
                vm.OrderDetails = _orderHelper.GetOrderDetails(OrderId);
                vm.OrderDetails.PrintableRepairType = "CHARGEABLE";
                //vm.OrderDetails.WarrantyTime = "3 months except PB Board.";
                vm.OrderDetails.WarrantyTime = "3 months except PB & Tupulo Board.";
                vm.OrderDetails.WarrantyTimeNotes = "PB & Tupulo Board warranty,1 month only"; //commented by ritu 15/09/2020   (From the date of Invoice)";
                if (vm.OrderDetails.RepairType != ConstantsHelper.RepairType.Loan.ToString())
                    vm.ConsumedParts = _orderHelper.GetConsumedParts(OrderId);
                else
                    vm.ConsumedParts = new List<OrderPartDto>();
                if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.NoRepairWarranty.ToString())
                {
                    vm.OrderDetails.PrintableRepairType = "NO REPAIR WARRANTY";
                    vm.OrderDetails.WarrantyTime = "0 months";
                    vm.OrderDetails.WarrantyTimeNotes = "";
                }

                vm.InvoiceDetails = new InvoiceDto();
                
                PicannolEntities context = new PicannolEntities();
                if (context.tblInvoices.Any(x => x.OrderGuid == OrderId && x.DelInd == false && x.InvoiceNo == invoiceNumber && x.Cancelled == true && x.InvoiceId == InvoiceId))
                {
                    var i = context.tblInvoices.Where(x => x.OrderGuid == OrderId
                    && (x.InvoiceNo != "" || x.InvoiceNo != null)
                    && x.DelInd == false && x.Cancelled == true && x.InvoiceNo == invoiceNumber && x.InvoiceId==InvoiceId).FirstOrDefault();
                    
                    vm.InvoiceDetails = _invoiceHelper.GetCancelledInvoiceItems(vm.ConsumedParts, vm.CustomerDetails, vm.OrderDetails, SelectedAction, invoiceNumber, "InvoiceGenerated");
                }
                else
                {
                    vm.InvoiceDetails.ErrorMessage = "Invoice is not generated";
                }
                
            }
            catch (Exception ex)
            {

                vm.InvoiceDetails.ErrorMessage = ex.Message.ToString();
            }

            
            return vm;

        }
        #endregion

        #region return invoice PDF view
        public ActionResult ReturnView()
        {
            string action = (string)Session["SelectedUserAction"];
            string response = "";
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            
            if ( action == ConstantsHelper.Actions.PreViewInvoice.ToString())
            {
                 //if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.Chargeable.ToString())
                    return RedirectToAction("PreviewInvoice");
            }
            else if (action == ConstantsHelper.Actions.PrintOriginalInvoice.ToString())
            {
                if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.FOC.ToString())
                    return RedirectToAction("FOCPDF", vm);
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.RepairWarranty.ToString())
                    return RedirectToAction("RepairWarrantyPDF",vm);
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.UnRepairedBoards.ToString())
                    return RedirectToAction("UnrepairedBoardsPDF");
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.Loan.ToString())
                    return RedirectToAction("LoanPDF");
                else
                    return RedirectToAction("GenerateInvoice");
            }
            else if (action == ConstantsHelper.Actions.PrintAll.ToString())
            {
                
                    return RedirectToAction("GenerateInvoice");
            }
            else if (action == ConstantsHelper.SelectAction.CreateCreditNote.ToString())
            {

                return RedirectToAction("CreateCreditNotePdf");
            }
            else if (action == ConstantsHelper.SelectAction.CreateDebitNote.ToString())
            {

                return RedirectToAction("CreateDebitNotePdf");
            }

            else if (action == ConstantsHelper.Actions.PrintDuplicateInvoice.ToString())
            {
                if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.FOC.ToString())
                    return RedirectToAction("FOCPDF",vm);
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.RepairWarranty.ToString())
                    return RedirectToAction("RepairWarrantyPDF", vm);
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.UnRepairedBoards.ToString())
                    return RedirectToAction("UnrepairedBoardsPDF");
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.Loan.ToString())
                    return RedirectToAction("LoanPDF");
                else
                    return RedirectToAction("GenerateInvoice");

            }
            else if (action == ConstantsHelper.Actions.PrintTriplicateInvoice.ToString())
            {
                if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.FOC.ToString())
                    return RedirectToAction("FOCPDF", vm);
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.RepairWarranty.ToString())
                    return RedirectToAction("RepairWarrantyPDF", vm);
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.UnRepairedBoards.ToString())
                    return RedirectToAction("UnrepairedBoardsPDF");
                else if (vm.OrderDetails.RepairType == ConstantsHelper.RepairType.Loan.ToString())
                    return RedirectToAction("LoanPDF");
             
                else
                    return RedirectToAction("GenerateInvoice");
            }
            else if (action == ConstantsHelper.Actions.PerformaInvoice.ToString())
            {
                return RedirectToAction("PerformaInvoicePDF");
            }
            else if (action == ConstantsHelper.Actions.PrintJobSheet.ToString())
            {
                return RedirectToAction("JobSheetPDF");
            }
            else
                return null;
        }
        #endregion

        #region generate Invoice PDF
        public ActionResult GenerateInvoice()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            string fileName = "";
            string fileName1 = "";
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            vm.SelectedAction = (string)Session["SelectedUserAction"];
            //var root = Server.MapPath("~/Content/PDF/Invoices/");
            var root = Server.MapPath("~/Content/PDF/Invoices/");
            //Delete from folder 
            DirectoryInfo di = new DirectoryInfo(root);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            if (vm.SelectedAction == ConstantsHelper.Actions.PrintOriginalInvoice.ToString())
            {
                 fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_" +"RP_" +ConstantsHelper.Invoice.Original + ".pdf";
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
            }

            else if (vm.SelectedAction == ConstantsHelper.Actions.PrintDuplicateInvoice.ToString())

            {
                 fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_" + "RP_"+ConstantsHelper.Invoice.Duplicate + ".pdf";
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
                logger.Info("PrintDuplicateInvoice-->"+vm.OrderDetails.TrackingNumber);
            }
            else if (vm.SelectedAction == ConstantsHelper.Actions.PrintTriplicateInvoice.ToString())
            {
                 fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_" + "RP_"+ConstantsHelper.Invoice.Triplicate + ".pdf";
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
                logger.Info("PrintTriplicateInvoice-->"+vm.OrderDetails.TrackingNumber);
            }

            else if (vm.SelectedAction == ConstantsHelper.Actions.PrintAll.ToString())
            {
               
                fileName1 = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_RP_" + ConstantsHelper.Invoice.Original + ".pdf";
                fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_" + "RP_" + ConstantsHelper.Invoice.All + ".pdf";
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
                if (fileName1.Contains("/"))
                {
                    fileName1 = fileName1.Replace("/", "_");
                }
                var path1 = Path.Combine(root, fileName1);
                path1 = Path.GetFullPath(path1);
                vm.SelectedAction = ConstantsHelper.Actions.PrintOriginalInvoice.ToString();
                var pdfResult = new ViewAsPdf("EmailInvoicePDF", vm);
                try {
                    byte[] pdfFile = pdfResult.BuildPdf(this.ControllerContext);

                } 
                catch (Exception ex) 
                {
                    throw ex;
                };

                vm.SelectedAction = (string)Session["SelectedUserAction"];
                logger.Info("Generating PrintAll Invoices");
                return new ViewAsPdf("PrintAll", vm) { FileName = fileName, SaveOnServerPath = path1 };
                //return null;

            }
            var path = Path.Combine(root, fileName);

            try
            {
                if (System.IO.File.Exists(path))
                {
                    logger.Info("Downloading File in Print Invoice-->"+fileName);
                    return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
                }
                else
                {
                    path = Path.GetFullPath(path);
                    vm.SelectedAction = (string)Session["SelectedUserAction"];
                    logger.Info("Generate E-Invoice for"+fileName);
                    //return new ViewAsPdf("EmailInvoicePDF", vm) { FileName = fileName/*, SaveOnServerPath = path */};
                    
                    return new ViewAsPdf("EmailInvoicePDF", vm) { FileName = fileName , SaveOnServerPath = path};
                    //logger.Info("Generate Invoice");
                }
            }
            catch (Exception ex)
            {
                
                logger.Error(ex, "Error in GenerateInvoice");
                //return request.Message = "Not Able to Retrieve Provisional Bill Detail";
                throw;
            }

        }
        #endregion

        public ActionResult EmailInvoiceDuplicatePDF()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_RP.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            logger.Info("EmailInvoiceDuplicatePDF"+vm.OrderDetails.TrackingNumber);
            return new ViewAsPdf("EmailInvoiceDuplicatePDF", vm) { FileName = fileName };
        }


        public ActionResult EmailInvoiceTriplicatePDF()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_RP.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            logger.Info("EmailInvoiceTriplicatePDF"+vm.OrderDetails.TrackingNumber);
            return new ViewAsPdf("EmailInvoiceTriplicatePDF", vm) { FileName = fileName };

        }

        public ActionResult PreviewInvoice()
        {
            //Session["prviewOrgnlInv"] = ov.previewOrigionalInv;
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            vm.previewOrigionalInv = Session["prviewOrgnlInv"] as string;
            return View(vm);
        }
        public ActionResult PerformaInvoice()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            return View(vm);
        }
        public ActionResult CreateCreditNotePdf()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;

            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_" + ConstantsHelper.InvoiceType.CN.ToString() + ".pdf";

            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/CreditNotes/");
            var path = Path.Combine(root, fileName);
            if (System.IO.File.Exists(path))
            {
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                logger.Info("CreateCreditNotePdf"+vm.OrderDetails.TrackingNumber);
                //return new ActionAsPdf("CreateCreditNotePdf",vm) { FileName = fileName };
                //created by Himanshu 16
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
                return new ViewAsPdf("CreateCreditNotePdf", vm) { FileName = fileName };

            }
        }

        public ActionResult CreateDebitNotePdf()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_" + ConstantsHelper.InvoiceType.DN.ToString() + ".pdf";

            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/DebitNotes/");
            var path = Path.Combine(root, fileName);
            if (System.IO.File.Exists(path))
            {
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                logger.Info("CreateDebitNotePdf" + vm.OrderDetails.TrackingNumber);
                //return new ActionAsPdf("CreateCreditNotePdf",vm) { FileName = fileName };
                //created by Himanshu 16
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
                return new ViewAsPdf("CreateDebitNotePdf", vm) { FileName = fileName };

            }
        }


        public ActionResult PerformaInvoicePDF()

        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_PI.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/PerformaInvoices/"); DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            var path = Path.Combine(root, fileName);
            if (System.IO.File.Exists(path))
            {
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                if (fileName.Contains("/"))
                {
                    fileName = fileName.Replace("/", "_");
                }
                return new ViewAsPdf("PerformaInvoicePDF", vm) { FileName = fileName };
            }
        }
        public ActionResult PerformaInvoiceView()

        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
                return View("PerformaInvoicePDF", vm);
        }

        public ActionResult UnrepairedBoards()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            return View(vm);
        }


        public ActionResult UnrepairedBoardsPDF()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_URD.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/Invoices/");
            DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            var path = Path.Combine(root, fileName);
            if (System.IO.File.Exists(path))
            {
                logger.Info("UnrepairedBoardPDF" + vm.OrderDetails.TrackingNumber);
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                path = Path.GetFullPath(path);
                logger.Info("GenerateUnrepairedBoardsPDF" + vm.OrderDetails.TrackingNumber);
                return new ViewAsPdf("UnrepairedBoardsPDF", vm) { FileName = fileName/*, SaveOnServerPath = path */};
            }
        }
        //end here
        public ActionResult RepairWarranty()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            return View(vm);
        }


        public ActionResult RepairWarrantyPDF()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_RW.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/Invoices/");
            DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            var path = Path.Combine(root, fileName);
            if (System.IO.File.Exists(path))
            {
                logger.Info("RepairWarrantyPDF" + vm.OrderDetails.TrackingNumber);
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                path = Path.GetFullPath(path);
                logger.Info("RepairWarrantyPDF" + vm.OrderDetails.TrackingNumber);
                return new ViewAsPdf("RepairWarrantyPDF", vm) { FileName = fileName/*, SaveOnServerPath = path */};
            }
        }
        //end here
        public ActionResult InvPdf(InvoiceViewModel vm)
        {
            return View(vm);
        }
        public ActionResult Invoice(InvoiceViewModel vm)
        {

            return View(vm);
        }
        public ActionResult Loan()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            return View(vm);
        }
        

        public ActionResult LoanPDF()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_Loan.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/Invoices/");
            var path = Path.Combine(root, fileName);
            DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            if (System.IO.File.Exists(path))
            {
                logger.Info("LoanPDF" + vm.OrderDetails.TrackingNumber);
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                path = Path.GetFullPath(path);
                logger.Info("LoanPDF" + vm.OrderDetails.TrackingNumber);
                return new ViewAsPdf("LoanPDF", vm) { FileName = fileName/*, SaveOnServerPath = path */};
            }
        }
        //end here
        public ActionResult FOC()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            return View(vm);
        }
        
        public ActionResult FocPDF()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_FOC.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/Invoices/");
            DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            var path = Path.Combine(root, fileName);
            if (System.IO.File.Exists(path))
            {
                logger.Info("FocPDF" + vm.OrderDetails.TrackingNumber);
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                path = Path.GetFullPath(path);
                logger.Info("FocPDF" + vm.OrderDetails.TrackingNumber);
                return new ViewAsPdf("FocPDF", vm) { FileName = fileName};
            }
        }
        //end Here
        public ActionResult JobSheet()
        {
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            return View(vm);
        }
        
        public ActionResult JobSheetPDF()
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            InvoiceViewModel vm = Session["Model"] as InvoiceViewModel;
            string fileName = vm.CustomerDetails.CustomerName + "_" + vm.OrderDetails.TrackingNumber + "_JobSheet.pdf";
            if (fileName.Contains("/"))
            {
                fileName = fileName.Replace("/", "_");
            }
            var root = Server.MapPath("~/Content/PDF/ProvisionalBill/");
            DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            var path = Path.Combine(root, fileName);

            if (System.IO.File.Exists(path))
            {
                logger.Info("JobSheetPDF" + vm.OrderDetails.TrackingNumber);
                return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
            }
            else
            {
                string ActionName = $"Print Job Sheet - {vm.InvoiceDetails.InvoiceNo}" +
                       $"OrderGUID - {vm.OrderDetails.OrderGUID}";
               string TableName = "TblOrder";
                if (ActionName != null)
                {
                    var userInfo = (UserSession)Session["UserInfo"];
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                path = Path.GetFullPath(path);
                logger.Info("JobSheetPDF" + vm.OrderDetails.TrackingNumber);
                return new ViewAsPdf("JobSheetPDF", vm) { FileName = fileName/*, SaveOnServerPath = path*/ };
               

            }
        }
        //end here
        #endregion

        //Added by ritu 24sep2020
        #region AllInvoice Action
        public ActionResult AllInvoices()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            vm.CustomersList = _customerHelper.GetCustomersList();
            vm.LastInvoiceNumber = _invoiceHelper.GetLastInvoiceNo(ConstantsHelper.InvoiceType.RP.ToString());
            vm.LastTrackingNumber = _orderHelper.GetLastTrackingNumber();
            return View(vm);
        }

        [HttpPost]
        public ActionResult GetAllInvoices(InvoiceListViewModel vm)
        {
            vm.InvoicesList = _invoiceHelper.GetInvoicesList(vm);
            vm.CountList = vm.InvoicesList.Count;
            vm.CurrentPage = vm.PageNo;
            return PartialView("_ALlInvoices", vm);
        }
        #endregion

        #region Get All Cancelled Invoice
        public ActionResult CancelledInvoices()
        {
            InvoiceListViewModel vm = new InvoiceListViewModel();
            vm.CustomersList = _customerHelper.GetCustomersList();
            vm.LastInvoiceNumber = _invoiceHelper.GetLastInvoiceNo(ConstantsHelper.InvoiceType.RP.ToString());
            vm.LastTrackingNumber = _orderHelper.GetLastTrackingNumber();
            return View(vm);
        }

        public ActionResult GetAllCancelledInvoice(InvoiceListViewModel vm)
        {
            vm.InvoicesList = _invoiceHelper.GetCancellInvoiceList(vm);
            vm.InvoicesList.RemoveAll(x => x.InvoiceNo.Contains("AC"));
            vm.CountList = vm.InvoicesList.Count;
            vm.CurrentPage = vm.PageNo;
            return PartialView("_CancelledInvoices", vm);
        }
        #endregion

        public ActionResult SearchPIData(ProformaViewModel svm)
        {

            var userInfo = (UserSession)Session["UserInfo"];

            svm.CurrnetPageNo = svm.PageNo;
            List<ProformaInvoiceDto> requests = _invoiceHelper.GetSearchListProformaInvoice(svm);
            if (svm.TypeId == 2)
            {
                return Json(requests, JsonRequestBehavior.AllowGet);
            }
            ProformaViewModel vm = new ProformaViewModel();
            //vm.ServiceRequestList = requests;
            vm.List = requests;
            vm.TotalNumberCounts = vm.List.Count;
            vm.CurrentPageNo = svm.PageNo;
            vm.CurrnetPageNo = svm.PageNo;
            return PartialView("GetAllProformaLIst", vm);
        }

        public ActionResult GetProformaInvoiceListForExcel()
        {
            ProformaViewModel svm = Session["ExcelParameters"] as ProformaViewModel;
            if (svm != null)
            {
                var gv = new GridView();

                svm.CurrnetPageNo = svm.PageNo;
                ProformaViewModel vm = new ProformaViewModel();

                gv.DataSource = _invoiceHelper.GetExcelofPI(svm).Select(x => new { x.CustomerName, x.ProformaInvoiceNo,  x.Amount, x.ProformaInvoiceDate, x.Zone });
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=ProformaInvoiceList_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("GetListofPI");
        }
        public JsonResult GetExcelDownloadParameter(ProformaViewModel svm)
        {
            try
            {
                Session["ExcelParameters"] = svm;
                return Json("success");
            }
            catch (Exception ex)
            {
                return Json("failure");
                throw ex;
            }
        }
    }
}