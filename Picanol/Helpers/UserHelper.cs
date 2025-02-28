using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class UserHelper : IDisposable
    {
        public static string PasswordEncryption = "KsTkyLV=";
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        private InvoiceHelper invoiceHelper;
        private MaterialHelper materialHelper;
        private OrderHelper orderHelper;
        protected iValidation validationDictionary { get; set; }
        private UserService _userManagerService;
        protected UserService UserService
        {
            get
            {
                if (_userManagerService == null) _userManagerService = new UserService(entities, validationDictionary);
                return _userManagerService;
            }
        }
        #endregion
        

        #region Ctor
        public UserHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }
        private ServiceRequestHelper serviceRequestHelper;

        

        public UserHelper(InvoiceHelper invhelpr)
        {
            this.invoiceHelper = invhelpr;
        }

        public UserHelper(MaterialHelper mh)
        {
            this.materialHelper = mh;
        }

        public UserHelper(OrderHelper oh)
        {
            this.orderHelper = oh;
        }

        public UserHelper(ServiceRequestHelper serviceRequestHelper)
        {
            this.serviceRequestHelper = serviceRequestHelper;
        }

        


        private WorkOrderEmail workOrderEmail;
        public UserHelper(WorkOrderEmail workOrderEmail)
        {
            this.workOrderEmail = workOrderEmail;
        }
        //private InvoiceHelper invoiceHelper;
        //public UserHelper(InvoiceHelper invoiceHelper)
        //{
        //    this.invoiceHelper = invoiceHelper;
        //}

        


        #endregion

        public string InsertUser(UserDto us)
        {
            tblUser Usr = new tblUser();
            Usr.UserName = us.UserName;
            Usr.MobileNo = us.MobileNo;
            Usr.Password = GetEncryptedPassword(us.Password);
            Usr.RoleId = us.RoleId;
            Usr.Email = us.Email;
            Usr.DelInd = false;
            UserService.AddUser(Usr);
            return "success";
        }

        public string DeleteUser(int id)
        {
            tblUser tn = new tblUser();
            tn.UserId = id;
            UserService.DeleteUser(tn);

            return "success";


        }

        
        public string UpdateUser(UserDto us)
        {
            tblUser Usr = new tblUser();

            Usr.UserName = us.UserName;
            Usr.Email = us.Email;
            Usr.Password = GetEncryptedPassword(us.Password);
            Usr.RoleId = us.RoleId;
            Usr.UserId = us.UserId;
            Usr.MobileNo = us.MobileNo;
            UserService.UpdateUser(Usr);

            return "success";
        }

        public List<UserDto> GetAllUsers()
        {
            var pass = "";
            List<UserDto> users = new List<UserDto>();

            users = UserService.GetAllUsers();
            foreach (var us in users)
            {
                if (us.Password != null)
                {
                    //int mm = us.Password.Replace(" ", "").Length % 4;
                    //if (mm > 0)
                    //{
                    //    //Trailing padding
                    //    us.Password += new string('=', 4 - mm);
                    //}
                    pass = GetDecreptedPassword(us.Password);
                }
                us.Password = pass;
                us.CPassword = pass;
            }
            return users;
        }
        public string GetDecreptedPassword(string pwd)
        {
            return EncryptionHelper.DecryptFromBase64String(pwd, PasswordEncryption);
        }

        public string DeleteUserAccount(string mobileNumber)
        {
            PicannolEntities _context = new PicannolEntities();
            string message = "";
           // mobileNumber = mobileNumber.Substring(mobileNumber.Length - 10);
            var user = _context.tblUsers.Where(x => x.MobileNo== mobileNumber).FirstOrDefault(); //returns a single item.

            if (user != null)
            {
                user.DelInd = true;
                _context.Entry(user).State = EntityState.Modified;
               // _context.tblUsers.Remove(user);
                _context.SaveChanges();
                message = "Success";


            }

            return message;
        }


        public List<UserDto> GetAllUsersById()
        {
            List<UserDto> users = new List<UserDto>();
            users = UserService.GetUsersList();
            return users; 
        }

        public List<UserDto> GetAssignedUsersList()
        {
            return UserService.GetAssignedUsersList();
        }

        public int GetUserId(string EmailId)
        {
            PicannolEntities context = new PicannolEntities();
            var t = context.tblUsers.Where(x => x.Email == EmailId && x.DelInd==false).Select(x => x.UserId).SingleOrDefault();
            return t;
        }
        public string CC()
        {
            string cc = "";
            PicannolEntities context = new PicannolEntities();
            var s = (from a in context.tblUsers
                     where a.RoleId == 6
                     select new
                     {
                         a.Email
                     }).ToList();
            foreach (var item in s)
            {
                cc = cc + ";" + item.Email;
            }
            return cc;
        }

        public string GetEncryptedPassword(string pwd)
        {
            return EncryptionHelper.Encryptor(pwd, PasswordEncryption);
        }
        #region IDisposable Members
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    disposed = true;
                    if (entities != null) entities.Dispose();
                    entities = null;

                    if (_userManagerService != null) _userManagerService.Dispose();
                    _userManagerService = null;


                }
            }
        }
        #endregion


        public void recordUserActivityHistory(int userid, string ActionName, string TableName)
        {

            try
            {
                PicannolEntities context = new PicannolEntities();
                HttpBrowserCapabilities browser = new HttpBrowserCapabilities();
                var Ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                Ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                var extractBrowser = HttpContext.Current.Request.Browser.Browser;

                tblAudit RecordData = new tblAudit();
                RecordData.ActionName = ActionName;
                RecordData.Browser = extractBrowser;
                RecordData.DateCreated = DateTime.Now;
                RecordData.IPAddress = Ip;
                RecordData.UserId = userid;
                RecordData.TableName = TableName;
                context.tblAudits.Add(RecordData);
                context.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}