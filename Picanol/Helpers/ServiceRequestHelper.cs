using NLog;
using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.Utils;
using Picanol.ViewModels;
using Picanol.ViewModels.EInvoiceModel.IRNModel;
using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Objects.SqlClient;


//using System.Data.Objects.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Picanol.Helpers
{
    public class ServiceRequestHelper
    {
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        private ServiceRequestService _serviceRequestService;
        EInvoiceHelper _eInvoiceHelper = new EInvoiceHelper();
        private readonly UserHelper _userHelper;
        UserSession userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
        public ServiceRequestHelper()
        {

            _userHelper = new UserHelper(this);
        }
        List<InvoiceItemDto> items = new List<InvoiceItemDto>();
        InvoiceDto invoice = new InvoiceDto();
        OrderDto orders = new OrderDto();
        RespPlGenIRNDec respPlGenIRN = new RespPlGenIRNDec();
        protected ServiceRequestService ServiceRequestService
        {
            get
            {
                if (_serviceRequestService == null) _serviceRequestService = new ServiceRequestService(entities, validationDictionary);
                return _serviceRequestService;
            }
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
        public ServiceRequestHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }
        #endregion

        #region Base Methods
        public int SaveServiceRequest(ServiceRequestDto request, int? type)
        {

            PicannolEntities context = new PicannolEntities();
            tblProvisionalBill sr = new tblProvisionalBill();
            RespPlGenIRNDec respPlGenIRNDec = new RespPlGenIRNDec();
            //var checkExists = context.tblProvisionalBills.Where(x => x.FromDate == request.FromDate && x.ToDate == request.ToDate && x.CustomerId == request.CustomerId && x.WorkOrderId == request.WorkOrderId && x.DelInd == false).FirstOrDefault();
            //if (checkExists != null)
            //{

            //    return checkExists.ProvisionalBillId;
            //}
            sr.CallType = request.CallType;
            sr.ServiceCharge = request.ServiceCharge;
            sr.CustomerId = request.CustomerId;
            sr.DateCreated = DateTime.Now;
            sr.Fare = request.Fare;
            sr.FromDate = request.FromDate;
            sr.ToDate = request.ToDate;
            sr.MachineName = request.MachineName;
            sr.BoardingCharges = request.BoardingCharges;
            sr.BoardingDays = request.BoardingDays;
            sr.ConveyanceExpenses = request.ConveyanceExpenses;
            sr.PocketExpenses = request.PocketExpenses;
            sr.GST = request.GST;
            sr.FinalAmount = request.FinalAmount;
            sr.ServiceDays = request.ServiceDays;
            sr.UserId = request.UserId;
            sr.PocketExpenseDays = request.PocketExpensesDays;
            sr.ConveyanceExpenseDays = request.ConveyanceExpensesDays;
            sr.WorkOrderId = request.WorkOrderId;
            sr.Authorized = false;
            sr.AuthorizedOn = DateTime.Now;
            sr.ServiceBillNo = GetServiceBillNo();
            sr.OvertimeHours = request.OvertimeHours;
            sr.OvertimeCharges = request.OvertimeCharges;
            sr.TotalDays = request.TotalDays;
            sr.FinalSubmit = request.FinalSubmit;

            if (request.SubCustomerId == 0)
            {
                sr.SubCustomerId = null;
            }
            else
            {
                sr.SubCustomerId = request.SubCustomerId;
                var SubCustomerDetails = context.tblSubCustomers.Where(x => x.SubCustomerId == sr.SubCustomerId).FirstOrDefault();
                if (SubCustomerDetails != null)
                {
                    sr.SubCustomerBillingAddress = SubCustomerDetails.AddressLine1 + "," + SubCustomerDetails.AddressLine2 + "," + SubCustomerDetails.District + "," + SubCustomerDetails.State + "," + SubCustomerDetails.PIN;
                    sr.SubCustomerStateCode = SubCustomerDetails.StateCode;
                    

                }
            }
            //code by Janesh _16012025 
            // Adding Billing and Shipping Address When Provisionalbill Generated.
            var CustomerDetails = context.tblCustomers.Where(x => x.CustomerId == sr.CustomerId).FirstOrDefault();
            if (CustomerDetails != null)
            {
                sr.BillingAddress = CustomerDetails.AddressLine1 + "," + CustomerDetails.AddressLine2 + "," + CustomerDetails.District + "," + CustomerDetails.State + "," + CustomerDetails.PIN;
                sr.StateCode = CustomerDetails.StateCode;
                sr.ShippingAddress = CustomerDetails.ShippingAddressLine1 + "," + CustomerDetails.ShippingAddressLine2 + "," + CustomerDetails.ShippingDistrict + "," + CustomerDetails.ShippingPIN;
                sr.ShippingStateCode = CustomerDetails.ShippingStateCode;

            }
            
            sr.CustomerId = context.tblWorkOrders.Where(x => x.WorkOrderId == request.WorkOrderId).Select(x => x.CustomerId).FirstOrDefault();
            int provisionalBillId = ServiceRequestService.SaveServiceRequest(sr);
            request.ProvisionalBillId = provisionalBillId;
            if (request.FareExpenseDetails != null && request.FareExpenseDetails.Count() > 0)
            {
                // Logger.info()
                UpdateDetailedExpenses(provisionalBillId, request.UserId, request.FareExpenseDetails);
            }
            if (request.ConeyanceExpenseDetails != null && request.ConeyanceExpenseDetails.Count() > 0)
            {
                UpdateDetailedExpenses(provisionalBillId, request.UserId, request.ConeyanceExpenseDetails);
            }
            if (request.ServiceDaysList != null && request.ServiceDaysList.Count() > 0)
            {
                SaveServiceDays(provisionalBillId, request.ServiceDaysList);
            }
            if (type == 2 && request.AuthorizedBy != null)
            {
                UpdateAuthorization(request, type);
            }
            return provisionalBillId;

        }

        public string GetServiceBillNo()
        {
            string lastWONo = ServiceRequestService.GetLastServiceBillNumber();
            string newWONo = "";
            string month = DateTime.Now.ToString("MM");
            string year = DateTime.Now.ToString("yy");
            if (lastWONo != null && lastWONo != "")
            {
                char[] splitChar = { '/' };
                string[] strArray = lastWONo.Split(splitChar);
                int i = Convert.ToInt32(strArray[0]);
                i = i + 1;
                newWONo = Convert.ToString(i).PadLeft(4, '0') + "/" + month + year + "/" + strArray[2];
            }
            else
            {

                newWONo = "0001/" + month + year + "/" + "PSB";
            }
            return newWONo;
        }

        public void UpdateDetailedExpenses(int id, int userId, List<DetailedExpenseDto> expensesList)
        {
            PicannolEntities context = new PicannolEntities();

            foreach (var item in expensesList)
            {
                tblDetailedExpens td = new tblDetailedExpens();
                td.ProvisionalBillId = id;
                td.Type = item.Type;
                td.ExpenseFrom = item.ExpenseFrom;
                td.ExpenseTo = item.ExpenseTo;
                td.Remarks = item.Remarks;
                td.Amount = item.Amount;
                td.DateCreated = DateTime.Now;
                td.DelInd = false;
                td.UserId = userId;
                context.tblDetailedExpenses.Add(td);
                context.SaveChanges();
            }
        }

        public void SaveServiceDays(int provisionalBillId, List<ServiceDaysDto> serviceDaysList)
        {

            PicannolEntities context = new PicannolEntities();
            foreach (var item in serviceDaysList)
            {
                tblServiceDay sd = new tblServiceDay();
                sd.ProvisionalBillId = provisionalBillId;
                sd.FromDate = item.FromDate;
                sd.ToDate = item.ToDate;
                context.tblServiceDays.Add(sd);
                context.SaveChanges();
            }
        }
        internal string GetPBEmailIds()
        {
            string emails = "";
            string pbIds = ConfigurationManager.AppSettings["PbIds"];
            PicannolEntities context = new PicannolEntities();
            var email = (from a in context.tblUsers
                         where (a.RoleId == 5 || a.RoleId == 6) && a.DelInd == false
                         select new
                         {
                             email = a.Email
                         }).ToList();
            foreach (var item in email)
            {
                emails += item.email + ";";
            }
            //Adding Emails Users
            emails = emails + pbIds;
            return emails;
        }

        #region update work order
        public WorkOrderDto UpdateWorkOrder(WorkOrderViewModel wm)
        {
            WorkOrderViewModel vs = new WorkOrderViewModel();
            PicannolEntities _context = new PicannolEntities();
            var p = _context.tblWorkOrders.Where(x => x.WorkOrderId == wm.WorkOrder.WorkOrderId).FirstOrDefault();

            p.DateCreated = DateTime.Now;
            p.AssignedTo = wm.WorkOrder.AssignedTo;
            p.CustomerId = wm.WorkOrder.CustomerId;
            p.WorkOrderType = wm.WorkOrder.WorkOrderType;
            p.ContractNumber = wm.WorkOrder.ContractNumber;
            p.ContactPerson = wm.WorkOrder.ContactPerson;
            p.EmailId = wm.WorkOrder.EmailId;
            p.Mobile = wm.WorkOrder.Mobile;
            p.Conditions = wm.WorkOrder.Conditions;
            var d = wm.WorkOrder.SDate.Replace("-", "/");
            var c = wm.WorkOrder.EDate.Replace("-", "/");
            p.StartDate = DateTime.ParseExact(d, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            p.EndDate = DateTime.ParseExact(c, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            p.Description = wm.WorkOrder.Description;
            p.CallType = wm.WorkOrder.CallType;
            wm.WorkOrder.CustomerName = _context.tblCustomers.Where(x => x.CustomerId == wm.WorkOrder.CustomerId).Select(x => x.CustomerName).FirstOrDefault();

            _context.Entry(p).State = EntityState.Modified;
            _context.SaveChanges();
            vs.WorkOrder = wm.WorkOrder;

            return vs.WorkOrder;

        }
        #endregion

        public string UpdateAuthorization(ServiceRequestDto request, int? type)
        {
            string fileName = string.Format("{0}{1}", request.ProvisionalBillId, ".png");
            string Usersign = string.Format("{0}{1}{2}{3}", request.ProvisionalBillId, "_", request.UserId, ".png");
            PicannolEntities context = new PicannolEntities();
            var sr = context.tblProvisionalBills.Find(request.ProvisionalBillId);
            sr.AuthorizedBy = request.AuthorizedBy;
            sr.Designation = request.Designation;
            sr.AuthorizedOn = DateTime.Now;
            sr.Authorized = true;
            sr.EmailId = request.AuthorizerEmail;
            sr.ImageId = fileName;
            sr.UserSignatureId = Usersign;
            context.tblProvisionalBills.Attach(sr);
            context.Entry(sr).State = EntityState.Modified;
            context.SaveChanges();
            if (request.ProvisionalBillId > 0 && request.AuthorizedSignature != null & request.AuthorizedSignature != "")
            {
                Image img = Base64ToImage(request.AuthorizedSignature);
                string savedFilePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Signature/"), fileName);
                img.Save(savedFilePath);
                Image img1 = Base64ToImage(request.UserSignature);
                string savedFilePath1 = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Signature/"), Usersign);
                img1.Save(savedFilePath1);
            }

            return "success";
        }
        public string SendProvisionalBillEmail(int srId, string email, string customername, string MailType, string UserName, int Userid, int? Type)
        {
            try
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Info("SendProvisionalBillMwthod");
                PicannolEntities context = new PicannolEntities();
                StringBuilder stringBuilder = new StringBuilder();

                //stringBuilder.Append("Please find attached the provisional bill against work order number <b>" + wo.WorkOrder + "</b> <br/>.");
                stringBuilder.Append("Dear Sir/Madam,<br />");

                //GMailer.GmailUsername = "noreply.picanol@gmail.com";
                //GMailer.GmailPassword = "9999907947";
                GMailer mailer = new GMailer();
                mailer.CcEmail = "";
                var userInfoEmail = context.tblUsers.Where(x => x.UserId == Userid && x.DelInd == false).Select(x => x.Email).FirstOrDefault();
                var sendCClist = GetPBEmailIds();
                sendCClist += userInfoEmail;
                mailer.CcEmail = sendCClist;
                mailer.ToEmail = email;
                var woid = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == srId).Select(x => x.WorkOrderId).FirstOrDefault();
                var woNo = context.tblWorkOrders.Where(x => x.WorkOrderId == woid && x.DelInd == false).Select(x => x.WorkOrderNo).FirstOrDefault();


                /*if (MailType == "PB" || Type == 2)*/
                if (MailType == "PB")
                {
                    mailer.Subject = "Proforma Invoice from Picanol India";
                    stringBuilder.Append("Please find attached the proforma invoice generated for customer  <b>" + customername + "<br/>.");
                    stringBuilder.Append("<br/><br/> Thanks <br/> " + UserName);
                    string fileName = customername + "_" + srId;

                    mailer.AttachmentPath = HttpContext.Current.Server.MapPath("~/Content/PDF/ProvisionalBill/" + fileName + ".pdf");

                }
                else if (MailType == "IB")
                {
                    mailer.Subject = "Proforma Invoice from Picanol India";
                    stringBuilder.Append("Please find attached the proforma invoice generated for customer  <b>" + customername + "<br/>.");
                    stringBuilder.Append("<br/><br/> Thanks <br/> " + UserName);
                    string fileName = customername + "_" + srId;
                    mailer.AttachmentPath = HttpContext.Current.Server.MapPath("~/Content/TempPBInvoice/" + fileName + ".pdf");
                    if (!System.IO.File.Exists(mailer.AttachmentPath))
                    {
                        return null;
                    }
                }

                mailer.Body = stringBuilder.ToString();

                mailer.IsHtml = true;
                mailer.Send();
                return "success";
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in Send Proforma Invoice Email!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        public static Image Base64ToImage(string base64String)
        {
            string a = Regex.Replace(base64String, "[%]", "");
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
        public ServiceRequestDto getWorkOrderDetails(int id)
        {
            return ServiceRequestService.getWorkOrderDetails(id);
        }
        public List<ServiceRequestDto> ProvisionalBillListByWorkOrderId(int WorkOrderId)
        {
            return ServiceRequestService.ProvisionalBillListByWorkOrderId(WorkOrderId);
        }

        /*public List<ServiceRequestDto> GetServiceRequestsList(ServiceRequestViewModel svm)
        {
            try
            {
                List<ServiceRequestDto> srList = ServiceRequestService.GetServiceRequestsList();

                if (svm.CustomerId != null & svm.CustomerId != 0)
                {
                    srList = srList.Where(x => x.CustomerId == svm.CustomerId).ToList();
                }

                if (svm.SubCustomerId == 0)
                    srList = srList.Where(x => x.SubCustomerId == 0).ToList();
                else
                    srList = srList.Where(x => x.SubCustomerId == svm.SubCustomerId).ToList();

                if (svm.ProvisionalNo != null)
                    srList = srList.Where(x => x.ProvisionalBillNo.Contains(svm.ProvisionalNo) || (x.InvoiceNumber.Contains(svm.ProvisionalNo))).ToList();

                if (svm.RoleId != 5 && svm.RoleId != 3 && svm.RoleId != 6 && svm.RoleId != 8)
                    srList = srList.Where(x => x.UserId == svm.UserId).ToList();
                
                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                    srList = srList.Where(x => x.TillFromDate >= svm.StartDate && x.EndDate <= svm.EndDate).ToList();

                if (svm.SelectedUserId != 0)
                    srList = srList.Where(x => x.UserId == svm.SelectedUserId).ToList();
               
                return srList;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetServiceRequestList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }*/


        public List<ServiceRequestDto> GetServiceRequestsList(ServiceRequestViewModel svm)
        {
            try
            {
                List<ServiceRequestDto> srList = ServiceRequestService.GetServiceRequestsList();

                if (svm.CustomerId != null & svm.CustomerId != 0)
                {
                    srList = srList.Where(x => x.CustomerId == svm.CustomerId).ToList();
                }
                if (svm.ProvisionalNo != null)
                {
                    srList = srList.Where(x => x.ProvisionalBillNo.Contains(svm.ProvisionalNo) || (x.InvoiceNumber.Contains(svm.ProvisionalNo))).ToList();

                }
                if (svm.RoleId != 5 && svm.RoleId != 3 && svm.RoleId != 6 && svm.RoleId != 8)
                {
                    srList = srList.Where(x => x.UserId == svm.UserId).ToList();
                }
                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                {
                    srList = srList.Where(x => x.TillFromDate >= svm.StartDate && x.EndDate <= svm.EndDate).ToList();
                }
                if (svm.SelectedUserId != 0)
                {
                    srList = srList.Where(x => x.UserId == svm.SelectedUserId).ToList();
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

        //new 260721

        /*public List<ServiceRequestDto> GetServiceRequestsListVersion6(ServiceRequestViewModel svm)
        {
            try
            {
                List<ServiceRequestDto> srList = ServiceRequestService.GetServiceRequestsListVersion6(svm);

                if (svm.CustomerId != null & svm.CustomerId != 0)
                {
                    srList = srList.Where(x => x.CustomerId == svm.CustomerId).ToList();
                }
                if (svm.ProvisionalNo != null)
                {
                    srList = srList.Where(x => x.ProvisionalBillNo.Contains(svm.ProvisionalNo) || (x.InvoiceNumber.Contains(svm.ProvisionalNo))).ToList();

                }
                if (svm.RoleId != 5 && svm.RoleId != 3 && svm.RoleId != 6 && svm.RoleId != 8)
                {
                    srList = srList.Where(x => x.UserId == svm.UserId).ToList();
                }
                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                {
                    srList = srList.Where(x => x.TillFromDate >= svm.StartDate && x.EndDate <= svm.EndDate).ToList();
                }
                if (svm.SelectedUserId != 0)
                {
                    srList = srList.Where(x => x.UserId == svm.SelectedUserId).ToList();
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
        }*/



        public List<ServiceRequestDto> GetServiceRequestsListVersion6(ServiceRequestViewModel svm)
        {
            try
            {
                List<ServiceRequestDto> srList = ServiceRequestService.GetServiceRequestsListVersion6(svm);

                if (svm.ProvisionalNo != null)
                    srList = srList.Where(x => x.ProvisionalBillNo.Contains(svm.ProvisionalNo) || (x.InvoiceNumber.Contains(svm.ProvisionalNo))).ToList();
                if (svm.RoleId != 5 && svm.RoleId != 3 && svm.RoleId != 6 && svm.RoleId != 8)
                    srList = srList.Where(x => x.UserId == svm.UserId).ToList();
                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                    srList = srList.Where(x => x.TillFromDate >= svm.StartDate && x.EndDate <= svm.EndDate).ToList();
                if (svm.SelectedUserId != 0)
                    srList = srList.Where(x => x.UserId == svm.SelectedUserId).ToList();
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

        //End


        public List<ServiceRequestDto> GetServiceRequestsListVersion2(ServiceRequestViewModel svm, int PageSize, int PageNo)
        {
            try
            {
                List<ServiceRequestDto> srList = ServiceRequestService.GetServiceRequestsListVersion2(PageSize, PageNo);

                if (svm.CustomerId != null & svm.CustomerId != 0)
                {
                    srList = srList.Where(x => x.CustomerId == svm.CustomerId).ToList();
                }
                if (svm.ProvisionalNo != null)
                {
                    srList = srList.Where(x => x.ProvisionalBillNo.Contains(svm.ProvisionalNo) || (x.InvoiceNumber.Contains(svm.ProvisionalNo))).ToList();

                }
                if (svm.RoleId != 5 && svm.RoleId != 3 && svm.RoleId != 6 && svm.RoleId != 8)
                {
                    srList = srList.Where(x => x.UserId == svm.UserId).ToList();
                }
                if (svm.StartDate != null && svm.StartDate != DateTime.MinValue && svm.EndDate != null && svm.EndDate != DateTime.MinValue)
                {
                    srList = srList.Where(x => x.TillFromDate >= svm.StartDate && x.EndDate <= svm.EndDate).ToList();
                }
                if (svm.SelectedUserId != 0)
                {
                    srList = srList.Where(x => x.UserId == svm.SelectedUserId).ToList();
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


        // End Here//

        public List<ServiceRequestDto> GetProvisionalBillByWorkOrder(int id)
        {
            try
            {
                PicannolEntities _context = new PicannolEntities();
                var s = (from a in _context.tblProvisionalBills
                         join b in _context.tblCustomers on a.CustomerId equals b.CustomerId
                         join c in _context.tblUsers on a.UserId equals c.UserId
                         where (a.DelInd == false) && (a.WorkOrderId == id) && (a.Cancelled == false || a.Cancelled == null)
                         select new ServiceRequestDto
                         {
                             SubCustomerId = a.SubCustomerId != null ? (int)a.SubCustomerId : 0,
                             ProvisionalBillId = a.ProvisionalBillId,
                             ProvisionalBillNo = a.ServiceBillNo,
                             WorkOrderId = (int)a.WorkOrderId,
                             CustomerName = b.CustomerName,
                             //FromDate = (DateTime) EntityFunctions.TruncateTime(a.FromDate),
                             FromDate = (DateTime)a.FromDate,
                             FDate = SqlFunctions.DateName("day", a.FromDate) + "/" + SqlFunctions.DateName("month", a.FromDate) + "/" + SqlFunctions.DateName("year", a.FromDate),
                             //ToDate = (DateTime)EntityFunctions.TruncateTime(a.ToDate),
                             ToDate = (DateTime)a.ToDate,
                             TDate = SqlFunctions.DateName("day", a.ToDate) + "/" + SqlFunctions.DateName("month", a.ToDate) + "/" + SqlFunctions.DateName("year", a.ToDate),
                             Date = SqlFunctions.DateName("day", a.DateCreated) + "/" + SqlFunctions.DateName("month", a.DateCreated) + "/" + SqlFunctions.DateName("year", a.DateCreated),
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
                             FinalSubmit = (bool)a.FinalSubmit
                         }).ToList();
                for (int i = 0; i < s.Count; i++)
                {
                    s[i] = ServiceRequestService.GetFareAndConveyanceDetails(s[i]);
                    int pbId = s[i].ProvisionalBillId;
                    if (_context.tblTimeSheets.Any(x => x.ProvisionalBillId == pbId))
                    {
                        s[i].TimeSheetId = _context.tblTimeSheets.Where(x => x.ProvisionalBillId == pbId).Select(x => x.TimeSheetId).FirstOrDefault();
                    }
                    else
                        s[i].TimeSheetId = 0;
                }
                return s.OrderByDescending(x => x.ProvisionalBillId).ToList();
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetProvisionalBillByWorkOrder!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        public ServiceRequestDto GetServiceRequestDetails(int id)
        {
            return ServiceRequestService.GetServiceRequestDetails(id);
        }
        public OrderPaymentDto GetPaymentDetails(string InvoiceNo)
        {
            return ServiceRequestService.GetPaymentDetails(InvoiceNo);
        }


        public int RecordPayment(InvoiceListViewModel vm)
        {
            tblOrderPayment op = new tblOrderPayment();
            try
            {
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
                    op.UserId = userInfo.UserId;
                    op.Paid = true;
                    op.DelInd = false;
                    op.ProvisionalBillId = vm.OrderPayment.ProvisionalBillId;
                    op.ReceivedAmount = (decimal)(op.AmountPaid + op.TDS);
                    op.BalanceAmount = (decimal)(op.InvoiceAmount - (op.AmountPaid + op.TDS));

                    decimal tt = 0;
                    decimal checkTDSamount = vm.OrderPayment.TDS;
                    decimal checkAmountpaid = vm.OrderPayment.AmountPaid;
                    var pk = context.tblOrderPayments.Where(x => x.InvoiceNo == vm.OrderPayment.InvoiceNo).ToList();
                    foreach (var item in pk)
                    {
                        tt += (int)item.ReceivedAmount;
                        checkTDSamount += item.TDS ?? 0;
                        checkAmountpaid += item.AmountPaid;
                        op.BalanceAmount = (int)op.InvoiceAmount - tt;
                        op.BalanceAmount = (int)(op.BalanceAmount - op.ReceivedAmount);
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
            catch (Exception ex)
            {

                throw;
            }


        }
        public List<ServiceDaysDto> GetServiceBillDays(int id)
        {
            return ServiceRequestService.GetServiceBillDays(id);
        }
        public TimeSheetDto GetTimeSheetDetailsByTimeSheetId(int id)
        {
            return ServiceRequestService.TimeSheetDetails(id);
        }

        public List<TimeSheetDetailDto> GetTimeSheetDetails(int id)
        {
            return ServiceRequestService.TimeSheetDetailsList(id);
        }

        public ServiceRequestDto GetServiceRequestDetailsByWorkOrder(int id, string initiator)
        {
            return ServiceRequestService.GetServiceRequestDetailsByWorkOrder(id, initiator);
        }


        public ServiceRequestDto GetServiceRequestDetailsByWorkOrderV1(int id, string initiator)
        {
            return ServiceRequestService.GetServiceRequestDetailsByWorkOrderV1(id, initiator);
        }

        public string EditServiceRequest(ServiceRequestDto pm)
        {

            PicannolEntities _context = new PicannolEntities();
            var p = _context.tblProvisionalBills.Where(x => x.ProvisionalBillId == pm.ProvisionalBillId).FirstOrDefault();
            if (pm.Type == 1 && p != null)
            {
                if (p.MachineName == null)
                {
                    p.MachineName = "MACHINE";
                }
                if (p.Authorized == false)
                {
                    p.DateCreated = DateTime.Now;
                }
                p.Fare = pm.Fare;
                p.BoardingCharges = pm.BoardingCharges;
                p.BoardingDays = pm.BoardingDays;
                p.ConveyanceExpenses = pm.ConveyanceExpenses;
                p.ServiceCharge = pm.ServiceCharge;
                p.PocketExpenses = pm.PocketExpenses;
                //p.GST = pm.GST;
                p.FinalAmount = pm.FinalAmount;
                p.ServiceCharge = pm.ServiceCharge;
                p.ServiceDays = pm.ServiceDays;
                p.UserId = pm.UserId;
                p.PocketExpenseDays = pm.PocketExpensesDays;
                p.OvertimeHours = pm.OvertimeHours;
                p.OvertimeCharges = pm.OvertimeCharges;
                p.TotalDays = pm.PocketExpensesDays;
            }
            else if (p != null)
            {
                p.CallType = pm.CallType;
                p.ServiceCharge = pm.ServiceCharge;
                p.CustomerId = pm.CustomerId;
                p.SubCustomerId = 1;
                if (p.Authorized == false)
                {
                    p.DateCreated = DateTime.Now;
                }
                p.Fare = pm.Fare;
                p.FromDate = pm.FromDate;
                p.ToDate = pm.ToDate;
                p.MachineName = pm.MachineName;
                p.BoardingCharges = pm.BoardingCharges;
                p.BoardingDays = pm.BoardingDays;
                p.ConveyanceExpenses = pm.ConveyanceExpenses;
                p.PocketExpenses = pm.PocketExpenses;
                p.GST = pm.GST;
                p.FinalAmount = pm.FinalAmount;
                p.ServiceDays = pm.ServiceDays;
                p.UserId = pm.UserId;
                p.PocketExpenseDays = pm.PocketExpensesDays;
                p.ConveyanceExpenseDays = pm.ConveyanceExpensesDays;
                p.OvertimeHours = pm.OvertimeHours;
                p.OvertimeCharges = pm.OvertimeCharges;
                p.TotalDays = pm.TotalDays;
                p.FinalSubmit = pm.FinalSubmit;

                if (pm.SubCustomerId == 0)
                {
                    p.SubCustomerId = null;
                }
                else
                {
                    p.SubCustomerId = pm.SubCustomerId;
                }

            }
            _context.Entry(p).State = EntityState.Modified;
            _context.SaveChanges();

            _context.tblDetailedExpenses.Where(a => a.ProvisionalBillId == pm.ProvisionalBillId)
                   .ToList().ForEach(a => _context.tblDetailedExpenses.Remove(a));
            _context.SaveChanges();
            if (pm.ServiceDaysList != null && pm.ServiceDaysList.Count() > 0)
            {
                _context.tblServiceDays.Where(a => a.ProvisionalBillId == pm.ProvisionalBillId)
                   .ToList().ForEach(a => _context.tblServiceDays.Remove(a));
                _context.SaveChanges();
                SaveServiceDays(pm.ProvisionalBillId, pm.ServiceDaysList);

            }
            if (pm.FareExpenseDetails != null && pm.FareExpenseDetails.Count() > 0)
            {


                UpdateDetailedExpenses(pm.ProvisionalBillId, pm.UserId, pm.FareExpenseDetails);
            }
            if (pm.ConeyanceExpenseDetails != null)
            {
                UpdateDetailedExpenses(pm.ProvisionalBillId, pm.UserId, pm.ConeyanceExpenseDetails);

            }

            if (pm.Type == 2 && pm.AuthorizedBy != null)
            {
                UpdateAuthorization(pm, pm.Type);
            }
            return "success";

        }

        internal InvoiceDto GetPartialCreditNote(int workOrderId, CustomerDto customer, int provisionalBillId, string PreviewType, int generatedBy, string actionInitiator, string invoiceNumber)
        {
            PicanolUtils utils = new PicanolUtils();
            PicannolEntities context = new PicannolEntities();
            ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrder(provisionalBillId, actionInitiator);
            DateTime dateTime = new DateTime(2023, 05, 31, 11, 59, 00);

            var creditNoteInvDtl = context.tblCreditNotes.Where(x => x.ProvisionalBillId == provisionalBillId
            && (x.CRNCreated == true && x.DelInd == false && x.Cancelled == false)).FirstOrDefault();

            var creditnoteDetail = (CreditNoteEditDto)HttpContext.Current.Session["CreditNoteEditable"];

            invoice = new InvoiceDto();
            invoice.ErrorMessage = "";
            items = new List<InvoiceItemDto>();
            InvoiceItemDto item = new InvoiceItemDto();
            item.Name = "SERVICE CHARGE";
            item.InvoiceItemID = 1;
            item.Qty = serviceRequest.ServiceDays;

            /*item.Rate = serviceRequest.ServiceCharge;
             item.Amount = serviceRequest.ServiceDays * serviceRequest.ServiceCharge;*/
            item.Rate = 0;
            item.Amount = (creditNoteInvDtl != null) ? creditNoteInvDtl.ServiceCharge :
                creditnoteDetail.ServiceChargeAmount;

            //item.HSNCode = "998739";
            item.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item.UnitOfMeasurement = "DAY";
            item.SGSTAmount = 0;
            item.CGSTAmount = 0;
            item.IGSTAmount = 0;
            item.AmountBTax = item.Amount;
            item.IsService = "Y";
            item.IGSTRate = 18;
            if (serviceRequest.DateCreated <= dateTime)
                item.IGSTAmount = Math.Truncate(item.Amount * (item.IGSTRate / 100));
            else
                item.IGSTAmount = item.Amount * (item.IGSTRate / 100);
            item.Total = (int)(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount);
            items.Add(item);

            InvoiceItemDto item1 = new InvoiceItemDto();
            item1.Name = "To & FRO AIR FARE/RAIL FARE";
            item1.InvoiceItemID = 2;
            item1.Qty = 1;
            item1.Rate = 0;
            item1.Amount = (creditNoteInvDtl != null) ? creditNoteInvDtl.Fare :
                creditnoteDetail.railairfareAmount;
            item1.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item1.IsService = "Y";
            item1.UnitOfMeasurement = "";
            item1.SGSTAmount = 0;
            item1.CGSTAmount = 0;
            item1.IGSTAmount = 0;
            item1.AmountBTax = item1.Amount;
            item1.IGSTRate = 18;
            if (serviceRequest.DateCreated <= dateTime)
                item1.IGSTAmount = Math.Truncate(item1.Amount * (item1.IGSTRate / 100));
            else
                item1.IGSTAmount = item1.Amount * (item1.IGSTRate / 100);
            item1.Total = (int)(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount);
            items.Add(item1);

            invoice.InvoiceItems = items;

            InvoiceItemDto item2 = new InvoiceItemDto();
            item2.Name = "POCKET EXPENSE";
            item2.InvoiceItemID = 3;
            item2.Qty = serviceRequest.PocketExpensesDays;
            item2.Rate = 0;
            item2.Amount = (creditNoteInvDtl != null)
                ?creditNoteInvDtl.PocketExpeses:
                creditnoteDetail.PocketexpenceAmount;
            item2.IsService = "Y";
            item2.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item2.UnitOfMeasurement = "DAY";
            item2.SGSTAmount = 0;
            item2.CGSTAmount = 0;
            item2.IGSTAmount = 0;
            item2.AmountBTax = item2.Amount;
            item2.IGSTRate = 18;
            if (serviceRequest.DateCreated <= dateTime)
                item2.IGSTAmount = Math.Truncate(item2.Amount * (item2.IGSTRate / 100));
            else
                item2.IGSTAmount = item2.Amount * (item2.IGSTRate / 100);
            item2.Total = (int)(item2.Amount + item2.IGSTAmount + item2.SGSTAmount + item2.CGSTAmount);
            items.Add(item2);

            invoice.InvoiceItems = items;

            InvoiceItemDto item3 = new InvoiceItemDto();
            item3.Name = "LODGING & BOARDING";
            item3.InvoiceItemID = 4;
            item3.Qty = (int)serviceRequest.BoardingDays;
            item3.Rate = 0;
            item3.Amount = (creditNoteInvDtl != null)?
                creditNoteInvDtl.BoardingCharges:
                creditnoteDetail.LoadingBoardingAmount;
            item3.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item3.IsService = "Y";
            item3.UnitOfMeasurement = "DAY";
            item3.SGSTAmount = 0;
            item3.CGSTAmount = 0;
            item3.IGSTAmount = 0;
            item3.AmountBTax = item3.Amount;
            item3.IGSTRate = 18;
            if (serviceRequest.DateCreated <= dateTime)
                item3.IGSTAmount = Math.Truncate(item3.Amount * (item3.IGSTRate / 100));
            else
                item3.IGSTAmount = item3.Amount * (item3.IGSTRate / 100);

            item3.Total = (int)(item3.Amount + item3.IGSTAmount + item3.SGSTAmount + item3.CGSTAmount);
            items.Add(item3);

            InvoiceItemDto item4 = new InvoiceItemDto();
            item4.Name = "CONVEYANCE & INCIDENTAL EXPENSES";
            item4.InvoiceItemID = 5;
            item4.Qty = 1;
            item4.Rate = 0;
            item4.Amount = (creditNoteInvDtl != null)?
                creditNoteInvDtl.ConveyanceExpenses:
                creditnoteDetail.ConvenyanceInceidentalAmount;
            item4.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item4.IsService = "Y";
            item4.UnitOfMeasurement = "";
            item4.SGSTAmount = 0;
            item4.CGSTAmount = 0;
            item4.IGSTAmount = 0;
            item4.AmountBTax = item4.Amount;
            item4.IGSTRate = 18;
            if (serviceRequest.DateCreated <= dateTime)
                item4.IGSTAmount = Math.Truncate(item4.Amount * (item4.IGSTRate / 100));
            else
                item4.IGSTAmount = item4.Amount * (item4.IGSTRate / 100);

            item4.Total = (int)(item4.Amount + item4.IGSTAmount + item4.SGSTAmount + item4.CGSTAmount);
            items.Add(item4);

            InvoiceItemDto item5 = new InvoiceItemDto();
            item5.Name = "OVER TIME HOUR";
            item5.InvoiceItemID = 6;
            item5.Qty = serviceRequest.OvertimeHours;
            item5.Rate = 0;
            item5.Amount = (creditNoteInvDtl != null)?
                creditNoteInvDtl.OvertimeCharges:
                creditnoteDetail.OverTimehourAmount;

            item5.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item5.IsService = "Y";
            item5.UnitOfMeasurement = "HOUR";
            item5.SGSTAmount = 0;
            item5.CGSTAmount = 0;
            item5.IGSTAmount = 0;
            item5.AmountBTax = item5.Amount;
            item5.IGSTRate = 18;
            if (serviceRequest.DateCreated <= dateTime)
                item5.IGSTAmount = Math.Truncate(item5.Amount * (item5.IGSTRate / 100));
            else
                item5.IGSTAmount = item5.Amount * (item5.IGSTRate / 100);
            item5.Total = (int)(item5.Amount + item5.IGSTAmount + item5.SGSTAmount + item5.CGSTAmount);
            items.Add(item5);
            invoice.InvoiceItems = items;
            foreach (var a in items)
            {
                invoice.Amount += a.AmountBTax;
                invoice.SGSTTax += a.SGSTAmount;
                invoice.CGSTTax += a.CGSTAmount;
                invoice.IGSTTax += a.IGSTAmount;
                invoice.TotalTax += (a.SGSTAmount + a.CGSTAmount + a.IGSTAmount);
            }

            invoice.AmountBTax = invoice.Amount;
            invoice.Amount = invoice.AmountBTax + (int)invoice.TotalTax;
            invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";

            HttpContext.Current.Session["CreditNoteEditable"] = null;

            invoice.SubCustomerId = serviceRequest.SubCustomerId;
            if (invoice.SubCustomerId != 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == invoice.SubCustomerId && x.DelInd == false).FirstOrDefault();

                customer.SubCustomerId = a.SubCustomerId;
                customer.SubCustomerName = a.SubCustomerName;
            }
            var order = (Guid)context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).Select(x => x.WorkOrderGUID).FirstOrDefault();
            if (PreviewType != "PreviewDetails" && PreviewType != "CreditNote" && PreviewType != "DebitNote")
            {

                invoice.InvoiceNo = SaveNewInvoice(order, invoice.Amount, customer, provisionalBillId, generatedBy, actionInitiator);
                var invDeatils = context.tblInvoices.Where(x => x.OrderGuid == order && x.DelInd == false && x.ProvisionalBillId == provisionalBillId).FirstOrDefault();
                if (invDeatils != null)
                {
                    invoice.InvoiceDate = invDeatils.InvoiceDate;
                }

            }
            else if (PreviewType == "CreditNote" && PreviewType != "PreviewDetails")
            {
                //invoice.Amount = FinalAmount; //coment by janesh
                invoice.CreditNoteNo = SaveCreditNotes(order, invoice, customer, provisionalBillId, generatedBy, actionInitiator, invoiceNumber);
                invoice.InvoiceNo = invoiceNumber;
                if (invoice.CreditNoteNo != null)
                {
                    var creditNoteType = ConfigurationManager.AppSettings["CreditNote"];
                    var ecreditNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.CreditNoteNo
                                            && x.DelInd == false && x.Cancelled == false
                                            && x.Type == creditNoteType).FirstOrDefault();
                    if (ecreditNote != null)
                    {
                        invoice.EInVoiceQrCode = ecreditNote.QRCode;
                        invoice.IRN = ecreditNote.IRN;
                        invoice.CreditNoteNo = invoice.CreditNoteNo;

                        //record user activity
                        string ActionName = $" Download E-CRN, InvoicNo - {ecreditNote.InvoiceNo}"
                        + $", OrderGUID - {ecreditNote.OrderID}"
                        + $", PSBID - {ecreditNote.ProvisionalBillId}";
                        string TableName = "tblCreditNote";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        invoice.ErrorMessage = "OOps! Something went wrong. Please try again latter";

                    }
                }
            }
            else
            {
                invoice.InvoiceDate = DateTime.Now;
            }
            invoice.ProvisionalBillNo = serviceRequest.ProvisionalBillNo;
            invoice.AttendedFrom = serviceRequest.FromDate.ToShortDateString();
            invoice.AttendedTo = serviceRequest.ToDate.ToShortDateString();
            invoice.EngineerName = serviceRequest.UserName;
            return invoice;
        }

        public InvoiceDto GetDebitNoteInvoiceDtl(int workOrderId, CustomerDto customer, int provisionalBillId, string PreviewType, int generatedBy, string actionInitiator, string invoiceNumber)
        {
            PicanolUtils utils = new PicanolUtils();
            PicannolEntities context = new PicannolEntities();
            DateTime dateTime = new DateTime(2023, 05, 31, 11, 59, 00);

            ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrder(provisionalBillId, actionInitiator);

            var creditNoteInvDtl = context.tblCreditNotes.Where(x => x.ProvisionalBillId == provisionalBillId
            && (x.CRNCreated == true && x.DelInd == false && x.Cancelled == false)).FirstOrDefault();

            var creditnoteDetail = (CreditNoteEditDto)HttpContext.Current.Session["CreditNoteEditable"];

            invoice = new InvoiceDto();
            invoice.ErrorMessage = "";
            items = new List<InvoiceItemDto>();
            InvoiceItemDto item = new InvoiceItemDto();
            item.Name = "SERVICE CHARGE";
            item.InvoiceItemID = 1;
            item.Qty = serviceRequest.ServiceDays;
            item.Rate = 0;
            item.Amount= (creditNoteInvDtl != null)?
                creditNoteInvDtl.ServiceCharge:
                creditnoteDetail.ServiceChargeAmount;
            item.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item.UnitOfMeasurement = "DAY";
            item.SGSTAmount = 0;
            item.CGSTAmount = 0;
            item.IGSTAmount = 0;
            item.AmountBTax = item.Amount;
            item.IsService = "Y";
            item.IGSTRate = 18;
            /*item.IGSTAmount = (serviceRequest.DateCreated <= dateTime) ?
                Math.Truncate(item.Amount * (item.IGSTRate / 100)) :
                Math.Round(item.Amount * (item.IGSTRate / 100));*/
            if (serviceRequest.DateCreated <= dateTime)
                item.IGSTAmount = Math.Truncate(item.Amount * (item.IGSTRate / 100));
            else
                item.IGSTAmount = Math.Round(item.Amount * (item.IGSTRate / 100));

            item.Total = (int)(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount);
            items.Add(item);

            InvoiceItemDto item1 = new InvoiceItemDto();
            item1.Name = "To & FRO AIR FARE/RAIL FARE";
            item1.InvoiceItemID = 2;
            item1.Qty = 1;
            item1.Rate = 0;
            item1.Amount = (creditNoteInvDtl != null)? 
                creditNoteInvDtl.Fare: 
                creditnoteDetail.railairfareAmount;
            item1.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item1.IsService = "Y";
            item1.UnitOfMeasurement = "";
            item1.SGSTAmount = 0;
            item1.CGSTAmount = 0;
            item1.IGSTAmount = 0;
            item1.AmountBTax = item1.Amount;
            item1.IGSTRate = 18;
            /*item1.IGSTAmount = (serviceRequest.DateCreated <= dateTime) ?
                Math.Truncate(item1.Amount * (item1.IGSTRate / 100)) :
                Math.Round(item1.Amount * (item1.IGSTRate / 100));*/
            if (serviceRequest.DateCreated <= dateTime)
                item1.IGSTAmount = Math.Truncate(item1.Amount * (item1.IGSTRate / 100));
            else
                item1.IGSTAmount = Math.Round(item1.Amount * (item1.IGSTRate / 100));

            item1.Total = (int)(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount);
            items.Add(item1);

            invoice.InvoiceItems = items;

            InvoiceItemDto item2 = new InvoiceItemDto();
            item2.Name = "POCKET EXPENSE";
            item2.InvoiceItemID = 3;
            item2.Qty = serviceRequest.PocketExpensesDays;
            item2.Rate = 0;
            item2.Amount = (creditNoteInvDtl != null)? creditNoteInvDtl.PocketExpeses:
                creditnoteDetail.PocketexpenceAmount;
            item2.IsService = "Y";
            item2.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item2.UnitOfMeasurement = "DAY";
            item2.SGSTAmount = 0;
            item2.CGSTAmount = 0;
            item2.IGSTAmount = 0;
            item2.AmountBTax = item2.Amount;
            item2.IGSTRate = 18;

            /*item2.IGSTAmount = (serviceRequest.DateCreated <= dateTime) ?
                Math.Truncate(item2.Amount * (item2.IGSTRate / 100)) :
                Math.Round(item2.Amount * (item2.IGSTRate / 100));*/
            if (serviceRequest.DateCreated <= dateTime)
                item2.IGSTAmount = Math.Truncate(item2.Amount * (item2.IGSTRate / 100));
            else
                item2.IGSTAmount = Math.Round(item2.Amount * (item2.IGSTRate / 100));

            item2.Total = Math.Round(item2.Amount + item2.IGSTAmount + item2.SGSTAmount + item2.CGSTAmount);
            items.Add(item2);

            invoice.InvoiceItems = items;

            InvoiceItemDto item3 = new InvoiceItemDto();
            item3.Name = "LODGING & BOARDING";
            item3.InvoiceItemID = 4;
            item3.Qty = (int)serviceRequest.BoardingDays;
            item3.Rate = 0;
            item3.Amount = (creditNoteInvDtl != null)? creditNoteInvDtl.BoardingCharges: 
                creditnoteDetail.LoadingBoardingAmount;

            item3.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item3.IsService = "Y";
            item3.UnitOfMeasurement = "DAY";
            item3.SGSTAmount = 0;
            item3.CGSTAmount = 0;
            item3.IGSTAmount = 0;
            item3.AmountBTax = item3.Amount;
            item3.IGSTRate = 18;

            /*item3.IGSTAmount = (serviceRequest.DateCreated <= dateTime) ?
                Math.Truncate(item3.Amount * (item3.IGSTRate / 100)) :
                Math.Round(item3.Amount * (item3.IGSTRate / 100));*/
            if (serviceRequest.DateCreated <= dateTime)
                item3.IGSTAmount = Math.Truncate(item3.Amount * (item3.IGSTRate / 100));
            else
                item3.IGSTAmount = Math.Round(item3.Amount * (item3.IGSTRate / 100));

            item3.Total = (int)(item3.Amount + item3.IGSTAmount + item3.SGSTAmount + item3.CGSTAmount);
            items.Add(item3);

            InvoiceItemDto item4 = new InvoiceItemDto();
            item4.Name = "CONVEYANCE & INCIDENTAL EXPENSES";
            item4.InvoiceItemID = 5;
            item4.Qty = 1;
            item4.Rate = 0;
            item4.Amount = (creditNoteInvDtl != null)? creditNoteInvDtl.ConveyanceExpenses:
                            creditnoteDetail.ConvenyanceInceidentalAmount;

            item4.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item4.IsService = "Y";
            item4.UnitOfMeasurement = "";
            item4.SGSTAmount = 0;
            item4.CGSTAmount = 0;
            item4.IGSTAmount = 0;
            item4.AmountBTax = item4.Amount;
            item4.IGSTRate = 18;
            /*item4.IGSTAmount = (serviceRequest.DateCreated <= dateTime) ?
                Math.Truncate(item4.Amount * (item4.IGSTRate / 100)) :
                Math.Round(item4.Amount * (item4.IGSTRate / 100));*/
            if (serviceRequest.DateCreated <= dateTime)
                item4.IGSTAmount = Math.Truncate(item4.Amount * (item4.IGSTRate / 100));
            else
                item4.IGSTAmount = Math.Round(item4.Amount * (item4.IGSTRate / 100));

            item4.Total = (int)(item4.Amount + item4.IGSTAmount + item4.SGSTAmount + item4.CGSTAmount);
            items.Add(item4);

            InvoiceItemDto item5 = new InvoiceItemDto();
            item5.Name = "OVER TIME HOUR";
            item5.InvoiceItemID = 6;
            item5.Qty = serviceRequest.OvertimeHours;
            item5.Rate = 0;
            item5.Amount = (creditNoteInvDtl != null)? creditNoteInvDtl.OvertimeCharges:
                             creditnoteDetail.OverTimehourAmount;
            item5.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item5.IsService = "Y";
            item5.UnitOfMeasurement = "HOUR";
            item5.SGSTAmount = 0;
            item5.CGSTAmount = 0;
            item5.IGSTAmount = 0;
            item5.AmountBTax = item5.Amount;
            item5.IGSTRate = 18;
            /*item5.IGSTAmount = (serviceRequest.DateCreated <= dateTime) ?
                Math.Truncate(item5.Amount * (item5.IGSTRate / 100)) :
                Math.Round(item5.Amount * (item5.IGSTRate / 100));*/
            if (serviceRequest.DateCreated <= dateTime)
                item5.IGSTAmount = Math.Truncate(item5.Amount * (item5.IGSTRate / 100));
            else
                item5.IGSTAmount = Math.Round(item5.Amount * (item5.IGSTRate / 100));

            item5.Total = (int)(item5.Amount + item5.IGSTAmount + item5.SGSTAmount + item5.CGSTAmount);
            items.Add(item5);

            invoice.InvoiceItems = items;
            foreach (var a in items)
            {
                invoice.Amount += a.AmountBTax;
                invoice.SGSTTax += a.SGSTAmount;
                invoice.CGSTTax += a.CGSTAmount;
                invoice.IGSTTax += a.IGSTAmount;
                invoice.TotalTax += (a.SGSTAmount + a.CGSTAmount + a.IGSTAmount);
            }

            invoice.AmountBTax = invoice.Amount;
            invoice.Amount = invoice.AmountBTax + (int)invoice.TotalTax;
            invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";
            //End

            HttpContext.Current.Session["CreditNoteEditable"] = null;
            invoice.SubCustomerId = serviceRequest.SubCustomerId;

            if (invoice.SubCustomerId != 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == invoice.SubCustomerId && x.DelInd == false).FirstOrDefault();

                customer.SubCustomerId = a.SubCustomerId;
                customer.SubCustomerName = a.SubCustomerName;
            }
            var order = (Guid)context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).Select(x => x.WorkOrderGUID).FirstOrDefault();

            if (PreviewType == "DebitNote" && PreviewType != "PreviewDetails" && PreviewType != "CreditNote")
            {
                
                invoice.DebitNoteNo = SaveDebitNotes(order, invoice, customer, provisionalBillId, generatedBy, actionInitiator, invoiceNumber);
                invoice.InvoiceNo = invoiceNumber;
                
                if (invoice.DebitNoteNo != null)
                {
                    var debitnottype = ConfigurationManager.AppSettings["DebitNote"];
                    var debitNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.DebitNoteNo
                                            && x.DelInd == false && x.Cancelled == false
                                            && x.Type == debitnottype).FirstOrDefault();


                    if (debitNote != null)
                    {
                        invoice.EInVoiceQrCode = debitNote.QRCode;
                        invoice.IRN = debitNote.IRN;
                        invoice.CreditNoteNo = invoice.CreditNoteNo;
                        invoice.DebitNoteDate = debitNote.InvoiceDate.ToString();

                        //record user activity
                        string ActionName = $" Download DBN, DebitNoteNo - {debitNote.InvoiceNo}"
                        + $", OrderGUID - {debitNote.OrderID}"
                        + $", PSBID - {debitNote.ProvisionalBillId}";
                        string TableName = "tblDebitNote";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        invoice.ErrorMessage = "OOps! Something went wrong. Please try again latter";
                    }
                }
            }
            else
            {
                invoice.InvoiceDate = DateTime.Now;
            }
            invoice.ProvisionalBillNo = serviceRequest.ProvisionalBillNo;
            invoice.AttendedFrom = serviceRequest.FromDate.ToShortDateString();
            invoice.AttendedTo = serviceRequest.ToDate.ToShortDateString();
            invoice.EngineerName = serviceRequest.UserName;
            return invoice;
        }

        public string UpdateServiceRequest(ServiceRequestDto pm, int? type)
        {

            tblProvisionalBill sr = new tblProvisionalBill();
            sr.ProvisionalBillId = pm.ProvisionalBillId;
            sr.CallType = pm.CallType;
            sr.ServiceCharge = pm.ServiceCharge;
            sr.CustomerId = pm.CustomerId;
            sr.DateCreated = DateTime.Now;
            sr.Fare = pm.Fare;
            sr.FromDate = Convert.ToDateTime(pm.FromDate);
            sr.ToDate = Convert.ToDateTime(pm.ToDate);
            sr.MachineName = pm.MachineName;
            sr.BoardingCharges = pm.BoardingCharges;
            sr.BoardingDays = pm.BoardingDays;
            sr.ConveyanceExpenses = pm.ConveyanceExpenses;
            sr.PocketExpenses = pm.PocketExpenses;
            sr.GST = pm.GST;
            sr.FinalAmount = pm.FinalAmount;
            sr.ServiceDays = pm.ServiceDays;
            sr.UserId = pm.UserId;
            ServiceRequestService.UpdateServiceRequest(sr);
            if (type == 2 && pm.AuthorizedBy != null)
                UpdateAuthorization(pm, type);

            //record user activity
            string ActionName = $"UpdateServiceRequest (PSBOD)  - {pm.ProvisionalBillId}";
            string TableName = "tblProvisionalBill";
            if (ActionName != null)
            {
                _userHelper.recordUserActivityHistory(pm.UserId, ActionName, TableName);
            }
            //End

            return "success";

        }

        public string TimeSheetAuthorization(TimeSheetDto v)
        {
            string fileName = string.Format("{0}{1}", v.TimeSheetId, ".png");
            string UserSign = string.Format("{0}{1}{2}{3}", v.TimeSheetId, "_", v.UserId, ".png");
            PicannolEntities context = new PicannolEntities();
            var sr = context.tblTimeSheets.Find(v.TimeSheetId);
            sr.AuthorizedBy = v.AuthorizedBy;
            sr.Designation = v.Designation;
            sr.AuthorizedOn = DateTime.Now;
            sr.Authorized = true;
            sr.EmailId = v.AuthorizerEmail;
            sr.ImageId = fileName;
            sr.UserSignatureId = UserSign;
            context.tblTimeSheets.Attach(sr);
            context.Entry(sr).State = EntityState.Modified;
            context.SaveChanges();
            if (v.TimeSheetId > 0 && v.AuthorizedSignature != null & v.AuthorizedSignature != "")
            {
                Image img = Base64ToImage(v.AuthorizedSignature);
                string savedFilePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/TimeSheetSignature/"), fileName);
                img.Save(savedFilePath);
                Image img1 = Base64ToImage(v.UserSignature);
                string savedFilePath1 = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/TimeSheetSignature/"), UserSign);
                img1.Save(savedFilePath1);
            }
            return "success";
        }

        public List<ServiceCallTypeDto> GetServiceCallTypes()
        {
            List<ServiceCallTypeDto> sc = new List<ServiceCallTypeDto>();
            PicannolEntities context = new PicannolEntities();
            sc = (from a in context.tblServiceCallTypes
                  where a.DelInd == false
                  select new ServiceCallTypeDto
                  {
                      CallTypeId = a.CallTypeId,
                      Description = a.Description,
                      ServiceCharges = a.ServiceCharges
                  }).ToList();
            return sc;
        }

        /*public InvoiceDto GetInvoiceDetails(int workOrderId, CustomerDto customer, int provisionalBillId, string PreviewType, int generatedBy, string actionInitiator, string invoiceNumber)
        {
            PicanolUtils utils = new PicanolUtils();
            ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrder(provisionalBillId, actionInitiator);
            DateTime dateTime = new DateTime(2023, 05, 31, 11, 59, 00);

            //ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrderV1(provisionalBillId, actionInitiator);

            //Added by Janesh
            //fetch credit note detail request from user
            var creditnoteDetail = (CreditNoteEditDto)HttpContext.Current.Session["CreditNoteEditable"];
            if (creditnoteDetail != null)
            {
                if (serviceRequest.ServiceDays != 0 && creditnoteDetail.ServiceChargeAmount != 0)
                {
                    var ServiceChargeAmount = (creditnoteDetail.ServiceChargeAmount / serviceRequest.ServiceDays);
                    serviceRequest.ServiceCharge = (int)ServiceChargeAmount;
                }
                if (creditnoteDetail.railairfareAmount != 0 && creditnoteDetail.railairfareAmount != null)
                {
                    serviceRequest.Fare = (int)creditnoteDetail.railairfareAmount;
                }
                if (serviceRequest.PocketExpensesDays != 0 && creditnoteDetail.PocketexpenceAmount != 0)
                {
                    var PocketexoencCharge = (creditnoteDetail.PocketexpenceAmount / serviceRequest.PocketExpensesDays);
                    serviceRequest.PocketExpenses = (int)PocketexoencCharge;
                }
                if (serviceRequest.BoardingDays != 0 && creditnoteDetail.LoadingBoardingAmount != 0)
                {
                    var LoadingBoardingcharge = (creditnoteDetail.LoadingBoardingAmount / serviceRequest.BoardingDays);
                    serviceRequest.BoardingCharges = (int)LoadingBoardingcharge;
                }
                if (creditnoteDetail.ConvenyanceInceidentalAmount != 0 && creditnoteDetail.ConvenyanceInceidentalAmount != null)
                {
                    serviceRequest.ConveyanceExpenses = (int)creditnoteDetail.ConvenyanceInceidentalAmount;
                }
                if (serviceRequest.OvertimeHours != 0 && creditnoteDetail.OverTimehourAmount != 0)
                {
                    var OvertimeHourscharge = (creditnoteDetail.OverTimehourAmount / serviceRequest.OvertimeHours);
                    serviceRequest.OvertimeCharges = (int)OvertimeHourscharge;
                }
            }
            //End

            //Added by Janesh
            //Putting check on date proforma invoice date for fixing the round off total amount value
            //Fixed round off total amount value

            if (serviceRequest.DateCreated <= dateTime)
            {
                invoice = new InvoiceDto();
                invoice.ErrorMessage = "";
                items = new List<InvoiceItemDto>();
                InvoiceItemDto item = new InvoiceItemDto();
                item.Name = "SERVICE CHARGE";
                item.InvoiceItemID = 1;
                item.Qty = serviceRequest.ServiceDays;
                item.Rate = serviceRequest.ServiceCharge;
                item.Amount = serviceRequest.ServiceDays * serviceRequest.ServiceCharge;
                item.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item.UnitOfMeasurement = "DAY";
                item.SGSTAmount = 0;
                item.CGSTAmount = 0;
                item.IGSTAmount = 0;
                item.AmountBTax = item.Amount;
                item.IsService = "Y";
                item.IGSTRate = 18;
                var igstAmt = item.Amount * (item.IGSTRate / 100);
                item.IGSTAmount = Math.Truncate(igstAmt);
                item.Total = (int)(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount);
                items.Add(item);

                InvoiceItemDto item1 = new InvoiceItemDto();
                item1.Name = "To & FRO AIR FARE/RAIL FARE";
                item1.InvoiceItemID = 2;
                item1.Qty = 1;
                item1.Rate = serviceRequest.Fare;
                item1.Amount = serviceRequest.Fare;
                item1.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item1.IsService = "Y";
                item1.UnitOfMeasurement = "";
                item1.SGSTAmount = 0;
                item1.CGSTAmount = 0;
                item1.IGSTAmount = 0;
                item1.AmountBTax = item1.Amount;
                item1.IGSTRate = 18;
                var igstAmt1 = item1.Amount * (item1.IGSTRate / 100);
                item1.IGSTAmount = Math.Truncate(igstAmt1);
                item1.Total = (int)(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount);
                items.Add(item1);

                invoice.InvoiceItems = items;

                InvoiceItemDto item2 = new InvoiceItemDto();
                item2.Name = "POCKET EXPENSE";
                item2.InvoiceItemID = 3;
                item2.Qty = serviceRequest.PocketExpensesDays;
                item2.Rate = serviceRequest.PocketExpenses;
                item2.Amount = serviceRequest.PocketExpensesDays * serviceRequest.PocketExpenses;
                item2.IsService = "Y";
                //item2.HSNCode = "998739";
                item2.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item2.UnitOfMeasurement = "DAY";
                item2.SGSTAmount = 0;
                item2.CGSTAmount = 0;
                item2.IGSTAmount = 0;
                item2.AmountBTax = item2.Amount;
                item2.IGSTRate = 18;
                var igstAmt2 = item2.Amount * (item2.IGSTRate / 100);
                item2.IGSTAmount = Math.Truncate(igstAmt2);
                item2.Total = Math.Round(item2.Amount + item2.IGSTAmount + item2.SGSTAmount + item2.CGSTAmount);
                items.Add(item2);

                invoice.InvoiceItems = items;

                InvoiceItemDto item3 = new InvoiceItemDto();
                item3.Name = "LODGING & BOARDING";
                item3.InvoiceItemID = 4;
                item3.Qty = (int)serviceRequest.BoardingDays;
                item3.Rate = serviceRequest.BoardingCharges;
                item3.Amount = serviceRequest.BoardingDays * serviceRequest.BoardingCharges;
                item3.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item3.IsService = "Y";
                item3.UnitOfMeasurement = "DAY";
                item3.SGSTAmount = 0;
                item3.CGSTAmount = 0;
                item3.IGSTAmount = 0;
                item3.AmountBTax = item3.Amount;
                item3.IGSTRate = 18;
                var igstAmt3 = item3.Amount * (item3.IGSTRate / 100);
                item3.IGSTAmount = Math.Truncate(igstAmt3);
                item3.Total = (int)(item3.Amount + item3.IGSTAmount + item3.SGSTAmount + item3.CGSTAmount);
                items.Add(item3);

                InvoiceItemDto item4 = new InvoiceItemDto();
                item4.Name = "CONVEYANCE & INCIDENTAL EXPENSES";
                item4.InvoiceItemID = 5;
                item4.Qty = 1;
                item4.Rate = serviceRequest.ConveyanceExpenses;
                item4.Amount = serviceRequest.ConveyanceExpenses;
                item4.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item4.IsService = "Y";
                item4.UnitOfMeasurement = "";
                item4.SGSTAmount = 0;
                item4.CGSTAmount = 0;
                item4.IGSTAmount = 0;
                item4.AmountBTax = item4.Amount;
                item4.IGSTRate = 18;
                var igstAmt4 = item4.Amount * (item4.IGSTRate / 100);
                item4.IGSTAmount = Math.Truncate(igstAmt4);
                item4.Total = (int)(item4.Amount + item4.IGSTAmount + item4.SGSTAmount + item4.CGSTAmount);
                items.Add(item4);

                InvoiceItemDto item5 = new InvoiceItemDto();
                item5.Name = "OVER TIME HOUR";
                item5.InvoiceItemID = 6;
                item5.Qty = serviceRequest.OvertimeHours;
                item5.Rate = serviceRequest.OvertimeCharges;
                item5.Amount = serviceRequest.OvertimeHours * serviceRequest.OvertimeCharges;
                item5.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item5.IsService = "Y";
                item5.UnitOfMeasurement = "HOUR";
                item5.SGSTAmount = 0;
                item5.CGSTAmount = 0;
                item5.IGSTAmount = 0;
                item5.AmountBTax = item5.Amount;
                item5.IGSTRate = 18;
                var igstAmt5 = item5.Amount * (item5.IGSTRate / 100);
                item5.IGSTAmount = Math.Truncate(igstAmt5);
                item5.Total = (int)(item5.Amount + item5.IGSTAmount + item5.SGSTAmount + item5.CGSTAmount);
                items.Add(item5);

                invoice.InvoiceItems = items;
                foreach (var a in items)
                {
                    invoice.Amount += a.AmountBTax;
                    invoice.SGSTTax += a.SGSTAmount;
                    invoice.CGSTTax += a.CGSTAmount;
                    invoice.IGSTTax += a.IGSTAmount;
                    invoice.TotalTax += (a.SGSTAmount + a.CGSTAmount + a.IGSTAmount);
                }

                invoice.AmountBTax = invoice.Amount;
                invoice.Amount = invoice.AmountBTax + invoice.TotalTax;
                invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";
            }
            else
            {
                invoice = new InvoiceDto();
                invoice.ErrorMessage = "";
                items = new List<InvoiceItemDto>();
                InvoiceItemDto item = new InvoiceItemDto();
                item.Name = "SERVICE CHARGE";
                item.InvoiceItemID = 1;
                item.Qty = serviceRequest.ServiceDays;
                item.Rate = serviceRequest.ServiceCharge;
                item.Amount = serviceRequest.ServiceDays * serviceRequest.ServiceCharge;
                //item.HSNCode = "998739";
                item.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item.UnitOfMeasurement = "DAY";
                item.SGSTAmount = 0;
                item.CGSTAmount = 0;
                item.IGSTAmount = 0;
                item.AmountBTax = item.Amount;
                item.IsService = "Y";
                item.IGSTRate = 18;
                //item.IGSTAmount = Math.Round((item.Amount * (item.IGSTRate / 100)), 0);
                item.IGSTAmount = Math.Round(item.Amount * (item.IGSTRate / 100));
                //item.Total = Math.Round(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount, 0);
                item.Total = (int)(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount);
                items.Add(item);

                InvoiceItemDto item1 = new InvoiceItemDto();
                item1.Name = "To & FRO AIR FARE/RAIL FARE";
                item1.InvoiceItemID = 2;
                item1.Qty = 1;
                item1.Rate = serviceRequest.Fare;
                item1.Amount = serviceRequest.Fare;
                item1.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item1.IsService = "Y";
                item1.UnitOfMeasurement = "";
                item1.SGSTAmount = 0;
                item1.CGSTAmount = 0;
                item1.IGSTAmount = 0;
                item1.AmountBTax = item1.Amount;
                item1.IGSTRate = 18;
                // item1.IGSTAmount = Math.Round((item1.Amount * (item1.IGSTRate / 100)), 0);
                item1.IGSTAmount = Math.Round(item1.Amount * (item1.IGSTRate / 100));
                //item1.Total = Math.Round(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount, 0);
                item1.Total = (int)(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount);
                items.Add(item1);

                invoice.InvoiceItems = items;

                InvoiceItemDto item2 = new InvoiceItemDto();
                item2.Name = "POCKET EXPENSE";
                item2.InvoiceItemID = 3;
                item2.Qty = serviceRequest.PocketExpensesDays;
                item2.Rate = serviceRequest.PocketExpenses;
                item2.Amount = serviceRequest.PocketExpensesDays * serviceRequest.PocketExpenses;
                item2.IsService = "Y";
                //item2.HSNCode = "998739";
                item2.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item2.UnitOfMeasurement = "DAY";
                item2.SGSTAmount = 0;
                item2.CGSTAmount = 0;
                item2.IGSTAmount = 0;
                item2.AmountBTax = item2.Amount;
                item2.IGSTRate = 18;
                //item2.IGSTAmount = Math.Round((item2.Amount * (item2.IGSTRate / 100)), 0);
                item2.IGSTAmount = Math.Round(item2.Amount * (item2.IGSTRate / 100));
                item2.Total = Math.Round(item2.Amount + item2.IGSTAmount + item2.SGSTAmount + item2.CGSTAmount);
                items.Add(item2);

                invoice.InvoiceItems = items;

                InvoiceItemDto item3 = new InvoiceItemDto();
                item3.Name = "LODGING & BOARDING";
                item3.InvoiceItemID = 4;
                item3.Qty = (int)serviceRequest.BoardingDays;
                item3.Rate = serviceRequest.BoardingCharges;
                item3.Amount = serviceRequest.BoardingDays * serviceRequest.BoardingCharges;
                //item3.HSNCode = "998739";
                item3.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item3.IsService = "Y";
                item3.UnitOfMeasurement = "DAY";
                item3.SGSTAmount = 0;
                item3.CGSTAmount = 0;
                item3.IGSTAmount = 0;
                item3.AmountBTax = item3.Amount;
                item3.IGSTRate = 18;
                item3.IGSTAmount = Math.Round(item3.Amount * (item3.IGSTRate / 100));
                item3.Total = (int)(item3.Amount + item3.IGSTAmount + item3.SGSTAmount + item3.CGSTAmount);
                items.Add(item3);

                InvoiceItemDto item4 = new InvoiceItemDto();
                item4.Name = "CONVEYANCE & INCIDENTAL EXPENSES";
                item4.InvoiceItemID = 5;
                item4.Qty = 1;
                item4.Rate = serviceRequest.ConveyanceExpenses;
                item4.Amount = serviceRequest.ConveyanceExpenses;
                //item4.HSNCode = "998739";
                item4.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item4.IsService = "Y";
                item4.UnitOfMeasurement = "";
                item4.SGSTAmount = 0;
                item4.CGSTAmount = 0;
                item4.IGSTAmount = 0;
                item4.AmountBTax = item4.Amount;
                item4.IGSTRate = 18;
                item4.IGSTAmount = Math.Round(item4.Amount * (item4.IGSTRate / 100));
                item4.Total = (int)(item4.Amount + item4.IGSTAmount + item4.SGSTAmount + item4.CGSTAmount);
                items.Add(item4);

                InvoiceItemDto item5 = new InvoiceItemDto();
                item5.Name = "OVER TIME HOUR";
                item5.InvoiceItemID = 6;
                item5.Qty = serviceRequest.OvertimeHours;
                item5.Rate = serviceRequest.OvertimeCharges;
                item5.Amount = serviceRequest.OvertimeHours * serviceRequest.OvertimeCharges;
                //item5.HSNCode = "998739";
                item5.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
                item5.IsService = "Y";
                item5.UnitOfMeasurement = "HOUR";
                item5.SGSTAmount = 0;
                item5.CGSTAmount = 0;
                item5.IGSTAmount = 0;
                item5.AmountBTax = item5.Amount;
                item5.IGSTRate = 18;
                item5.IGSTAmount = Math.Round(item5.Amount * (item5.IGSTRate / 100));
                item5.Total = (int)(item5.Amount + item5.IGSTAmount + item5.SGSTAmount + item5.CGSTAmount);
                items.Add(item5);

                invoice.InvoiceItems = items;
                foreach (var a in items)
                {
                    invoice.Amount += a.AmountBTax;
                    invoice.SGSTTax += a.SGSTAmount;
                    invoice.CGSTTax += a.CGSTAmount;
                    invoice.IGSTTax += a.IGSTAmount;
                    invoice.TotalTax += (a.SGSTAmount + a.CGSTAmount + a.IGSTAmount);
                }

                invoice.AmountBTax = invoice.Amount;
                invoice.Amount = invoice.AmountBTax + invoice.TotalTax;
                invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";
            }
            //End


            HttpContext.Current.Session["CreditNoteEditable"] = null;
            *//*Newly Added to get invoice for SubCustomer 
             * Date:22 March 2021
             * if Customer id = 0 
             *//*
            invoice.SubCustomerId = serviceRequest.SubCustomerId;
            PicannolEntities context = new PicannolEntities();
            if (invoice.SubCustomerId != 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == invoice.SubCustomerId && x.DelInd == false).FirstOrDefault();

                customer.SubCustomerId = a.SubCustomerId;
                customer.SubCustomerName = a.SubCustomerName;
            }
            var order = (Guid)context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).Select(x => x.WorkOrderGUID).FirstOrDefault();
            if (PreviewType != "PreviewDetails" && PreviewType != "CreditNote" && PreviewType != "DebitNote")
            {

                invoice.InvoiceNo = SaveNewInvoice(order, invoice.Amount, customer, provisionalBillId, generatedBy, actionInitiator);
                var invDeatils = context.tblInvoices.Where(x => x.OrderGuid == order && x.DelInd == false && x.ProvisionalBillId == provisionalBillId).FirstOrDefault();
                if (invDeatils != null)
                {
                    invoice.InvoiceDate = invDeatils.InvoiceDate;
                }

            }
            else if (PreviewType == "CreditNote" && PreviewType != "PreviewDetails")
            {
                //invoice.Amount = FinalAmount; //coment by janesh
                invoice.CreditNoteNo = SaveCreditNotes(order, invoice, customer, provisionalBillId, generatedBy, actionInitiator, invoiceNumber);
                //invoice.CreditNoteDate = DateTime.Now.Date;
                invoice.InvoiceNo = invoiceNumber;
                if (invoice.CreditNoteNo != null)
                {
                    var creditNoteType = ConfigurationManager.AppSettings["CreditNote"];
                    var ecreditNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.CreditNoteNo
                                            && x.DelInd == false && x.Cancelled == false
                                            && x.Type == creditNoteType).FirstOrDefault();
                    if (ecreditNote != null)
                    {
                        invoice.EInVoiceQrCode = ecreditNote.QRCode;
                        invoice.IRN = ecreditNote.IRN;
                        invoice.CreditNoteNo = invoice.CreditNoteNo;

                        //record user activity
                        string ActionName = $" Download E-Inv, Inv No - {ecreditNote.InvoiceNo}"
                        + $", OrderGUID - {ecreditNote.OrderID}"
                        + $", PSBID - {ecreditNote.ProvisionalBillId}";
                        string TableName = "tblEInvoice";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        invoice.ErrorMessage = "OOps! Something went wrong. Please try again latter";

                    }
                }
            }
            else if (PreviewType == "DebitNote" && PreviewType != "PreviewDetails" && PreviewType != "CreditNote")
            {
                //coment by janesh
                //invoice.Amount = FinalAmount;

                invoice.DebitNoteNo = SaveDebitNotes(order, invoice, customer, provisionalBillId, generatedBy, actionInitiator, invoiceNumber);
                invoice.InvoiceNo = invoiceNumber;
                if (invoice.DebitNoteNo != null)
                {
                    var debitnottype = ConfigurationManager.AppSettings["DebitNote"];
                    var debitNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.DebitNoteNo
                                            && x.DelInd == false && x.Cancelled == false
                                            && x.Type == debitnottype).FirstOrDefault();


                    if (debitNote != null)
                    {
                        invoice.EInVoiceQrCode = debitNote.QRCode;
                        invoice.IRN = debitNote.IRN;
                        invoice.CreditNoteNo = invoice.CreditNoteNo;
                        invoice.DebitNoteDate = debitNote.InvoiceDate.ToString();

                        //record user activity
                        string ActionName = $" Download E-Inv, Inv No - {debitNote.InvoiceNo}"
                        + $", OrderGUID - {debitNote.OrderID}"
                        + $", PSBID - {debitNote.ProvisionalBillId}";
                        string TableName = "tblEInvoice";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        invoice.ErrorMessage = "OOps! Something went wrong. Please try again latter";
                    }
                }
            }
            else
            {
                invoice.InvoiceDate = DateTime.Now;
            }
            invoice.ProvisionalBillNo = serviceRequest.ProvisionalBillNo;
            invoice.AttendedFrom = serviceRequest.FromDate.ToShortDateString();
            invoice.AttendedTo = serviceRequest.ToDate.ToShortDateString();
            invoice.EngineerName = serviceRequest.UserName;
            return invoice;
        }*/


        public InvoiceDto GetInvoiceDetails(int workOrderId, CustomerDto customer, int provisionalBillId, string PreviewType, int generatedBy, string actionInitiator, string invoiceNumber)
        {
            PicanolUtils utils = new PicanolUtils();
            ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrder(provisionalBillId, actionInitiator);
            DateTime dateTime = new DateTime(2023, 05, 31, 11, 59, 00);

            //ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrderV1(provisionalBillId, actionInitiator);

            //Added by Janesh
            //fetch credit note detail request from user
            var creditnoteDetail = (CreditNoteEditDto)HttpContext.Current.Session["CreditNoteEditable"];
            if (creditnoteDetail != null)
            {
                if (serviceRequest.ServiceDays != 0 && creditnoteDetail.ServiceChargeAmount != 0)
                {
                    var ServiceChargeAmount = (creditnoteDetail.ServiceChargeAmount / serviceRequest.ServiceDays);
                    serviceRequest.ServiceCharge = (int)ServiceChargeAmount;
                }
                if (creditnoteDetail.railairfareAmount != 0 && creditnoteDetail.railairfareAmount != null)
                {
                    serviceRequest.Fare = (int)creditnoteDetail.railairfareAmount;
                }
                if (serviceRequest.PocketExpensesDays != 0 && creditnoteDetail.PocketexpenceAmount != 0)
                {
                    var PocketexoencCharge = (creditnoteDetail.PocketexpenceAmount / serviceRequest.PocketExpensesDays);
                    serviceRequest.PocketExpenses = (int)PocketexoencCharge;
                }
                if (serviceRequest.BoardingDays != 0 && creditnoteDetail.LoadingBoardingAmount != 0)
                {
                    var LoadingBoardingcharge = (creditnoteDetail.LoadingBoardingAmount / serviceRequest.BoardingDays);
                    serviceRequest.BoardingCharges = (int)LoadingBoardingcharge;
                }
                if (creditnoteDetail.ConvenyanceInceidentalAmount != 0 && creditnoteDetail.ConvenyanceInceidentalAmount != null)
                {
                    serviceRequest.ConveyanceExpenses = (int)creditnoteDetail.ConvenyanceInceidentalAmount;
                }
                if (serviceRequest.OvertimeHours != 0 && creditnoteDetail.OverTimehourAmount != 0)
                {
                    var OvertimeHourscharge = (creditnoteDetail.OverTimehourAmount / serviceRequest.OvertimeHours);
                    serviceRequest.OvertimeCharges = (int)OvertimeHourscharge;
                }
            }
            //End

            //Added by Janesh
            //Putting check on date proforma invoice date for fixing the round off total amount value
            //Fixed round off total amount value

            invoice = new InvoiceDto();
            invoice.ErrorMessage = "";
            items = new List<InvoiceItemDto>();
            InvoiceItemDto item = new InvoiceItemDto();
            item.Name = "SERVICE CHARGE";
            item.InvoiceItemID = 1;
            item.Qty = serviceRequest.ServiceDays;
            item.Rate = serviceRequest.ServiceCharge;
            item.Amount = serviceRequest.ServiceDays * serviceRequest.ServiceCharge;
            item.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item.UnitOfMeasurement = "DAY";
            item.SGSTAmount = 0;
            item.CGSTAmount = 0;
            item.IGSTAmount = 0;
            item.AmountBTax = item.Amount;
            item.IsService = "Y";
            item.IGSTRate = 18;
            var igstAmt = item.Amount * (item.IGSTRate / 100);
            if (serviceRequest.DateCreated <= dateTime)
                item.IGSTAmount = Math.Truncate(igstAmt);
            else
                item.IGSTAmount = Math.Round(item.Amount * (item.IGSTRate / 100));

            item.Total = (int)(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount);

            items.Add(item);

            InvoiceItemDto item1 = new InvoiceItemDto();
            item1.Name = "To & FRO AIR FARE/RAIL FARE";
            item1.InvoiceItemID = 2;
            item1.Qty = 1;
            item1.Rate = serviceRequest.Fare;
            item1.Amount = serviceRequest.Fare;
            item1.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item1.IsService = "Y";
            item1.UnitOfMeasurement = "";
            item1.SGSTAmount = 0;
            item1.CGSTAmount = 0;
            item1.IGSTAmount = 0;
            item1.AmountBTax = item1.Amount;
            item1.IGSTRate = 18;
            var igstAmt1 = item1.Amount * (item1.IGSTRate / 100);
            if (serviceRequest.DateCreated <= dateTime)
                item1.IGSTAmount = Math.Truncate(igstAmt1);
            else
                item1.IGSTAmount = Math.Round(item1.Amount * (item1.IGSTRate / 100));

            item1.Total = (int)(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount);
            items.Add(item1);

            invoice.InvoiceItems = items;

            InvoiceItemDto item2 = new InvoiceItemDto();
            item2.Name = "POCKET EXPENSE";
            item2.InvoiceItemID = 3;
            item2.Qty = serviceRequest.PocketExpensesDays;
            item2.Rate = serviceRequest.PocketExpenses;
            item2.Amount = serviceRequest.PocketExpensesDays * serviceRequest.PocketExpenses;
            item2.IsService = "Y";
            //item2.HSNCode = "998739";
            item2.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item2.UnitOfMeasurement = "DAY";
            item2.SGSTAmount = 0;
            item2.CGSTAmount = 0;
            item2.IGSTAmount = 0;
            item2.AmountBTax = item2.Amount;
            item2.IGSTRate = 18;
            var igstAmt2 = item2.Amount * (item2.IGSTRate / 100);
            if (serviceRequest.DateCreated <= dateTime)
                item2.IGSTAmount = Math.Truncate(igstAmt2);
            else
                item2.IGSTAmount = Math.Round(item2.Amount * (item2.IGSTRate / 100));

            item2.Total = Math.Round(item2.Amount + item2.IGSTAmount + item2.SGSTAmount + item2.CGSTAmount);
            items.Add(item2);

            invoice.InvoiceItems = items;

            InvoiceItemDto item3 = new InvoiceItemDto();
            item3.Name = "LODGING & BOARDING";
            item3.InvoiceItemID = 4;
            item3.Qty = (int)serviceRequest.BoardingDays;
            item3.Rate = serviceRequest.BoardingCharges;
            item3.Amount = serviceRequest.BoardingDays * serviceRequest.BoardingCharges;
            item3.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item3.IsService = "Y";
            item3.UnitOfMeasurement = "DAY";
            item3.SGSTAmount = 0;
            item3.CGSTAmount = 0;
            item3.IGSTAmount = 0;
            item3.AmountBTax = item3.Amount;
            item3.IGSTRate = 18;
            var igstAmt3 = item3.Amount * (item3.IGSTRate / 100);
            if (serviceRequest.DateCreated <= dateTime)
                item3.IGSTAmount = Math.Truncate(igstAmt3);
            else
                item3.IGSTAmount = Math.Round(item3.Amount * (item3.IGSTRate / 100));

            item3.Total = (int)(item3.Amount + item3.IGSTAmount + item3.SGSTAmount + item3.CGSTAmount);
            items.Add(item3);

            InvoiceItemDto item4 = new InvoiceItemDto();
            item4.Name = "CONVEYANCE & INCIDENTAL EXPENSES";
            item4.InvoiceItemID = 5;
            item4.Qty = 1;
            item4.Rate = serviceRequest.ConveyanceExpenses;
            item4.Amount = serviceRequest.ConveyanceExpenses;
            item4.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item4.IsService = "Y";
            item4.UnitOfMeasurement = "";
            item4.SGSTAmount = 0;
            item4.CGSTAmount = 0;
            item4.IGSTAmount = 0;
            item4.AmountBTax = item4.Amount;
            item4.IGSTRate = 18;
            var igstAmt4 = item4.Amount * (item4.IGSTRate / 100);
            if (serviceRequest.DateCreated <= dateTime)
                item4.IGSTAmount = Math.Truncate(igstAmt4);
            else
                item4.IGSTAmount = Math.Round(item4.Amount * (item4.IGSTRate / 100));

            item4.Total = (int)(item4.Amount + item4.IGSTAmount + item4.SGSTAmount + item4.CGSTAmount);
            items.Add(item4);

            InvoiceItemDto item5 = new InvoiceItemDto();
            item5.Name = "OVER TIME HOUR";
            item5.InvoiceItemID = 6;
            item5.Qty = serviceRequest.OvertimeHours;
            item5.Rate = serviceRequest.OvertimeCharges;
            item5.Amount = serviceRequest.OvertimeHours * serviceRequest.OvertimeCharges;
            item5.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item5.IsService = "Y";
            item5.UnitOfMeasurement = "HOUR";
            item5.SGSTAmount = 0;
            item5.CGSTAmount = 0;
            item5.IGSTAmount = 0;
            item5.AmountBTax = item5.Amount;
            item5.IGSTRate = 18;
            var igstAmt5 = item5.Amount * (item5.IGSTRate / 100);
            if (serviceRequest.DateCreated <= dateTime)
                item5.IGSTAmount = Math.Truncate(igstAmt5);
            else
                item5.IGSTAmount = Math.Round(item5.Amount * (item5.IGSTRate / 100));

            item5.Total = (int)(item5.Amount + item5.IGSTAmount + item5.SGSTAmount + item5.CGSTAmount);
            items.Add(item5);

            invoice.InvoiceItems = items;
            foreach (var a in items)
            {
                invoice.Amount += a.AmountBTax;
                invoice.SGSTTax += a.SGSTAmount;
                invoice.CGSTTax += a.CGSTAmount;
                invoice.IGSTTax += a.IGSTAmount;
                invoice.TotalTax += (a.SGSTAmount + a.CGSTAmount + a.IGSTAmount);
            }

            invoice.AmountBTax = invoice.Amount;
            invoice.Amount = invoice.AmountBTax + invoice.TotalTax;
            invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";

            HttpContext.Current.Session["CreditNoteEditable"] = null;
            /*Newly Added to get invoice for SubCustomer 
             * Date:22 March 2021
             * if Customer id = 0 
             */
            invoice.SubCustomerId = serviceRequest.SubCustomerId;
            PicannolEntities context = new PicannolEntities();
            if (invoice.SubCustomerId != 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == invoice.SubCustomerId && x.DelInd == false).FirstOrDefault();

                customer.SubCustomerId = a.SubCustomerId;
                customer.SubCustomerName = a.SubCustomerName;
            }
            var order = (Guid)context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).Select(x => x.WorkOrderGUID).FirstOrDefault();
            if (PreviewType != "PreviewDetails" && PreviewType != "CreditNote" && PreviewType != "DebitNote")
            {
                invoice.InvoiceNo = SaveNewInvoice(order, invoice.Amount, customer, provisionalBillId, generatedBy, actionInitiator);
                var invDeatils = context.tblInvoices.Where(x => x.OrderGuid == order && x.DelInd == false && x.ProvisionalBillId == provisionalBillId).FirstOrDefault();
                if (invDeatils != null)
                {
                    invoice.InvoiceDate = invDeatils.InvoiceDate;
                }

            }
            else if (PreviewType == "CreditNote" && PreviewType != "PreviewDetails")
            {
                //invoice.Amount = FinalAmount; //coment by janesh
                invoice.CreditNoteNo = SaveCreditNotes(order, invoice, customer, provisionalBillId, generatedBy, actionInitiator, invoiceNumber);
                //invoice.CreditNoteDate = DateTime.Now.Date;
                invoice.InvoiceNo = invoiceNumber;
                if (invoice.CreditNoteNo != null)
                {
                    var creditNoteType = ConfigurationManager.AppSettings["CreditNote"];
                    var ecreditNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.CreditNoteNo
                                            && x.DelInd == false && x.Cancelled == false
                                            && x.Type == creditNoteType).FirstOrDefault();
                    if (ecreditNote != null)
                    {
                        invoice.EInVoiceQrCode = ecreditNote.QRCode;
                        invoice.IRN = ecreditNote.IRN;
                        invoice.CreditNoteNo = invoice.CreditNoteNo;

                        //record user activity
                        string ActionName = $" Download E-Inv, Inv No - {ecreditNote.InvoiceNo}"
                        + $", OrderGUID - {ecreditNote.OrderID}"
                        + $", PSBID - {ecreditNote.ProvisionalBillId}";
                        string TableName = "tblEInvoice";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        invoice.ErrorMessage = "OOps! Something went wrong. Please try again latter";

                    }
                }
            }
            else if (PreviewType == "DebitNote" && PreviewType != "PreviewDetails" && PreviewType != "CreditNote")
            {
                //comment by janesh
                //invoice.Amount = FinalAmount;
                invoice.DebitNoteNo = SaveDebitNotes(order, invoice, customer, provisionalBillId, generatedBy, actionInitiator, invoiceNumber);
                invoice.InvoiceNo = invoiceNumber;
                if (invoice.DebitNoteNo != null)
                {
                    var debitnottype = ConfigurationManager.AppSettings["DebitNote"];
                    var debitNote = context.tblEInvoices.Where(x => x.InvoiceNo == invoice.DebitNoteNo
                                            && x.DelInd == false && x.Cancelled == false
                                            && x.Type == debitnottype).FirstOrDefault();
                    if (debitNote != null)
                    {
                        invoice.EInVoiceQrCode = debitNote.QRCode;
                        invoice.IRN = debitNote.IRN;
                        invoice.CreditNoteNo = invoice.CreditNoteNo;
                        invoice.DebitNoteDate = debitNote.InvoiceDate.ToString();

                        //record user activity
                        string ActionName = $" Download E-DBN, InvNo - {debitNote.InvoiceNo}"
                        + $", OrderGUID - {debitNote.OrderID}"
                        + $", PSBID - {debitNote.ProvisionalBillId}";
                        string TableName = "tblDebitNote, tblEInvoice";
                        if (ActionName != null)
                        {
                            new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                        }
                        //End
                    }
                    else
                    {
                        invoice.ErrorMessage = "Oops! Something went wrong. Please try again latter";
                    }
                }
            }
            else
            {
                invoice.InvoiceDate = DateTime.Now;
            }
            invoice.ProvisionalBillNo = serviceRequest.ProvisionalBillNo;
            invoice.AttendedFrom = serviceRequest.FromDate.ToShortDateString();
            invoice.AttendedTo = serviceRequest.ToDate.ToShortDateString();
            invoice.EngineerName = serviceRequest.UserName;
            return invoice;
        }

        //Save Credit note
        private string SaveCreditNotes(Guid order, InvoiceDto invoice, CustomerDto customer, int provisionalBillId, int generatedById, string actionInitiator, string invoiceNumber)
        {
            PicannolEntities context = new PicannolEntities();
            string newInvNo = "";
            string lastInvoiceNo = "";
            RespPlGenIRNDec respPlGenIRNDec = new RespPlGenIRNDec();
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];

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
            cred.OrderGuid = order;
            cred.InvoiceNumber = invoiceNumber;
            cred.CreditNoteDate = DateTime.Now;

            //Added by Janesh
            foreach (var item in invoice.InvoiceItems)
            {
                if (item.Name == "SERVICE CHARGE")
                {
                    cred.ServiceDays = (decimal)item.Qty;
                    //cred.ServiceCharge = (decimal)item.Rate;
                    cred.ServiceCharge = (item.Rate == 0) ? item.Amount : item.Rate;
                }
                if (item.Name == "To & FRO AIR FARE/RAIL FARE")
                {
                    //cred.Fare = item.Rate;
                    cred.Fare = (item.Rate == 0) ? item.Amount : item.Rate;
                }
                if (item.Name == "POCKET EXPENSE")
                {
                    cred.PocketExpenseDays = item.Qty;
                    //cred.PocketExpeses = item.Rate;
                    cred.PocketExpeses = (item.Rate == 0) ? item.Amount : item.Rate;
                }
                if (item.Name == "LODGING & BOARDING")
                {
                    cred.BoardingDays = item.Qty;
                    //cred.BoardingCharges = item.Rate;
                    cred.BoardingCharges = (item.Rate == 0) ? item.Amount : item.Rate;

                }
                if (item.Name == "CONVEYANCE & INCIDENTAL EXPENSES")
                {
                    //cred.ConveyanceExpenses = item.Rate;
                    cred.ConveyanceExpenses = (item.Rate == 0) ? item.Amount : item.Rate;

                }
                if (item.Name == "OVER TIME HOUR")
                {
                    cred.OvertimeHours = item.Qty;
                    cred.OvertimeCharges = (item.Rate == 0) ? item.Amount : item.Rate;
                    //cred.OvertimeCharges = item.Rate;
                }
            }
            //End

            cred.CRNCreated = true;
            cred.TaxableAmount = invoice.Amount;
            cred.CustomerId = customer.CustomerId;
            cred.TaxAmount = invoice.Amount;
            cred.Createdby = UserInfo.UserId;
            cred.TotalAmount = invoice.Amount;
            cred.ProvisionalBillId = provisionalBillId;
            cred.DelInd = false;
            cred.SubCustomerId = customer.SubCustomerId;
            cred.CreditNoteFileName = (customer.SubCustomerId > 0)
                ? customer.SubCustomerName + "_" + ConstantsHelper.InvoiceType.CN.ToString() + ".pdf"
                : customer.CustomerName + "_" + ConstantsHelper.InvoiceType.CN.ToString() + ".pdf";

            var result = context.tblCreditNotes.Where(x => x.OrderGuid == order && x.InvoiceNumber == invoiceNumber && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
            if (result != null)
            {
                cred.CreditNoteNo = result.CreditNoteNo;
                invoice.CreditNoteDate = result.CreditNoteDate;
                newInvNo = cred.CreditNoteNo;
            }
            else
            {
                OrderDto orderDto = new OrderDto();
                orderDto.OrderGUID = order;
                orderDto.ProvisionalBillId = provisionalBillId;
                invoice.InvoiceNo = newInvNo;
                invoice.ProvisionalBillId = provisionalBillId;
                invoice.CreditNoteDate = DateTime.Now.Date;
                try
                {
                    respPlGenIRNDec = _eInvoiceHelper.GenerateIRN(customer, invoice, items, orderDto);
                    if (respPlGenIRNDec.Irn != null)
                    {
                        cred.CreditNoteNo = newInvNo;
                        context.tblCreditNotes.Add(cred);
                        context.SaveChanges();

                        //record user activity
                        string ActionName = $"E-CRN created, CRN No - {newInvNo}"
                        + $", OrderGUID - {orderDto.OrderGUID}"
                        + $", PSBID - {orderDto.ProvisionalBillId}";
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
                            return invoice.ErrorMessage = respPlGenIRNDec.errorMsg;
                        }

                    }
                }
                catch (Exception ex)
                {
                    return invoice.ErrorMessage = "Something went wrong. Try again latter"; ;
                }
            }

            return newInvNo;
        }
        //End


        // Generate Debit note service 
        private string SaveDebitNotes(Guid order, InvoiceDto invoice, CustomerDto customer, int provisionalBillId, int generatedById, string actionInitiator, string invoiceNumber)
        {
            PicannolEntities context = new PicannolEntities();
            string newInvNo = "";
            string lastInvoiceNo = "";
            RespPlGenIRNDec respPlGenIRNDec = new RespPlGenIRNDec();
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var creditNoteDtl = context.tblCreditNotes.Where(x => x.OrderGuid == order 
            && x.InvoiceNumber == invoiceNumber 
            && x.DelInd == false && x.Cancelled == false).FirstOrDefault();

            string invType = ConstantsHelper.InvoiceType.DN.ToString();
            lastInvoiceNo = GetLastDebitInvoiceNo(invType);
            if (lastInvoiceNo != null && lastInvoiceNo != "")
            {
                newInvNo = GetNewInvoiceNumber(lastInvoiceNo, invType);

            }
            else
            {
                newInvNo = GetFirstInvoiceNo(invType);
            }
            var debitNote = new tblDebitNote();
            debitNote.OrderGuid = order;
            debitNote.CreditNoteNo = creditNoteDtl.CreditNoteNo;
            debitNote.InvoiceNo = invoiceNumber;
            debitNote.DebitNoteDate = DateTime.Now;
            debitNote.DateCreated = DateTime.Now;
            debitNote.TotalAmount = invoice.Amount;
            debitNote.CustomerId = customer.CustomerId;
            debitNote.SubCustomerId = customer.SubCustomerId;
            debitNote.UserId = UserInfo.UserId;
            debitNote.ProvisionalBillId = provisionalBillId;
            debitNote.DelInd = false;
            debitNote.DebitNoteFileName = (customer.SubCustomerId > 0)
                ? customer.SubCustomerName + "_" + ConstantsHelper.InvoiceType.DN.ToString() + ".pdf"
                :customer.CustomerName + "_" + ConstantsHelper.InvoiceType.DN.ToString() + ".pdf"; ;

            //debitNote.DebitNoteFileName = customer.CustomerName + "_" + ConstantsHelper.InvoiceType.DN.ToString() + ".pdf";
            var result = context.tblDebitNotes.Where(x => x.OrderGuid == order && x.InvoiceNo == invoiceNumber && x.DelInd == false).FirstOrDefault();
            if (result != null)
            {
                debitNote.DebitNoteNo = result.DebitNoteNo;
                invoice.CreditNoteDate = result.DebitNoteDate;
                newInvNo = debitNote.DebitNoteNo;
            }
            else
            {
                OrderDto orderDto = new OrderDto();
                orderDto.OrderGUID = order;
                orderDto.ProvisionalBillId = provisionalBillId;
                invoice.InvoiceNo = newInvNo;
                invoice.ProvisionalBillId = provisionalBillId;
                invoice.CreditNoteDate = DateTime.Now.Date;
                try
                {
                    respPlGenIRNDec = _eInvoiceHelper.GenerateIRN(customer, invoice, items, orderDto);
                    if (respPlGenIRNDec.Irn != null)
                    {
                        //debitNote.CreditNoteNo = "";
                        debitNote.DebitNoteNo = newInvNo;
                        context.tblDebitNotes.Add(debitNote);
                        context.SaveChanges();

                        //record user activity
                        string ActionName = $"E-CRN created, CRN No - {newInvNo}"
                        + $", OrderGUID - {orderDto.OrderGUID}"
                        + $", PSBID - {orderDto.ProvisionalBillId}";
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
                            return invoice.ErrorMessage = respPlGenIRNDec.errorMsg;
                        }

                    }
                }
                catch (Exception ex)
                {

                    return invoice.ErrorMessage = "Something went wrong. Try again latter"; ;
                }
            }

            return newInvNo;
        }

        private string GetNewInvoiceNumber(string lastInvoiceNo, string invType)
        {
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
            //end

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
        //End

        public string GetLastInvoiceNo(string invType)
        {
            var i = "";
            PicannolEntities context = new PicannolEntities();
            var inv = (from a in context.tblCreditNotes
                       where a.DelInd == false && a.CreditNoteNo.Contains(invType)
                       select a).ToList();

            inv = inv.OrderByDescending(x => x.CreditNoteId).ToList();
            i = inv.Select(x => x.CreditNoteNo).FirstOrDefault();
            return i;

        }

        public string GetLastDebitInvoiceNo(string invType)
        {
            var i = "";
            PicannolEntities context = new PicannolEntities();
            var inv = (from a in context.tblDebitNotes
                       where a.DelInd == false && a.DebitNoteNo.Contains(invType)
                       select a).ToList();

            inv = inv.OrderByDescending(x => x.DebitNoteId).ToList();
            i = inv.Select(x => x.DebitNoteNo).FirstOrDefault();
            return i;

        }
        //End



        #region cancelled InvoiceDetail
        public InvoiceDto GetCancelledInvoiceDetail(int workOrderId, CustomerDto customer, int provisionalBillId, string PreviewType, int GeneratedById = 0, string actionInitiator = "")
        {
            PicanolUtils utils = new PicanolUtils();
            ServiceRequestDto serviceRequest = GetServiceRequestDetailsByWorkOrder(provisionalBillId, "");
            invoice = new InvoiceDto();
            invoice.ErrorMessage = "";
            items = new List<InvoiceItemDto>();
            InvoiceItemDto item = new InvoiceItemDto();
            item.Name = "SERVICE CHARGE";
            item.InvoiceItemID = 1;
            item.Qty = serviceRequest.ServiceDays;
            item.Rate = serviceRequest.ServiceCharge;
            item.Amount = serviceRequest.ServiceDays * serviceRequest.ServiceCharge;
            //item.HSNCode = "998739";
            item.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item.UnitOfMeasurement = "DAY";
            item.SGSTAmount = 0;
            item.CGSTAmount = 0;
            item.IGSTAmount = 0;
            item.AmountBTax = item.Amount;
            item.IsService = "Y";
            item.IGSTRate = 18;
            item.IGSTAmount = (int)(item.Amount * (item.IGSTRate / 100));
            item.Total = (int)(item.Amount + item.IGSTAmount + item.SGSTAmount + item.CGSTAmount);
            items.Add(item);
            InvoiceItemDto item1 = new InvoiceItemDto();
            item1.Name = "To & FRO AIR FARE/RAIL FARE";
            item1.InvoiceItemID = 2;
            item1.Qty = 1;
            item1.Rate = serviceRequest.Fare;
            item1.Amount = serviceRequest.Fare;
            //item1.HSNCode = "998739";
            item1.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item1.IsService = "Y";
            item1.UnitOfMeasurement = "";
            item1.SGSTAmount = 0;
            item1.CGSTAmount = 0;
            item1.IGSTAmount = 0;
            item1.AmountBTax = item1.Amount;
            item1.IGSTRate = 18;
            item1.IGSTAmount = (int)(item1.Amount * (item1.IGSTRate / 100));
            item1.Total = (int)(item1.Amount + item1.IGSTAmount + item1.SGSTAmount + item1.CGSTAmount);
            items.Add(item1);
            invoice.InvoiceItems = items;
            InvoiceItemDto item2 = new InvoiceItemDto();
            item2.Name = "POCKET EXPENSE";
            item2.InvoiceItemID = 3;
            item2.Qty = serviceRequest.PocketExpensesDays;
            item2.Rate = serviceRequest.PocketExpenses;
            item2.Amount = serviceRequest.PocketExpensesDays * serviceRequest.PocketExpenses;
            item2.IsService = "Y";
            //item2.HSNCode = "998739";
            item2.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item2.UnitOfMeasurement = "DAY";
            item2.SGSTAmount = 0;
            item2.CGSTAmount = 0;
            item2.IGSTAmount = 0;
            item2.AmountBTax = item2.Amount;
            item2.IGSTRate = 18;
            item2.IGSTAmount = (int)(item2.Amount * (item2.IGSTRate / 100));
            item2.Total = (int)(item2.Amount + item2.IGSTAmount + item2.SGSTAmount + item2.CGSTAmount);
            items.Add(item2);
            invoice.InvoiceItems = items;
            InvoiceItemDto item3 = new InvoiceItemDto();
            item3.Name = "LODGING & BOARDING";
            item3.InvoiceItemID = 4;
            item3.Qty = (int)serviceRequest.BoardingDays;
            item3.Rate = serviceRequest.BoardingCharges;
            item3.Amount = serviceRequest.BoardingDays * serviceRequest.BoardingCharges;
            //item3.HSNCode = "998739";
            item3.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item3.IsService = "Y";
            item3.UnitOfMeasurement = "DAY";
            item3.SGSTAmount = 0;
            item3.CGSTAmount = 0;
            item3.IGSTAmount = 0;
            item3.AmountBTax = item3.Amount;
            item3.IGSTRate = 18;
            item3.IGSTAmount = (int)(item3.Amount * (item3.IGSTRate / 100));
            item3.Total = (int)(item3.Amount + item3.IGSTAmount + item3.SGSTAmount + item3.CGSTAmount);
            items.Add(item3);
            InvoiceItemDto item4 = new InvoiceItemDto();
            item4.Name = "CONVEYANCE & INCIDENTAL EXPENSES";
            item4.InvoiceItemID = 5;
            item4.Qty = 1;
            item4.Rate = serviceRequest.ConveyanceExpenses;
            item4.Amount = serviceRequest.ConveyanceExpenses;
            //item4.HSNCode = "998739";
            item4.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item4.IsService = "Y";
            item4.UnitOfMeasurement = "";
            item4.SGSTAmount = 0;
            item4.CGSTAmount = 0;
            item4.IGSTAmount = 0;
            item4.AmountBTax = item4.Amount;
            item4.IGSTRate = 18;
            item4.IGSTAmount = (int)(item4.Amount * (item4.IGSTRate / 100));
            item4.Total = (int)(item4.Amount + item4.IGSTAmount + item4.SGSTAmount + item4.CGSTAmount);
            items.Add(item4);
            InvoiceItemDto item5 = new InvoiceItemDto();
            item5.Name = "OVER TIME HOUR";
            item5.InvoiceItemID = 6;
            item5.Qty = serviceRequest.OvertimeHours;
            item5.Rate = serviceRequest.OvertimeCharges;
            item5.Amount = serviceRequest.OvertimeHours * serviceRequest.OvertimeCharges;
            item5.HSNCode = ConfigurationManager.AppSettings["ServiceHSN"];
            item5.IsService = "Y";
            item5.UnitOfMeasurement = "HOUR";
            item5.SGSTAmount = 0;
            item5.CGSTAmount = 0;
            item5.IGSTAmount = 0;
            item5.AmountBTax = item5.Amount;
            item5.IGSTRate = 18;
            item5.IGSTAmount = (int)(item5.Amount * (item5.IGSTRate / 100));
            item5.Total = (int)(item5.Amount + item5.IGSTAmount + item5.SGSTAmount + item5.CGSTAmount);
            items.Add(item5);
            invoice.InvoiceItems = items;
            foreach (var a in items)
            {
                invoice.Amount += a.AmountBTax;
                invoice.SGSTTax += a.SGSTAmount;
                invoice.CGSTTax += a.CGSTAmount;
                invoice.IGSTTax += a.IGSTAmount;
                invoice.TotalTax += (a.SGSTAmount + a.CGSTAmount + a.IGSTAmount);
            }
            invoice.AmountBTax = invoice.Amount;
            invoice.Amount = invoice.AmountBTax + invoice.TotalTax;
            invoice.AmountWords = utils.ConvertNumbertoWords((long)invoice.Amount) + " ONLY";

            /*Newly Added to get invoice for SubCustomer 
             * Date:22 March 2021
             * if Customer id = 0 
             */
            invoice.SubCustomerId = serviceRequest.SubCustomerId;
            PicannolEntities context = new PicannolEntities();
            if (invoice.SubCustomerId != 0)
            {
                var a = context.tblSubCustomers.Where(x => x.SubCustomerId == invoice.SubCustomerId).FirstOrDefault();
                customer.SubCustomerId = a.SubCustomerId;
                customer.SubCustomerName = a.SubCustomerName;
            }
            var order = (Guid)context.tblWorkOrders.Where(x => x.WorkOrderId == workOrderId).Select(x => x.WorkOrderGUID).FirstOrDefault();
            invoice.InvoiceNo = GetCancelledInvoiceNo(order, invoice.Amount, customer, provisionalBillId, GeneratedById, actionInitiator);
            var invDeatils = context.tblInvoices.Where(x => x.OrderGuid == order && x.DelInd == false
            && x.ProvisionalBillId == provisionalBillId && x.Cancelled == true).FirstOrDefault();
            if (invDeatils != null)
            {
                invoice.Cancelled = true;
                invoice.InvoiceDate = invDeatils.InvoiceDate;
            }

            invoice.ProvisionalBillNo = serviceRequest.ProvisionalBillNo;
            invoice.AttendedFrom = serviceRequest.FromDate.ToShortDateString();
            invoice.AttendedTo = serviceRequest.ToDate.ToShortDateString();
            invoice.EngineerName = serviceRequest.UserName;

            return invoice;
        }
        #endregion

        public string SaveNewInvoice(Guid order, decimal amount, CustomerDto customer, int? ProvisionalBillID, int GeneratedById = 0, string actionInitiator = "")
        {
            string newInvNo = "";
            PicannolEntities context = new PicannolEntities();
            var invDeatils = context.tblInvoices.Where(x => x.OrderGuid == order && x.DelInd == false && x.ProvisionalBillId == ProvisionalBillID && x.Cancelled == false).FirstOrDefault();
            if (invDeatils != null)
            {
                newInvNo = invDeatils.InvoiceNo;
                var eInVoice = context.tblEInvoices.Where(x => x.InvoiceNo == newInvNo && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                if (eInVoice != null)
                {
                    invoice.EInVoiceQrCode = eInVoice.QRCode;
                    invoice.IRN = eInVoice.IRN;

                    //record user activity
                    string ActionName = $" Download E-Inv, Inv No - {eInVoice.InvoiceNo}"
                    + $", OrderGUID - {eInVoice.OrderID}"
                    + $", PSBID - {eInVoice.ProvisionalBillId}";
                    string TableName = "tblEInvoice";
                    if (ActionName != null)
                    {
                        new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                    //End
                }

            }
            else if (invDeatils == null && amount == 0)
            {
                string lastInvoiceNo = "";
                string invType = "";
                invType = ConstantsHelper.InvoiceType.AC1FOC.ToString();
                lastInvoiceNo = InvoiceService.GetLastAC1FOC(invType);

                if (lastInvoiceNo != null && lastInvoiceNo != "")
                {
                    char[] splitChar = { '/' };
                    string[] strArray = lastInvoiceNo.Split(splitChar);
                    int month = DateTime.Now.Month;
                    var year = DateTime.Now.ToString("yy");
                    int yy = Convert.ToInt32(year);
                    var currYear = year + Convert.ToString(yy + 1);
                    var prevYear = Convert.ToString(yy - 1) + year;
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
                else
                {
                    newInvNo = GetFirstInvoiceNo(invType);
                }

                tblInvoice ti = new tblInvoice();
                ti.InvoiceNo = newInvNo;
                ti.DateCreated = DateTime.Now;
                ti.Amount = amount;
                ti.CustomerId = customer.CustomerId;
                ti.OrderGuid = order;
                ti.InvoiceDate = DateTime.Now.Date;
                ti.ProvisionalBillId = ProvisionalBillID;
                ti.Status = 1;
                ti.Cancelled = false;
                ti.DelInd = false;
                ti.GeneratedByUserId = GeneratedById;
                /*
                 * Date:23 March 2021
                 * Author:Prince Dhiman
                 * Comment: Adding Sub Customer Detail Condition 
                            Earlier using Customer Name 
                 */
                ti.SubCustomerId = customer.SubCustomerId;
                if (customer.SubCustomerId > 0)
                {
                    ti.InvoiceFileName = customer.SubCustomerName + "_" + invType + ".pdf";
                }
                else
                {
                    ti.InvoiceFileName = customer.CustomerName + "_" + invType + ".pdf";
                }

                InvoiceService.AddInvoice(ti);
            }
            else
            {
                string lastInvoiceNo = "";
                string invType = "";

                //invType = ConstantsHelper.InvoiceType.AC1.ToString();
                //***Addede by prince now using AC type dated 6 may 2021 ****

                invType = ConstantsHelper.InvoiceType.AC.ToString();
                lastInvoiceNo = InvoiceService.GetLastInvoiceNumber(invType);
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
                else
                {
                    newInvNo = GetFirstInvoiceNo(invType);
                }
                tblInvoice ti = new tblInvoice();
                ti.InvoiceNo = newInvNo;
                ti.DateCreated = DateTime.Now;
                ti.Amount = amount;
                ti.CustomerId = customer.CustomerId;
                ti.OrderGuid = order;
                ti.InvoiceDate = DateTime.Now.Date;
                ti.ProvisionalBillId = ProvisionalBillID;
                ti.Status = 1;
                ti.Cancelled = false;
                ti.DelInd = false;
                ti.GeneratedByUserId = GeneratedById;
                /*
                 * Date:23 March 2021
                 * Author:Prince Dhiman
                 * Comment: Adding Sub Customer Detail Condition 
                            Earlier using Customer Name 
                 */
                ti.SubCustomerId = customer.SubCustomerId;
                if (customer.SubCustomerId > 0)
                {
                    ti.InvoiceFileName = customer.SubCustomerName + "_" + invType + ".pdf";
                }
                else
                {
                    ti.InvoiceFileName = customer.CustomerName + "_" + invType + ".pdf";
                }

                invoice.InvoiceNo = newInvNo;
                invoice.OrderGuid = order;
                orders.OrderGUID = order;
                invoice.eInvType = "Service";
                invoice.ProvisionalBillId = (int)ProvisionalBillID;
                respPlGenIRN = _eInvoiceHelper.GenerateIRN(customer, invoice, items, orders);
                if (respPlGenIRN.Irn != null)
                {
                    if (ProvisionalBillID != null)
                    {
                        var ProvisionalBillDetails = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillID).FirstOrDefault();
                        if (ProvisionalBillDetails != null)
                        {
                            ti.BillingAddress = ProvisionalBillDetails.BillingAddress;
                            ti.StateCode = ProvisionalBillDetails.StateCode;
                            ti.ShippingAddress = ProvisionalBillDetails.ShippingAddress;
                            ti.ShippingStateCode = ProvisionalBillDetails.ShippingStateCode;
                        }
                   
                    invoice.EInVoiceQrCode = respPlGenIRN.EInvoiceQrCodeImg;
                        invoice.IRN = respPlGenIRN.Irn;
                        InvoiceService.AddInvoice(ti);
                    }
                     

                    //record user activity
                    string ActionName = $"E-Inv created, Inv No - {newInvNo}"
                    + $", OrderGUID - {orders.OrderGUID}"
                    + $", PSBID - {orders.ProvisionalBillId}";
                    string TableName = "tblEInvoice, tblInvoices";
                    if (ActionName != null)
                    {
                        new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                    //End
                }
                else
                {
                    //return invoice.ErrorMessage = respPlGenIRN.ErrorDetails[0].ErrorMessage.ToString();
                    if (respPlGenIRN.ErrorDetails != null)
                    {
                        return invoice.ErrorMessage = respPlGenIRN.ErrorDetails[0].ErrorMessage.ToString(); ;
                    }
                    else
                    {
                        return invoice.ErrorMessage = respPlGenIRN.errorMsg; ;
                    }
                }

            }
            return newInvNo;

        }

        public string GetCancelledInvoiceNo(Guid order, decimal amount, CustomerDto customer, int? ProvisionalBillID, int GeneratedById = 0, string actionInitiator = "")
        {
            string newInvNo = "";
            PicannolEntities context = new PicannolEntities();
            var invDeatils = context.tblInvoices.Where(x => x.OrderGuid == order
            && x.DelInd == false
            && x.ProvisionalBillId == ProvisionalBillID
            && x.Cancelled == true).FirstOrDefault();
            if (invDeatils != null)
            {
                newInvNo = invDeatils.InvoiceNo;
                var eInVoice = context.tblEInvoices.Where(x => x.InvoiceNo == newInvNo && x.DelInd == false && x.Cancelled == true).FirstOrDefault();
                if (eInVoice != null)
                {
                    invoice.Cancelled = true;
                    invoice.EInVoiceQrCode = eInVoice.QRCode;
                    invoice.IRN = eInVoice.IRN;
                }
            }
            else
            {
                invoice.ErrorMessage = "Invoice is not generated";
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
            /*return "0001/" + yy + "/" + invType;*/
            return "1/" + yy + "/" + invType;
        }
        public DateTime CheckPreviousDate(int Id)
        {


            PicannolEntities context = new PicannolEntities();
            if (context.tblProvisionalBills.Any(x => x.WorkOrderId == Id && x.DelInd == false))
            {
                ServiceRequestDto sc = new ServiceRequestDto();
                sc = (from a in context.tblProvisionalBills
                      where a.WorkOrderId == Id && a.DelInd == false
                      select new ServiceRequestDto
                      {
                          ToDate = a.ToDate,
                      }).OrderByDescending(x => x.ToDate).FirstOrDefault();
                return sc.ToDate;
            }

            else
            {
                return DateTime.Now.AddDays(-1);
            }
        }
        public string GetWorkOrderByWorkOrderId(int WorkOrderId)
        {
            PicannolEntities _context = new PicannolEntities();
            var response = _context.tblWorkOrders.Where(x => x.WorkOrderId == WorkOrderId).Select(x => x.WorkOrderNo).FirstOrDefault();
            return response;
        }
        public ServiceRequestDto GetProvisionalBillDetails(ServiceRequestDto serviceRequest)
        {
            try
            {
                PicanolUtils utils = new PicanolUtils();
                ServiceRequestDto sr = new ServiceRequestDto();
                sr = serviceRequest;

                sr.ServiceChargeAmount = (int)(sr.ServiceDays * sr.ServiceCharge);
                sr.BoardingAmount = (int)(sr.BoardingDays * sr.BoardingCharges);
                sr.PocketExpensesAmount = (int)(sr.PocketExpensesDays * sr.PocketExpenses);
                sr.OvertimeAmount = (int)(sr.OvertimeHours * sr.OvertimeCharges);
                sr.Date = sr.DateCreated.ToString();
                var v = Convert.ToDateTime(sr.Date);
                sr.Date = v.ToString("dd/MM/yyyy");
                sr.FDate = sr.FromDate.ToShortDateString();
                sr.TDate = sr.ToDate.ToShortDateString();
                sr.AmountBeforeTax = (int)(sr.ServiceChargeAmount + sr.BoardingAmount +
                                                    sr.PocketExpensesAmount + sr.ConveyanceExpenses +
                                                    sr.Fare + sr.OvertimeAmount);
                sr.TaxAmount = sr.AmountBeforeTax * sr.GST / 100;
                int amount = sr.AmountBeforeTax + sr.TaxAmount;
                sr.FinalAmount = amount;
                string s = sr.FinalAmount.ToString("0.00", CultureInfo.InvariantCulture);
                string[] parts = s.Split('.');
                int i1 = int.Parse(parts[0]);
                int i2 = int.Parse(parts[1]);
                sr.AmountInWords = "Rupees " + utils.ConvertNumbertoWords(sr.FinalAmount) + " only";
                sr.Imagepath = sr.ProvisionalBillId + ".png";
                sr.UImagepath = sr.ProvisionalBillId + "_" + sr.UserId + ".png";
                return sr;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetProvisionalBillDetail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }


        }


        public WorkOrderDto GetWorkOrderDetailsByTimeSheet(int timeSheetId)
        {
            try
            {
                PicannolEntities _context = new PicannolEntities();
                var wo = (from a in _context.tblTimeSheets
                          join b in _context.tblWorkOrders on a.WorkOrderId equals b.WorkOrderId
                          join c in _context.tblCustomers on b.CustomerId equals c.CustomerId
                          where a.TimeSheetId == timeSheetId && a.DelInd == false
                          select new WorkOrderDto
                          {
                              WorkOrderId = b.WorkOrderId,
                              WorkOrderNo = b.WorkOrderNo,
                              Customer = new CustomerDto
                              {
                                  AddressLine1 = c.AddressLine1,
                                  AddressLine2 = c.AddressLine2,
                                  District = c.District,
                                  City = c.City,
                                  State = c.State,
                                  PIN = c.PIN,
                              }

                          }).FirstOrDefault();
                string ab = wo.Customer.AddressLine1 + ',' + wo.Customer.AddressLine2 + ',';
                ab += wo.Customer.District == null ? "" : (wo.Customer.District + ",");
                ab += wo.Customer.City == null ? "" : (wo.Customer.City + ",");
                ab += wo.Customer.State == null ? "" : wo.Customer.State;
                ab += wo.Customer.PIN == null ? "" : ("-" + wo.Customer.PIN);
                wo.CustomerAddress = ab;
                return wo;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetWorkOrderDetailsByTimeSheet!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        #endregion

        public string SaveEmailRecord(ServiceRequestDto dto)
        {
            PicannolEntities _context = new PicannolEntities();
            var orderGuid = _context.tblWorkOrders.Where(x => x.WorkOrderId == dto.WorkOrderId && x.DelInd == false).Select(x => x.WorkOrderGUID).FirstOrDefault();

            if (_context.tblInvoices.Any(x => x.OrderGuid == orderGuid && x.DelInd == false))
            {
                var invoice = _context.tblInvoices.Where(x => x.OrderGuid == orderGuid && x.ProvisionalBillId == dto.ProvisionalBillId).FirstOrDefault();
                if (invoice == null)
                {
                    return "Invoive Not Generated";
                }
                else
                {
                    invoice.SendOnDate = dto.SendON;
                    invoice.SendTo = dto.CustomerName;
                    _context.Entry(invoice).State = EntityState.Modified;
                    _context.SaveChanges();
                    return "Success";
                }
            }
            else
            {
                return "Invoice Not Generated";
            }

        }

        //new helper for cancel  Provisional Invoice
        public string CancelProvisionalInvoice(string InvoiceNumber, int userId)
        {

            PicannolEntities _context = new PicannolEntities();
            RespPlCancelIRN respPlCancelIRN = new RespPlCancelIRN();
            if (InvoiceNumber != null)
            {
                var Invoice = _context.tblInvoices.Where(x => x.InvoiceNo == InvoiceNumber).FirstOrDefault();
                if (Invoice == null)
                {
                    return "Invoice Not Generated";
                }
                else
                {
                    var eInv = _context.tblEInvoices.Where(x => x.InvoiceNo == InvoiceNumber && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    if (eInv != null)
                    {
                        respPlCancelIRN = _eInvoiceHelper.CancelEInvoice(eInv);
                        if (respPlCancelIRN.CancelDate != null || respPlCancelIRN.Irn != null)
                        {
                            Invoice.Cancelled = true;
                            Invoice.CancelledBy = userId;
                            Invoice.CancelledDate = DateTime.Now;
                            _context.Entry(Invoice).State = EntityState.Modified;
                            _context.SaveChanges();
                            return "Success";
                        }
                        else
                        {
                            //return respPlCancelIRN.ErrorDetails[0].ErrorMessage.ToString();
                            if (respPlCancelIRN.ErrorDetails != null)
                            {
                                return respPlCancelIRN.ErrorDetails[0].ErrorMessage.ToString();
                            }
                            else
                            {
                                return respPlCancelIRN.errorMsg.ToString();
                            }

                        }

                    }
                    else
                    {
                        return "Invalid Invoice";
                    }

                }
            }

            return "Invalid Invoice";
        }
        //end here



        public string CancelCreditNoteDtl(string InvoiceNumber, int userId)
        {

            PicannolEntities _context = new PicannolEntities();
            RespPlCancelIRN respPlCancelIRN = new RespPlCancelIRN();
            if (InvoiceNumber != null)
            {
                var creditNote = _context.tblCreditNotes.Where(x => x.CreditNoteNo == InvoiceNumber).FirstOrDefault();
                if (creditNote == null)
                {
                    return "Invoice Not Generated";
                }
                else
                {
                    var eInv = _context.tblEInvoices.Where(x => x.InvoiceNo == InvoiceNumber && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    if (eInv != null)
                    {
                        respPlCancelIRN = _eInvoiceHelper.CancelEInvoice(eInv);
                        if (respPlCancelIRN.CancelDate != null || respPlCancelIRN.Irn != null)
                        {
                            creditNote.Cancelled = true;
                            creditNote.CancelledBy = userId;
                            creditNote.CancelledDate = DateTime.Now;
                            _context.Entry(creditNote).State = EntityState.Modified;
                            _context.SaveChanges();
                            return "Success";
                        }
                        else
                        {
                            //return respPlCancelIRN.ErrorDetails[0].ErrorMessage.ToString();
                            if (respPlCancelIRN.ErrorDetails != null)
                            {
                                return respPlCancelIRN.ErrorDetails[0].ErrorMessage.ToString();
                            }
                            else
                            {
                                return respPlCancelIRN.errorMsg.ToString();
                            }

                        }

                    }
                    else
                    {
                        return "Invalid Invoice";
                    }

                }
            }

            return "Invalid Invoice";
        }

        public string CancelProvisonalBill(int id)
        {
            PicannolEntities _context = new PicannolEntities();
            var pbid = _context.tblProvisionalBills.Where(x => x.ProvisionalBillId == id && x.DelInd == false).FirstOrDefault();
            if (pbid != null)
            {
                pbid.Cancelled = true;
                _context.Entry(pbid).State = EntityState.Modified;
                _context.SaveChanges();

            }
            return "Success";
        }


        public string PaidProvisonalBill(int id)
        {
            PicannolEntities _context = new PicannolEntities();
            var pbid = _context.tblProvisionalBills.Where(x => x.ProvisionalBillId == id && x.DelInd == false).FirstOrDefault();
            if (pbid != null && pbid.Paid == true)
            {
                pbid.Paid = false;
                _context.Entry(pbid).State = EntityState.Modified;
                _context.SaveChanges();

            }
            else
            {
                pbid.Paid = true;
                _context.Entry(pbid).State = EntityState.Modified;
                _context.SaveChanges();

            }
            return "Success";
        }

        public List<UserDto> GetUsernameList()
        {
            PicannolEntities _context = new PicannolEntities();
            var a = (from s in _context.tblUsers
                     where s.DelInd == false && s.RoleId == 2
                     select new UserDto
                     {
                         UserName = s.UserName,
                         UserId = s.UserId
                     }).ToList();
            return a;
        }
    }
}