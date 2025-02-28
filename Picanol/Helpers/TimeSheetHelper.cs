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
//using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class TimeSheetHelper
    {
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        private WorkOrderService _workOrderService;
        protected WorkOrderService WorkOrderService
        {
            get
            {
                if (_workOrderService == null) _workOrderService = new WorkOrderService(entities, validationDictionary);
                return _workOrderService;
            }
        }

        protected iValidation validationDictionary { get; set; }



        #endregion

        #region Ctor
        public TimeSheetHelper(Controller controller)
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
       

       
        public List<TimeSheetDto> GetTimeSheetList(int workOrderId)
        {
            PicannolEntities _context = new PicannolEntities();

            var tsList = (from a in _context.tblTimeSheets
                      join b in _context.tblWorkOrders on a.WorkOrderId equals b.WorkOrderId

                      where a.WorkOrderId == workOrderId
                      select new TimeSheetDto
                      {
                          WorkOrderId = a.WorkOrderId,
                          WorkOrderNo = b.WorkOrderNo,
                          TimeSheetNo = a.TimeSheetNo,
                          TimeSheetId = a.TimeSheetId,
                          FromDate = (DateTime)a.FromDate,
                          ToDate = (DateTime)a.ToDate
                      }).ToList();
            
            return tsList;
        }

        #endregion

        #region TimeSheet Management
        public string SaveTimeSheet(TimeSheetDto tsh, List<TimeSheetDetailDto> tsd)
        {
            string response = "";
            PicannolEntities context = new PicannolEntities();
            if (tsh.TimeSheetId != 0)
            {
                var a = context.tblTimeSheets.Where(x => x.TimeSheetId == tsh.TimeSheetId).FirstOrDefault();
                a.CustomerId = tsh.CustomerId;
                var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
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
            }
            else
            {
                
               
                tblTimeSheet ts = new tblTimeSheet();
                ts.CustomerId = tsh.CustomerId;
                var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
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
                ts.FinalSubmit = false;
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
            var UserInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
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

       
        #endregion
    }
}