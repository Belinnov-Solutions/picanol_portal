using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class OrderHelper
    {
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        private CustomerService _customerService;
        UserSession userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
        protected CustomerService CustomerService
        {
            get
            {
                if (_customerService == null) _customerService = new CustomerService(entities, validationDictionary);
                return _customerService;
            }
        }



        private PartsService _partsService;
        protected PartsService PartsService
        {
            get
            {
                if (_partsService == null) _partsService = new PartsService(entities, validationDictionary);
                return _partsService;
            }
        }

        private UserService _userService;
        protected UserService UserService
        {
            get
            {
                if (_userService == null) _userService = new UserService(entities, validationDictionary);
                return _userService;
            }
        }
        private ChallanService _challanService;
        protected ChallanService ChalanService
        {
            get
            {
                if (_challanService == null) _challanService = new ChallanService(entities, validationDictionary);
                return _challanService;
            }
        }
        private OrderService _orderService;
        protected OrderService OrderService
        {
            get
            {
                if (_orderService == null) _orderService = new OrderService(entities, validationDictionary);
                return _orderService;
            }
        }
        private OrderPartService _orderPartService;
        private InvoiceHelper invoiceHelper;

        protected OrderPartService OrderPartService
        {
            get
            {
                if (_orderPartService == null) _orderPartService = new OrderPartService(entities, validationDictionary);
                return _orderPartService;
            }
        }
        protected iValidation validationDictionary { get; set; }



        #endregion

        #region Ctor
        public OrderHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }

        public OrderHelper()
        {

        }

        public OrderHelper(InvoiceHelper invoiceHelper)
        {
            this.invoiceHelper = invoiceHelper;
        }

        #endregion

        #region Base Methods
        public List<OrderDto> GetAllOrders(int id)
        {
            return OrderService.GetOrdersList(id);
        }
        public List<OrderDto> GetFilteredOrders(OrdersViewModel ovm)
        {
            return OrderService.GetFilteredOrdersList(ovm);
        }

        
        public List<OrderPartDto> GetConsumedParts(Guid orderId)
        {
            return OrderService.GetConsumedParts(orderId);
        }

        public OrderDto GetOrderDetails(Guid orderId)
        {
            return OrderService.GetOrderDetails(orderId);

        }
        

        public DispatchDetailsDto GetDispatchDetailsByOrderId(Guid orderId)
        {
            return OrderService.GetDispatchDetailsByOrderId(orderId);
        }

        public long GetLastTrackingNumber()
        {
            PicannolEntities context = new PicannolEntities();
            long t = 0;
            if (context.tblOrders.Any())
            {
                //t = context.tblOrders.Where(x => x.DelInd == false).Select(x => x.TrackingNumber).Max();
                //t = context.tblOrders.Where(x => x.DelInd == false).OrderByDescending(x=>x.DateCreated).Select(x => x.TrackingNumber).FirstOrDefault();
                t = context.tblOrders.Where(x => x.DelInd == false).OrderByDescending(x => x.DateCreated).
                    OrderByDescending(x => x.TrackingNumber).Select(x => x.TrackingNumber).FirstOrDefault();
            }
            return t;
        }
        public void UpdateOrderStatus(Guid orderId , string status)
        {
            PicannolEntities context = new PicannolEntities();
            tblOrder or = new tblOrder();
            or = context.tblOrders.Where(x => x.OrderGUID == orderId && x.DelInd == false).FirstOrDefault();
            {
                or.Status = status;
                context.tblOrders.Attach(or);
                context.Entry(or).Property(x => x.Status).IsModified = true;
                context.SaveChanges();
            }
        }

        public void UpdateOrder(OrderDto order)
        {
            tblOrder to = new tblOrder();
            to.OrderGUID = order.OrderGUID;
            to.PackingType = order.PackingType;
            to.RepairTime = order.TimeTaken;
            to.Remarks = order.Remarks;
            to.Status = order.Status;
            to.LastModified = DateTime.Now;
            to.RepairType = order.RepairType;
            to.AssignedUserId = order.AssignedUserId;
            to.ExemptComponentCost = order.ExemptComponentCost;
            to.ExemptLabourCost = order.ExemptLabourCost;
            to.Discount = order.Discount;
            to.EditedBy = order.Editby;
            to.DateEdited = order.EditDate;
            OrderService.Update(to);
        }

        public string EditOrder(OrderDto order, UserSession user)
        {
            PicannolEntities context = new PicannolEntities();
            var o = context.tblOrders.Where(x => x.TrackingNumber == order.TrackingNumber).FirstOrDefault();
            var p = context.tblParts.Where(x => x.PartId == o.PartId).FirstOrDefault();
            try
            {
                o.EditedBy = user.UserId;
                o.DateEdited = DateTime.Now;
                p.SerialNo = order.SerialNo;
                context.Entry(p).Property(x => x.SerialNo).IsModified = true;
                context.SaveChanges();
                o.RepairType = order.RepairType;
                context.Entry(o).Property(x => x.RepairType).IsModified = true;
                context.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }
            
        }
        public void InsertOrderPart(Guid orderId, string partNo, int qty , string status, int userId)
        {
            try
            {
                PartDto pt = PartsService.GetPartDetails(partNo);
                PicannolEntities context = new PicannolEntities();
                tblOrderPart top = new tblOrderPart();
                if (context.tblOrderParts.Any(x => x.PartId == pt.PartId && x.OrderGUID == orderId && x.DelInd == false))
                {
                    top = context.tblOrderParts.Where(x => x.PartId == pt.PartId && x.OrderGUID == orderId && x.DelInd == false).FirstOrDefault();
                    if (top.Qty != qty)
                    {
                        top.Qty = top.Qty + qty;
                        top.Price = context.tblParts.Where(x => x.PartId == top.PartId).Select(x => x.Price).FirstOrDefault();
                        context.tblOrderParts.Attach(top);
                        context.Entry(top).Property(x => x.Qty).IsModified = true;
                        context.Entry(top).Property(x => x.Price).IsModified = true;
                        context.SaveChanges();
                    }
                }
                else
                {
                    top.OrderGUID = orderId;
                    top.PartId = pt.PartId;
                    top.Qty = qty;
                    top.UserId = userId;
                    top.Deletedby = 0;
                    top.LastModified = DateTime.Now;
                    top.DateCreated = DateTime.Now;
                    top.Price = context.tblParts.Where(x => x.PartId == pt.PartId).Select(x => x.Price).FirstOrDefault();
                    OrderPartService.AddOrderPart(top);
                }
                //if (status == ConstantsHelper.OrderStatus.Completed.ToString())
                //{
                tblPart p = new tblPart();
                p.PartId = pt.PartId;
                p.Stock = qty;
                PartsService.SavePart(p);
                // }
            }
            catch (Exception ex)
            {

            }
            
        }

        public int DeleteOrderPart(Guid orderId, int partId, int userId)
        {
            using (PicannolEntities ctx = new PicannolEntities())
            {
                var query = (from q in ctx.tblOrderParts
                             where q.OrderPartId == partId
                             select q).First();
                query.Deletedby = userId;
                query.DeletedDate = DateTime.Now;
                query.LastModified = DateTime.Now;
                query.DelInd = true;
                int result = ctx.SaveChanges();
                return result;
            }
        }

        public decimal GetComponentCost(Guid orderId)
        {
            List<OrderPartDto> consumedParts = OrderService.GetConsumedParts(orderId);
            decimal partsCost = 0;
            foreach (var item in consumedParts)
            {
                partsCost += (decimal)(item.Qty * item.Price);
            }
            partsCost += 2 * partsCost;
            if (partsCost < 3000)
            {
                partsCost = 3000;
            }
            return partsCost;
        }

        public decimal CalculateRepairCharges(Guid orderId, int custId, int time)
        {
            decimal repairCharges = 0;
            decimal repairRate = CustomerService.GetRepairRate(custId);
            //decimal repairTime = OrderService.GetRepairTime(orderId);
            repairCharges = repairRate * time;
            return repairCharges;
        }

        public string ReturnLoan(Guid orderId)
        {
            string response = "";
            PicannolEntities context = new PicannolEntities();
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            if (context.tblPartLoans.Any(x => x.OrderGUID == orderId && x.DelInd == false && x.Returned == false))
            {

                var pl = context.tblPartLoans.Where(x => x.OrderGUID == orderId && x.DelInd == false && x.Returned == false).FirstOrDefault();
                var or = context.tblOrders.Where(x => x.OrderGUID == orderId && x.DelInd == false).FirstOrDefault();
                try
                {
                    pl.ReturnDate = DateTime.Now.Date;
                    pl.Returned = true;
                    pl.ReturnedBy = userInfo.UserId;
                    context.tblPartLoans.Attach(pl);
                    context.Entry(pl).State = EntityState.Modified;
                    context.SaveChanges();
                    or.Status = ConstantsHelper.OrderStatus.Completed.ToString();
                    context.tblOrders.Attach(or);
                    context.Entry(or).State =EntityState.Modified;
                    context.SaveChanges();

                    //record user activity
                    string ActionName = $"Return Recorded, OrderGUI - {orderId}";
                    string TableName = "tblOrders, tblPartLoans";
                    if (ActionName != null)
                    {
                        new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                    //End

                    response = "Return Recorded";


                }
                catch (Exception ex)
                {
                    response = ex.Message;
                    return response;
                }
            }
            else
                response = "Item doesn't exist for return";
            return response;
        }

        public void UpdateOrderDetails(OrderDto order)
        {
            PicannolEntities context = new PicannolEntities();
            PartDto part = new PartDto();
            part.PartName = order.PartName;
            part.PartNumber = order.PartNo;
            part.SerialNo = order.SerialNo;
            int partId = PartsService.GetPartId(part);
            var o = context.tblOrders.Where(x => x.OrderGUID == order.OrderGUID).FirstOrDefault();
            o.ChallanId = order.ChallanId;
            o.CustomerId = order.CustomerId;
            o.AssignedUserId = order.AssignedUserId;
            o.PartId = partId;
            o.Remarks = order.Remarks;
            o.Status = ConstantsHelper.OrderStatus.Open.ToString();
            //o.OrderDate = order.OrderDate;
            o.RepairType = order.RepairType;
            if(o.RepairType == ConstantsHelper.RepairType.Loan.ToString() && o.AssignedUserId == 0 
                && order.RepairType != ConstantsHelper.RepairType.Loan.ToString())
            {
                var pl = context.tblPartLoans.Where(x => x.OrderGUID == order.OrderGUID && x.Returned == false && x.DelInd == false).FirstOrDefault();
                pl.DelInd = true;
                context.tblPartLoans.Attach(pl);
                context.Entry(pl).State = EntityState.Modified;
                context.SaveChanges();
            }
            if(o.RepairType != ConstantsHelper.RepairType.Loan.ToString() && order.RepairType == ConstantsHelper.RepairType.Loan.ToString())
            {
                tblPartLoan pl = new tblPartLoan();
                pl.OrderGUID = order.OrderGUID;
                pl.DateCreated = DateTime.Now;
                pl.DelInd = false;
                pl.Returned = false;
                context.tblPartLoans.Add(pl);
                context.SaveChanges();
            }
            context.tblOrders.Attach(o);
            context.Entry(o).State = EntityState.Modified;
            context.SaveChanges();

            //record user activity
            string ActionName = $"UpdateOrderDetails,OrderGUI - {order.OrderGUID}" +
                $"Status - {o.Status}" + $",RepairType - {o.RepairType}"
                + $",PartNumber - {o.PartId}"
                +$",Assigned User - {order.AssignedUserId}"
                 +$",CustomerID - {order.CustomerId}";
            string TableName = "tblOrder";
            if (ActionName != null)
            {
                new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
            }
            //End
        }

        public void UpdatePartStock(List<OrderPartDto> os)
        {
            PicannolEntities context = new PicannolEntities();
            foreach (var op in os)
            {
                var partValue = context.tblParts.Where(x => x.PartId == op.PartId).FirstOrDefault();
                if (partValue != null)
                {
                    partValue.Stock = partValue.Stock - op.Qty;
                    context.tblParts.Add(partValue);
                    context.Entry(partValue).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }

        #endregion

    }
}