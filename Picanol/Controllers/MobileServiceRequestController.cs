using Newtonsoft.Json;
using NLog;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.Utils;
using Picanol.ViewModels;
using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
 //Created This mobile service Request Controller to seprate mobile Api from portal and add authentication on them 
 //While Creating APIController. facing Some issue with Json Format. that's why use this approach
    [Authorize]
    public class MobileServiceRequestController : Controller
    {
        private readonly MaterialHelper _materialHelper;
        private readonly CustomerHelper _customerHelper;
        private readonly ServiceRequestHelper _serviceRequestHelper;
        private readonly WorkOrderHelper _workOrderHelper;
        private readonly TimeSheetHelper _timeSheetHelper;
        //private readonly UserHelper _userHelper;

        public MobileServiceRequestController()
        {
            _materialHelper = new MaterialHelper(this);
            _customerHelper = new CustomerHelper(this);
            _serviceRequestHelper = new ServiceRequestHelper(this);
            _workOrderHelper = new WorkOrderHelper(this);
           // _userHelper = new UserHelper(this);
            _timeSheetHelper = new TimeSheetHelper(this);
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
        public ActionResult GetServiceRequestDetails(int WorkOrderId)
        {
            ServiceRequestDto serviceRequest = new ServiceRequestDto();
            serviceRequest = _serviceRequestHelper.GetServiceRequestDetailsByWorkOrder(WorkOrderId, "");
            return Json(serviceRequest, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SendEmail(int ProvisionalBillId, int? WorkOrderId, string EmailID, string CName, string MailType, string UserName, int? type)
        {
            try
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
                    var userInfo = (Picanol.Helpers.UserSession)Session["UserInfo"];
                    response = _serviceRequestHelper.SendProvisionalBillEmail(ProvisionalBillId, EmailID, CName, MailType, UserName, userInfo.UserId, type);

                }
                if (type == 2)
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                if (response == null)
                {
                    return Json("Please Generate e-invoice");
                }
                return Json("success");
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in SendEmail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ActionResult WorkOrdersList(WorkOrderViewModel wvm)
        {
            try
            {
                var jsonvalue = JsonConvert.SerializeObject(wvm);
                var userInfo = (UserSession)Session["UserInfo"];
                if (wvm.RoleId == null || wvm.RoleId == 0)
                    wvm.RoleId = userInfo.RoleId;
                if (wvm.UserId == null || wvm.UserId == 0)
                    wvm.UserId = userInfo.UserId;
                List<WorkOrderDto> workOrders = _workOrderHelper.GetWorkOrdersList(wvm);
                if (wvm.TypeId == 2)
                {
                    var Value = Json(workOrders, JsonRequestBehavior.AllowGet);
                    Value.MaxJsonLength = int.MaxValue;
                    return Value;
                }

                WorkOrderViewModel vm = new WorkOrderViewModel();
                vm.WorkOrdersList = workOrders;
                Session["WorkOrderList"] = vm;
                return PartialView("_WorkOrderListAfterFilter", vm);
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in WorkOrderList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ActionResult GetSubCustomerList(int CustomerId)
        {
            List<SubCustomerDto> subCustomerList = new List<SubCustomerDto>();
            subCustomerList = _customerHelper.GetSubCustomerListByCustomerId(CustomerId);
            return Json(subCustomerList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TimeSheetAuthorization(TimeSheetDto vm)
        {
            try
            {
                string response = "";
                response = _serviceRequestHelper.TimeSheetAuthorization(vm);
                
                /*if(response == "success")
                {
                    string ActionName = $"SaveTimeSheetAuthorization(CustomerID)  - {vm.CustomerId}";
                    string TableName = "TblwOder";
                    if (ActionName != null)
                    {
                        new UserHelper(this).recordUserActivityHistory(vm.UserId, ActionName, TableName);
                    }
                }*/


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
                        tsdt.Description = (item.Description!=null)? item.Description:"";
                        tsList.Add(tsdt);
                    }
                    else
                    {
                        tsdt.WorkDate = item.WorkDate;
                        tsdt.TotalHours = item.TotalHours;
                        tsdt.TimeSheetId = item.TimeSheetId;
                        tsdt.Description = (item.Description!=null)? item.Description:"";
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
                    ViewAsPdf pdf = new ViewAsPdf("TimeSheetPDF", wm) { FileName = pdfname, SaveOnServerPath = path };
                    byte[] bytes = pdf.BuildPdf(this.ControllerContext);
                    SendTimeSheetEmail(pdfname, wm);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                return Json(response);
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in TimeSheetAuthorization!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ActionResult ProvisionalBillList(int WorkOrderId, int? Type)
        {
            try
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
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in ProvisionalBillList !");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ActionResult ServiceRequestsList(ServiceRequestViewModel svm)
        {
            try
            {
                var userInfo = (UserSession)Session["UserInfo"];
                if (svm.RoleId == null || svm.RoleId == 0)
                    svm.RoleId = userInfo.RoleId;
                if (svm.UserId == null || svm.UserId == 0)
                    svm.UserId = userInfo.UserId;
                List<ServiceRequestDto> requests = _serviceRequestHelper.GetServiceRequestsList(svm);
                if (svm.TypeId == 2)
                {
                    return Json(requests, JsonRequestBehavior.AllowGet);
                }
                ServiceRequestViewModel vm = new ServiceRequestViewModel();
                vm.ServiceRequestList = requests;
                return PartialView("_ServiceRequestList", vm);
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in serviceRequestList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ActionResult ProvisionalBillDetails(int ProvisionalBillId)
        {
            try
            {

                ServiceRequestViewModel model = new ServiceRequestViewModel();

                //model.Customers = _materialHelper.GetCustomersList();

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
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in ProvisionalBillDetail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public ActionResult TimeSheet(int ProvisionalBillId, int workOrderId, int? type)
        {
            try
            {
                WorkOrderViewModel vm = new WorkOrderViewModel();
                //int ? ProvisionalBID = ProvisionalBillId != 14? ProvisionalBillId : 14;
                // ProvisionalBillId = 14;
                vm = _workOrderHelper.GetTimeSheetDetailsByWorkOrder(ProvisionalBillId, workOrderId);

                if (type == 2)
                {
                    WorkOrderDto woOrder = _workOrderHelper.GetWorkOrderDetailsByWorkOrder(workOrderId);
                    vm.WorkOrder = woOrder;
                    return Json(vm, JsonRequestBehavior.AllowGet);
                }
                else
                {
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
                    return View(vm);

                }
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in TimeSheet!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public void SendTimeSheetEmail(string fileName, WorkOrderViewModel vm)
        {

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Dear Sir/Madam,<br />");
            stringBuilder.Append("Please find attached the Time Sheet generated for  <b>" + vm.TimeSheet.UserName + "<br/>.");
            stringBuilder.Append("<br/><br/> Thanks <br/> " + vm.TimeSheet.UserName);
            //GMailer.GmailUsername = "noreply.picanol@gmail.com";
            //GMailer.GmailPassword = "9999907947";
            GMailer mailer = new GMailer();
            string email = _serviceRequestHelper.GetPBEmailIds();

            email = email +";"+ vm.TimeSheet.UserEmail;

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
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        public ActionResult ServiceRequest(ServiceRequestDto request)
        {
         
            try
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Info("ServiceRequest");
                string response = "";
                int provisionalBillId = 0;
                string Message = "";
                if (request.ProvisionalBillId != 0)
                {
                    response =_serviceRequestHelper.EditServiceRequest(request);
                    
                    /*if (response == "sucess")
                    {
                        string ActionName = $"EditProformaInv(CustomerID And UserId)  - {request.CustomerId+"and PI Id " + request.ProvisionalBillId}";
                        string TableName = "TblProvisionalBill,tblDetailedExpenses,tblServiceDays,tblDetailedExpenses";
                        
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(request.UserId, ActionName, TableName);
                        }
                    }*/

                    if (request.Type == 2 && request.AuthorizedBy != null)
                    {
                        Message = SendProvisonalDetailsByMail(request);

                        logger.Info("SendProvisonalDetailsByMail");
                        if (Message == "")
                        {
                            request.Message = "Proforma invoice generated successfully!";

                            //return Json(provisionalBillId=0, JsonRequestBehavior.AllowGet);
                            return Json(request, JsonRequestBehavior.AllowGet);
                        }
                        request.Message = Message;
                        return Json(request, JsonRequestBehavior.AllowGet);
                        //return Json(provisionalBillId, JsonRequestBehavior.AllowGet);
                    }

                }

                if (request.ProvisionalBillId == 0 && request.Type == 2)
                {
                
                    provisionalBillId = _serviceRequestHelper.SaveServiceRequest(request, request.Type);
                    logger.Info("SaveServiceRequest");
                    if (request.AuthorizedBy != null)
                    {
                        request.ProvisionalBillId = provisionalBillId;
                        
                        //record user activity

                            /*string ActionName = $"Generate Proforma Inv with customer signature  - {request.CustomerId+"and PI id"+ provisionalBillId}";
                            string TableName = "TblProvisionalBill,tblDetailedExpenses,tblServiceDays,tblDetailedExpenses";

                            if (ActionName != null)
                            {
                                new UserHelper(this).recordUserActivityHistory(request.UserId, ActionName, TableName);
                            }*/

                            //End
                        
                        Message = SendProvisonalDetailsByMail(request);
                        logger.Info("SendProvisonalDetailsByMail");
                    }
                }
                if (request.ProvisionalBillId == 0 && request.Type != 2)
                {

                    if (Session["WorkOrderID"] != null)
                        request.WorkOrderId = (int)Session["WorkOrderID"];
                        provisionalBillId = _serviceRequestHelper.SaveServiceRequest(request, request.Type);
                    
                        //record user activity
                        
                        /*string ActionName = $"Save Proforma Inv  - {request.CustomerId+"and PI Id. "+ provisionalBillId}";
                        string TableName = "TblProvisionalBill,tblDetailedExpenses,tblServiceDays,tblDetailedExpenses";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(request.UserId, ActionName, TableName);
                        }*/

                        //end

                        logger.Info("SaveServiceRequest");
                    
                }

                if (request.Type == 2)
                {
                    if (Message == "")
                    {
                        request.Message = "Proforma invoice generated successfully!";
                      
                        //return Json(provisionalBillId=0, JsonRequestBehavior.AllowGet);
                        return Json(request, JsonRequestBehavior.AllowGet);
                    }
                    request.Message = Message;
                    return Json(request, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("success");
                }
            }
            catch(Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in Sending Credentials!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        private string SendProvisonalDetailsByMail(ServiceRequestDto request)
        {
            try
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
                    ViewAsPdf pdf = new ViewAsPdf("PreviewServiceRequestPDF", vm) { FileName = pdfname, SaveOnServerPath = path };
                    byte[] bytes = pdf.BuildPdf(this.ControllerContext);

                    return request.Message = "Your proforma invoice is generated. please check it in the portal";
                    

                }
                catch (Exception ex)
                {
                    Logger logger = LogManager.GetLogger("databaseLogger");
                    logger.Error(ex, "While Getting File Path");
                    return request.Message = "Not Able to Retrieve Proforma Invoice Details";
                }
            }
            catch(Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in SendProvisonalDetailsByMail");
                return request.Message = "Not Able to Retrieve Proforma Invoice Detail";
            }
        }
        public string SendProvisionalBillEmail(string fileName, ServiceRequestViewModel vm)
        {

            try {
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

                //Commet by Sunit
                /*if (vm.SubCustomer != null)
                    pdfname = String.Format("{0}.pdf", vm.SubCustomer.SubCustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);
                else
                    pdfname = String.Format("{0}.pdf", vm.CustomerDetails.CustomerName + "_" + vm.ServiceRequest.ProvisionalBillId);*/

                //End


                var customerName = (vm.SubCustomer != null && vm.SubCustomer.SubCustomerId > 0)
                ? vm.SubCustomer.SubCustomerName
                : vm.CustomerDetails.CustomerName;

                // Replace spaces with underscores
                customerName = Regex.Replace(customerName, @"\s+", "_");

                // Remove all other special characters
                customerName = Regex.Replace(customerName, @"[^a-zA-Z0-9_]+", "");
                pdfname = String.Format("{0}.pdf", customerName + "_" + vm.ServiceRequest.ProvisionalBillId);

                //end



                var path = Path.Combine(root, pdfname);
                mailer.AttachmentPath = path;
                try
                {
                    mailer.Send();
                    return "success";
                }
                catch (Exception ex)
                {

                    Logger logger = LogManager.GetLogger("databaseLogger");
                    logger.Error(ex, "Error in SendProvisonalBillEmail");
                    return "failure";
                }
              
            }
            catch(Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in SendProvisonalBillEMail");
                return "failure";
            }
            }
        public ActionResult SaveTimeSheet(WorkOrderViewModel vm)
        {
            
            string response = _workOrderHelper.SaveTimeSheet(vm.TimeSheet, vm.TimeSheetDetails, vm.TimeSheet.UserId, vm.TimeSheet.Email);
            if (response == "success")
            {
                //record user activity
                
                /*string ActionName = $"SaveTimeSheet  (CustomerID And UserId)  - {vm.TimeSheet.CustomerId +"And"+vm.TimeSheet.UserId}";
                string TableName = "tblTimeSheetDetails, tblTimeSheets";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(vm.TimeSheet.UserId, ActionName, TableName);

                }*/
                //end
 
            }
            
            return Json(response);
        }
    }
}