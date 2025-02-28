using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Picanol.Helpers;
using Picanol.ViewModels;
using Picanol.Models;
using System.Text;
using ServiceStack.Redis;
using Picanol.Utils;

namespace Picanol.Controllers
{
    //[RedirectingAction]
    public class CustomerController : BaseController
    {
        private readonly UserHelper _userHelper;
        private readonly CustomerHelper _customerHelper;

        public CustomerController()
        {
            _customerHelper = new CustomerHelper(this);
            _userHelper = new UserHelper(this);

        }

        // GET: Customer
        public ActionResult Index()
        {
             return View();
        }
        public ActionResult AddCustomer(int? id, int? customerEdit)
        {
            CustomerViewModel uv = new CustomerViewModel();
            
            /*var customer = _customerHelper.GetCustomersList();
            var List = new List<string>()
            {
                "Zone1","Zone2","Zone3","Zone4"
            };

            ViewBag.List = List;*/

            if (id > 0)
            {
                uv.Customer = _customerHelper.GetCustomerDetails((int) id);
            }
            
            if(customerEdit == 0)
            {
                CustomerDto vm = (CustomerDto) Session["CutomerDetails"];
                uv.Customer = vm;
                return View(uv);
            }
           
            uv.StateList = _customerHelper.GetStatesList();
            uv.ZoneList = _customerHelper.GetZoneList();
            return View(uv);
        }


        [HttpPost]
        public ActionResult AddCustomer(CustomerDto um)
        {
            string response = "";
            if (um.CustomerId == 0)
            {
                response = _customerHelper.InsertCustomer(um);
                
                if (response == "success")
                {
                    string ActionName = $"Add Customer - {um.CustomerName}";
                    string TableName = "TblCustomer";
                    if (ActionName != null)
                    {
                        var userInfo = (UserSession)Session["UserInfo"];
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
            }

            else
            {
                response = _customerHelper.UpdateCustomer(um);
                if (response == "Updated")
                {
                    string ActionName = $"Update customer - {um.CustomerName}";
                    string TableName = "TblCustomer";
                    if (ActionName != null)
                    {
                        var userInfo = (UserSession)Session["UserInfo"];

                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
                return Json("Updated");
            }

            if (response == "success")
            {
                //ModelState.Clear();
                Response.Write("<script>alert('Customer Added successfully')</script>");
                return RedirectToAction("CustomerList", "Customer");
                //return View();
            }
            else
            {
                return View();
            }

        }
       
        //public void GetAllCustomerDataIntoExcel()
        //{
        //    CustomerViewModel vm = new CustomerViewModel();
        //    var gv = new GridView();
        //    vm.SearchCustomersList = _customerHelper.GetCustomersList();
        //    gv.DataSource = vm.SearchCustomersList.Select(x => new { x.CustomerName, x.AddressLine1, x.Zone, x.State, x.GSTIN });
        //    gv.DataBind();
        //    Response.ClearContent();
        //    Response.Buffer = true;
        //    Response.AddHeader("content-disposition", "attachment; filename=All Customer Data_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xls");
        //    Response.ContentType = "application/ms-excel";
        //    Response.Charset = "";
        //    StringWriter objStringWriter = new StringWriter();
        //    HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
        //    gv.RenderControl(objHtmlTextWriter);
        //    Response.Output.Write(objStringWriter.ToString());
        //    Response.Flush();
        //    Response.End();

        //}
        public ActionResult CustomerList(string Inactive, int PageSize=10, int PageNo=1, int? CustomerId = 0)
		{
            //if (CustomerId == null && Inactive==null)
            if (CustomerId == 0 && Inactive==null)
            {
               List<CustomerDto> nl = new List<CustomerDto>();
                CustomerViewModel vm = new CustomerViewModel();
                vm.CurrnetPageNo = PageNo;
                //customers = _customerHelper.GetCustomersList(/*PageSize,PageNo*/);// new call for pagination by Himanshu//
               vm.SearchCustomersList = _customerHelper.GetCustomersList();
               var customers = _customerHelper.GetCustomersListVersion2(PageSize, PageNo, (int)CustomerId, Inactive);
                vm.CustomersList = customers;
                vm.CountList = vm.CustomersList.Count;
                return View(vm);
            }
            
            else if(Inactive == null)
            {
                CustomerViewModel uv = new CustomerViewModel();

                // var customer = _customerHelper.GetCustomersList();//new call for pagination by Himanshu
               // uv.CustomersList = _customerHelper.GetCustomersList();
                var customer = _customerHelper.GetCustomersListVersion2(PageSize, PageNo, (int)CustomerId, Inactive);
                
                foreach (var item in customer)
                {

                    if (item.CustomerId == CustomerId)
                    {
                        uv.CustomersList.Add(item);

                        break;
                    }
                }
                uv.CurrnetPageNo = PageNo;
                uv.CountList = uv.CustomersList.Count;
                return PartialView("_FilterCustomerList", uv);
                

            }
            else
            {
                List<CustomerDto> nl = new List<CustomerDto>();
                CustomerViewModel vm = new CustomerViewModel();
                // var customers = _customerHelper.GetCustomersList(/*PageSize,PageNo*/);//new call for pagination by Himanshu
                //vm.CustomersList = _customerHelper.GetCustomersList();
                var customers = _customerHelper.GetCustomersListVersion2(PageSize, PageNo, (int)CustomerId, Inactive);
                vm.CustomersList = customers.Where(x=>x.InActive==true).ToList();
                vm.SearchCustomersList = vm.CustomersList;
                vm.CurrnetPageNo = PageNo;
                vm.CountList = vm.CustomersList.Count;
                return View(vm);
            }
        }


        public ActionResult DeleteCustomer(int id)
        {
            CustomerViewModel nvm = new CustomerViewModel();
            var customers = _customerHelper.GetCustomersList();
            nvm.Customer = customers.Where(x => x.CustomerId == id).FirstOrDefault();
            var userInfo = (UserSession)Session["UserInfo"];
            //foreach (var item in customers)
            //{
            //    if (item.CustomerId == id)
            //        nvm.Customer = item;

            //}
            string response = "";
            if (nvm.Customer.CustomerId != 0)
            {
                response = _customerHelper.DeleteCustomer(nvm.Customer.CustomerId);
            }
            if (response == "success")
            {
                string ActionName = $"Delete Customer - {nvm.Customer.CustomerId}";
                string TableName = "TblCustomer";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }
            Response.Write("<script>alert('Customer Deleted successfully')</script>");
            return RedirectToAction("CustomerList", "Customer");
        }

        public ActionResult SearchCustomersListByName(string searchString)
        {
            CustomerViewModel vm = new CustomerViewModel();
            vm.CustomersList = _customerHelper.GetCustomerListByName(searchString);
            return PartialView("_FilteredCustomerListByName",vm);
            
        }
        #region  for Sub Customer CRUD

        public ActionResult AddSubCustomer(int? id)
        {
            CustomerViewModel vm = new CustomerViewModel();
            if (id > 0)
            {
                vm.StateList = _customerHelper.GetStatesList();
               // vm.CustomersList = _customerHelper.GetCustomersList();
                vm.CustomersList = _customerHelper.GetCustomersListVersion0();
                var customer = _customerHelper.GetAllSubCustomer();

                foreach (var item in customer)
                {
                    if (item.SubCustomerId == id)
                        vm.SubCustomer = item;
                }
                vm.CustomerIsEdit = true;
            }
            else
            {
                //vm.CustomersList = _customerHelper.GetCustomersList();
                vm.CustomersList = _customerHelper.GetCustomersListVersion0();
                vm.StateList = _customerHelper.GetStatesList();
                vm.CustomerIsEdit = false;
            }


            return View(vm);
        }
        public ActionResult DeleteSubCustomer(int id)
        {
            var response = _customerHelper.DeleteSubCustomer(id);
            if (response == "Success")
            {
                string ActionName = $"Delete Subcustomer id  - {id}";
                string TableName = "TblSubCustomer";
                var userInfo = (UserSession)Session["UserInfo"];
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
            }
            return RedirectToAction("SubCustomerList", "Customer");
        }
        public JsonResult AddNewSubCustomer(SubCustomerDto request)
        {
            string response = "";
            if (request.SubCustomerId == 0)
            {
                response = _customerHelper.SaveSubCustomerDetails(request);
                if (response == "Successs")
                {
                    var userInfo = (UserSession)Session["UserInfo"];
                    string ActionName = $"add Subcustomer - {request.SubCustomerName}";
                    string TableName = "TblSubCustomer";
                    if (ActionName != null)
                    {
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
            }
            else
            {
                response = _customerHelper.UpdateSubCustomerDetails(request);
                if (response == "Updated")
                {
                    string ActionName = $"Edit Subcustomer(CustomerName) - {request.SubCustomerName}";
                    string TableName = "TblSubCustomer";
                    var userInfo = (UserSession)Session["UserInfo"];
                    if (ActionName != null)
                    {
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubCustomerList(int PageSize=10,int PageNo=1)
        {

            // search all and pagination only 
            CustomerViewModel vm = new CustomerViewModel();
            string search = "";
            vm.CurrnetPageNo = PageNo;
            // var subCustomersList = _customerHelper.GetAllSubCustomer(PageSize,PageNo);
           
            var subCustomersList = _customerHelper.GetAllSubCustomerV1(PageSize, PageNo,search);
            vm.SubCustomerList = subCustomersList;
            vm.CountList = vm.SubCustomerList.Count;
           // vm.SubCustomerList = subCustomersList.Take(10).ToList();
            return View(vm);
        }


        [HttpPost]
        public ActionResult SearchSubCustomerList(string Searching,int PageSize=10,int PageNo=1)
        {
            CustomerViewModel vm = new CustomerViewModel();
            vm.CurrnetPageNo = PageNo;
            var subCustomersList = _customerHelper.GetAllSubCustomerV1(PageSize, PageNo, Searching);
             //vm.SubCustomerList = _customerHelper.SearchSubCustomerList(Searching).ToList();
            vm.SubCustomerList = subCustomersList;
            vm.CountList = vm.SubCustomerList.Count;
            return View("_FilterSubCustomerList", vm);
        }

        public JsonResult GetCustomerDetails(int CustomerId)
        {
            var customerDetails = _customerHelper.GetCustomerDetailsById(CustomerId);
            return Json(customerDetails);
        }

        public JsonResult GetSubCustomerDetail(int CustomerId)
        {
            var SubCustomerDetail = _customerHelper.GetSubCustomerDetail(CustomerId);
            return Json(SubCustomerDetail); ;
        }


        #endregion

        private static bool Save(string host, string key, string value)
        {
            bool isSuccess = false;
            using (RedisClient redisClient = new RedisClient(host))
            {
                if (redisClient.Get<string>(key) == null)
                {
                    //isSuccess = redisClient.Set(key, value);
                    isSuccess = redisClient.Set(key, value,TimeSpan.FromMinutes(2));
                }
            }
            return isSuccess;
        }
        public static string Get(string host, string key)
        {
            using (RedisClient redisClient = new RedisClient(host))
            {
               var a =  redisClient.Get<string>(key);
              
                return a;
            }
        }
        //[MyAuthorizeAttribute]
        public JsonResult AddValueToMain()
        {
            string host = "localhost";
            string key = "8527238402";
            bool success = Save(host, key, "1234");

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public static void Main(string[] args)
        {
            string host = "localhost";
            string key = "IDG";
            // Store data in the cache
            bool success = Save(host, key, "Hello World!");
            // Retrieve data from the cache using the key
            Console.WriteLine("Data retrieved from Redis Cache: " + Get(host, key));
            Console.Read();
        }
    }
}