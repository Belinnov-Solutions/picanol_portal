using Microsoft.SqlServer.Server;
using NLog;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{ //Created This mobile service Request Controller to seprate mobile Api from portal and add authentication on them 
  //While Creating APIController. facing Some issue with Json Format. that's why use this approach
    [Authorize]
    public class MobileHomeController : Controller
    {
        private readonly MaterialHelper _materialHelper;
        private readonly CustomerHelper _customerHelper;
        private readonly AccountHelper _accountHelper;
        private readonly UserHelper _userHelper;
        Logger logger = LogManager.GetLogger("databaseLogger");
        public MobileHomeController()
        {

            _materialHelper = new MaterialHelper(this);
            _customerHelper = new CustomerHelper(this);
            _accountHelper = new AccountHelper(this);
            _userHelper = new UserHelper(this);
            
        }
        /*[HttpPost]
        public ActionResult Login(LoginDto ld)
        {
            try
            {
                string encryptedPwd = "";
                if (ld.Password != null)
                {
                    encryptedPwd = _accountHelper.GetEncryptedPassword(ld.Password);
                }
                if (ld.type == 2)
                {

                    PicannolEntities c = new PicannolEntities();
                    LoginDto User = new LoginDto();
                    if (ld.UserName.Length == 10)
                    {
                        ld.UserName = "+91" + ld.UserName;
                    }
                    User = (from r in c.tblUsers
                                // where r.Email.Contains(ld.Email) && r.Password == encryptedPwd && r.DelInd == false
                            where r.Email == ld.UserName || r.MobileNo == ld.UserName && r.DelInd == false
                            select new LoginDto
                            {
                                UserID = r.UserId,
                                UserName = r.UserName,
                                RoleID = r.RoleId,
                                Email = r.Email,


                            }).FirstOrDefault();
                    return Json( User , JsonRequestBehavior.AllowGet);



                }
                PicannolEntities context = new PicannolEntities();
                encryptedPwd = _accountHelper.GetEncryptedPassword(ld.Password);
                var result = (from a in context.tblUsers
                              join b in context.tblRoles on a.RoleId equals b.RoleId
                              where a.Email.Contains(ld.Email) && a.Password == encryptedPwd && a.DelInd == false
                              select new LoginDto
                              {
                                  UserID = a.UserId,
                                  UserName = a.UserName,
                                  RoleID = a.RoleId,
                                  Email = a.Email,
                                  RoleName = b.RoleName,
                              }).FirstOrDefault();
                if (result == null)
                {
                    Response.Write("<script>alert('Email and Password not Matched')</script>");
                    return View();
                }
                LoginDto l = new LoginDto();
                l.UserID = result.UserID;

                if (l.UserID == 0)
                {
                    Response.Write("<script>alert('Email and Password not Matched')</script>");
                    return View();
                }

                Session["UserInfo"] = new UserSession()
                {
                    UserId = result.UserID,
                    UserName = result.UserName,
                    Email = result.Email,
                    RoleId = result.RoleID,
                    RoleName = result.RoleName
                };

                if (encryptedPwd == "dBZt8rVL2v0=")
                {

                    return RedirectToAction("ChangePassword", "Home");
                }
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }*/
        public ActionResult GetCustomersList()
        {
            List<CustomerDto> customers = _materialHelper.GetCustomersList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult SendOtp(string MobileNo)

        {
            try
            {

                OneTimePassword os = new OneTimePassword();
                var a = os.GenerateOTP(MobileNo);
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in Sending Credentials!");
                logger.Error(ex.InnerException, "Inner exception");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }





        [HttpPost]
        public JsonResult Login(LoginDto loginDto)
        {
            try
            {
                loginDto.Password = "";
                if (loginDto == null)
                    throw new ArgumentNullException(nameof(loginDto), "Login details cannot be null.");

                string encryptedPwd = string.Empty;

                if (!string.IsNullOrEmpty(loginDto.Password))
                {
                    encryptedPwd = _accountHelper.GetEncryptedPassword(loginDto.Password);
                }

                using (var context = new PicannolEntities())
                {
                    // Add country code if the username is a 10-digit mobile number
                    if (!string.IsNullOrEmpty(loginDto.UserName) && loginDto.UserName.Length == 10)
                    {
                        loginDto.UserName = "+91" + loginDto.UserName;
                    }

                    var user = (from a in context.tblUsers
                                join b in context.tblRoles on a.RoleId equals b.RoleId
                                /* where (a.Email == loginDto.UserName || a.MobileNo == loginDto.UserName)
                                       && a.Password == encryptedPwd*/
                                where (a.Email == loginDto.UserName || a.MobileNo == loginDto.UserName)
                                      && a.DelInd == false
                                select new LoginDto
                                {
                                    UserID = a.UserId,
                                    UserName = a.UserName,
                                    RoleID = a.RoleId,
                                    Email = a.Email,
                                    RoleName = b.RoleName,
                                }).FirstOrDefault();

                    return Json(user, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception before rethrowing for debugging or auditing
                // Replace with your logging mechanism
                Console.Error.WriteLine(ex);

                // Avoid throwing the raw exception; wrap it in a custom exception
                throw new ApplicationException("An error occurred while authenticating the user.", ex);
            }
        }

    }




}