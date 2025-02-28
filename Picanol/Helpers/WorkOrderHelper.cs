using NLog;
using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.Utils;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Objects.SqlClient;


//using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class WorkOrderHelper
    {
        
        PicannolEntities entities = new PicannolEntities();
        private WorkOrderService _workOrderService;
        //private readonly UserHelper _userHelper;
        protected WorkOrderService WorkOrderService
        {
            get
            {
                if (_workOrderService == null) _workOrderService = new WorkOrderService(entities, validationDictionary);
                return _workOrderService;
            }
        }
        protected iValidation validationDictionary { get; set; }



        #region Ctor
        public WorkOrderHelper(Controller controller)
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
        public WorkOrderDto SaveNewWorkOrder(WorkOrderDto wo)
        {
            tblWorkOrder two = new tblWorkOrder();

            two.WorkOrderNo = wo.WorkOrder;
            two.AssignedTo = wo.AssignedTo;
            two.ContactPerson = wo.ContactPerson;
            two.ContractNumber = wo.ContractNumber;
            two.Conditions = wo.Conditions;
            two.CreatedBy = wo.CreatedBy;
            two.CustomerId = wo.CustomerId;
            two.DateCreated = DateTime.Now;
            two.DelInd = false;
            two.Description = wo.Description;
            two.EmailId = wo.EmailId;

            var d = wo.SDate.Replace("-", "/");
            var c = wo.EDate.Replace("-", "/");
            two.StartDate = DateTime.ParseExact(d, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            two.EndDate = DateTime.ParseExact(c, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            /*two.StartDate = DateTime.ParseExact(wo.SDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            two.EndDate = DateTime.ParseExact(wo.EDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);*/
            
            two.Mobile = wo.Mobile;
            two.Phone = wo.Phone;
            two.CallType = wo.CallType;
            two.WorkOrderType = wo.WorkOrderType;
            two.WorkOrderGUID = Guid.NewGuid();
            int i = 0;
            try
            {
                i = WorkOrderService.SaveWorkOrder(two);
            }
            catch (Exception ex)
            {

                throw;
            }

            wo.WorkOrderId = i;
            return wo;
        }

        //public List<WorkOrderDto> GetWorkOrdersList(int? UserId, int? CustomerId, int? type, int? RoleId)
        public List<WorkOrderDto> GetWorkOrdersList(WorkOrderViewModel wvm)
        {
            try
            {
                List<WorkOrderDto> woList = WorkOrderService.GetWorkOrdersList();
               
                if (wvm.CustomerId != null & wvm.CustomerId != 0)
                {
                    woList = woList.Where(x => x.CustomerId == wvm.CustomerId).ToList();
                }
                if (wvm.RoleId != 5 && wvm.RoleId != 3 && wvm.RoleId != 6)
                {
                    woList = woList.Where(x => x.AssignedTo == wvm.UserId).ToList();
                }
                if (wvm.AssignedTo != 0)
                {
                    woList = woList.Where(x => x.AssignedTo == wvm.AssignedTo).ToList();
                }
                if (wvm.StartDate != null && wvm.StartDate != DateTime.MinValue && wvm.EndDate != null && wvm.EndDate != DateTime.MinValue)
                {
                    woList = woList.Where(x => x.StartDate >= wvm.StartDate && x.EndDate <= wvm.EndDate).ToList();
                }
                return woList;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetWorkOrderList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }


        //new 170721
        public List<WorkOrderDto> GetWorkOrdersListVersion2(WorkOrderViewModel wvm)
        {
            try
            {
                // List<WorkOrderDto> woList = WorkOrderService.GetWorkOrdersList();
                List<WorkOrderDto> woList = WorkOrderService.GetWorkOrdersListVersion2(wvm);

                if (wvm.CustomerId != null & wvm.CustomerId != 0)
                {
                    woList = woList.Where(x => x.CustomerId == wvm.CustomerId).ToList();
                }
                if (wvm.RoleId != 5 && wvm.RoleId != 3 && wvm.RoleId != 6)
                {
                    woList = woList.Where(x => x.AssignedTo == wvm.UserId).ToList();
                }
                if (wvm.AssignedTo != 0)
                {
                    woList = woList.Where(x => x.AssignedTo == wvm.AssignedTo).ToList();
                }
                //if (wvm.StartDate != null && wvm.StartDate != DateTime.MinValue && wvm.EndDate != null && wvm.EndDate != DateTime.MinValue)
                //{
                //    woList = woList.Where(x => x.StartDate >= wvm.StartDate && x.EndDate <= wvm.EndDate).ToList();
                //}
                return woList;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetWorkOrderList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        //end here
        //new method for pagination of workorder//


        public List<WorkOrderDto> GetWorkOrdersListVersion1(WorkOrderViewModel wvm,int PageSize,int PageNo)
        {
            try
            {
                wvm.PageNo = PageNo;
                wvm.PageSize = PageSize;
                List<WorkOrderDto> woList = WorkOrderService.GetWorkOrdersListVersion2(wvm);

              //  List<WorkOrderDto> woList = WorkOrderService.GetWorkOrdersListVersion1(PageSize, PageNo);

                if (wvm.CustomerId != null & wvm.CustomerId != 0)
                {
                    woList = woList.Where(x => x.CustomerId == wvm.CustomerId).ToList();
                }
                if (wvm.RoleId != 5 && wvm.RoleId != 3 && wvm.RoleId != 6)
                {
                    woList = woList.Where(x => x.AssignedTo == wvm.UserId).ToList();
                }
                if (wvm.AssignedTo != 0)
                {
                    woList = woList.Where(x => x.AssignedTo == wvm.AssignedTo).ToList();
                }
                    if (wvm.StartDate != null && wvm.StartDate != DateTime.MinValue && wvm.EndDate != null && wvm.EndDate != DateTime.MinValue)
                    {
                        woList = woList.Where(x => x.StartDate >= wvm.StartDate && x.EndDate <= wvm.EndDate).ToList();
                    }
                    return woList;

                
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetWorkOrderList!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        //End Here//

        public string GetWorkOrderNumber()
        {
            string lastWONo = WorkOrderService.GetLastWorkOrderNumber();
            string newWONo = "";
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            string email = UserInfo.Email;
            String[] parts = email.Split(new[] { '@' });
            string initials = parts[0].ToUpper();
            string month = DateTime.Now.ToString("MM");
            string year = DateTime.Now.ToString("yy");
            if (lastWONo != null && lastWONo != "")
            {
                char[] splitChar = { '/' };
                string[] strArray = lastWONo.Split(splitChar);
                int i = Convert.ToInt32(strArray[0]);
                i = i + 1;
                newWONo = Convert.ToString(i).PadLeft(4, '0') + "/" + month + year + "/" + strArray[2] + "/" + initials;
            }
            else
            {

                newWONo = "0001/" + month + year + "/" + "WO" + "/" + initials;
            }
            return newWONo;
        }

        public string SendWOEmailCustomer(WorkOrderDto WO)
        {
            string response = "";
            WorkOrderDto wo = WorkOrderService.GetWorkOrderDetails(WO.WorkOrderId);
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Dear " + wo.ContactPerson + ",");
                stringBuilder.Append("<br />Kindly find below the details of work order :<br/>.");
                stringBuilder.Append("WorkOrder Number : <b>" + wo.WorkOrder + "</b> <br/>");
                stringBuilder.Append("Start Date : <b>" + wo.StartDate + "</b> <br/>");
                stringBuilder.Append("End Date : <b>" + wo.EndDate + "</b> <br/>");
                stringBuilder.Append("WorkOrder Type : <b>" + wo.WorkOrderType + "</b> <br/>");
                stringBuilder.Append("ContractNumber : <b>" + wo.ContractNumber + "</b> <br/>");
                stringBuilder.Append("Mission Description : <b>" + wo.Description + "</b> <br/>");
                stringBuilder.Append("Mission Condition : <b>" + wo.Conditions + "</b> <br/>");
                stringBuilder.Append("Engineer Name : <b>" + wo.AssignedUserName + "</b> <br/><br/>");
                stringBuilder.Append("In case of any queries, kindly contact <b>" + UserInfo.Email + "</b> <br/>");

                //GMailer.GmailUsername = "noreply.picanol@gmail.com";
                //GMailer.GmailPassword = "9999907947";
                GMailer mailer = new GMailer();
                mailer.ToEmail = WO.EmailId;

                mailer.Subject = "New Work Order - Picanol India";
                mailer.Body = stringBuilder.ToString();
                mailer.IsHtml = true;
                mailer.Send();
            }
            catch (Exception)
            {

                throw;
            }
            return response;
        }
        
        public void SendWorkOrderNotification(int UserId)
        {

        }

        public WorkOrderDto GetWorkOrderDetailsByWorkOrder(int? woID)
        {
            return WorkOrderService.GetWorkOrderDetails(woID);
        }
        #endregion

        #region TimeSheet Management
        public string SaveTimeSheet(TimeSheetDto tsh, List<TimeSheetDetailDto> tsd,int UserId = 0,string Email="")
        {
            string response = "";
            PicannolEntities context = new PicannolEntities();
            if (tsh.TimeSheetId != 0)
            {
                var UserInfo = new UserSession();
                var a = context.tblTimeSheets.Where(x => x.TimeSheetId == tsh.TimeSheetId).FirstOrDefault();
                a.CustomerId = tsh.CustomerId;
                if ((UserSession)HttpContext.Current.Session["UserInfo"] != null)
                {
                    UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];

                }
                else
                {
                    UserInfo.UserId = UserId;
                    UserInfo.Email = Email;
                    HttpContext.Current.Session["UserInfo"] = UserInfo;
                }
                a.TimeSheetNo = GetTimeSheetNumber();
                a.UserId = UserInfo.UserId;
                a.WorkOrderId = tsh.WorkOrderId;
                a.Remarks = tsh.Remarks;
                a.DateCreated = DateTime.Now;
                a.DelInd = false;
                a.FromDate = tsh.FromDate;
                a.ToDate = tsh.ToDate;
                //ts.AuthorizedOn = tsh.AuthorizedOn;
                //ts.AuthorizedBy = tsh.AuthorizedBy;
                a.Authorized = false;
                a.ProvisionalBillId = tsh.ProvisionalBillId;
                if (tsh.ButtonValue == 1)
                {
                    a.FinalSubmit = false;
                }
                else
                {
                    a.FinalSubmit = true;
                }
                context.Entry(a).State = EntityState.Modified;
                context.SaveChanges();

                context.tblTimeSheetDetails.Where(b => b.TimeSheetId == tsh.TimeSheetId)
                  .ToList().ForEach(b => context.tblTimeSheetDetails.Remove(b));
                foreach (var item in tsd)
                {
                    tblTimeSheetDetail tshd = new tblTimeSheetDetail();
                    tshd.TimeSheetId = tsh.TimeSheetId;
                    tshd.WorkDate = item.WorkDate;
                    tshd.TotalHours = item.TotalHours;
                    tshd.Description = item.Description;
                    tshd.WeekNo = Convert.ToInt32(item.WeekNo);
                    tshd.DelInd = false;
                    context.tblTimeSheetDetails.Add(tshd);
                    context.SaveChanges();
                }
                response = "success";
            }
            else
            {

                if (!context.tblTimeSheets.Any(x => x.ProvisionalBillId == tsh.ProvisionalBillId && x.DelInd == false))
                {
                    tblTimeSheet ts = new tblTimeSheet();
                    ts.CustomerId = tsh.CustomerId;
                    var UserInfo = new UserSession();
                    if ((UserSession)HttpContext.Current.Session["UserInfo"] != null)
                    {
                        UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
                    }
                    else
                    {
                        UserInfo.UserId = UserId;
                        UserInfo.Email = Email;
                        HttpContext.Current.Session["UserInfo"] = UserInfo;
                    }
                    ts.TimeSheetNo = GetTimeSheetNumber();
                    ts.UserId = UserInfo.UserId;
                    ts.WorkOrderId = tsh.WorkOrderId;
                    ts.Remarks = tsh.Remarks;
                    ts.DateCreated = DateTime.Now;
                    ts.DelInd = false;
                    ts.FromDate = tsh.FromDate;
                    ts.ToDate = tsh.ToDate;
                    //ts.AuthorizedOn = tsh.AuthorizedOn;
                    //ts.AuthorizedBy = tsh.AuthorizedBy;
                    ts.Authorized = false;
                    ts.ProvisionalBillId = tsh.ProvisionalBillId;
                    //ts.FinalSubmit = false;
                    //Change on 06 feb 2021 for Mobile App
                    if (tsh.ButtonValue == 1)
                    {
                        ts.FinalSubmit = false;
                    }
                    else
                    {
                        ts.FinalSubmit = true;
                    }
                    try
                    {
                        context.tblTimeSheets.Add(ts);
                        context.SaveChanges();
                        int tsId = ts.TimeSheetId;
                        foreach (var item in tsd)
                        {
                            tblTimeSheetDetail tshd = new tblTimeSheetDetail();
                            tshd.TimeSheetId = tsId;
                            tshd.WorkDate = item.WorkDate;
                            tshd.TotalHours = item.TotalHours;
                            tshd.Description = item.Description;
                            tshd.WeekNo = Convert.ToInt32(item.WeekNo);
                            tshd.DelInd = false;
                            context.tblTimeSheetDetails.Add(tshd);
                            context.SaveChanges();
                        }

                        response = "success";

                       

                    }

                    catch (Exception ex)
                    {

                        response = ex.Message;
                    }
                }
            }

            return response;
        }

        public TimeSheetDto GetTimeSheetNumber(int id)
        {
            PicannolEntities context = new PicannolEntities();
            var ts = (from a in context.tblTimeSheets
                      join b in context.tblCustomers on a.CustomerId equals b.CustomerId
                      join c in context.tblWorkOrders on a.WorkOrderId equals c.WorkOrderId
                      join d in context.tblUsers on a.UserId equals d.UserId
                      where a.WorkOrderId == id && a.DelInd == false
                      select new TimeSheetDto
                      {
                          TimeSheetId = a.TimeSheetId,
                          TimeSheetNo = a.TimeSheetNo,
                          CustomerName = b.CustomerName,
                          WorkOrderNo = c.WorkOrderNo,
                          UserName = d.UserName,
                          AuthorizedBy = a.AuthorizedBy,
                          Remarks = a.Remarks,
                      }).FirstOrDefault();
            return ts;
        }

        public List<TimeSheetDetailDto> GetTimeSheetDetails(int id)
        {
            PicannolEntities context = new PicannolEntities();
            var tsd = (from a in context.tblTimeSheetDetails
                       where a.TimeSheetId == id && a.DelInd == false
                       select new TimeSheetDetailDto
                       {
                           WorkDate = a.WorkDate,
                           TotalHours = a.TotalHours,
                           Description = a.Description,

                       }).ToList();
            return tsd;
        }
        public string GetTimeSheetNumber()
        {
            string lastTSNo = GetLastTimeSheetNumber();
            string newTSNo = "";
            var UserInfo = new UserSession();
            if ((UserSession)HttpContext.Current.Session["UserInfo"] != null)
            {
                UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            }
            string email = UserInfo.Email;
            String[] parts = email.Split(new[] { '@' });
            string initials = parts[0].ToUpper();
            string month = DateTime.Now.ToString("MM");
            string year = DateTime.Now.ToString("yy");
            if (lastTSNo != null && lastTSNo != "")
            {
                char[] splitChar = { '/' };
                string[] strArray = lastTSNo.Split(splitChar);
                int i = Convert.ToInt32(strArray[0]);
                i = i + 1;
                newTSNo = Convert.ToString(i).PadLeft(4, '0') + "/" + month + year + "/" + strArray[2] + "/" + initials;
            }
            else
            {
                newTSNo = "0001/" + month + year + "/" + "TS" + "/" + initials;
            }
            return newTSNo;
        }

        public string GetLastTimeSheetNumber()
        {
            PicannolEntities context = new PicannolEntities();
            string i = "";
            var inv = (from a in context.tblTimeSheets
                       where a.DelInd == false
                       select a).ToList();
            inv = inv.OrderByDescending(x => x.TimeSheetId).ToList();
            i = inv.Select(x => x.TimeSheetNo).FirstOrDefault();
            return i;
        }

        public WorkOrderViewModel GetTimeSheetDetailsByWorkOrder(int  ProvisionalBillId,int workOrderId)
        {
            PicannolEntities context = new PicannolEntities();
            WorkOrderViewModel vm = new WorkOrderViewModel();
            vm.ErrorMessage = "";

            try
            {
                if (context.tblTimeSheets.Any(x => x.ProvisionalBillId == ProvisionalBillId && x.DelInd==false))
            {
                vm.TimeSheetFilled = true;
                    vm.TimeSheet = (from a in context.tblTimeSheets
                                    join b in context.tblCustomers on a.CustomerId equals b.CustomerId
                                    join c in context.tblUsers on a.UserId equals c.UserId
                                    where a.WorkOrderId == workOrderId && a.ProvisionalBillId == ProvisionalBillId
                                    select new TimeSheetDto
                                    {
                                        TimeSheetId = a.TimeSheetId,
                                        TimeSheetNo = a.TimeSheetNo,
                                        CustomerName = b.CustomerName,
                                        UserId = a.UserId,
                                        DateCreated = a.DateCreated,
                                        FromDate = (DateTime)a.FromDate,
                                        ToDate = (DateTime)a.ToDate,
                                        Remarks = a.Remarks != null ? a.Remarks : "",
                                        UserName = c.UserName,
                                        Designation = a.Designation != null ? a.Designation : "",
                                        AuthorizerEmail = a.EmailId != null ? a.EmailId : "",
                                        //AuthorizedBy = a.AuthorizedBy,
                                        AuthorizedBy = a.AuthorizedBy != null ? a.AuthorizedBy : "",
                                        FinalSubmit = a.FinalSubmit != null ? (bool)a.FinalSubmit : false,
                                        ProvisionalBillId = (int)a.ProvisionalBillId,
                                        WorkOrderId = a.WorkOrderId,
                                        Authorized = a.Authorized,
                                        TillFromDate = (DateTime)a.FromDate,
                                        TillDate = (DateTime)a.ToDate
                                        //ImageId = a.ImageId,
                                        //UserSignature = a.UserSignatureId
                                    }).FirstOrDefault();

                    if (context.tblTimeSheetDetails.Any(x => x.TimeSheetId == vm.TimeSheet.TimeSheetId))
                {
                    var tsd = (from a in context.tblTimeSheets
                               join b in context.tblTimeSheetDetails on a.TimeSheetId equals b.TimeSheetId
                               where a.ProvisionalBillId == ProvisionalBillId && b.DelInd==false
                               select new TimeSheetDetailDto
                               {
                                   WeekNo = SqlFunctions.StringConvert((double)b.WeekNo),
                                   WorkDate = b.WorkDate!=null? b.WorkDate:"",
                                   TotalHours = b.TotalHours!=null? b.TotalHours:"",
                                   Description = b.Description!=null? b.Description:""
                               }).ToList();
                    foreach (var item in tsd)
                    {

                        List<TimeSheetDetailDto> tsList = new List<TimeSheetDetailDto>();
                        TimeSheetDetailDto tsdt = new TimeSheetDetailDto();
                        if (vm.WeekDates.ContainsKey(item.WeekNo))
                        {
                            tsList = vm.WeekDates[item.WeekNo];
                            tsdt.WorkDate = item.WorkDate;
                            tsdt.TotalHours = item.TotalHours;
                            tsdt.Description = item.Description;
                            tsdt.TimeSheetDetailId = item.TimeSheetDetailId;
                            tsList.Add(tsdt);
                        }
                        else
                        {
                            tsdt.WorkDate = item.WorkDate;
                            tsdt.TotalHours = item.TotalHours;
                            tsdt.TimeSheetId = item.TimeSheetId;
                            tsdt.Description = item.Description;
                            tsdt.TimeSheetDetailId = item.TimeSheetDetailId;
                            tsList.Add(tsdt);
                            vm.WeekDates.Add(item.WeekNo.ToString(), tsList);
                        }

                    }
                }
                //Added on 30-12-2019
                else
                {

                    PicanolUtils utils = new PicanolUtils();
                    int startWeek = utils.GetIso8601WeekOfYear(vm.TimeSheet.FromDate);
                    int endWeek = utils.GetIso8601WeekOfYear(vm.TimeSheet.ToDate);
                    int sDay = (int)vm.TimeSheet.FromDate.DayOfWeek;
                    DateTime startDate = (DateTime)vm.TimeSheet.FromDate;


                    int lastWeek = 0;
                    bool nextYear = false;
                    if (endWeek < startWeek)
                    {
                        nextYear = true;
                        lastWeek = startWeek + endWeek;
                    }
                    else
                        lastWeek = endWeek;
                    int currentWeek;
                    for (int s = startWeek; s <= lastWeek; s++)
                    {
                        currentWeek = s;
                        List<TimeSheetDetailDto> timeSheetDetailsList = new List<TimeSheetDetailDto>();
                        for (int i = sDay; i <= 7; i++)
                        {
                            if (startDate > vm.TimeSheet.ToDate)
                                break;
                            TimeSheetDetailDto tsd = new TimeSheetDetailDto();
                            tsd.WorkDate = startDate.ToShortDateString();
                            tsd.isWeekend = false;
                            if ((startDate.DayOfWeek == DayOfWeek.Saturday) || (startDate.DayOfWeek == DayOfWeek.Sunday))
                            {
                                tsd.isWeekend = true;
                            }
                            timeSheetDetailsList.Add(tsd);
                            startDate = startDate.AddDays(1);
                        }

                        if (vm.WeekDates.ContainsKey(s.ToString()) == true)
                        {
                            var itemsInDictionary = vm.WeekDates[s.ToString()];
                            itemsInDictionary.AddRange(timeSheetDetailsList);
                            //foreach (var timeSheet in timeSheetDetailsList)
                            //{
                            //    var itemsNotInDictionaryButInTimeSheet = itemsInDictionary.FirstOrDefault(x => x.WorkDate == timeSheet.WorkDate);
                            //    if (itemsNotInDictionaryButInTimeSheet == null)
                            //        itemsInDictionary.Add(timeSheet);
                            //}
                        }
                        else
                        {

                            if (nextYear && currentWeek > 52)
                            {
                                currentWeek = endWeek;
                            }
                            vm.WeekDates.Add(currentWeek.ToString(), timeSheetDetailsList);
                        }
                        //startDate = startDate.AddDays(1);
                        if (startDate > vm.TimeSheet.ToDate)
                            break;
                        sDay = 1;
                        endWeek = endWeek++;
                    }
                }

            }
            else
            {
                vm.TimeSheetFilled = false;

                if (context.tblProvisionalBills.Any(x => x.ProvisionalBillId == ProvisionalBillId))
                {

                    var sr = context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillId).FirstOrDefault();
                    var provisionalServiceDays = context.tblServiceDays.Where(x => x.ProvisionalBillId == ProvisionalBillId).Select(x=>new ServiceDaysDto {FromDate=x.FromDate,ToDate=x.ToDate }).ToList();
                    if(provisionalServiceDays.Count<=0)
                    {
                        provisionalServiceDays.Add(new ServiceDaysDto
                        {
                            FromDate = sr.FromDate,
                            ToDate =sr.ToDate,
                        });
                    }
                    vm.TimeSheet = new TimeSheetDto();
                    foreach (var item in provisionalServiceDays)
                    {
                        var lastitem = provisionalServiceDays[provisionalServiceDays.Count - 1];
                        vm.TimeSheet.TillDate = lastitem.ToDate;
                        var firstitem = provisionalServiceDays.FirstOrDefault();
                        vm.TimeSheet.TillFromDate = firstitem.FromDate;
                        vm.TimeSheet.FromDate = item.FromDate;
                        vm.TimeSheet.ToDate = item.ToDate;
                        vm.TimeSheet.WorkOrderId = workOrderId;
                        PicanolUtils utils = new PicanolUtils();
                        int startWeek = utils.GetIso8601WeekOfYear(item.FromDate);
                        int endWeek = utils.GetIso8601WeekOfYear(item.ToDate);
                        int sDay = (int)item.FromDate.DayOfWeek;
                        DateTime startDate = (DateTime)item.FromDate;
                        int lastWeek = 0;
                        bool nextYear = false;
                        if (endWeek < startWeek)
                        {
                            nextYear = true;
                            lastWeek = startWeek + endWeek;
                        }
                        else
                            lastWeek = endWeek;
                        int currentWeek;
                        for (int s = startWeek; s <= lastWeek; s++)
                        {
                            currentWeek = s;
                            List<TimeSheetDetailDto> timeSheetDetailsList = new List<TimeSheetDetailDto>();
                            for (int i = sDay; i <= 7; i++)
                            {
                                if (startDate > vm.TimeSheet.ToDate)
                                    break;
                                TimeSheetDetailDto tsd = new TimeSheetDetailDto();
                                tsd.WorkDate = startDate.ToShortDateString();
                                tsd.isWeekend = false;
                                if ((startDate.DayOfWeek == DayOfWeek.Saturday) || (startDate.DayOfWeek == DayOfWeek.Sunday))
                                {
                                    tsd.isWeekend = true;
                                }
                                timeSheetDetailsList.Add(tsd);
                                startDate = startDate.AddDays(1);
                            }

                            if (vm.WeekDates.ContainsKey(s.ToString()) == true)
                            {
                                var itemsInDictionary = vm.WeekDates[s.ToString()];
                                itemsInDictionary.AddRange(timeSheetDetailsList);
                                //foreach (var timeSheet in timeSheetDetailsList)
                                //{
                                //    var itemsNotInDictionaryButInTimeSheet = itemsInDictionary.FirstOrDefault(x => x.WorkDate == timeSheet.WorkDate);
                                //    if (itemsNotInDictionaryButInTimeSheet == null)
                                //        itemsInDictionary.Add(timeSheet);
                                //}
                            }
                            else
                            {

                                if (nextYear && currentWeek > 52)
                                {
                                    currentWeek = endWeek;
                                }
                                vm.WeekDates.Add(currentWeek.ToString(), timeSheetDetailsList);
                            }
                            //startDate = startDate.AddDays(1);
                            if (startDate > vm.TimeSheet.ToDate)
                                break;
                            sDay = 1;
                            //endWeek = endWeek++;
                            endWeek = endWeek+1;
                        }
                    }
                    //earlier code chnged on 30122019
                    //    for (int s = startWeek; s <= endWeek; s++)
                    //    {
                    //        List<TimeSheetDetailDto> timeSheetDetailsList = new List<TimeSheetDetailDto>();
                    //        for (int i = sDay; i <= 7; i++)
                    //        {
                    //            if (startDate > item.ToDate)
                    //                break;
                    //            TimeSheetDetailDto tsd = new TimeSheetDetailDto();
                    //            tsd.WorkDate = startDate.ToShortDateString();
                    //            tsd.isWeekend = false;
                    //            if ((startDate.DayOfWeek == DayOfWeek.Saturday) || (startDate.DayOfWeek == DayOfWeek.Sunday))
                    //            {
                    //                tsd.isWeekend = true;
                    //            }
                    //            timeSheetDetailsList.Add(tsd);
                    //            startDate = startDate.AddDays(1);
                    //        }

                    //        if (vm.WeekDates.ContainsKey(s.ToString())==true)
                    //        {
                    //            var itemsInDictionary = vm.WeekDates[s.ToString()];
                    //            itemsInDictionary.AddRange(timeSheetDetailsList);
                    //            //foreach (var timeSheet in timeSheetDetailsList)
                    //            //{
                    //            //    var itemsNotInDictionaryButInTimeSheet = itemsInDictionary.FirstOrDefault(x => x.WorkDate == timeSheet.WorkDate);
                    //            //    if (itemsNotInDictionaryButInTimeSheet == null)
                    //            //        itemsInDictionary.Add(timeSheet);
                    //            //}
                    //        }
                    //        else
                    //        {
                    //           vm.WeekDates.Add(s.ToString(), timeSheetDetailsList);
                    //        }
                    //        //startDate = startDate.AddDays(1);
                    //        if (startDate > item.ToDate)
                    //            break;
                    //        sDay = 1;
                    //    }
                }
            
                else
                    vm.ErrorMessage = "Proforma Invoice not created. Please fill the proforma invoice first.";
            }
            
            }
            catch (Exception ex)
            {

                vm.ErrorMessage = ex.Message.ToString();
            }
            return vm;
        }
        #endregion
        #region for Private Method
        //private void TimeSheetCalcualtions(TimeSheetDto sr)
        //{
        //    WorkOrderViewModel vm = new WorkOrderViewModel();
        //    PicanolUtils utils = new PicanolUtils();
        //    int startWeek = utils.GetIso8601WeekOfYear(sr.FromDate);
        //    int endWeek = utils.GetIso8601WeekOfYear(sr.ToDate);
        //    int sDay = (int)sr.FromDate.DayOfWeek;
        //    DateTime startDate = (DateTime)sr.FromDate;
        //    for (int s = startWeek; s <= endWeek; s++)
        //    {
        //        List<TimeSheetDetailDto> timeSheetDetailsList = new List<TimeSheetDetailDto>();
        //        for (int i = sDay; i <= 7; i++)
        //        {
        //            if (startDate > sr.ToDate)
        //                break;
        //            TimeSheetDetailDto tsd = new TimeSheetDetailDto();
        //            tsd.WorkDate = startDate.ToShortDateString();
        //            tsd.isWeekend = false;
        //            if ((startDate.DayOfWeek == DayOfWeek.Saturday) || (startDate.DayOfWeek == DayOfWeek.Sunday))
        //            {
        //                tsd.isWeekend = true;
        //            }
        //            timeSheetDetailsList.Add(tsd);
        //            startDate = startDate.AddDays(1);
        //        }
        //        vm.WeekDates.Add(s.ToString(), timeSheetDetailsList);
        //        //startDate = startDate.AddDays(1);
        //        if (startDate > sr.ToDate)
        //            break;
        //        sDay = 1;
        //    }
        //}
        #endregion
    }
}