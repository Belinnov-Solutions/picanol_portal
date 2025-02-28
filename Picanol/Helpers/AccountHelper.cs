using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.RightsManagement;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class AccountHelper 
    {
        public static string PasswordEncryption = "KsTkyLV=";
        protected iValidation validationDictionary { get; set; }

        #region Ctor
        public AccountHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }
        #endregion
        public string SaveNewPassword(LoginDto user)
        {
            string response = "";
            //use above method to decrept User Id 
            if (user.EncryptedUserId != null) 
            { 
            string ui = GetDecreptedPassword(user.EncryptedUserId);
            user.UserID = Convert.ToInt32(ui);
            }
            string password = GetEncryptedPassword(user.Password);
            PicannolEntities context = new PicannolEntities();
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            var usr = context.tblUsers.Where(x => x.UserId == user.UserID).FirstOrDefault();
            usr.Password = password;
            context.tblUsers.Attach(usr);
            context.Entry(usr).State = EntityState.Modified;
            try
            {
                context.SaveChanges();
                response = "success";
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return response;
        }

        public string GetEncryptedPassword(string pwd)
        {
            return EncryptionHelper.Encryptor(pwd, PasswordEncryption);
        }

        public string GetDecreptedPassword(string Pwd)
        {
            return EncryptionHelper.DecryptFromBase64String(Pwd, PasswordEncryption);
        }
        public bool CheckUserExist(string userId)
        {
            bool isAvailable = false;
            PicannolEntities context = new PicannolEntities();
            var a = context.tblUsers.Where(x => x.Email == userId && x.DelInd == false).FirstOrDefault();
            if(a != null)
            {
                isAvailable = true;
                return isAvailable;
            }
            return isAvailable;
        }
    }
}