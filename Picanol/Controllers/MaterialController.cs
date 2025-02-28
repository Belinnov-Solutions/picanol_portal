using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;




namespace Picanol.Controllers
{
    [RedirectingAction]
    public class MaterialController : BaseController
    {
        //private List<InwardMaterialViewModel> InwardMaterialViewArray = new List<InwardMaterialViewModel>();

        private readonly MaterialHelper _materialHelper;
        private readonly InvoiceHelper _invoiceHelper;
        private readonly OrderHelper _orderHelper;
        private readonly CustomerHelper _customerHelper;
        PicannolEntities _context = new PicannolEntities();
        public MaterialController()
        {
            _materialHelper = new MaterialHelper(this);
            _invoiceHelper = new InvoiceHelper(this);
            _orderHelper = new OrderHelper(this);
            _customerHelper = new CustomerHelper(this);
        }
        // GET: Material
        public ActionResult Index()
        {
            InwardMaterialViewModel vm = new InwardMaterialViewModel();
            vm.Customers = _materialHelper.GetCustomersList();
            return View(vm);
        }

        [HttpPost]
        public ActionResult GetOrdersList(InwardMaterialViewModel vm)
        {
            vm.Orders = _materialHelper.GetOrdersList(vm);
            return View();
        }

        public ActionResult AddNewTest(InwardMaterialViewModel vm)
        {

            return View();
        }

        [HttpPost]
        public ActionResult GetCustomersList(string Prefix)
        {
            List<CustomerDto> customers = _materialHelper.GetCustomersList();
            var customer = (from N in customers
                            where N.CustomerName.Contains(Prefix)
                            select new
                            {
                                N.CustomerName,
                                N.CustomerId
                            });

            return Json(customer, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult AddNew()
        {
            InwardMaterialViewModel vm = new InwardMaterialViewModel();
            vm.Customers = _materialHelper.GetCustomersList();
            //vm.Parts = _materialHelper.GetPartsList(1);
            //vm.Parts = _materialHelper.GetCombinedPartsList(1);
            vm.Parts = _materialHelper.GetBoardsList();
            vm.Users = _materialHelper.GetUsersList();

            vm.LastInvoiceNumber = _invoiceHelper.GetLastInvoiceNo(ConstantsHelper.InvoiceType.RP.ToString());
            vm.LastTrackingNumber = _orderHelper.GetLastTrackingNumber();
            Session["BoardsList"] = vm.Parts;
            Session["UsersList"] = vm.Users;
            return View(vm);
        }


        [HttpGet]
        public ActionResult EditInwardMaterial(Guid orderId)
        {
            //var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
           
            InwardMaterialViewModel vm = new InwardMaterialViewModel();
            vm.Customers = _materialHelper.GetCustomersList();
            vm.Parts = _materialHelper.GetCombinedPartsList(1);
            vm.Users = _materialHelper.GetUsersList();
            vm.NewOrder = _orderHelper.GetOrderDetails(orderId);
            
            Session["BoardsList"] = vm.Parts;
            Session["UsersList"] = vm.Users;
            /*var order = _context.tblProformaInvoices.Where(x => x.OrderGuid == orderId && x.DelInd == false).FirstOrDefault();
            if (order == null)
            {
                vm.proformaInvoiceGenerated = false;
            }
            else
            {
                vm.proformaInvoiceGenerated = true;
            }*/
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditInwardMaterial(InwardMaterialViewModel ivm)
        {
            

            PicannolEntities context = new PicannolEntities();
            var invoices = Server.MapPath("~/Content/PDF/Invoices/");
            var cancelledInvoices = Server.MapPath("~/Content/PDF/CancelledInvoice/");
            string FileName = "";
            var file = _invoiceHelper.GetInvoiceDetailsbyOrderId(ivm.NewOrder.OrderGUID);
            var source = Path.Combine(invoices, FileName);
            var destination = Path.Combine(cancelledInvoices, FileName);
            var userInfo = Session["UserInfo"] as UserSession;

            if (System.IO.File.Exists(source))
            {
                System.IO.File.Copy(source, destination, true);
                var path = Path.Combine(invoices, FileName);
                System.IO.File.Delete(path);
            }
            int chalanId = 0;
            ChallanDto ch = new ChallanDto();
            ch.CustomerId = ivm.CustomerId;
            if (ivm.ChallanDate != null)
                ch.ChallanDate = Convert.ToDateTime(ivm.ChallanDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            if (ivm.CustomerRef != null)
                ch.ChallanNumber = ivm.CustomerRef;
            chalanId = _materialHelper.InsertChallanDetail(ch);
            var existingOrder = context.tblOrders.Where(x=>x.OrderGUID ==ivm.NewOrder.OrderGUID).FirstOrDefault();

            var repType = ivm.NewOrder.RepairType;
            if (existingOrder.RepairType != repType)
            {
               
                if (context.tblInvoices.Any(x => x.OrderGuid == ivm.NewOrder.OrderGUID && x.DelInd == false && x.Cancelled == false))
                {
                    var orderInvoice = context.tblInvoices.Where(x => x.OrderGuid == ivm.NewOrder.OrderGUID && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                    var latestInvoice = context.tblInvoices.Where(x => x.DelInd == false && x.Cancelled == false).OrderByDescending(x => x.InvoiceId).Select(x => x.InvoiceNo).FirstOrDefault();
                    if (orderInvoice.InvoiceNo == latestInvoice)
                    {
                        orderInvoice.DelInd = true;
                        orderInvoice.Cancelled = true;
                    }
                    else
                    {
                        orderInvoice.Cancelled = true;
                       
                    }
                    context.tblInvoices.Attach(orderInvoice);
                    context.Entry(orderInvoice).State = EntityState.Modified;
                    context.SaveChanges();

                    //record user activity
                    string ActionName = $"Edit Invoices - OrderGUID {orderInvoice.OrderGuid}" +
                        $"Inv No. - {orderInvoice.InvoiceNo}";
                    string TableName = "tblInvoices";
                    if (ActionName != null)
                    {
                        new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                    //End
                }
                else if(context.tblProformaInvoices.Any(x => x.OrderGuid == ivm.NewOrder.OrderGUID && x.DelInd == false)){
                    var orderPi = context.tblProformaInvoices.Where(x => x.OrderGuid == ivm.NewOrder.OrderGUID && x.DelInd == false).FirstOrDefault();
                    var lastPi = context.tblProformaInvoices.Where(x => x.DelInd == false).OrderByDescending(x => x.ProformaInvoiceId).Select(x => x.ProformaInvoiceNo).FirstOrDefault();
                    
                    if (orderPi.ProformaInvoiceNo == lastPi)
                    {
                        orderPi.DelInd = true;
                        orderPi.DateEdited = DateTime.Now;
                        orderPi.EditedBy = userInfo.UserId;
                    }
                    context.tblProformaInvoices.Attach(orderPi);
                    context.Entry(orderPi).State = EntityState.Modified;
                    context.SaveChanges();

                }
            }
            if (chalanId > 0)
            {
                OrderDto order = new OrderDto();
                order.OrderGUID = ivm.NewOrder.OrderGUID;
                order.CustomerId = ivm.CustomerId;
                order.ChallanId = chalanId;
                //order.OrderDate = Convert.ToDateTime(ivm.NewOrder.OrderDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                order.PartName = ivm.NewOrder.PartName;
                order.SerialNo = ivm.NewOrder.SerialNo;
                order.PartNo = ivm.NewOrder.PartNo;
                order.RepairType = ivm.NewOrder.RepairType;
                
                if (ivm.NewOrder.AssignedUserId != null && ivm.NewOrder.AssignedUserId.ToString() != "")
                    order.AssignedUserId = ivm.NewOrder.AssignedUserId;
                else
                    order.AssignedUserId = 0;
                if (ivm.NewOrder.Remarks != null && ivm.NewOrder.Remarks.ToString() != "")
                {
                    order.Remarks = ivm.NewOrder.Remarks;
                }
                _orderHelper.UpdateOrderDetails(order);
                _invoiceHelper.UpdateInvoice(order);
            }
            return Json("Edit");
        }


        [HttpPost]
        public ActionResult AddRow(InwardMaterialViewModel InwardMaterialViewModel)
        {
            bool loanedPart = _materialHelper.CheckLoanReturn(InwardMaterialViewModel.NewOrder);
            if (loanedPart && InwardMaterialViewModel.NewOrder.RepairType != ConstantsHelper.RepairType.ReturnLoan.ToString())
            {
                return Json("failure");
            }
            else
            {
                InwardMaterialViewModel.Orders = InwardMaterialViewModel.Orders;
                OrderDto newOrder = InwardMaterialViewModel.NewOrder;
                OrderDto delOrder = new OrderDto();
                InwardMaterialViewModel.LoanItem = false;
                InwardMaterialViewModel.Parts = _materialHelper.GetBoardsList();
                InwardMaterialViewModel.Users = _materialHelper.GetUsersList();
                InwardMaterialViewModel.SavedOrders = (List<OrderDto>)Session["SavedOrders"];
                if (InwardMaterialViewModel.SavedOrders != null && InwardMaterialViewModel.SavedOrders.Count > 0)
                    InwardMaterialViewModel.ExistingOrder = true;
                foreach (var item in InwardMaterialViewModel.Orders)
                {
                    item.UserName = InwardMaterialViewModel.Users.Where(x => x.UserId == item.AssignedUserId).Select(x => x.UserName).FirstOrDefault();
                }

              
                if (InwardMaterialViewModel.NewOrder.ChallanId > 0)
                    return PartialView("_InwardMaterialDetails", InwardMaterialViewModel);
                else
                    return PartialView("_AddNewTable", InwardMaterialViewModel);
                //}
            }

        }

        [HttpPost]
        //public ActionResult AddRow(InwardMaterialViewModel vm, List<OrderDto> NewOrder)
        public ActionResult DeletePart(InwardMaterialViewModel InwardMaterialViewModel)
        {
            var itema = InwardMaterialViewModel.Orders.SingleOrDefault(x => x.SerialNo == InwardMaterialViewModel.DelSerialNo
                                                                            && x.PartNo == InwardMaterialViewModel.DelPartNo);
            if (itema != null)
                InwardMaterialViewModel.Orders.Remove(itema);
            InwardMaterialViewModel.Orders = InwardMaterialViewModel.Orders;
            InwardMaterialViewModel.Parts = _materialHelper.GetBoardsList();
            InwardMaterialViewModel.Users = _materialHelper.GetUsersList();

            foreach (var item in InwardMaterialViewModel.Orders)
            {
                item.UserName = InwardMaterialViewModel.Users.Where(x => x.UserId == item.AssignedUserId).Select(x => x.UserName).FirstOrDefault();
            }
            return PartialView("_AddNewTable", InwardMaterialViewModel);
        }
        //public ActionResult submitData(InwardMaterialViewModel vm , HttpPostedFileBase files)
        //public ActionResult submitData(InwardMaterialViewModel vm)
        [HttpPost]
        public ActionResult SubmitData()
        {
            var userInfo = Session["UserInfo"] as UserSession;
            PicannolEntities context = new PicannolEntities();
            string imagePath = "";
            HttpBrowserCapabilities browser = new HttpBrowserCapabilities();
            var Ip = Request.UserHostAddress;

            var extractBrowser = Request.Browser;
            var Browser = extractBrowser.Browser;
            
          /*  tblAudit RecordData = new tblAudit();
            RecordData.ActionName = "Add inward material";
            RecordData.TableName = "TblOrder and TblChallan";
            RecordData.UserId = userInfo.UserId;
            RecordData.Browser = Browser;
            RecordData.DateCreated = DateTime.Now;
            RecordData.IPAddress = Ip;
            context.tblAudits.Add(RecordData);
            context.SaveChanges();*/

            imagePath = ConfigurationManager.AppSettings["ImagePath"] + "InwardChallan/";
            var pic = System.Web.HttpContext.Current.Request.Files["imageUploadForm"];
            if (pic != null)
            {
                var filePath = Server.MapPath("~/Content/Images/InwardChallan/" + pic.FileName);
                pic.SaveAs(filePath);
            }
            var id = System.Web.HttpContext.Current.Request.Params["InvoiceViewModel"];
            var customerId = System.Web.HttpContext.Current.Request.Params["InvoiceViewModel"];
            JArray obj = JArray.Parse(id);
            int cId = Convert.ToInt32(System.Web.HttpContext.Current.Request.Params["ChallanId"]);
            int chalanId = 0;
            ChallanDto ch = new ChallanDto();
            ch.CustomerId = Convert.ToInt32(System.Web.HttpContext.Current.Request.Params["CustomerId"]);
            if (System.Web.HttpContext.Current.Request.Params["ChallanDate"] != "")
                ch.ChallanDate = Convert.ToDateTime(System.Web.HttpContext.Current.Request.Params["ChallanDate"], System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

            ch.ChallanNumber = System.Web.HttpContext.Current.Request.Params["CustomerRef"];
            if (cId > 0)
                chalanId = cId;
            else
            {
                chalanId = _materialHelper.InsertChallanDetail(ch);
            }
            if (chalanId > 0)
            {
                var CustomerData = _customerHelper.GetCustomerDetails(ch.CustomerId);
                foreach (var item in obj)
                {
                    OrderDto order = new OrderDto();
                    order.OrderDate = Convert.ToDateTime(System.Web.HttpContext.Current.Request.Params["DateCreated"], System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                    order.PartName = (string)item["PartName"];
                    order.SerialNo = (string)item["SerialNo"];
                    order.PartNo = (string)item["PartNo"];
                    order.RepairType = (string)item["RepairType"];
                    if (item["AssignedUserId"] != null && item["AssignedUserId"].ToString() != "")
                        order.AssignedUserId = (int)item["AssignedUserId"];
                    else
                        order.AssignedUserId = 0;
                    if (item["Remarks"] != null && item["Remarks"].ToString() != "")
                    {
                        order.Remarks = (string)item["Remarks"];
                    }
                    _materialHelper.InsertOrder(order, ch.CustomerId, chalanId);
                }
            }
            return Json("Success");
        }

        [HttpPost]
        public ActionResult AddNew(InwardMaterialViewModel vm, OrderDto NewOrder)
        {

            ChallanDto ch = new ChallanDto();
            ch.CustomerId = vm.CustomerId;
            ch.ChallanDate = vm.ChallanDate;
            ch.ChallanNumber = vm.CustomerRef;
            int chalanId = _materialHelper.InsertChallanDetail(ch);
            if (chalanId > 0)
            {
                //_materialHelper.InsertOrder(NewOrder, vm.CustomerId, chalanId,(int)vm.NewOrder.Qty);
                _materialHelper.InsertOrder(NewOrder, vm.CustomerId, chalanId);
            }
            vm.Orders = _materialHelper.GetOrdersListByChallan(chalanId);
            //vm.Parts = _materialHelper.GetCombinedPartsList(1);
            vm.Parts = _materialHelper.GetBoardsList();
            vm.Users = _materialHelper.GetUsersList();
            return PartialView("_AddNewTable", vm);
        }

        public ActionResult GetPartNo(string mainselection)
        {

            string PartNumber = _materialHelper.GetPartNumber(mainselection);
            //return View(PartNumber);
            return Json(PartNumber);
        }

        [HttpPost]
        public ActionResult DeleteOrder(InwardMaterialViewModel vm, OrderDto NewOrder)
        {
            ChallanDto ch = new ChallanDto();
            ch.CustomerId = vm.CustomerId;
            ch.ChallanDate = vm.ChallanDate;
            ch.ChallanNumber = vm.CustomerRef;
            int chalanId = _materialHelper.InsertChallanDetail(ch);
            if (chalanId > 0)
            {
                //foreach (var item in vm.Orders)
                //{
                _materialHelper.DeleteOrder(NewOrder);
                //}
            }
            vm.Orders = _materialHelper.GetOrdersListByChallan(chalanId);
            vm.Parts = _materialHelper.GetCombinedPartsList(1);
            vm.Users = _materialHelper.GetUsersList();
            return PartialView("_AddNewTable", vm);
        }

        public ActionResult InwardMaterialList()

        {
            //InwardMaterialViewModel vm = new InwardMaterialViewModel();
            //vm.InwardMaterialList = _materialHelper.GetInwardMaterialList();
            //vm.InwardMaterialList = vm.InwardMaterialList.Take(10).ToList();
            return View();

        }


        [HttpPost]
        public ActionResult GetFilteredInwardMaterialList(InwardMaterialViewModel vm)
        {
            var arr = vm.fdate.Split('/');
            vm.fdate = arr[2] + "-" + arr[1] + "-" + arr[0];
            var arr1 = vm.tdate.Split('/');
            vm.tdate = arr1[2] + "-" + arr1[1] + "-" + arr1[0];
            vm.FromDate = Convert.ToDateTime(vm.fdate);
            vm.ToDate = Convert.ToDateTime(vm.tdate);
            vm.CurrentPage =vm.PageNo ;
           // vm.InwardMaterialList = _materialHelper.GetInwardMaterialList();
            vm.InwardMaterialList = _materialHelper.GetInwardMaterialListV1(vm);
            //vm.InwardMaterialList = vm.InwardMaterialList.Where(x => x.DateCreated >= vm.FromDate && x.DateCreated <= vm.ToDate).ToList();

            Session["InwardMaterialList"] = vm.InwardMaterialList;
            vm.CountList = vm.InwardMaterialList.Count;
            return PartialView("_InwardMaterialList", vm);
        }

        public ActionResult InwardMaterialDetails(int challanId)

        {
            InwardMaterialViewModel vm = new InwardMaterialViewModel();
            vm.SavedOrders = _materialHelper.GetOrdersListByChallan(challanId);
            Session["SavedOrders"] = vm.SavedOrders;
            vm.InwardMaterialList = (List<ChallanDto>)Session["InwardMaterialList"];
            vm.CustomerDetails = vm.InwardMaterialList.Where(x => x.ChallanId == challanId).FirstOrDefault();
            vm.Parts = _materialHelper.GetCombinedPartsList(1);
            vm.Users = _materialHelper.GetUsersList();
            vm.ExistingOrder = true;
            return View(vm);
        }
    }
}