using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Picanol.Helpers
{
    public class WorkOrderEmail
    {
        private readonly UserHelper _userHelper;
        public WorkOrderEmail()
        {

            _userHelper = new UserHelper(this);
        }
       
        public static void SendEditWorkOrderEmail(WorkOrderDto wo)
        {
            PicannolEntities context = new PicannolEntities();
            var email = context.tblUsers.Where(x => x.UserId == wo.AssignedTo).Select(x => x.Email).FirstOrDefault();
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var customerInfo = context.tblCustomers.Where(x => x.CustomerId == wo.CustomerId).FirstOrDefault();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("You  work order edited for <b>" + customerInfo.CustomerName + "</b>");
            stringBuilder.Append("<br /><br />Details<br/>");
            stringBuilder.Append("WorkOrder Number : <b>" + wo.WorkOrder + "</b> <br/>");
            stringBuilder.Append("Customer : <b>" + customerInfo.CustomerName + "</b> <br/>");
            stringBuilder.Append("Customer Address: <b>" + wo.CustomerAddress + "</b> <br/>");
            stringBuilder.Append("Start Date : <b>" + wo.SDate + "</b> <br/>");
            stringBuilder.Append("End Date : <b>" + wo.EDate + "</b> <br/>");
            stringBuilder.Append("WorkOrder Type : <b>" + wo.WorkOrderType + "</b> <br/>");
            stringBuilder.Append("ContractNumber : <b>" + wo.ContractNumber + "</b> <br/>");
            stringBuilder.Append("Mission Description : <b>" + wo.Description + "</b> <br/>");
            stringBuilder.Append("Mission Condition : <b>" + wo.Conditions + "</b> <br/>");
            stringBuilder.Append("Contact Person : <b>" + wo.ContactPerson + "</b> <br/>");
            stringBuilder.Append("Contact Person Email : <b>" + wo.EmailId + "</b> <br/>");
            stringBuilder.Append("Contact Person Number: <b>" + wo.Mobile + "</b> <br/>");

            //GMailer.GmailUsername = "noreply.picanol@gmail.com";
            //GMailer.GmailPassword = "9999907947";
            GMailer mailer = new GMailer();
            //mailer.ToEmail = ConfigurationManager.AppSettings["emailID"];
            mailer.ToEmail = email;
            mailer.CcEmail = userInfo.Email;
            mailer.Subject = "Edited Work Order - " + wo.CustomerName;
            mailer.Body = stringBuilder.ToString();
            mailer.IsHtml = true;
            mailer.AttachmentPath = "";
            foreach (var item in wo.workOrderImageList)
            {

                mailer.AttachmentPath += HttpContext.Current.Server.MapPath("~/Content/PDF/WorkOrderPdf/" + item.ImageName) + ";";

            }
            mailer.Send();
        }
        public static void SendWorkOrderEmail(WorkOrderDto wo)
        {
            PicannolEntities context = new PicannolEntities();
            var email = context.tblUsers.Where(x => x.UserId == wo.AssignedTo).Select(x => x.Email).FirstOrDefault();
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("You received new work order for <b>" + wo.CustomerName + "</b>");
            stringBuilder.Append("<br /><br />Details<br/>.");
            stringBuilder.Append("WorkOrder Number : <b>" + wo.WorkOrder + "</b> <br/>");
            stringBuilder.Append("Customer : <b>" + wo.CustomerName + "</b> <br/>");
            stringBuilder.Append("Customer Address: <b>" + wo.CustomerAddress + "</b> <br/>");
            stringBuilder.Append("Start Date : <b>" + wo.SDate + "</b> <br/>");
            stringBuilder.Append("End Date : <b>" + wo.EDate + "</b> <br/>");
            stringBuilder.Append("WorkOrder Type : <b>" + wo.WorkOrderType + "</b> <br/>");
            stringBuilder.Append("ContractNumber : <b>" + wo.ContractNumber + "</b> <br/>");
            stringBuilder.Append("Mission Description : <b>" + wo.Description + "</b> <br/>");
            stringBuilder.Append("Mission Condition : <b>" + wo.Conditions + "</b> <br/>");
            stringBuilder.Append("Contact Person : <b>" + wo.ContactPerson + "</b> <br/>");
            stringBuilder.Append("Contact Person Email : <b>" + wo.EmailId + "</b> <br/>");
            stringBuilder.Append("Contact Person Number: <b>" + wo.Mobile + "</b> <br/>");

            //GMailer.GmailUsername = "noreply.picanol@gmail.com";
            //GMailer.GmailPassword = "9999907947";
            GMailer mailer = new GMailer();
            //mailer.ToEmail = ConfigurationManager.AppSettings["emailID"];
            mailer.ToEmail = email;
            mailer.CcEmail = userInfo.Email;
            mailer.Subject = "New Work Order - " + wo.CustomerName;
            mailer.Body = stringBuilder.ToString();
            foreach (var item in wo.workOrderImageList)
            {

                mailer.AttachmentPath += HttpContext.Current.Server.MapPath("~/Content/PDF/WorkOrderPdf/" + item.ImageName) + ";";

            }
            mailer.IsHtml = true;
            mailer.Send();

            //string ActionName = $"SendWorkOrderEmail(CustomerName)  - { wo.CustomerName}";
            //string TableName = "TblWorkOrder";
            //if (ActionName != null)
            //{
            //    //var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            //    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
            //}
        }


    }
}