using NLog;
using OfficeOpenXml;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.Utils;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Picanol.Controllers
{

    public class HomeController : BaseController
    {
        private readonly MaterialHelper _materialHelper;
        private readonly CustomerHelper _customerHelper;
        private readonly AccountHelper _accountHelper;
        private readonly UserHelper _userHelper;
        public HomeController()
        {

            _materialHelper = new MaterialHelper(this);
            _customerHelper = new CustomerHelper(this);
            _accountHelper = new AccountHelper(this);
            _userHelper = new UserHelper(this);
        }
        Logger logger = LogManager.GetLogger("databaseLogger");
        public ActionResult TestCreaditnote()
        {
            return View();
        }
        public ActionResult Index()
        {
            try
            {
                if (!IsUserLoggedIn)
                {

                    return RedirectToAction("Login", "Home");
                    //return RedirectToAction("Index", "Home", null);
                }
                var userSession = GetUserSession();
                return View(userSession);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "IndexPage");
                return RedirectToAction("Index", "Home", null);
                //throw ex;
            }
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult LoggedOut()
        {
            Session["UserInfo"] = null;
            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            try
            {
                Session["UserInfo"] = new UserSession();
                return View();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Login");
                throw ex;
            }
        }
        //public ActionResult ChangePassword(int userId)
        //{
        //    LoginDto a = new LoginDto();
        //    a.UserID = userId;
        //    return View(a);
        //}
        public ActionResult ChangePassword(string userId)
        {
            string response = _accountHelper.GetDecreptedPassword(userId);

            int UserIds = Convert.ToInt32(response);
                LoginDto a = new LoginDto();
                a.UserID = UserIds;
                a.EncryptedUserId = userId;
                return View(a);
           
        }

        [HttpPost]
        public ActionResult ChangePassword(LoginDto ld)
        {
            try
            {
                string response = _accountHelper.SaveNewPassword(ld);
                if (response == "success")
                {
                    Response.Write("<script>alert('Password Changed Successfully!')</script>");
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    Response.Write("<script>alert('Please try again!!')</script>");

                    return View();
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "ChangePassword");
                throw ex;
            }
        }
        
        public ActionResult ForgotPassword(string EmailId)
        {
            var changePassword = ConfigurationManager.AppSettings["changePasswordLink"];
            try
            {
                bool UserExist = _accountHelper.CheckUserExist(EmailId);
                if (UserExist)
                {
                    //Session["UserInfo"] = new LoginDto()
                    //{
                    //    Email = EmailId
                    //};
                    int UserId = _userHelper.GetUserId(EmailId);
                    //using this to encrypt the UserID of user
                    string value = GetEncryptedPassword(UserId.ToString());
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("Dear Sir/Madam,<br />");
                    //stringBuilder.Append("Please click on below link to change your Password.<b><br/>http://picanol.belinnov.in/Home/ChangePassword?userId=" + value);

                    stringBuilder.Append($"Please click on below link to change your Password.<b><br/>{changePassword}" + value);

                    //stringBuilder.Append("Please click on below link to change your Password.<b><br/>http://localhost:11907/Home/ChangePassword?userId=" + value);
                    stringBuilder.Append("<br/><br/> Thanks <br/> ");                    
                    GMailer mailer = new GMailer();


                    mailer.ToEmail = EmailId;
                    mailer.Subject = "Create New Password ";
                    mailer.Body = stringBuilder.ToString();
                    mailer.IsHtml = true;

                    try
                    {
                        mailer.Send();

                    }
                    catch (Exception ex)
                    {

                        throw;

                    }

                    return Json("success");
                }

                return Json("failure");
            }
            catch(Exception ex)
            {
                logger.Error(ex, "ForgetPassword");
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult Login(LoginDto ld)
        {
            try {

                string encryptedPwd = _accountHelper.GetEncryptedPassword(ld.Password);
                if (ld.type == 2)
                {

                    PicannolEntities c = new PicannolEntities();
                    LoginDto User = new LoginDto();
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
                    return Json(new { result = User }, JsonRequestBehavior.AllowGet);



                }
                PicannolEntities context = new PicannolEntities();
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

                return RedirectToAction("Index", "Home");

               /* if (encryptedPwd == "dBZt8rVL2v0=")
                {

                    return RedirectToAction("ChangePassword", "Home");
                }
                else
                    return RedirectToAction("Index", "Home");*/
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult UnderConstruction()
        {
            return View();
        }

        public ActionResult GetCustomersList()
        {
            List<CustomerDto> customers = _materialHelper.GetCustomersList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

      

        public ActionResult UploadProducts()
        {

            return View();
        }

        public ActionResult UploadChallan(FormCollection formCollection)
        {
            var challansList = new List<ChallanDto>();
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;

                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        {
                            var challan = new ChallanDto();
                            if (workSheet.Cells[rowIterator, 1].Value != null)
                                challan.ChallanNumber = workSheet.Cells[rowIterator, 1].Value.ToString();
                            if (workSheet.Cells[rowIterator, 2].Value != null)
                                challan.ChallanDate = Convert.ToDateTime(workSheet.Cells[rowIterator, 2].Value.ToString());
                            if (workSheet.Cells[rowIterator, 7].Value != null)
                            {
                                CustomerDto customer = _customerHelper.GetCustomerDetailsByName(workSheet.Cells[rowIterator, 7].Value.ToString());
                                if (customer != null)
                                {
                                    challan.CustomerId = customer.CustomerId;
                                }
                                else
                                    challan.CustomerId = 0;
                            }
                            if (challan.CustomerId > 0)
                            {
                                if (challan.ChallanDate == null)
                                {
                                    challan.ChallanDate = DateTime.Now.Date;
                                }
                                challansList.Add(challan);
                            }
                        }
                    }
                }
            }
            foreach (var item in challansList)
            {
                try
                {
                    _materialHelper.InsertChallanDetail(item);

                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            //Response.Write("<script>alert('Stock Updated successfully')</script>");
            return RedirectToAction("UpdateStock", "Product");
        }


        public ActionResult UploadOrder(FormCollection formCollection)
        {
            var ordersList = new List<OrderDto>();
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;

                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        {
                            var order = new OrderDto();
                            if (workSheet.Cells[rowIterator, 1].Value != null)
                                order.DateCreated = Convert.ToDateTime(workSheet.Cells[rowIterator, 1].Value.ToString());
                            if (workSheet.Cells[rowIterator, 2].Value != null)
                                order.TrackingNumber = Convert.ToInt64(workSheet.Cells[rowIterator, 2].Value.ToString());
                            if (workSheet.Cells[rowIterator, 3].Value != null)
                            {
                                CustomerDto customer = _customerHelper.GetCustomerDetailsByName(workSheet.Cells[rowIterator, 3].Value.ToString());
                                if (customer != null)
                                {
                                    order.CustomerId = customer.CustomerId;
                                }
                                else
                                    order.CustomerId = 0;
                            }
                            if (order.CustomerId > 0 && workSheet.Cells[rowIterator, 4].Value != null)
                            {
                                ChallanDto challan = new ChallanDto();
                                challan =   _materialHelper.GetChallanDetailsByChallanNo(order.CustomerId, workSheet.Cells[rowIterator, 4].Value.ToString());
                                if (challan != null)
                                {
                                    if (challan.ChallanId > 0)
                                    {
                                        order.ChallanId = challan.ChallanId;
                                    }
                                    else
                                        order.ChallanId = 0;
                                }
                                else
                                    order.ChallanId = 0;
                            }
                            else
                                order.ChallanId = 0;
                            if (workSheet.Cells[rowIterator, 6].Value != null)
                            {
                                order.RepairType = workSheet.Cells[rowIterator, 6].Value.ToString();
                            }
                            string partNo = "";
                            string partName = "";
                            string serialNo = "";
                            if (workSheet.Cells[rowIterator, 9].Value != null)
                            {
                                partName = workSheet.Cells[rowIterator, 9].Value.ToString();
                            }
                            if (workSheet.Cells[rowIterator, 10].Value != null)
                            {
                                partNo = workSheet.Cells[rowIterator, 10].Value.ToString();
                            }
                            if (workSheet.Cells[rowIterator, 11].Value != null)
                            {
                                serialNo = workSheet.Cells[rowIterator, 11].Value.ToString();
                            }
                            int partId = _materialHelper.GetPartIDByPartDetails(partNo, partName, serialNo);
                            order.PartId = partId;
                            ordersList.Add(order);
                        }
                    }
                }
            }
            foreach (var item in ordersList)
            {
                try
                {
                    if (item.CustomerId != 0)
                    {
                    tblOrder ord = new tblOrder();
                    ord.ChallanId = item.ChallanId;
                        ord.TrackingNumber = (long) item.TrackingNumber;
                    ord.DateCreated = Convert.ToDateTime(item.DateCreated);
                    ord.CustomerId = item.CustomerId;
                    ord.DelInd = false;
                    ord.LastModified = DateTime.Now;
                    ord.OrderGUID = Guid.NewGuid();
                    ord.PartId = item.PartId;
                    ord.Qty = 1;
                    ord.RepairType = item.RepairType;
                    ord.Status = "Open";
                    PicannolEntities ctx = new PicannolEntities();
                    ctx.tblOrders.Add(ord);
                    ctx.SaveChanges();
                    }


                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            //Response.Write("<script>alert('Stock Updated successfully')</script>");
            return RedirectToAction("UpdateStock", "Product");
        }


        #region helper
        public static string PasswordEncryption = "KsTkyLV=";
        public string GetEncryptedPassword(string pwd)
        {
            
            return EncryptionHelper.Encryptor(pwd, PasswordEncryption);
        }
        #endregion



    }
}