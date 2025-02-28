using NLog;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.ViewModels;
using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Picanol.Controllers
{

    //[RedirectingAction]
    public class ServiceRequestController : BaseController
    {
        private readonly MaterialHelper _materialHelper;
        private readonly CustomerHelper _customerHelper;
        private readonly ServiceRequestHelper _serviceRequestHelper;
        private readonly WorkOrderHelper _workOrderHelper;
        private readonly TimeSheetHelper _timeSheetHelper;
        private readonly UserHelper _userHelper;
        string eInvoiceURL = ConfigurationManager.AppSettings["eInvoiceAPIURL"];
        EInvoiceHelper _eInvoiceHelper = new EInvoiceHelper();
        private readonly BusinessHelper _businessHelper;
        public ServiceRequestController()
        {
            _materialHelper = new MaterialHelper(this);
            _customerHelper = new CustomerHelper(this);
            _serviceRequestHelper = new ServiceRequestHelper(this);
            _workOrderHelper = new WorkOrderHelper(this);
            _userHelper = new UserHelper(this);
            _timeSheetHelper = new TimeSheetHelper(this);
            _businessHelper = new BusinessHelper(this);
        }
        Logger logger = LogManager.GetLogger("databaseLogger");

        PicannolEntities context = new PicannolEntities();
        // GET: ServiceBill
        public ActionResult ServiceRequest(int? ProvisionalBillId, int? CustomerId, string CallType, int? WorkOrderId)
        {
            ServiceRequestViewModel model = new ServiceRequestViewModel();

            model.Customers = _materialHelper.GetCustomersList();
            //changed
            model.SubCustomerList = _customerHelper.GetSubCustomerListByCustomerId((int)CustomerId);
            if (model.SubCustomerList.Count > 0)
            {
                model.CustomerIsEdit = true;
                if (ProvisionalBillId != null)
                {
                    var ProvisionalSubCustomer = _customerHelper.GetSubCustomerByProvisionalBillId((int)ProvisionalBillId);
                    if (ProvisionalSubCustomer.CustomerName != null)
                    {
                        model.SubCustomer = ProvisionalSubCustomer;
                        model.IsProvisional = true;
                    }
                }

            }//Till
            if (CustomerId != null)
            {
                foreach (var item in model.Customers)
                {
                    if (item.CustomerId == CustomerId)
                        model.CustomerDetails = item;
                }
                model.ServiceRequest = new ServiceRequestDto();
                model.ServiceRequest.CallType = CallType;
                model.ServiceRequest.DateCreated = DateTime.Now;
                model.ServiceRequest.FromDate = DateTime.Now;
                model.ServiceRequest.ToDate = DateTime.Now;
                model.ServiceRequest.ConeyanceExpenseDetails = new List<DetailedExpenseDto>();
                model.ServiceRequest.FareExpenseDetails = new List<DetailedExpenseDto>();
                model.ServiceRequest.ServiceDaysList = new List<ServiceDaysDto>();
            }
            if (ProvisionalBillId != null)
            {

                model.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetails((int)ProvisionalBillId);
                model.ServiceRequest.BoardingAmount = model.ServiceRequest.BoardingCharges * model.ServiceRequest.BoardingDays;
                model.ServiceRequest.PocketExpensesAmount = model.ServiceRequest.PocketExpenses * model.ServiceRequest.PocketExpensesDays;
                model.ServiceRequest.ServiceChargeAmount = model.ServiceRequest.ServiceCharge * model.ServiceRequest.ServiceDays;
                model.ServiceRequest.OvertimeAmount = model.ServiceRequest.OvertimeCharges * model.ServiceRequest.OvertimeHours;
                model.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays((int)ProvisionalBillId);
                if (model.ServiceRequest.ServiceDaysList.Count <= 0)
                {
                    model.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                    {
                        FromDate = model.ServiceRequest.FromDate,
                        ToDate = model.ServiceRequest.ToDate
                    });
                }
                model.ServiceRequest.AmountBeforeTax = model.ServiceRequest.BoardingAmount + model.ServiceRequest.PocketExpensesAmount + model.ServiceRequest.ServiceChargeAmount + model.ServiceRequest.OvertimeAmount + model.ServiceRequest.ConveyanceExpenses + model.ServiceRequest.Fare;
                foreach (var item in model.Customers)
                {
                    if (item.CustomerId == model.ServiceRequest.CustomerId)
                        model.CustomerDetails = item;
                }
            }

            model.ServiceRequest.WorkOrderId = (int)WorkOrderId;
            if (ProvisionalBillId == null)
            {
                model.ServiceRequest.ToDate = _serviceRequestHelper.CheckPreviousDate((int)WorkOrderId);
                model.ServiceRequest.FromDate = model.ServiceRequest.ToDate.AddDays(1);
                model.ServiceRequest.ToDate = model.ServiceRequest.FromDate;
            }
            return View(model);
        }
        public ActionResult ProvisionalBillDetails(int ProvisionalBillId)
        {

            ServiceRequestViewModel model = new ServiceRequestViewModel();
            model.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetails((int)ProvisionalBillId);
            model.ServiceRequest.BoardingAmount = model.ServiceRequest.BoardingCharges * model.ServiceRequest.BoardingDays;
            model.ServiceRequest.PocketExpensesAmount = model.ServiceRequest.PocketExpenses * model.ServiceRequest.PocketExpensesDays;
            model.ServiceRequest.ServiceChargeAmount = model.ServiceRequest.ServiceCharge * model.ServiceRequest.ServiceDays;
            model.ServiceRequest.OvertimeAmount = model.ServiceRequest.OvertimeCharges * model.ServiceRequest.OvertimeHours;
            model.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays(ProvisionalBillId);
            if (model.ServiceRequest.ServiceDaysList.Count <= 0)
            {
                model.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                {
                    FromDate = model.ServiceRequest.FromDate,
                    ToDate = model.ServiceRequest.ToDate
                });
            }
            return Json(model.ServiceRequest, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ServiceRequest(ServiceRequestDto request)
        {
            int provisionalBillId = 0;
            if (request.ProvisionalBillId != 0)
            {
                _serviceRequestHelper.EditServiceRequest(request);
                if (request.Type == 2 && request.AuthorizedBy != null)
                {
                    SendProvisonalDetailsByMail(request);
                    return Json(provisionalBillId, JsonRequestBehavior.AllowGet);
                }

            }

            if (request.ProvisionalBillId == 0 && request.Type == 2)
            {
                provisionalBillId = _serviceRequestHelper.SaveServiceRequest(request, request.Type);
                if (request.AuthorizedBy != null)

                {
                    request.ProvisionalBillId = provisionalBillId;
                    SendProvisonalDetailsByMail(request);
                }
            }
            if (request.ProvisionalBillId == 0 && request.Type != 2)
            {

                if (Session["WorkOrderID"] != null)
                    request.WorkOrderId = (int)Session["WorkOrderID"];
                provisionalBillId = _serviceRequestHelper.SaveServiceRequest(request, request.Type);
            }

            if (request.Type == 2)
            {
                return Json(provisionalBillId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string ActionName = $"ServiceRequest(Create PI)  - {provisionalBillId}";
                string TableName = "TblOrder";
                if (ActionName != null)
                {
                    var userInfo = (UserSession)Session["UserInfo"];
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                return Json("success");
            }

        }
        public ActionResult TimeSheetAuthorization(TimeSheetDto vm)
        {
            string response = "";
            response = _serviceRequestHelper.TimeSheetAuthorization(vm);
            WorkOrderViewModel wm = new WorkOrderViewModel();
            wm.TimeSheet = _serviceRequestHelper.GetTimeSheetDetailsByTimeSheetId(vm.TimeSheetId);
            wm.TimeSheetDetails = _serviceRequestHelper.GetTimeSheetDetails(vm.TimeSheetId);
            foreach (var item in wm.TimeSheetDetails)
            {
                List<TimeSheetDetailDto> tsList = new List<TimeSheetDetailDto>();
                TimeSheetDetailDto tsdt = new TimeSheetDetailDto();
                if (wm.WeekDates.ContainsKey(item.WeekNo))
                {
                    tsList = wm.WeekDates[item.WeekNo];
                    tsdt.WorkDate = item.WorkDate;
                    tsdt.TotalHours = item.TotalHours;
                    tsdt.Description = item.Description;
                    tsList.Add(tsdt);
                }
                else
                {
                    tsdt.WorkDate = item.WorkDate;
                    tsdt.TotalHours = item.TotalHours;
                    tsdt.TimeSheetId = item.TimeSheetId;
                    tsdt.Description = item.Description;
                    tsList.Add(tsdt);
                    wm.WeekDates.Add(item.WeekNo.ToString(), tsList);
                }

            }
            wm.WorkOrder = _serviceRequestHelper.GetWorkOrderDetailsByTimeSheet(vm.TimeSheetId);
            var root = Server.MapPath("~/Content/PDF/TimeSheet/");
            var pdfname = String.Format("{0}.pdf", wm.TimeSheet.UserName + "_" + wm.TimeSheet.CustomerName + "_TS");
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            try
            {
                path = Path.GetFullPath(path);
                ViewAsPdf pdf = new ViewAsPdf("TimeSheetPDF", wm) { FileName = pdfname/*, SaveOnServerPath = path*/ };
                byte[] bytes = pdf.BuildPdf(this.ControllerContext);
                SendTimeSheetEmail(pdfname, wm);
                logger.Info("ServiceRequestController: SendTimeSheetEmail" + vm.TimeSheetId + vm.TimeSheetNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TimeSheetAuthorization");
                logger.Error(ex.InnerException, "TimeSheetAuthorization");
                throw ex;
            }
            return Json(response);
        }

        public void SendTimeSheetEmail(string fileName, WorkOrderViewModel vm)
        {

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Dear Sir/Madam,<br />");
            stringBuilder.Append("Please find attached the Time Sheet generated for  <b>" + vm.TimeSheet.UserName + "<br/>.");
            stringBuilder.Append("<br/><br/> Thanks <br/> " + vm.TimeSheet.UserName);
            //GMailer.GmailUsername = "noreply.picanol@gmail.com";
            //GMailer.GmailPassword = "wqhxhsfsehewnpei";
            GMailer mailer = new GMailer();
            string email = _serviceRequestHelper.GetPBEmailIds();
            email = email + ";" + vm.TimeSheet.UserEmail;
            mailer.ToEmail = email;
            mailer.Subject = "Time Sheet - " + vm.TimeSheet.UserName;
            mailer.Body = stringBuilder.ToString();
            mailer.IsHtml = true;
            var root = Server.MapPath("~/Content/PDF/TimeSheet/");
            var pdfname = String.Format("{0}.pdf", vm.TimeSheet.UserName + "_" + vm.TimeSheet.CustomerName + "_TS");
            var path = Path.Combine(root, pdfname);
            mailer.AttachmentPath = path;
            try
            {
                mailer.Send();
                logger.Info("After:SendingTimeSheetEmail" + vm.TimeSheet.TimeSheetNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SendTimeSheetEmail");
                logger.Error(ex.InnerException, "SendTimeSheetEmail");
                throw ex;
            }
        }



        public void SendProvisionalBillEmail(string fileName, ServiceRequestViewModel vm)
        {

            //Generate PDF for invoice
            PicannolEntities context = new PicannolEntities();
            StringBuilder stringBuilder = new StringBuilder();
            var pdfname = "";
            stringBuilder.Append("Dear Sir/Madam,<br />");
            if (vm.SubCustomer != null)
                stringBuilder.Append("Please find attached the proforma invoice generated for  <b>" + vm.SubCustomer.SubCustomerName + "<br/>.");
            else
                stringBuilder.Append("Please find attached the proforma invoice generated for  <b>" + vm.CustomerDetails.CustomerName + "<br/>.");

            stringBuilder.Append("<br/><br/> Thanks <br/> " + vm.ServiceRequest.UserName);
            //GMailer.GmailUsername = "noreply.picanol@gmail.com";
            //GMailer.GmailPassword = "9999907947";
            GMailer mailer = new GMailer();
            string email = _serviceRequestHelper.GetPBEmailIds();
            string userSendEmail = context.tblUsers.Where(x => x.UserId == vm.ServiceRequest.UserId && x.DelInd == false).Select(x => x.Email).FirstOrDefault();
            if (vm.TypeId == 2)
            {

                email += vm.ServiceRequest.AuthorizerEmail + ";" + userSendEmail;

            }
            else
            {
                var userInfo = (UserSession)Session["UserInfo"];
                email = email + userInfo.Email;
            }
            mailer.ToEmail = email;
            if (vm.SubCustomer != null)
                mailer.Subject = "Proforma Invoice - " + vm.SubCustomer.SubCustomerName;

            else
                mailer.Subject = "Proforma Invoice - " + vm.CustomerDetails.CustomerName;
            mailer.Body = stringBuilder.ToString();
            mailer.IsHtml = true;
            var root = Server.MapPath("~/Content/PDF/ProvisionalBill/");
            if (vm.SubCustomer != null)
                pdfname = String.Format("{0}.pdf", vm.SubCustomer.SubCustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);
            else
                pdfname = String.Format("{0}.pdf", vm.CustomerDetails.CustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);

            var path = Path.Combine(root, pdfname);
            mailer.AttachmentPath = path;
            try
            {
                mailer.Send();
                logger.Info("After:SendProvisionalBillEmail" + vm.ServiceRequest.ProvisionalBillId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SendProvisionalBillEmail");
                logger.Error(ex.InnerException, "SendProvisionalBillEmail" + vm.ServiceRequest.ProvisionalBillId);
                throw ex;
            }
        }


        public ActionResult PreviewServiceRequest(int ProvisionalBillId)
        {
            var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            vm.BusinessDetails = _businessHelper.GetBusinessDetails();
            vm.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetails(ProvisionalBillId);
            vm.ServiceRequest = _serviceRequestHelper.GetProvisionalBillDetails(vm.ServiceRequest);
            vm.ServiceRequest.ProvisionalBillId = ProvisionalBillId;
            
            if (vm.ServiceRequest.SubCustomerId != 0)
            {
                vm.SubCustomer = _customerHelper.GetSubCustomerListBySubCustomerId(vm.ServiceRequest.SubCustomerId);
            }
            vm.CustomerDetails = _customerHelper.GetCustomerDetails(vm.ServiceRequest.CustomerId);
            if (vm.CustomerDetails == null)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('This Data is Deleted Please Contact Administrator !'); window.location.href = 'FilterServiceRequest','ServiceRequest';</script>");
            }
            else
            {
                //code by Janesh _16012025
                var ProvisionalBillDetails = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillId).FirstOrDefault();
                if (ProvisionalBillDetails != null && vm.ServiceRequest.SubCustomerId != 0)
                {
                    vm.ServiceRequest.BillingAddress = ProvisionalBillDetails.SubCustomerBillingAddress;
                    vm.ServiceRequest.StateCode = ProvisionalBillDetails.SubCustomerStateCode;
                    vm.ServiceRequest.ShippingAddress = ProvisionalBillDetails.SubCustomerShippingAddress;
                    vm.ServiceRequest.ShippingStateCode = ProvisionalBillDetails.SubCustomerShippingStateCode;
                }
                else if(ProvisionalBillDetails != null&& vm.ServiceRequest.SubCustomerId ==0)
                {
                    vm.ServiceRequest.BillingAddress = ProvisionalBillDetails.BillingAddress;
                    vm.ServiceRequest.StateCode = ProvisionalBillDetails.StateCode;
                    vm.ServiceRequest.ShippingAddress = ProvisionalBillDetails.ShippingAddress;
                    vm.ServiceRequest.ShippingStateCode = ProvisionalBillDetails.ShippingStateCode;

                }

                vm.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays((int)ProvisionalBillId);
                if (vm.ServiceRequest.ServiceDaysList.Count <= 0)
                {
                    vm.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                    {
                        FromDate = vm.ServiceRequest.FromDate,
                        ToDate = vm.ServiceRequest.ToDate
                    });
                }
                Session["ProvisionalBill"] = vm;
                if (userInfo.RoleName == "Accounts")
                {
                    return View("EditPreviewServiceRequest", vm);
                }
                return View(vm);
            }
        }
        //code by Janesh _15012025
        public ActionResult PrintProvisionalBill(int ProvisionalBillId, int WorkOrderId)
        {
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            WorkOrderDto w = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(WorkOrderId);
            vm.WorkOrderNo = w.WorkOrderNo;
            //
            //vm.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(ProvisionalBillId);
            vm.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(ProvisionalBillId, "");
            if (vm.ServiceRequest.SubCustomerId != 0)
            {
                vm.SubCustomer = _customerHelper.GetSubCustomerListBySubCustomerId(vm.ServiceRequest.SubCustomerId);
            }
            //code By janesh_15012025
            if (ProvisionalBillId != null && vm.ServiceRequest.SubCustomerId==0)
            {
                var ProvisionalDetails = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillId).FirstOrDefault();
                vm.ServiceRequest.BillingAddress = ProvisionalDetails.BillingAddress;
                vm.ServiceRequest.StateCode = ProvisionalDetails.StateCode;
                vm.ServiceRequest.ShippingAddress = ProvisionalDetails.ShippingAddress;
                vm.ServiceRequest.ShippingStateCode = ProvisionalDetails.ShippingStateCode;
            }
            else
            {
                var ProvisionalDetails = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillId).FirstOrDefault();
                vm.ServiceRequest.SubCustomerBillingAddress = ProvisionalDetails.SubCustomerBillingAddress;
                vm.ServiceRequest.SubCustomerStateCode = ProvisionalDetails.SubCustomerStateCode;
                

            }
            
            vm.CustomerDetails = _customerHelper.GetCustomerDetails(vm.ServiceRequest.CustomerId);


            vm.ServiceRequest = _serviceRequestHelper.GetProvisionalBillDetails(vm.ServiceRequest);
            vm.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays((int)ProvisionalBillId);
            if (vm.ServiceRequest.ServiceDaysList.Count <= 0)
            {
                vm.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                {
                    FromDate = vm.ServiceRequest.FromDate,
                    ToDate = vm.ServiceRequest.ToDate
                });
            }

            var root = Server.MapPath("~/Content/PDF/ProvisionalBill/");
            //var pdfname = String.Format("{0}.pdf", vm.CustomerDetails.CustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);

            //Coment by Sunit 
            /* var pdfname = (vm.SubCustomer != null) ? (vm.SubCustomer.SubCustomerId > 0) ? String.Format("{0}.pdf", vm.SubCustomer.SubCustomerName + "_" + vm.ServiceRequest.ProvisionalBillId) :
                 String.Format("{0}.pdf", vm.CustomerDetails.CustomerName + "_" + vm.ServiceRequest.ProvisionalBillId) :
                 String.Format("{0}.pdf", vm.CustomerDetails.CustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);*/
            //end

            //Changes by Sunit
            //Date 20-02-2025
            //replace special character from customer name while downloading the pdf
            var customerName = (vm.SubCustomer != null && vm.SubCustomer.SubCustomerId > 0)
            ? vm.SubCustomer.SubCustomerName
            : vm.CustomerDetails.CustomerName;

            // Replace spaces with underscores
            customerName = Regex.Replace(customerName, @"\s+", "_");

            // Remove all other special characters
            customerName = Regex.Replace(customerName, @"[^a-zA-Z0-9_]+", "");
            var pdfname = String.Format("{0}.pdf", customerName + "_" + vm.ServiceRequest.ProvisionalBillId);
            //end

            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            logger.Info("PathofProvisionalBill:-" + path);
            try

            {
                path = Path.GetFullPath(path);
                logger.Info("PathofProvisionalBill:-" + path);
                logger.Info("PreviewServiceRequestPDF" + vm.ServiceRequest.ProvisionalBillId);
                return new ViewAsPdf("PreviewServiceRequestPDF", vm) { FileName = pdfname, SaveOnServerPath = path };
                // return new ViewAsPdf("PrintProvisionalBill", vm) { FileName = pdfname, SaveOnServerPath = path };
            }
            catch (Exception ex)
            {
                logger.Error(ex, "PrintProvisionalBill");
                logger.Error(ex.InnerException, "PrintProvisionalBill");
                throw ex;
            }

        }
        public ActionResult FilterServiceRequest(int PageSize = 10, int PageNo = 1)
        {
            IEnumerable<ZoneDto> Zone = _materialHelper.ZoneList();
            List<SelectListItem> List = new List<SelectListItem>();

            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            string cstname = "";
            var userInfo = (UserSession)Session["UserInfo"];
            ServiceRequestViewModel model = new ServiceRequestViewModel();

            model.RoleId = userInfo.RoleId;
            model.UserId = userInfo.UserId;
            model.Customers = _materialHelper.GetCustomersListSearchVersion1(cstname);
            model.CurrnetPageNo = PageNo;
            model.ServiceRequestList = _serviceRequestHelper.GetServiceRequestsListVersion2(model, PageSize, PageNo);
            model.Username = _serviceRequestHelper.GetUsernameList();
            UserDto ud = new UserDto();
            ud.UserName = "Please Select Username";
            ud.UserId = 0;
            model.Username.Add(ud);
            Session["ProvisionalBillList"] = model;
            model.TotalNumberCounts = model.ServiceRequestList.Count;
            Zone.ToList().ForEach(c => List.Add(new SelectListItem() { Text = c.Zone, Value = c.ZoneId.ToString() }));

            model.Zone = List;

            return View(model);
        }

        public ActionResult ServiceRequestsList(ServiceRequestViewModel svm)
        {
            var userInfo = (UserSession)Session["UserInfo"];
            if (svm.RoleId == null || svm.RoleId == 0)
                svm.RoleId = userInfo.RoleId;
            if (svm.UserId == null || svm.UserId == 0)
                svm.UserId = userInfo.UserId;
            svm.CurrnetPageNo = svm.PageNo;
            List<ServiceRequestDto> requests = _serviceRequestHelper.GetServiceRequestsListVersion6(svm);
            if (svm.TypeId == 2)
            {
                return Json(requests, JsonRequestBehavior.AllowGet);
            }
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            vm.ServiceRequestList = requests;
            vm.TotalNumberCounts = vm.ServiceRequestList.Count;
            vm.CurrentPageNo = svm.PageNo;
            vm.CurrnetPageNo = svm.PageNo;
            return PartialView("_ServiceRequestList", vm);
        }


        public ActionResult CreateCreditNoteV2(int ProvisionalBillId, int workOrderId, string PreviewType, string Initiator, string invoiceNumber,
            decimal ServiceChargeAmount, decimal railairfareAmount, decimal PocketexpenceAmount, decimal LoadingBoardingAmount,
            decimal ConvenyanceInceidentalAmount, decimal OverTimehourAmount, decimal totalCrnPartialAmt)
        {
            invoiceNumber = invoiceNumber.Replace(" ", "");

            CreditNoteEditDto creditnoteEditDto = new CreditNoteEditDto();
            creditnoteEditDto.ProvisionalBillId = ProvisionalBillId;
            creditnoteEditDto.PreviewType = PreviewType;
            creditnoteEditDto.invoiceNumber = invoiceNumber;
            creditnoteEditDto.Initiator = Initiator;
            creditnoteEditDto.ServiceChargeAmount = ServiceChargeAmount;
            creditnoteEditDto.railairfareAmount = railairfareAmount;
            creditnoteEditDto.PocketexpenceAmount = PocketexpenceAmount;
            creditnoteEditDto.LoadingBoardingAmount = LoadingBoardingAmount;
            creditnoteEditDto.ConvenyanceInceidentalAmount = ConvenyanceInceidentalAmount;
            creditnoteEditDto.OverTimehourAmount = OverTimehourAmount;
            creditnoteEditDto.workOrderId = workOrderId;
            Session["CreditNoteEditable"] = creditnoteEditDto;


            InvoiceViewModel vm = new InvoiceViewModel();
            string response = "";
            string file1 = "";
            var actionInitiator = "";
            var userInfo = (UserSession)Session["UserInfo"];
            actionInitiator = ConstantsHelper.SelectAction.CreateCreditNote.ToString();
            string SelectedAction = ConstantsHelper.SelectAction.CreateCreditNote.ToString();
            try
            {
                var woOrder = context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).FirstOrDefault();
                //code by janesh _15012025
                var invoiceDtl = context.tblInvoices.Where(x => x.ProvisionalBillId == ProvisionalBillId && x.Cancelled == false && x.DelInd == false).FirstOrDefault();
                var crnTotalAmount = invoiceDtl.Amount;
                vm.BillingAddresss = invoiceDtl.BillingAddress;
                vm.BillingStateCode = invoiceDtl.StateCode;
                vm.ShippingAddress = invoiceDtl.ShippingAddress;
                vm.ShippingStateCode = invoiceDtl.ShippingStateCode;

                vm.CustomerDetails = _customerHelper.GetCustomerDetails(woOrder.CustomerId);
                if (crnTotalAmount != totalCrnPartialAmt)
                {
                    vm.InvoiceDetails = _serviceRequestHelper.GetPartialCreditNote(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);
                }
                else
                {
                    vm.InvoiceDetails = _serviceRequestHelper.GetInvoiceDetails(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);
                }

                vm.BusinessDetails = _businessHelper.GetBusinessDetails();

                if (vm.InvoiceDetails.ErrorMessage != "")
                {
                    return RedirectToAction("Index", "Error", vm.InvoiceDetails.ErrorMessage);

                }
                else
                {
                    var root = Server.MapPath("~/Content/PDF/CreditNotes/");
                    var tempFileName = "";
                    //end here
                    if (vm.InvoiceDetails.SubCustomerId > 0)
                    {
                        var a = context.tblSubCustomers.Where(x => x.SubCustomerId == vm.InvoiceDetails.SubCustomerId).FirstOrDefault();
                        tempFileName = a.SubCustomerName + "_" + ProvisionalBillId;
                        CustomerDto cdt = new CustomerDto();
                        cdt = _customerHelper.GetSubCustomerDetails(vm.InvoiceDetails.SubCustomerId);
                        vm.CustomerDetails = cdt;
                    }
                    else
                    {
                        tempFileName = vm.CustomerDetails.CustomerName + "_" + ProvisionalBillId;
                    }

                    string fileName = String.Format("{0}.pdf", tempFileName);
                    if (PreviewType == "CreditNote")
                    {
                        logger.Info("CreateCreditNote" + ProvisionalBillId);

                        //record user activity
                        string ActionName = $"Download CRN, PSBID  - {ProvisionalBillId}";
                        string TableName = "tblCreditNote";
                        if (ActionName != null)
                        {
                            _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End

                        return new ViewAsPdf("ServiceCreditNotePDF", vm) { FileName = fileName, };
                    }

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


        public ActionResult CreateDebitNote(int ProvisionalBillId, int workOrderId, string PreviewType, string Initiator, string invoiceNumber,
                   decimal ServiceChargeAmount, decimal railairfareAmount, decimal PocketexpenceAmount, decimal LoadingBoardingAmount,
                   decimal ConvenyanceInceidentalAmount, decimal OverTimehourAmount, decimal crnTotalAmount)
        {
            invoiceNumber = invoiceNumber.Replace(" ", "");
            CreditNoteEditDto creditnoteEditDto = new CreditNoteEditDto();
            creditnoteEditDto.ProvisionalBillId = ProvisionalBillId;
            creditnoteEditDto.PreviewType = PreviewType;

            creditnoteEditDto.invoiceNumber = invoiceNumber;
            creditnoteEditDto.Initiator = Initiator;
            creditnoteEditDto.ServiceChargeAmount = ServiceChargeAmount;

            creditnoteEditDto.railairfareAmount = railairfareAmount;

            creditnoteEditDto.PocketexpenceAmount = PocketexpenceAmount;

            creditnoteEditDto.LoadingBoardingAmount = LoadingBoardingAmount;
            creditnoteEditDto.ConvenyanceInceidentalAmount = ConvenyanceInceidentalAmount;
            creditnoteEditDto.OverTimehourAmount = OverTimehourAmount;

            creditnoteEditDto.workOrderId = workOrderId;
            Session["CreditNoteEditable"] = creditnoteEditDto;


            InvoiceViewModel vm = new InvoiceViewModel();
            string response = "";
            string file1 = "";
            var actionInitiator = "";
            actionInitiator = Initiator;
            var userInfo = (UserSession)Session["UserInfo"];
            string SelectedAction = ConstantsHelper.SelectAction.CreateCreditNote.ToString();
            PreviewType = "DebitNote";
            try
            {
                var woOrder = context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).FirstOrDefault();
                var invoiceDtl = context.tblInvoices.Where(x => x.ProvisionalBillId == ProvisionalBillId && x.Cancelled == false && x.DelInd == false).FirstOrDefault();

                vm.CustomerDetails = _customerHelper.GetCustomerDetails(woOrder.CustomerId);
                //vm.InvoiceDetails = _serviceRequestHelper.GetInvoiceDetails(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);

                //vm.InvoiceDetails = _serviceRequestHelper.GetDeBitNoteInVoiceDtl(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);

                if (invoiceDtl.Amount != crnTotalAmount)
                {
                    vm.InvoiceDetails = _serviceRequestHelper.GetDebitNoteInvoiceDtl(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);
                }
                else
                {
                    vm.InvoiceDetails = _serviceRequestHelper.GetInvoiceDetails(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);
                }

                vm.BusinessDetails = _businessHelper.GetBusinessDetails();

                //Code bY Janesh _15012025
                if (vm.InvoiceDetails != null)
                {
                    var invoiseDetails = context.tblInvoices.Where(x => x.InvoiceNo == vm.InvoiceDetails.InvoiceNo).FirstOrDefault();
                    if (invoiseDetails != null)
                    { vm.BillingAddresss = invoiseDetails.BillingAddress;
                        vm.BillingStateCode = invoiseDetails.StateCode;
                        vm.ShippingAddress = invoiseDetails.ShippingAddress;
                        vm.ShippingStateCode = invoiseDetails.ShippingStateCode;
                    }
                }
                if (vm.InvoiceDetails.ErrorMessage != "")
                {
                    return RedirectToAction("Index", "Error", vm.InvoiceDetails.ErrorMessage);

                }
                else
                {
                    var root = Server.MapPath("~/Content/PDF/DebitNotes/");
                    var tempFileName = "";
                    //end here
                    if (vm.InvoiceDetails.SubCustomerId > 0)
                    {
                        var a = context.tblSubCustomers.Where(x => x.SubCustomerId == vm.InvoiceDetails.SubCustomerId).FirstOrDefault();
                        tempFileName = a.SubCustomerName + "_" + ProvisionalBillId;
                        CustomerDto cdt = new CustomerDto();
                        cdt = _customerHelper.GetSubCustomerDetails(vm.InvoiceDetails.SubCustomerId);
                        vm.CustomerDetails = cdt;
                    }
                    else
                    {
                        tempFileName = vm.CustomerDetails.CustomerName + "_" + ProvisionalBillId;
                    }

                    string fileName = String.Format("{0}.pdf", tempFileName);
                    if (PreviewType == "DebitNote")
                    {
                        logger.Info("CreateDebitNote" + ProvisionalBillId);

                        //record user activity
                        string ActionName = $"Download DBN, PSBID  - {ProvisionalBillId}";
                        string TableName = "tblDebitNote";
                        if (ActionName != null)
                        {
                            _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End

                        return new ViewAsPdf("ServiceDebitNotePDF", vm) { FileName = fileName, };
                    }

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

        public ActionResult ServiceRequestInvoice(int ProvisionalBillId, int workOrderId, string PreviewType, string Initiator, string invoiceNumber/*,int FinalAmount*/)
        {
            PicannolEntities context = new PicannolEntities();
            InvoiceViewModel vm = new InvoiceViewModel();
            var actionInitiator = "";
            actionInitiator = Initiator;
            var userInfo = (UserSession)Session["UserInfo"];
            var woOrder = context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).FirstOrDefault();
            vm.CustomerDetails = _customerHelper.GetCustomerDetails(woOrder.CustomerId);
            vm.InvoiceDetails = _serviceRequestHelper.GetInvoiceDetails(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator, invoiceNumber);
            
            if (vm.InvoiceDetails.ErrorMessage != "")
            {
                TempData["ErrorMessage"] = vm.InvoiceDetails.ErrorMessage.ToString();

                return RedirectToAction("Index", "Error", vm);
            }
            var root = Server.MapPath("~/Content/TempPBInvoice/");
            var tempFileName = "";

            /*
             * Date:23 March 2021
             * Author: Prince Dhiman
             * Comment: Add this below condition for invoice detail of sub customer 
             
             */
            //new delete folder for previewInvoice


            DirectoryInfo di = new DirectoryInfo(root);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }


            //end here
            if (vm.InvoiceDetails.SubCustomerId > 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == vm.InvoiceDetails.SubCustomerId).FirstOrDefault();
                tempFileName = a.SubCustomerName + "_" + ProvisionalBillId;
                CustomerDto cdt = new CustomerDto();
                cdt = _customerHelper.GetSubCustomerDetails(vm.InvoiceDetails.SubCustomerId);
                vm.CustomerDetails = cdt;
            }
            else
            {
                tempFileName = vm.CustomerDetails.CustomerName + "_" + ProvisionalBillId;
            }
            //Code By Janesh_15012025
            if (vm.InvoiceDetails.InvoiceNo != "" && vm.InvoiceDetails.InvoiceNo != null)
            {
                var invoiceDetails = context.tblInvoices.Where(x => x.InvoiceNo == vm.InvoiceDetails.InvoiceNo).FirstOrDefault();
                vm.BillingAddresss = invoiceDetails.BillingAddress;
                vm.ShippingAddress = invoiceDetails.ShippingAddress;
                vm.BillingStateCode = invoiceDetails.StateCode;
                vm.ShippingStateCode = invoiceDetails.ShippingStateCode;

            }
            
            string fileName = String.Format("{0}.pdf", tempFileName);
            if (PreviewType == "PreviewDetails")
            {
                vm.InvoiceDetails.selectedItem = "PreviewDetails";
                logger.Info("ServiceRequestInvoice" + ProvisionalBillId);
                return new ViewAsPdf("ServiceRequestInvoice", vm) { FileName = fileName, };
            }

            var path = Path.Combine(root, fileName);
            path = Path.GetFullPath(path);
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                path = Path.GetFullPath(path);
                logger.Info("ServiceRequestInfo--- line 508" + ProvisionalBillId);
                return new ViewAsPdf("ServiceRequestInvoice", vm) { FileName = fileName/*, SaveOnServerPath = path */};

            }
            catch (Exception ex)
            {
                logger.Error(ex, "ServiceRequestInvoice");
                logger.Error(ex.InnerException, "ServiceRequestInvoice");
                throw ex;
            }
        }


        public ActionResult GetPaymentDetails(string InvoiceNo)
        {
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            vm.OrderPayment = new OrderPaymentDto();
            vm.OrderPayment = _serviceRequestHelper.GetPaymentDetails(InvoiceNo);
            if (vm.OrderPayment != null)
            {
                vm.OrderPayment.PDate = vm.OrderPayment.DatePaid.Date.ToShortDateString();
                var split1 = vm.OrderPayment.PDate.Split('-');
                vm.OrderPayment.PDate = split1[1] + "/" + split1[0] + "/" + split1[2];
            }
            return Json(vm, JsonRequestBehavior.AllowGet);
        }


        #region Cancelled Service Bill

        public ActionResult CancelledServiceBill(int ProvisionalBillId, int workOrderId, string PreviewType, string Initiator)
        {


            PicannolEntities context = new PicannolEntities();
            InvoiceViewModel vm = new InvoiceViewModel();
            var actionInitiator = "";
            actionInitiator = Initiator;
            var userInfo = (UserSession)Session["UserInfo"];

            var woOrder = context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).FirstOrDefault();
            vm.CustomerDetails = _customerHelper.GetCustomerDetails(woOrder.CustomerId);
            vm.InvoiceDetails = _serviceRequestHelper.GetCancelledInvoiceDetail(workOrderId, vm.CustomerDetails, ProvisionalBillId, PreviewType, userInfo.UserId, actionInitiator);

            if (vm.InvoiceDetails.ErrorMessage != "")
            {
                TempData["ErrorMessage"] = vm.InvoiceDetails.ErrorMessage.ToString();

                return RedirectToAction("Index", "Error", vm);
            }
            var root = Server.MapPath("~/Content/TempPBInvoice/");
            var tempFileName = "";

            /*
             * Date:23 March 2021
             * Author: Prince Dhiman
             * Comment: Add this below condition for invoice detail of sub customer 
             
             */
            //new delete folder for previewInvoice
            DirectoryInfo di = new DirectoryInfo(root);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            //end here
            if (vm.InvoiceDetails.SubCustomerId > 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == vm.InvoiceDetails.SubCustomerId).FirstOrDefault();
                tempFileName = a.SubCustomerName + "_" + ProvisionalBillId;
                CustomerDto cdt = new CustomerDto();
                cdt = _customerHelper.GetSubCustomerDetails(vm.InvoiceDetails.SubCustomerId);
                vm.CustomerDetails = cdt;
            }
            else
            {
                tempFileName = vm.CustomerDetails.CustomerName + "_" + ProvisionalBillId;
            }

            string fileName = String.Format("{0}.pdf", tempFileName);
            if (PreviewType == "PreviewDetails")
            {
                vm.InvoiceDetails.selectedItem = "PreviewDetails";
                logger.Info("ServiceRequestInvoice" + ProvisionalBillId);
                return new ViewAsPdf("ServiceRequestInvoice", vm) { FileName = fileName, };
            }

            var path = Path.Combine(root, fileName);
            path = Path.GetFullPath(path);
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                path = Path.GetFullPath(path);
                logger.Info("ServiceRequestInfo--- line 508" + ProvisionalBillId);
                return new ViewAsPdf("ServiceRequestInvoice", vm) { FileName = fileName/*, SaveOnServerPath = path */};

            }
            catch (Exception ex)
            {
                logger.Error(ex, "ServiceRequestInvoice");
                logger.Error(ex.InnerException, "ServiceRequestInvoice");
                throw ex;
            }
        }

        #endregion


        #region get customer detail
        public ActionResult GetCustomerDetails(int CustomerId)
        {
            WorkOrderViewModel model = new WorkOrderViewModel();
            model.CustomerDetails = _customerHelper.GetCustomerDetails(CustomerId);
            model.WorkOrder.FullAddress = model.CustomerDetails.AddressLine1 + model.CustomerDetails.AddressLine2;
            model.WorkOrder.FullAddress += model.CustomerDetails.District == null ? "" : (model.CustomerDetails.District + ",");
            model.WorkOrder.FullAddress += model.CustomerDetails.City == null ? "" : (model.CustomerDetails.City + ",");
            model.WorkOrder.FullAddress += model.CustomerDetails.State == null ? "" : model.CustomerDetails.State;
            model.WorkOrder.FullAddress += model.CustomerDetails.PIN == null ? "" : ("-" + model.CustomerDetails.PIN);
            model.WorkOrder.ContactPerson += model.CustomerDetails.ContactPerson == null ? "" : ("" + model.CustomerDetails.ContactPerson);
            model.WorkOrder.EmailId += model.CustomerDetails.Email == null ? "" : ("" + model.CustomerDetails.Email);
            model.WorkOrder.Mobile += model.CustomerDetails.Mobile == null ? "" : ("" + model.CustomerDetails.Mobile);
            model.WorkOrder.GstIn += model.CustomerDetails.GSTIN == null ? "" : ("" + model.CustomerDetails.GSTIN);

            return Json(model);

        }
        #endregion
        public ActionResult SendEmail(int ProvisionalBillId, int? WorkOrderId, string EmailID, string CName, string MailType, string UserName, int? type)
        {
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            WorkOrderDto w = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(WorkOrderId);
            var response = "";
            vm.WorkOrderNo = w.WorkOrderNo;
            vm.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(ProvisionalBillId, "");
            vm.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays(ProvisionalBillId);
            if (vm.ServiceRequest.ServiceDaysList.Count <= 0)
            {
                vm.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                {
                    FromDate = vm.ServiceRequest.FromDate,
                    ToDate = vm.ServiceRequest.ToDate
                });
            }
            if (vm.ServiceRequest.SubCustomerId != 0)
            {
                vm.SubCustomer = _customerHelper.GetSubCustomerListBySubCustomerId(vm.ServiceRequest.SubCustomerId);
            }

            vm.CustomerDetails = _customerHelper.GetCustomerDetails(vm.ServiceRequest.CustomerId);
            vm.ServiceRequest = _serviceRequestHelper.GetProvisionalBillDetails(vm.ServiceRequest);

            var root = Server.MapPath("~/Content/PDF/ProvisionalBill/");
            string fileName = CName + "_" + ProvisionalBillId;
            string fileName1 = String.Format("{0}.pdf", fileName);
            var path = Path.Combine(root, fileName1);
            path = Path.GetFullPath(path);
            if (System.IO.File.Exists(path))
            {
                var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];

                response = _serviceRequestHelper.SendProvisionalBillEmail(ProvisionalBillId, EmailID, CName, MailType, UserName, userInfo.UserId, type);
            }
            else
            {
                var pdfResult = new ViewAsPdf("PreviewServiceRequestPDF", vm);
                byte[] pdfFile = pdfResult.BuildPdf(this.ControllerContext);
                var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                fileStream.Write(pdfFile, 0, pdfFile.Length);
                fileStream.Close();
                var userInfo = (UserSession)Session["UserInfo"];
                response = _serviceRequestHelper.SendProvisionalBillEmail(ProvisionalBillId, EmailID, CName, MailType, UserName, userInfo.UserId, type);

            }
            if (response == null)
            {
                return Json("Please generate E-Invoice");
            }
            else
            {
                return Json("success", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetServiceRequestDetails(int WorkOrderId)
        {
            ServiceRequestDto serviceRequest = new ServiceRequestDto();
            serviceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(WorkOrderId, "");
            return Json(serviceRequest, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSubCustomerList(int CustomerId)
        {
            List<SubCustomerDto> subCustomerList = new List<SubCustomerDto>();
            subCustomerList = _customerHelper.GetSubCustomerListByCustomerId(CustomerId);
            return Json(subCustomerList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetExcelDownloadParameter(ServiceRequestViewModel svm)
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


        #region work order download parameter
        public JsonResult GetWorkOrderDownloadParameter(WorkOrderViewModel wvm)
        {
            try
            {
                Session["WoExcelParameters"] = wvm;
                return Json("success");
            }
            catch (Exception ex)
            {
                return Json("failure");
                throw ex;
            }
        }
        #endregion


        #region Get Proforma Invoice List
        public ActionResult GetProformaLIst(int PageSize = 10, int PageNo = 1)
        {
            ProformaViewModel model = new ProformaViewModel();
            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            model.CurrentPageNo = PageNo;
            model.List = _materialHelper.GetProformaList(PageSize, PageNo);

            return View(model);
        }
        #endregion


        public ActionResult GetWoExcelParameter()
        {
            WorkOrderViewModel wvm = Session["WoExcelParameters"] as WorkOrderViewModel;
            if (wvm != null)
            {
                var gv = new GridView();
                var userInfo = (UserSession)Session["UserInfo"];
                if (wvm.RoleId == null || wvm.RoleId == 0)
                    wvm.RoleId = userInfo.RoleId;
                if (wvm.UserId == null || wvm.UserId == 0)
                    wvm.UserId = userInfo.UserId;
                ServiceRequestViewModel vm = new ServiceRequestViewModel();
                gv.DataSource = _workOrderHelper.GetWorkOrdersListVersion2(wvm).Select(x => new { x.WorkOrderNo, x.CustomerName, x.AssignedUserName, x.SDate, x.EDate });
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=WorkOrder_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("FilterServiceRequest");
        }

        public ActionResult GetServiceRequestsListForExcel()
        {
            ServiceRequestViewModel svm = Session["ExcelParameters"] as ServiceRequestViewModel;
            if (svm != null)
            {
                var gv = new GridView();
                var userInfo = (UserSession)Session["UserInfo"];
                if (svm.RoleId == null || svm.RoleId == 0)
                    svm.RoleId = userInfo.RoleId;
                if (svm.UserId == null || svm.UserId == 0)
                    svm.UserId = userInfo.UserId;
                svm.CurrnetPageNo = svm.PageNo;
                ServiceRequestViewModel vm = new ServiceRequestViewModel();
                gv.DataSource = _serviceRequestHelper.GetServiceRequestsList(svm).Select(x => new { x.CustomerName, x.ProvisionalBillNo, x.InvoiceNumber, x.LastEmailSendOn, x.TillFromDate, x.EndDate, x.UserName, x.FinalAmount, x.Remark, x.Zone });
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
            return RedirectToAction("FilterServiceRequest");
        }

        #region RecordAllpayment

        public ActionResult GetAllPayemntDetails()
        {
            OrderPaymentDto n = new OrderPaymentDto();
            string cstname = "";
            var f = (from a in context.tblOrderPayments
                     join b in context.tblCustomers on a.CustomerId equals b.CustomerId
                     //join c in context.tblInvoices on a.ProvisionalBillId equals c.ProvisionalBillId
                     where a.DelInd == false
                     //where a.Delind == true
                     select new OrderPaymentDto
                     {
                         OrderPaymentId = a.OrderPaymentId,
                         DatePaid = (DateTime)a.DatePaid,
                         CustomerName = b.CustomerName,
                         ProvisionalBillNo = a.InvoiceNo,
                         //InvoiceNo = c.InvoiceNo,
                         ProvisionalBillId = (int)a.ProvisionalBillId,
                         PaymentType = a.PaymentType,
                         AmountPaid = (decimal)a.AmountPaid != null ? (decimal)a.AmountPaid : 0,
                         TDS = (decimal)a.TDS != null ? (decimal)a.TDS : 0,
                         ReceivedAmount = (decimal)a.ReceivedAmount,
                         InvoiceAmount = (decimal)a.InvoiceAmount,
                         Status = a.Status
                     }).ToList().OrderByDescending(x => x.OrderPaymentId);

            foreach (var item in f)
            {
                var chechkinvoicenumber = context.tblInvoices.Where(x => x.ProvisionalBillId == item.ProvisionalBillId).FirstOrDefault();

                if (chechkinvoicenumber != null)
                {
                    item.InvoiceNo = chechkinvoicenumber.InvoiceNo;
                    item.DatePaid = item.DatePaid.Date;

                    item.PDate = item.DatePaid.Date.ToShortDateString();
                    var split1 = item.PDate.Split('-');
                    item.PDate = split1[1] + "/" + split1[0] + "/" + split1[2];

                }
                else
                {
                    item.PDate = item.DatePaid.Date.ToShortDateString();
                    var split1 = item.PDate.Split('-');
                    item.PDate = split1[1] + "/" + split1[0] + "/" + split1[2];
                }

            }

            f.Select(x => x.Customers = _materialHelper.GetCustomersListSearchVersion1(cstname)).ToList();

            return View("GetAllPayemntDetails", f);
        }
        #endregion

        public ActionResult FilterPaymentList(OrderPaymentDto svm)
        {
            var f = (from a in context.tblOrderPayments
                     join b in context.tblCustomers on a.CustomerId equals b.CustomerId
                     //join c in context.tblInvoices on a.ProvisionalBillId equals c.ProvisionalBillId
                     where a.DelInd == false && a.CustomerId == svm.CustomerId || a.InvoiceNo == svm.ProvisionalBillNo/*||c.InvoiceNo==svm.ProvisionalBillNo*/
                     //where a.Delind == true
                     select new OrderPaymentDto
                     {
                         DatePaid = (DateTime)a.DatePaid,
                         CustomerName = b.CustomerName,
                         ProvisionalBillNo = a.InvoiceNo,
                         //InvoiceNo = c.InvoiceNo,
                         ProvisionalBillId = (int)a.ProvisionalBillId,
                         PaymentType = a.PaymentType,
                         AmountPaid = (a.AmountPaid != null) ? (decimal)a.AmountPaid : 0,
                         TDS = (a.TDS != null) ? (decimal)a.TDS : 0,
                         ReceivedAmount = (decimal)a.ReceivedAmount,
                         InvoiceAmount = (decimal)a.InvoiceAmount,
                         Status = a.Status
                     }).ToList();

            foreach (var item in f)
            {
                var chechkinvoicenumber = context.tblInvoices.Where(x => x.ProvisionalBillId == item.ProvisionalBillId).FirstOrDefault();

                if (chechkinvoicenumber != null)
                {
                    item.InvoiceNo = chechkinvoicenumber.InvoiceNo;
                    item.DatePaid = item.DatePaid.Date;

                    item.PDate = item.DatePaid.Date.ToShortDateString();
                    var split1 = item.PDate.Split('-');
                    item.PDate = split1[1] + "/" + split1[0] + "/" + split1[2];

                }
                else
                {
                    item.PDate = item.DatePaid.Date.ToShortDateString();
                    var split1 = item.PDate.Split('-');
                    item.PDate = split1[1] + "/" + split1[0] + "/" + split1[2];
                }

            }
            string cstname = "";
            f.Select(x => x.Customers = _materialHelper.GetCustomersListSearchVersion1(cstname)).ToList();

            //return View("GetAllPayemntDetails", f);
            return PartialView("FilterPaymentList", f);
        }

        #region SavePymentDetails

        [HttpPost]
        public ActionResult SaveOrderPayments(InvoiceListViewModel vm, List<HttpPostedFileBase> images)
        {
            int i = _serviceRequestHelper.RecordPayment(vm);
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
        #endregion


        #region WorkOrder Management
        public ActionResult WorkOrder(int? id, int? Type)
        {
            WorkOrderViewModel model = new WorkOrderViewModel();
            if (id > 0)
            {
                Session["WorkOrderID"] = id;
                WorkOrderViewModel workOrders = Session["WorkOrderDetails"] as WorkOrderViewModel;
                //model.WorkOrder.WorkOrderId = (int)id;
                model = workOrders;
                model.workOrderImageList = _materialHelper.GetWorkOrderImageList((int)id);//changed
                if (model.WorkOrder.CustomerId != 0)
                {
                    model.WorkOrder.FullAddress = _customerHelper.GetCustomerAddress(model.WorkOrder.CustomerId);//changed
                    model.WorkOrder.GstIn = _customerHelper.GetCustomerDetailsById(model.WorkOrder.CustomerId).GSTIN;

                }

                model.WorkOrder.WorkOrder = workOrders.WorkOrder.WorkOrderNo;
                model.Customers = _materialHelper.GetCustomersList();
                model.Users = _userHelper.GetAllUsers();
                //model.WorkOrder.WorkOrderNo = _workOrderHelper.GetWorkOrderNumber();
                model.Users = model.Users.Where(x => x.RoleId == 2 || x.RoleId == 1 || x.RoleId == 7 || x.RoleId == 5).ToList();
                model.CallTypeList = _serviceRequestHelper.GetServiceCallTypes();
                if (Type == 1)
                {
                    model.Edit = 1;
                }
            }
            else
            {
                model.Customers = _materialHelper.GetCustomersList();
                model.Users = _userHelper.GetAllUsers();
                //commented by prince to get the workeorderno in "Workorder parameter rather then workorderno" 
                //model.WorkOrder.WorkOrderNo = _workOrderHelper.GetWorkOrderNumber();
                model.WorkOrder.WorkOrder = _workOrderHelper.GetWorkOrderNumber();
                model.Users = model.Users.Where(x => x.RoleId == 2 || x.RoleId == 1 || x.RoleId == 7 || x.RoleId == 5).ToList();
                model.CallTypeList = _serviceRequestHelper.GetServiceCallTypes();
                model.WorkOrder.DateCreated = DateTime.Now;
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SaveWorkOrder(WorkOrderViewModel wm)
        {
            var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];
            try
            {
                var path1 = "";
                var root = "";
                bool IsEdit = false;
                var res = new WorkOrderDto();
                if (wm.WorkOrder.WorkOrderId > 0)
                {
                    IsEdit = true;
                    res = _serviceRequestHelper.UpdateWorkOrder(wm);//chnages in impltn
                    var fileName1 = wm.WorkOrder.WorkOrder + ".pdf";
                    Session["SelectedUserAction"] = wm.WorkOrder.WorkOrder;
                    fileName1 = fileName1.Replace("/", "_");
                    root = Server.MapPath("~/Content/PDF/WorkOrders/");
                    path1 = Path.Combine(root, fileName1);
                    path1 = Path.GetFullPath(path1);
                    var pdfResult = new ViewAsPdf("WorkOrderPDF", wm);
                    byte[] pdfFile = pdfResult.BuildPdf(this.ControllerContext);
                    var fileStream = new FileStream(path1, FileMode.Create, FileAccess.Write);
                    fileStream.Write(pdfFile, 0, pdfFile.Length);
                    fileStream.Close();

                    //record user activity
                    if (res.WorkOrderId > 0)
                    {
                        string ActionName = $"Update work order (WorkOrderGUID and Work order no.)  - { res.WorkOrderId + "and" + res.WorkOrderNo}";
                        string TableName = "TblWorkOrder";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                    }
                    //End
                }
                else
                {
                    //Session["WorkOrder"] = wm.WorkOrder;
                    ServiceRequestViewModel model = new ServiceRequestViewModel();
                    model.Customers = _materialHelper.GetCustomersList();


                    foreach (var item in model.Customers)
                    {
                        if (item.CustomerId == wm.WorkOrder.CustomerId)
                            //model.CustomerDetails = item;
                            wm.WorkOrder.CustomerName = item.CustomerName;
                    }
                    // var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];
                    int Id = (int)userInfo.UserId;
                    wm.WorkOrder.CreatedBy = Id;
                    CustomerDto n = _customerHelper.GetCustomerDetails(wm.WorkOrder.CustomerId);
                    wm.WorkOrder.CustomerName = n.CustomerName;
                    res = _workOrderHelper.SaveNewWorkOrder(wm.WorkOrder);
                    var fileName1 = wm.WorkOrder.WorkOrder + ".pdf";
                    Session["SelectedUserAction"] = wm.WorkOrder.WorkOrder;
                    fileName1 = fileName1.Replace("/", "_");
                    root = Server.MapPath("~/Content/PDF/WorkOrders/");
                    path1 = Path.Combine(root, fileName1);
                    path1 = Path.GetFullPath(path1);
                    var pdfResult = new ViewAsPdf("WorkOrderPDF", wm);
                    byte[] pdfFile = pdfResult.BuildPdf(this.ControllerContext);
                    var fileStream = new FileStream(path1, FileMode.Create, FileAccess.Write);
                    fileStream.Write(pdfFile, 0, pdfFile.Length);
                    fileStream.Close();

                    //record user activity
                    if (res.WorkOrderId > 0)
                    {
                        string ActionName = $"Save NewWorkOrder(WorkOrderGUID and Work Order No.)  - { res.WorkOrderId + "and" + res.WorkOrderNo}";
                        string TableName = "TblWorkOrder";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                    }
                    //End

                }

                //Newly Added
                #region for file upload 
                var fileName = "";
                string destFile = string.Empty;
                string extention = string.Empty;
                string sourcePath = Server.MapPath("~/Content/Temp/");
                string targetPath = Server.MapPath("~/Content/PDF/WorkOrderPdf/");
                if (System.IO.Directory.Exists(sourcePath))
                {

                    string[] filess = System.IO.Directory.GetFiles(sourcePath);

                    PicannolEntities _context = new PicannolEntities();

                    foreach (string file in filess)
                    {

                        extention = System.IO.Path.GetExtension(filess[0]);
                        fileName = System.IO.Path.GetFileName(file);
                        destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(filess[0], destFile, true);

                        tblWorkOrderImage img = new tblWorkOrderImage();
                        if (wm.WorkOrder.WorkOrderId == 0)
                        {
                            img.WorkOrderId = res.WorkOrderId;
                        }
                        else
                        {
                            img.WorkOrderId = res.WorkOrderId;
                        }
                        img.ImageName = fileName;
                        img.DelInd = false;
                        _context.tblWorkOrderImages.Add(img);
                        _context.SaveChanges();
                    }
                    RemoveFiles();
                }
                if (IsEdit == true)
                {

                    PicannolEntities _context = new PicannolEntities();
                    res.workOrderImageList = _context.tblWorkOrderImages.Where(x => x.WorkOrderId == res.WorkOrderId && x.DelInd == false).Select(x => new WorkOrderImageDto { ImageName = x.ImageName }).ToList();

                    WorkOrderEmail.SendEditWorkOrderEmail(res);
                }
                else
                {
                    PicannolEntities _context = new PicannolEntities();
                    res.workOrderImageList = _context.tblWorkOrderImages.Where(x => x.WorkOrderId == res.WorkOrderId && x.DelInd == false).Select(x => new WorkOrderImageDto { ImageName = x.ImageName }).ToList();

                    WorkOrderEmail.SendWorkOrderEmail(res);
                }

                return Json("Updated");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public ActionResult WorkOrderPDF(string WorkOrderNo)
        //{
        //    var root = Server.MapPath("~/Content/PDF/WorkOrders/");
        //    string fileName = WorkOrderNo + ".pdf";
        //    fileName = fileName.Replace("/", "_");
        //    var path = Path.Combine(root, fileName);
        //    //var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        //    return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));

        //}
        public ActionResult WorkOrderPDF(int? WorkOrderNo)
        {
            PicannolEntities context = new PicannolEntities();
            var response = new WorkOrderViewModel();
            ServiceRequestViewModel model = new ServiceRequestViewModel();
            var check = context.tblWorkOrders.Where(x => x.WorkOrderId == WorkOrderNo).FirstOrDefault();
            var customerDetails = context.tblCustomers.Where(x => x.CustomerId == check.CustomerId).FirstOrDefault();
            model.Customers = _materialHelper.GetCustomersList();


            foreach (var item in model.Customers)
            {
                if (item.CustomerId == check.CustomerId)
                    //model.CustomerDetails = item;
                    response.WorkOrder.CustomerName = item.CustomerName;
            }
            response.WorkOrder.CreatedBy = check.CreatedBy;
            CustomerDto n = _customerHelper.GetCustomerDetails(check.CustomerId);

            if (n == null)
            {

                return Content("<script language='javascript' type='text/javascript'>alert('This Data is Deleted Please Contact Administrator !'); window.location.href = 'WorkOrderListFilter','ServiceRequest';</script>");
            }
            else

            {

                response.WorkOrder.CustomerName = n.CustomerName;
                response.WorkOrder.CustomerAddress = _customerHelper.GetCustomerAddress(n.CustomerId);
                response.WorkOrder.WorkOrderNo = check.WorkOrderNo;
                response.WorkOrder.AssignedTo = check.AssignedTo;
                response.WorkOrder.ContactPerson = check.ContactPerson;
                response.WorkOrder.ContractNumber = check.ContractNumber;
                response.WorkOrder.Conditions = check.Conditions;
                response.WorkOrder.CreatedBy = check.CreatedBy;
                response.WorkOrder.CustomerId = check.CustomerId;
                response.WorkOrder.DateCreated = check.DateCreated;

                response.WorkOrder.Description = check.Description;
                response.WorkOrder.EmailId = check.EmailId;
                response.WorkOrder.SDate = check.StartDate.ToString();
                response.WorkOrder.EDate = check.EndDate.ToString();
                response.WorkOrder.Mobile = check.Mobile;
                response.WorkOrder.Phone = check.Phone;
                response.WorkOrder.CallType = check.CallType;
                response.WorkOrder.WorkOrderType = check.WorkOrderType;
                var fileName = response.WorkOrder.WorkOrderNo + ".pdf";
                return new ViewAsPdf("WorkOrderPDF", response) { FileName = fileName };
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ImageTest(HttpPostedFileBase file)
        {
            try
            {
                string filename = string.Empty;
                if (file != null)
                {
                    filename = file.FileName.Replace("_", "").Replace(" ", "").Replace("-", "");
                    string extension = Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Temp"), filename);
                    file.SaveAs(path);
                }
                string[] filecheck = filename.Split('.');
                string name = filecheck[0];
                return Json(name, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;

            }



        }
        public void RemoveFiles()
        {
            string sourcePath = Server.MapPath("~/Content/Temp/");
            string[] files = System.IO.Directory.GetFiles(sourcePath);
            foreach (string file in files)
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(sourcePath, file)))
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (System.IO.IOException ex)
                    {
                        return;
                    }
                }
            }
        }

        public ActionResult RemoveImage(string imageName)
        {
            PicannolEntities context = new PicannolEntities();
            var vp = context.tblWorkOrderImages.Where(x => x.ImageName == imageName && x.DelInd == false).ToList();
            foreach (var item in vp)
            {
                item.DelInd = true;
                // item.Main = false;
                context.Entry(item).State = System.Data.EntityState.Modified;
                context.SaveChanges();

            }
            return Json("Success");
        }
        public ActionResult RemoveFile(string imageName)
        {
            string rootFolderPath = Server.MapPath("~/Content/Temp/");
            string filesToDelete = imageName;
            //string[] fileList = System.IO.Directory.GetFiles(rootFolderPath, filesToDelete);
            string[] files = System.IO.Directory.GetFiles(rootFolderPath);
            foreach (var item in files)
            {
                var filename = System.IO.Path.GetFileName(item);
                string[] file = filename.Split('.');
                string name = file[0];
                if (name == imageName)
                {
                    System.IO.File.Delete(item);
                }
            }

            return Json(imageName, JsonRequestBehavior.AllowGet);
        }

        #endregion



        public ActionResult DownloadAndDeleteWorkOrder()
        {
            string WorkOrderNo = (string)Session["SelectedUserAction"];
            var root = Server.MapPath("~/Content/PDF/WorkOrders/");
            string fileName = WorkOrderNo + ".pdf";
            fileName = fileName.Replace("/", "_");
            var path = Path.Combine(root, fileName);
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(fs, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));

        }

        public ActionResult SendWOEmailCustomer(WorkOrderDto WO)
        {
            string response = _workOrderHelper.SendWOEmailCustomer(WO);
            return Json(response);
        }
        public ActionResult WorkOrdersList(WorkOrderViewModel wvm)
        {
            var userInfo = (UserSession)Session["UserInfo"];
            if (wvm.RoleId == null || wvm.RoleId == 0)
                wvm.RoleId = userInfo.RoleId;
            if (wvm.UserId == null || wvm.UserId == 0)
                wvm.UserId = userInfo.UserId;

            // List<WorkOrderDto> workOrders = _workOrderHelper.GetWorkOrdersList(wvm);
            List<WorkOrderDto> workOrders = _workOrderHelper.GetWorkOrdersListVersion2(wvm);
            if (wvm.TypeId == 2)
            {
                return Json(workOrders, JsonRequestBehavior.AllowGet);
            }

            WorkOrderViewModel vm = new WorkOrderViewModel();
            vm.WorkOrdersList = workOrders;
            Session["WorkOrderList"] = vm;

            vm.TotalNumberRecord = vm.WorkOrdersList.Count;
            vm.CurrnetPageNo = wvm.PageNo;
            return PartialView("_WorkOrderListAfterFilter", vm);
        }
        public ActionResult WorkOrderDetails(int WorkOrderId, int? Type)
        {
            WorkOrderViewModel vm = new WorkOrderViewModel();
            WorkOrderViewModel workOrders = Session["WorkOrderList"] as WorkOrderViewModel;
            foreach (var item in workOrders.WorkOrdersList)
            {
                if (item.WorkOrderId == WorkOrderId)
                {
                    vm.WorkOrder = item;
                    break;
                }
            }
            if (Type == 1)
            {
                vm.Edit = 1;
            }
            Session["WorkOrderDetails"] = vm;

            return RedirectToAction("WorkOrder", "ServiceRequest", new { id = WorkOrderId });
        }
        public ActionResult WorkOrderListFilter(int PageSize = 10, int PageNo = 1)
        {
            WorkOrderViewModel model = new WorkOrderViewModel();
            model.Customers = _materialHelper.GetCustomersList();
            model.Users = _userHelper.GetAllUsers();
            var userInfo = (UserSession)Session["UserInfo"];
            model.RoleId = userInfo.RoleId;
            model.UserId = userInfo.UserId;

            //List<WorkOrderDto> workOrders = _workOrderHelper.GetWorkOrdersList(model);
            List<WorkOrderDto> workOrders = _workOrderHelper.GetWorkOrdersListVersion1(model, PageSize, PageNo);
            model.WorkOrdersList = workOrders;
            WorkOrderViewModel vm = new WorkOrderViewModel();
            model.CurrnetPageNo = PageNo;
            vm.WorkOrdersList = workOrders;
            Session["WorkOrderList"] = vm;
            model.TotalNumberRecord = model.WorkOrdersList.Count;
            return View(model);

        }

        #endregion

        #region TimeSheet Management
        public ActionResult TimeSheet(int ProvisionalBillId, int workOrderId, int? type)
        {
            WorkOrderViewModel vm = new WorkOrderViewModel();
            //int ? ProvisionalBID = ProvisionalBillId != 14? ProvisionalBillId : 14;
            // ProvisionalBillId = 14;
            vm = _workOrderHelper.GetTimeSheetDetailsByWorkOrder(ProvisionalBillId, workOrderId);

            try
            {
                if (type == 2)
                {
                    WorkOrderDto woOrder = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(workOrderId);
                    vm.WorkOrder = woOrder;
                    return Json(vm, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    //var w = (WorkOrderViewModel)Session["WorkOrderList"];
                    //var woList = w.WorkOrdersList;

                    WorkOrderDto wo = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(workOrderId);
                    //changed
                    var CustomerAddress = _customerHelper.CheckCustomerDeatils(ProvisionalBillId, wo.CustomerId);
                    if (CustomerAddress.SubCustomerId != 0)
                    {
                        wo.CustomerAddress = _customerHelper.GetSubCustomerAddress(CustomerAddress.SubCustomerId);
                        wo.CustomerName = CustomerAddress.SubCustomerName;
                    }
                    else
                    {
                        wo.CustomerAddress = _customerHelper.GetCustomerAddress(wo.CustomerId);
                        wo.CustomerName = CustomerAddress.CustomerName;
                    }//Till

                    vm.WorkOrder = wo;
                    Session["TimeSheetViewModel"] = vm;
                    //vm.TimeSheet.ProvisionalBillId = ProvisionalBillId;
                    var provisionalId = ProvisionalBillId;
                    vm.TimeSheet.ProvisionalBillId = provisionalId;

                }
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = ex.Message.ToString();

            }
            return View(vm);
        }

        public ActionResult SaveTimeSheet(WorkOrderViewModel vm)
        {
            string response = "";
            try
            {
                response = _workOrderHelper.SaveTimeSheet(vm.TimeSheet, vm.TimeSheetDetails);



                string ActionName = $"SaveTimeSheet CustomerID - {vm.CustomerId}";
                string TableName = "tblTimeSheetDetails";
                if (ActionName != null)
                {
                    var userInfo = (UserSession)Session["UserInfo"];
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }


            }
            catch (Exception ex)
            {
                response = "OOPS!! Something went wrong. Please try again";
            }



            return Json(response);
        }

        [Obsolete]
        public ActionResult PrintTimeSheet(int ProvisionalBillId)
        {
            WorkOrderViewModel vm = new WorkOrderViewModel();
            //vm.TimeSheet = _workOrderHelper.GetTimeSheetNumber(workOrderId);
            //vm.TimeSheetDetails = _workOrderHelper.GetTimeSheetDetails(vm.TimeSheet.TimeSheetId);

            //string fileName = vm.TimeSheet.TimeSheetNo + "_" + vm.TimeSheet.CustomerName + "_TS.pdf";
            vm = (WorkOrderViewModel)Session["TimeSheetViewModel"];
            string fileName = vm.TimeSheet.UserName + "_" + vm.TimeSheet.CustomerName + "_TS.pdf";
            var root = Server.MapPath("~/Content/PDF/TimeSheets/");
            var path = Path.Combine(root, fileName);
            try
            {
                //delete folder for Timesheet by himanshu


                //end here
                //if (System.IO.File.Exists(path))
                //{
                //    return File(path, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(path));
                //}
                //else
                //{
                //delete folder for Timesheet by himanshu

                DirectoryInfo di = new DirectoryInfo(root);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                //end here
                path = Path.GetFullPath(path);
                vm.TimeSheet.CImagePath = vm.TimeSheet.TimeSheetId + ".png";
                vm.TimeSheet.UImagePath = vm.TimeSheet.TimeSheetId + "_" + vm.TimeSheet.UserId + ".png";
                return new ViewAsPdf("TimeSheetPDF", vm) { FileName = fileName/*, SaveOnServerPath = path*/ };
                //}
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ActionResult ProvisionalBillList(int WorkOrderId, int? Type)
        {
            //List<ServiceRequestDto> ProvisionalBillList = _serviceRequestHelper.ProvisionalBillListByWorkOrderId(WorkOrderId);
            //ServiceRequestDto details = _serviceRequestHelper.GetDetails(WorkOrderId);
            //ServiceRequestDto sr = ProvisionalBillList.FirstOrDefault();
            ServiceRequestViewModel srm = new ServiceRequestViewModel();
            //srm.ServiceRequestList = ProvisionalBillList;
            PicannolEntities _context = new PicannolEntities();

            srm.ServiceRequest = _serviceRequestHelper.getWorkOrderDetails(WorkOrderId);
            srm.ServiceRequestList = _serviceRequestHelper.GetProvisionalBillByWorkOrder(WorkOrderId);
            if (Type == 2)
            {
                return Json(srm, JsonRequestBehavior.AllowGet);
            }
            return View(srm);
        }



        public ActionResult DeleteProvisionalBill(int ProvisionalBillId, int WorkOrderId)
        {
            PicannolEntities _context = new PicannolEntities();
            tblProvisionalBill p = new tblProvisionalBill();

            p = _context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillId).FirstOrDefault();
            p.DelInd = true;

            _context.Entry(p).State = EntityState.Modified;
            _context.SaveChanges();
            string ActionName = $"DeleteProvisionalBill(ProvisionalBillId)  - {ProvisionalBillId}";
            string TableName = "tblProvisionalBills";
            if (ActionName != null)
            {
                var userInfo = (UserSession)HttpContext.Session["UserInfo"];
                _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
            }
            return RedirectToAction("ProvisionalBillList", new { WorkOrderId = WorkOrderId });
        }

        public ActionResult GetProvisionalBillName(int ProvisionalBillId)
        {
            PicannolEntities _context = new PicannolEntities();
            var p = (from a in _context.tblProvisionalBills
                     join b in _context.tblCustomers on a.CustomerId equals b.CustomerId
                     where a.ProvisionalBillId == ProvisionalBillId
                     select new
                     {
                         c = b.CustomerName
                     }).FirstOrDefault();
            var pdfname = String.Format("{0}.pdf", p.c + "_" + ProvisionalBillId); ;
            return Json(pdfname, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region for EditPreviewServiceRequest
        public JsonResult EditPreviewProvisonalBill(ServiceRequestDto request)
        {
            request.Type = 1;
            if (request.ProvisionalBillId != 0)
            {
                _serviceRequestHelper.EditServiceRequest(request);
            }
            var response = new ServiceRequestDto
            {
                WorkOrderId = request.WorkOrderId,
                ProvisionalBillId = request.ProvisionalBillId
            };

            return Json(response);
        }

        #endregion

        #region Private methods
        private void SendProvisonalDetailsByMail(ServiceRequestDto request)
        {

            var pdfname = "";
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            vm.TypeId = request.Type;
            vm.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(request.ProvisionalBillId, "");
            if (vm.ServiceRequest.SubCustomerId > 0)
                vm.SubCustomer = _customerHelper.GetSubCustomerListBySubCustomerId(vm.ServiceRequest.SubCustomerId);

            vm.CustomerDetails = _customerHelper.GetCustomerDetails(vm.ServiceRequest.CustomerId);
            vm.ServiceRequest = _serviceRequestHelper.GetProvisionalBillDetails(vm.ServiceRequest);
            var root = Server.MapPath("~/Content/PDF/ProvisionalBill/");
            if (vm.ServiceRequest.SubCustomerId > 0)
                pdfname = String.Format("{0}.pdf", vm.SubCustomer.SubCustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);
            else
                pdfname = String.Format("{0}.pdf", vm.CustomerDetails.CustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);

            var path = Path.Combine(root, pdfname);
            vm.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays(request.ProvisionalBillId);
            if (vm.ServiceRequest.ServiceDaysList.Count() <= 0)
            {
                vm.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                {
                    FromDate = vm.ServiceRequest.FromDate,
                    ToDate = vm.ServiceRequest.ToDate
                });
            }
            vm.WorkOrderNo = _serviceRequestHelper.GetWorkOrderByWorkOrderId(request.WorkOrderId);
            vm.ServiceRequest.UserId = request.UserId;
            path = Path.GetFullPath(path);

            try
            {
                path = Path.GetFullPath(path);
                ViewAsPdf pdf = new ViewAsPdf("PreviewServiceRequestPDF", vm) { FileName = pdfname/*, SaveOnServerPath = path*/ };
                byte[] bytes = pdf.BuildPdf(this.ControllerContext);
                SendProvisionalBillEmail(pdfname, vm);
                logger.Info("SendProvisionalBillEmail", vm.ServiceRequest.ProvisionalBillId);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public JsonResult SaveEmailRecord(ServiceRequestDto dto)
        {
            var response = _serviceRequestHelper.SaveEmailRecord(dto);
            if (response == "sucess")
            {
                string ActionName = $"SaveEmailRecord(SendInvoiceTo)  - {dto.CustomerName}";
                string TableName = "tblInvoices";
                if (ActionName != null)
                {
                    var userInfo = (UserSession)Session["UserInfo"];
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }

            return Json(response);
        }
        [HttpPost]
        public JsonResult CancelPB(int id)
        {
            var response = _serviceRequestHelper.CancelProvisonalBill(id);
            if (response == "Success")
            {
                string ActionName = $"CancelPB, (PSBID)  - {id}";
                string TableName = "tblProvisionalBills";
                if (ActionName != null)
                {
                    var userInfo = (UserSession)Session["UserInfo"];
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Paid(int id)
        //{
        //    var response = _serviceRequestHelper.PaidProvisonalBill(id);
        //    if (response == "Success")
        //    {
        //        string ActionName = $"Paid, (PSBID)  - {id}";
        //        string TableName = "tblProvisionalBills";
        //        if (ActionName != null)
        //        {
        //            var userInfo = (UserSession)Session["UserInfo"];
        //            _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
        //        }
        //    }
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}



        //Add new method 
        public JsonResult CancelInvoiceDetails(string InvoiceNumber)
        {

            var userInfo = (UserSession)Session["UserInfo"];
            var response = _serviceRequestHelper.CancelProvisionalInvoice(InvoiceNumber, userInfo.UserId);
            if (response == "Success")
            {
                string ActionName = $"Cancelled, Inv No.  - {InvoiceNumber}";
                string TableName = "tblEInvoice";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        #endregion




        //Cancel Credit Note
        public JsonResult CancelCreditNote(string InvoiceNumber)
        {

            var userInfo = (UserSession)Session["UserInfo"];
            var response = _serviceRequestHelper.CancelCreditNoteDtl(InvoiceNumber, userInfo.UserId);
            if (response == "Success")
            {
                string ActionName = $"Cancelled, Inv No.  - {InvoiceNumber}";
                string TableName = "tblEInvoice";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        //End



        #region For Api

        public ServiceRequestViewModel V1ServiceRequest(int? ProvisionalBillId, int? CustomerId, string CallType, int? WorkOrderId)
        {
            ServiceRequestViewModel model = new ServiceRequestViewModel();

            model.Customers = _materialHelper.GetCustomersList();
            //changed
            model.SubCustomerList = _customerHelper.GetSubCustomerListByCustomerId((int)CustomerId);
            if (model.SubCustomerList.Count > 0)
            {
                model.CustomerIsEdit = true;
                if (ProvisionalBillId != null)
                {
                    var ProvisionalSubCustomer = _customerHelper.GetSubCustomerByProvisionalBillId((int)ProvisionalBillId);
                    if (ProvisionalSubCustomer.CustomerName != null)
                    {
                        model.SubCustomer = ProvisionalSubCustomer;
                        model.IsProvisional = true;
                    }
                }

            }//Till
            if (CustomerId != null)
            {

                foreach (var item in model.Customers)
                {
                    if (item.CustomerId == CustomerId)
                        model.CustomerDetails = item;

                }
                model.ServiceRequest = new ServiceRequestDto();
                model.ServiceRequest.CallType = CallType;
                model.ServiceRequest.DateCreated = DateTime.Now;
                model.ServiceRequest.FromDate = DateTime.Now;
                model.ServiceRequest.ToDate = DateTime.Now;
                model.ServiceRequest.ConeyanceExpenseDetails = new List<DetailedExpenseDto>();
                model.ServiceRequest.FareExpenseDetails = new List<DetailedExpenseDto>();
                model.ServiceRequest.ServiceDaysList = new List<ServiceDaysDto>();
            }
            if (ProvisionalBillId != null)
            {

                model.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetails((int)ProvisionalBillId);
                model.ServiceRequest.BoardingAmount = model.ServiceRequest.BoardingCharges * model.ServiceRequest.BoardingDays;
                model.ServiceRequest.PocketExpensesAmount = model.ServiceRequest.PocketExpenses * model.ServiceRequest.PocketExpensesDays;
                model.ServiceRequest.ServiceChargeAmount = model.ServiceRequest.ServiceCharge * model.ServiceRequest.ServiceDays;
                model.ServiceRequest.OvertimeAmount = model.ServiceRequest.OvertimeCharges * model.ServiceRequest.OvertimeHours;
                model.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays((int)ProvisionalBillId);
                if (model.ServiceRequest.ServiceDaysList.Count <= 0)
                {
                    model.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                    {
                        FromDate = model.ServiceRequest.FromDate,
                        ToDate = model.ServiceRequest.ToDate
                    });
                }
                model.ServiceRequest.AmountBeforeTax = model.ServiceRequest.BoardingAmount + model.ServiceRequest.PocketExpensesAmount + model.ServiceRequest.ServiceChargeAmount + model.ServiceRequest.OvertimeAmount + model.ServiceRequest.ConveyanceExpenses + model.ServiceRequest.Fare;
                foreach (var item in model.Customers)
                {
                    if (item.CustomerId == model.ServiceRequest.CustomerId)
                        model.CustomerDetails = item;
                }
            }

            model.ServiceRequest.WorkOrderId = (int)WorkOrderId;
            if (ProvisionalBillId == null)
            {
                model.ServiceRequest.ToDate = _serviceRequestHelper.CheckPreviousDate((int)WorkOrderId);
                model.ServiceRequest.FromDate = model.ServiceRequest.ToDate.AddDays(1);
                model.ServiceRequest.ToDate = model.ServiceRequest.FromDate;
            }
            return model;
        }

        public WorkOrderViewModel V1TimeSheet(int ProvisionalBillId, int workOrderId, int? type)
        {
            WorkOrderViewModel vm = new WorkOrderViewModel();
            vm = _workOrderHelper.GetTimeSheetDetailsByWorkOrder(ProvisionalBillId, workOrderId);
            if (type == 2)
            {
                WorkOrderDto woOrder = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(workOrderId);
                vm.WorkOrder = woOrder;
            }
            return vm;
        }

        public ServiceRequestViewModel V1ProvisionalBillDetails(int ProvisionalBillId)
        {

            ServiceRequestViewModel model = new ServiceRequestViewModel();
            model.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetails((int)ProvisionalBillId);
            model.ServiceRequest.BoardingAmount = model.ServiceRequest.BoardingCharges * model.ServiceRequest.BoardingDays;
            model.ServiceRequest.PocketExpensesAmount = model.ServiceRequest.PocketExpenses * model.ServiceRequest.PocketExpensesDays;
            model.ServiceRequest.ServiceChargeAmount = model.ServiceRequest.ServiceCharge * model.ServiceRequest.ServiceDays;
            model.ServiceRequest.OvertimeAmount = model.ServiceRequest.OvertimeCharges * model.ServiceRequest.OvertimeHours;
            model.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays(ProvisionalBillId);
            if (model.ServiceRequest.ServiceDaysList.Count <= 0)
            {
                model.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                {
                    FromDate = model.ServiceRequest.FromDate,
                    ToDate = model.ServiceRequest.ToDate
                });
            }
            return model;
        }


        public List<ServiceRequestDto> V1ServiceRequestsList(ServiceRequestViewModel svm)
        {
            List<ServiceRequestDto> requests = _serviceRequestHelper.GetServiceRequestsList(svm);
            if (svm.TypeId == 2)
            {
                return requests;
                // return Json(requests, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }



        public ServiceRequestViewModel V1ProvisionalBillList(int WorkOrderId, int? Type)
        {
            ServiceRequestViewModel srm = new ServiceRequestViewModel();
            PicannolEntities _context = new PicannolEntities();

            srm.ServiceRequest = _serviceRequestHelper.getWorkOrderDetails(WorkOrderId);
            srm.ServiceRequestList = _serviceRequestHelper.GetProvisionalBillByWorkOrder(WorkOrderId);
            if (Type == 2)
            {
                return srm;
            }
            else
            {
                return null;
            }
        }


        public string V1TimeSheetAuthorization(TimeSheetDto vm)
        {
            string response = "";
            response = _serviceRequestHelper.TimeSheetAuthorization(vm);
            WorkOrderViewModel wm = new WorkOrderViewModel();
            wm.TimeSheet = _serviceRequestHelper.GetTimeSheetDetailsByTimeSheetId(vm.TimeSheetId);
            wm.TimeSheetDetails = _serviceRequestHelper.GetTimeSheetDetails(vm.TimeSheetId);
            foreach (var item in wm.TimeSheetDetails)
            {

                List<TimeSheetDetailDto> tsList = new List<TimeSheetDetailDto>();
                TimeSheetDetailDto tsdt = new TimeSheetDetailDto();
                if (wm.WeekDates.ContainsKey(item.WeekNo))
                {
                    tsList = wm.WeekDates[item.WeekNo];
                    tsdt.WorkDate = item.WorkDate;
                    tsdt.TotalHours = item.TotalHours;
                    tsdt.Description = item.Description;
                    tsList.Add(tsdt);
                }
                else
                {
                    tsdt.WorkDate = item.WorkDate;
                    tsdt.TotalHours = item.TotalHours;
                    tsdt.TimeSheetId = item.TimeSheetId;
                    tsdt.Description = item.Description;
                    tsList.Add(tsdt);
                    wm.WeekDates.Add(item.WeekNo.ToString(), tsList);
                }

            }
            wm.WorkOrder = _serviceRequestHelper.GetWorkOrderDetailsByTimeSheet(vm.TimeSheetId);
            var root = Server.MapPath("~/Content/PDF/TimeSheet/");
            var pdfname = String.Format("{0}.pdf", wm.TimeSheet.UserName + "_" + wm.TimeSheet.CustomerName + "_TS");
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            try
            {
                path = Path.GetFullPath(path);
                ViewAsPdf pdf = new ViewAsPdf("TimeSheetPDF", wm) { FileName = pdfname/*, SaveOnServerPath = path*/ };
                byte[] bytes = pdf.BuildPdf(this.ControllerContext);
                SendTimeSheetEmail(pdfname, wm);
                logger.Info("SendProvisionalBillEmail", vm.TimeSheetNo + "" + vm.TimeSheetId);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return response;       //Json(response);
        }


        public List<SubCustomerDto> V1GetSubCustomerList(int CustomerId)
        {
            List<SubCustomerDto> subCustomerList = new List<SubCustomerDto>();
            subCustomerList = _customerHelper.GetSubCustomerListByCustomerId(CustomerId);
            return subCustomerList;       //Json(subCustomerList, JsonRequestBehavior.AllowGet);
        }

        public List<WorkOrderDto> V1WorkOrdersList(WorkOrderViewModel wvm)
        {
            List<WorkOrderDto> workOrders = _workOrderHelper.GetWorkOrdersList(wvm);
            if (wvm.TypeId == 2)
            {
                return workOrders;        //Json(workOrders, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }


        public ServiceRequestDto V1GetServiceRequestDetails(int WorkOrderId)
        {
            ServiceRequestDto serviceRequest = new ServiceRequestDto();
            serviceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(WorkOrderId, "");
            return serviceRequest;       //Json(serviceRequest, JsonRequestBehavior.AllowGet);
        }


        public string V1SendEmail(int ProvisionalBillId, int? WorkOrderId, string EmailID, string CName, string MailType, string UserName, int? type)
        {
            ServiceRequestViewModel vm = new ServiceRequestViewModel();
            WorkOrderDto w = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(WorkOrderId);
            var response = "";
            vm.WorkOrderNo = w.WorkOrderNo;

            vm.ServiceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(ProvisionalBillId, "");
            vm.ServiceRequest.ServiceDaysList = _serviceRequestHelper.GetServiceBillDays(ProvisionalBillId);
            if (vm.ServiceRequest.ServiceDaysList.Count <= 0)
            {
                vm.ServiceRequest.ServiceDaysList.Add(new ServiceDaysDto
                {
                    FromDate = vm.ServiceRequest.FromDate,
                    ToDate = vm.ServiceRequest.ToDate
                });
            }
            if (vm.ServiceRequest.SubCustomerId != 0)
            {
                vm.SubCustomer = _customerHelper.GetSubCustomerListBySubCustomerId(vm.ServiceRequest.SubCustomerId);
            }

            vm.CustomerDetails = _customerHelper.GetCustomerDetails(vm.ServiceRequest.CustomerId);
            vm.ServiceRequest = _serviceRequestHelper.GetProvisionalBillDetails(vm.ServiceRequest);
            var root = Server.MapPath("~/Content/PDF/ProvisionalBill/");
            string fileName = CName + "_" + ProvisionalBillId;
            string fileName1 = String.Format("{0}.pdf", fileName);
            var path = Path.Combine(root, fileName1);
            path = Path.GetFullPath(path);
            if (System.IO.File.Exists(path))
            {
                var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];
                response = _serviceRequestHelper.SendProvisionalBillEmail(ProvisionalBillId, EmailID, CName, MailType, UserName, userInfo.UserId, type);
                logger.Info("SendProvisionalBillEmail", ProvisionalBillId + "");
            }
            else
            {
                var pdfResult = new ViewAsPdf("PreviewServiceRequestPDF", vm);
                byte[] pdfFile = pdfResult.BuildPdf(this.ControllerContext);
                var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                fileStream.Write(pdfFile, 0, pdfFile.Length);
                fileStream.Close();
                var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];
                response = _serviceRequestHelper.SendProvisionalBillEmail(ProvisionalBillId, EmailID, CName, MailType, UserName, userInfo.UserId, type);
                logger.Info("SendProvisionalBillEmail", ProvisionalBillId + "");

            }
            if (type == 2)
            {
                return "success"; //Json("success", JsonRequestBehavior.AllowGet);
            }
            if (response == null)
            {
                return "Please generate e-invoice";   //Json("Please Generate invoice");
            }
            return "success";       //Json("success");

        }

        public string V1GetProvisionalBillName(int ProvisionalBillId)
        {
            PicannolEntities _context = new PicannolEntities();
            var p = (from a in _context.tblProvisionalBills
                     join b in _context.tblCustomers on a.CustomerId equals b.CustomerId
                     where a.ProvisionalBillId == ProvisionalBillId
                     select new
                     {
                         c = b.CustomerName
                     }).FirstOrDefault();
            var pdfname = String.Format("{0}.pdf", p.c + "_" + ProvisionalBillId); ;
            return pdfname;     //Json(pdfname, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadProvisionalBillPDF(int Id)
        {
            try
            {
                PicannolEntities _context = new PicannolEntities();
                var WorkOrderDetails = (from a in _context.tblTimeSheets
                                        join b in _context.tblUsers on a.UserId equals b.UserId
                                        join c in _context.tblCustomers on a.CustomerId equals c.CustomerId
                                        where a.ProvisionalBillId == Id && a.DelInd == false
                                        select new TimeSheetDto
                                        {
                                            UserName = b.UserName,
                                            CustomerName = c.CustomerName
                                        }).FirstOrDefault();

                var root = Server.MapPath("~/Content/PDF/TimeSheet/");
                var pdfname = String.Format("{0}.pdf", WorkOrderDetails.UserName + "_" + WorkOrderDetails.CustomerName + "_TS");
                //pdfname = pdfname + ".pdf";
                Response.ContentType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + pdfname);
                Response.TransmitFile(root + pdfname);
                Response.End();
                logger.Info("DownloadProvisionalBillPDF", pdfname + "");
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}