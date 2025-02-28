using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using Picanol.ViewModels;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.DataModel;

namespace Picanol.Controllers
{
    [RedirectingAction]
    public class OrderController : BaseController
    {
        private readonly OrderHelper _orderHelper;
        private readonly CustomerHelper _customerHelper;
        private readonly MaterialHelper _partsHelper;
        private readonly BusinessHelper _businessHelper;
        private readonly UserHelper _userHelper;
        private readonly InvoiceHelper _invoiceHelper;

        public int GUID { get; private set; }

        public OrderController()
        {
            //CheckSession();
            _orderHelper = new OrderHelper(this);
            _customerHelper = new CustomerHelper(this);
            _partsHelper = new MaterialHelper(this);
            _businessHelper = new BusinessHelper(this);
            _userHelper = new UserHelper(this);
            _invoiceHelper = new InvoiceHelper(this);
        }

      



        #region Order Management
        [HttpGet]
        public ActionResult OrdersList()
        //public ActionResult OrdersList(int PageSize, int PageNo)
        {
            OrdersViewModel vm = new OrdersViewModel();
            //CustomerDto customer = new CustomerDto();
            //customer.CustomerName = "All";
           // customer.CustomerId = 0;
            //vm.Customers.Add(customer);
            List<CustomerDto> customers = _customerHelper.GetCustomersList();
            foreach (var item in customers)
            {
                vm.Customers.Add(item);
            }
            vm.assignedUsersList = _userHelper.GetAssignedUsersList();
            OrdersViewModel q = new OrdersViewModel();
            q = Session["TrackingNo"] as OrdersViewModel;
            if (q != null)
            {
                vm.TrackingNo = q.TrackingNo;
            }
            vm.LastInvoiceNumber = _invoiceHelper.GetLastInvoiceNo(ConstantsHelper.InvoiceType.RP.ToString());
            return View(vm);
        }

        [HttpPost]
        public ActionResult GetFilteredOrdersList(OrdersViewModel ovm)
        {
            Session["TrackingNo"] = ovm;
            OrdersViewModel vm = new OrdersViewModel();
            //vm.CurrentPageNo = PageNo;
            vm.ordersList = _orderHelper.GetFilteredOrders(ovm);
            var userInfo = Session["UserInfo"] as UserSession;
            if (userInfo.RoleId == 8)
            {
               vm.ActionList = vm.ActionList.Where(x => x.Text == "PrintOriginalInvoice" ||x.Text== "DispatchDetails");
            }
            Session["FilteredOrdersList"] = vm.ordersList;
            return PartialView("_AllOrders", vm);

        }

        //new Get method for Pagination //

        [HttpPost]
        public PartialViewResult GetFilteredOrdersListVersion1(OrdersViewModel ovm)
            {
            Session["TrackingNo"] = ovm;
            OrdersViewModel vm = new OrdersViewModel();
           
            vm.CurrentPageNo = ovm.PageNo;
              //Add Paze size here remove it from params.
            vm.ordersList = _orderHelper.GetFilteredOrders(ovm);
            var userInfo = Session["UserInfo"] as UserSession;
            if (userInfo.RoleId == 8)
            {
                vm.ActionList = vm.ActionList.Where(x => x.Text == "PrintOriginalInvoice"  || x.Text == "DispatchDetails");
            }
            Session["FilteredOrdersList"] = vm.ordersList;
            vm.NumberOfRecord = vm.ordersList.Count;
            ModelState.Clear();
            
            return PartialView("_AllOrders", vm);

        }
        //End Here//

        //[HttpPost]
        //public ActionResult GetAllOrdersList(int CustomerId)
        //{
        //    OrdersViewModel vm = new OrdersViewModel();
        //    vm.ordersList = _orderHelper.GetAllOrders(CustomerId);
        //    return PartialView("_AllOrders", vm);

        //}

        [HttpGet]
        public ActionResult OrderDetails(Guid id)
        {
            OrderDetailsViewModel vm = new OrderDetailsViewModel();
            vm.OrderDetails = _orderHelper.GetOrderDetails(id);
            //vm.Parts = _partsHelper.GetPartsList(2);
            vm.Parts = _partsHelper.GetCombinedPartsList(2);
            vm.Users = _partsHelper.GetUsersList();
            vm.ConsumedParts = _orderHelper.GetConsumedParts(id);
            vm.SelectedOrderGuid = id;
            return View(vm);
        }
        public ActionResult getDetailView(OrderDetailsViewModel OrderDetailsViewModel)
        {
            OrderDetailsViewModel.ConsumedParts = _orderHelper.GetConsumedParts(OrderDetailsViewModel.SelectedOrderGuid);
            OrderDetailsViewModel.OrderDetails.Status = ConstantsHelper.OrderStatus.Open.ToString();
            OrderDetailsViewModel.Parts = _partsHelper.GetCombinedPartsList(2);
            return PartialView("_AllOrderParts", OrderDetailsViewModel);
        }
        public ActionResult Submitbtn(OrderDetailsViewModel vm2)
        {
            var userInfo = Session["UserInfo"] as UserSession;
            if (vm2.SelectedOrderGuid != null)
            {
                foreach (var item in vm2.ConsumedParts)
                {
                    _orderHelper.InsertOrderPart(vm2.SelectedOrderGuid, item.PartNumber, item.Qty , vm2.OrderDetails.Status, userInfo.UserId);
                }
            }
            vm2.ConsumedParts = _orderHelper.GetConsumedParts(vm2.SelectedOrderGuid);
            OrderDto order = new OrderDto();
            order.OrderGUID = vm2.OrderDetails.OrderGUID;
            order.Status = ConstantsHelper.OrderStatus.InProgress.ToString();
            order.TimeTaken = vm2.OrderDetails.TimeTaken;
            order.Remarks = vm2.OrderDetails.Remarks;
            order.PackingType = vm2.OrderDetails.PackingType;
            order.Status = vm2.OrderDetails.Status;
            order.RepairType = vm2.OrderDetails.RepairType;
            order.AssignedUserId = vm2.OrderDetails.AssignedUserId;
            order.ExemptComponentCost = vm2.OrderDetails.ExemptComponentCost;
            order.ExemptLabourCost = vm2.OrderDetails.ExemptLabourCost;
            order.Discount = vm2.OrderDetails.Discount;
            order.Editby = userInfo.UserId;
            order.EditDate = DateTime.Now;
            if (vm2.OrderDetails.URDCheck == 1)
                order.RepairType = ConstantsHelper.RepairType.UnRepairedBoards.ToString();
            else
                order.RepairType = vm2.OrderDetails.RepairType;
            _orderHelper.UpdateOrder(order);


            //Edit packing type after PI generation
            PicannolEntities context = new PicannolEntities();
            var pi = context.tblProformaInvoices.Where(x => x.OrderGuid == order.OrderGUID && x.DelInd == false).FirstOrDefault();
            if (pi != null)
            {
                var customer = context.tblCustomers.Where(x => x.CustomerId == pi.CustomerId && x.Delind == false && x.Active == false).FirstOrDefault();
                var eInvoice = context.tblEInvoices.Where(x => x.OrderID == order.OrderGUID && x.DelInd == false && x.Cancelled == false).FirstOrDefault();
                if (pi != null && eInvoice == null && customer != null)
                {
                    if (order.RepairType == ConstantsHelper.RepairType.Chargeable.ToString() ||
                         order.RepairType == ConstantsHelper.RepairType.NoRepairWarranty.ToString())
                    {
                        if (order.PackingType == ConstantsHelper.PackingTypes.Small.ToString())
                        {
                            pi.Packing = (decimal)(customer.SmallPacking);
                            pi.Forwading = (decimal)(customer.SmallForwarding);

                        }
                        else if (order.PackingType == ConstantsHelper.PackingTypes.Big.ToString())
                        {
                            pi.Packing = (decimal)(customer.BigPacking);
                            pi.Forwading = (decimal)(customer.BigForwarding);
                        }

                        pi.PackingType = order.PackingType;
                        pi.EditedBy = userInfo.UserId;
                        pi.DateEdited = DateTime.Now;
                        context.tblProformaInvoices.Attach(pi);
                        context.Entry(pi).State = EntityState.Modified;
                        context.SaveChanges();

                        #region record user Activity while PI editing
                        string actionName = $"P.I. Packing Type Edited - {pi.OrderGuid}" +
                            $", P.I. - {pi.ProformaInvoiceNo}" +
                            $", Packing Type - {order.PackingType}" +
                            $", AssignedUser - {order.AssignedUserId}" +
                            $",CustomerID - {pi.CustomerId}";
                        string tblName = "tblOrder, tblProformaInvoice";
                        if (actionName != null)
                        {
                            _userHelper.recordUserActivityHistory(userInfo.UserId, actionName, tblName);
                        }
                        #endregion
                    }
                }


            }
            

            //End

            #region record user Activity
            string ActionName = $"Submit order detail OrderGUID - {order.OrderGUID}" +
                $",Staus - {order.Status}" + $",ExemptComponentCost - {order.ExemptComponentCost}" +
                 $",TimeTaken - {order.TimeTaken}" + 
                $",RepairType - {order.RepairType}"+
                $",AssignedUser - {order.AssignedUserId}"+
                $",CustomerID - {order.CustomerId}";
            string TableName = "tblOrder";
            if (ActionName != null)
            {
                _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
            }
            #endregion

            vm2.OrderDetails = new OrderDto();
            vm2.OrderDetails.Status = ConstantsHelper.OrderStatus.InProgress.ToString();
            return PartialView("_AllOrderParts", vm2);
        }

        [HttpPost]
        public ActionResult DeletePart(OrderDetailsViewModel vm)
        {
            var userInfo = Session["UserInfo"] as UserSession;
            if (vm.PartId > 0)
            {
                int row = _orderHelper.DeleteOrderPart(vm.SelectedOrderGuid, vm.PartId, userInfo.UserId);
            }
            else
            {
                var itema = vm.NewConsumedParts.SingleOrDefault(x => x.PartName == vm.orderPart.PartName);
                if (itema != null)
                    vm.NewConsumedParts.Remove(itema);
            }
            vm.Parts = _partsHelper.GetCombinedPartsList(2);
            vm.ConsumedParts = _orderHelper.GetConsumedParts(vm.SelectedOrderGuid);
            return PartialView("_AllOrderParts", vm);
        }


        #endregion

        #region Action Methods


        public ActionResult SendOrderEmail(OrdersViewModel vm)
        {
            var userInfo = Session["UserInfo"] as UserSession;

            InvoiceDto invoice = _invoiceHelper.GetInvoiceDetailsbyOrderId(vm.SelectedOrderId);

            string[] strArray = invoice.InvoiceFileName.Split(new string[] { ".pdf" }, StringSplitOptions.None);
            string FileName = "";
            //string FileDuplicate = "";
            //string FileTriplicate = "";

            FileName = strArray[0] + strArray[1] + "_Original.pdf";
            invoice.InvoiceFileName = FileName;
            OrderDto order = _orderHelper.GetOrderDetails(vm.SelectedOrderId);
            string response = "";
            if (invoice != null)
            {
                if (invoice.InvoiceFileName != null)
                {
                    try
                    {
                        _customerHelper.SaveCustomerEmail(order.CustomerId, vm.Email);
                        string cc = _userHelper.CC();
                        response = _invoiceHelper.EmailInvoiceToCustomer(invoice, order, vm.Email, cc);
                        if (response == "success")
                        {
                            string ActionName = $"Send Email To - {vm.Email}";
                            string TableName = "TblCustomer";
                            if (ActionName != null)
                            {
                                _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                            }
                        }
                        //if(response == "success")
                        //{
                        //    var root = Server.MapPath("~/Content/PDF/Invoices/"); 
                        //    DirectoryInfo di = new DirectoryInfo(root);

                        //    foreach (FileInfo file in di.GetFiles())
                        //    {
                        //        file.Delete();
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        response = ex.Message;
                        throw;
                    }

                }
            }
            else
                response = "Invoice not generated. Please generate e-invoice first";
            return Json(response);
        }

        public ActionResult DispatchDetails(Guid orderId)
        {
            OrdersViewModel vm = new OrdersViewModel();
            vm.DispatchDetails = new DispatchDetailsDto();
            vm.DispatchDetails = _orderHelper.GetDispatchDetailsByOrderId(orderId);
            vm.DispatchDetails.Ddate = vm.DispatchDetails.DispatchDate.Value.ToShortDateString();


            return Json(vm);
        }


        public ActionResult EditOrder(OrderDto orderDetails)
        // public ActionResult EditOrder(long trackingNumber)
        {
            List<OrderDto> filteredOrders = (List<OrderDto>)Session["FileteredOrdersList"];
            OrderDto editOrder = new OrderDto();
            //string response = "failure";
            //foreach (var item in filteredOrders)
            //{
            //    if (item.TrackingNumber == trackingNumber)
            //    {
            //        editOrder = item;
            //        Session["EditOrderDetails"] = editOrder;
            //        response = "success";
            //    }
            //}

            var userInfo = Session["UserInfo"] as UserSession;
            string response = _orderHelper.EditOrder(orderDetails, userInfo);
            return Json(response);
        }

        [HttpPost]
        public ActionResult SaveDispatchDetails(OrdersViewModel vm)
        {
            int i = _invoiceHelper.RecordDispatchDetails(vm);
            if (i > 0)
            {
                  string ActionName = $"RecordDispatchDetails(TrackingNumber) - {vm.DispatchDetails.TrackingNumber}";
                    string TableName = "TblOrder and tblDispatchDetails";
                    if (ActionName != null)
                    {
                        var userInfo = (UserSession)Session["UserInfo"];
                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                
                return Json("success");

            }
            else
                return Json("failure");
        }

        #endregion

        #region Unused Methods
        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult AddPaymentInformation(InvoiceListViewModel ap, List<HttpPostedFileBase> images)
        //{
        //    InvoiceListViewModel Iap = new InvoiceListViewModel();
        //    return View(Iap);

        //}
        //public ActionResult ListInvoicesImage(InvoiceViewModel lm, List<HttpPostedFileBase> images)
        //{
        //	InvoiceViewModel lv = new InvoiceViewModel();
        //	return View(lv);
        //}
        //public ActionResult DispatchInvoicesImage(InvoiceViewModel di, List<HttpPostedFileBase> images)
        //{
        //    InvoiceViewModel dv = new InvoiceViewModel();
        //    return View(dv);

        //}
        //public ActionResult getDetailView(OrderDetailsViewModel vm1, List<OrderPartDto> NewOrder, Guid OrderId)
        //[HttpPost]
        //public ActionResult InsertOrderPart(OrderDetailsViewModel vm)
        //{
        //    _orderHelper.InsertOrderPart(vm.SelectedOrderGuid, vm.PartNo, vm.Quantity);
        //    return Json(vm.SelectedOrderGuid); ;
        //}
        //public InvoiceViewModel CalculateAmounts()
        //{
        //    InvoiceViewModel vm = new InvoiceViewModel();

        //    return vm;
        //}
        //public ActionResult FinalInvoice(Guid OrderId, string rate1, string rate2, string amount1, string amount2)
        //{
        //    return View();
        //}

        #endregion

    }
}