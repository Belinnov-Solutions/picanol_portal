using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class MaterialHelper
    {
        PicannolEntities context = new PicannolEntities();

        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        UserSession userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];


        private CustomerService _customerService;
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
        protected iValidation validationDictionary { get; set; }



        #endregion

        #region Ctor
        public MaterialHelper(Controller controller)
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
        public List<CustomerDto> GetCustomersList()
        {
            return CustomerService.GetCustomersList();
        }
        //new for customer filter //
        [OutputCache(Duration = 120)]
        public List<CustomerDto> GetCustomersListSearchVersion1(string CustomerName)
        {
            return CustomerService.GetCustomersListSearchVersion1(CustomerName);
        }


        //end here//

        public List<ProformaInvoiceDto> GetProformaList(int PageSize, int PageNo)
        {
            return CustomerService.GetProformaList(PageSize, PageNo);
        }
        public List<PartDto> GetPartsList(int type)
        {
            return PartsService.GetPartsList(type);
        }

        public List<PartDto> GetCombinedPartsList(int type)
        {
            return PartsService.GetCombinedPartsList(type);
        }

        public List<PartDto> GetBoardsList()
        {
            return PartsService.GetBoardsList();
        }
        public List<UserDto> GetUsersList()
        {
            return UserService.GetUsersList();
        }

        public int InsertChallanDetail(ChallanDto ch)
        {
            int id = ChalanService.GetChallanDetails(ch);
            int i = 0;
            if (id == 0)
            {
                tblChallan tc = new tblChallan();
                 tc.ChallanDate = ch.ChallanDate;
                tc.ChallanNumber = ch.ChallanNumber;
                tc.CustomerId = ch.CustomerId;
                tc.DateCreated = DateTime.Now;
                i = ChalanService.SaveChallan(tc);
            }
            else
                i = id;
            return i;
        }

        public string GetPartNumber(string pd)
        {
            return PartsService.GetPartNumber(pd);
        }

        public PartDto GetPartDetails(int? id)
        {
            return PartsService.GetPartDetailsById(id);
        }
        public string GetLastPartNo()
        {
            PicannolEntities context = new PicannolEntities();
            var t = context.tblParts.Where(x => x.DelInd == false && x.PartNumber.Contains("R") && x.PartTypeId ==2)
                .OrderByDescending(x => x.PartId).Select(a => a.PartNumber).FirstOrDefault();
            //string i = t.Select(x => x.PartNumber).FirstOrDefault();
            var y = context.tblParts.Where(a => a.DelInd == false && a.PartNumber.Contains("E") && a.PartTypeId == 2).OrderByDescending(a => a.PartId).Select(a => a.PartNumber).FirstOrDefault();
            //string j = t.Select(x => x.PartNumber).FirstOrDefault();
            string z = t + ";" + y;
            return z;
        }

        public string InsertPart(PartsViewModel pv)
        {
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            pv.SelectedPart.PurchaseDate = DateTime.Now.Date;
            PicannolEntities context = new PicannolEntities();
            tblPart part = new tblPart();
            part.PartName = (pv.SelectedPart.PartName != null) ? pv.SelectedPart.PartName : "";
            part.PartNumber = (pv.SelectedPart.PartNumber != null) ? pv.SelectedPart.PartNumber : "";
            part.SerialNo = (pv.SelectedPart.SerialNo != null) ? pv.SelectedPart.SerialNo : "";
            part.Stock = pv.SelectedPart.PartTypeId == 1 ? 1 : pv.SelectedPart.PartTypeId;
            part.Price = pv.SelectedPart.Price;
            part.PartTypeId = pv.SelectedPart.PartTypeId;
            part.DelInd = false;
            part.OpeningStock = pv.SelectedPart.Stock;
            part.OpeningStockDate = DateTime.Now;
            //PartsService.SavePart(P);
            context.tblParts.Add(part);
            context.SaveChanges();
            int PartId = part.PartId;
            
            if (pv.SelectedPart.PartTypeId != 1)
            {
                tblPurchase purchase = new tblPurchase();
                purchase.PartId = PartId;
                purchase.Quantity = (int)pv.SelectedPart.Stock;
                purchase.PurchaseDate = (pv.SelectedPart.PurchaseDate).Date;
                //pr.PurchaseDate = DateTime.Now.Date;
                purchase.DateCreated = DateTime.Now;
                purchase.UserId = userInfo.UserId;
                purchase.Remarks = pv.SelectedPart.Remarks;
                purchase.Price = pv.SelectedPart.Price;
                purchase.DelInd = false;
                context.tblPurchases.Add(purchase);
                context.SaveChanges();
            }
            return "success";
        }

        //Coment by Sunit - 16032024

        /*public string UpdatePart(PartsViewModel pv)
        {
            pv.SelectedPart.PurchaseDate = DateTime.Now.Date;
            PicannolEntities context = new PicannolEntities();
            var P = context.tblParts.Where(x => x.PartId == pv.SelectedPart.PartId && x.DelInd == false).FirstOrDefault();
            P.PartId = pv.SelectedPart.PartId;
            if (pv.SelectedPart.PartName == null)
            {
                P.PartName = "";
            }
            else
            {
                P.PartName = pv.SelectedPart.PartName;
            }
            if (pv.SelectedPart.PartNumber == null)
            {
                P.PartNumber = "";
            }
            else
            {
                P.PartNumber = pv.SelectedPart.PartNumber;
            }

            if (pv.SelectedPart.SerialNo == null)
            {
                P.SerialNo = "";
            }
            else
            {
                P.SerialNo = pv.SelectedPart.SerialNo;
            }
            P.Stock = pv.SelectedPart.Stock + pv.SelectedPart.NewStock;
            P.Price = pv.SelectedPart.Price;
            context.tblParts.Attach(P);
            context.Entry(P).State = EntityState.Modified;
            context.SaveChanges();
            if (pv.SelectedPart.NewStock > 0)
            {
                var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
                tblPurchase pr = new tblPurchase();
                pr.PartId = P.PartId;
                pr.Quantity = (int)pv.SelectedPart.NewStock;
                pr.PurchaseDate = (pv.SelectedPart.PurchaseDate).Date;
                pr.DateCreated = DateTime.Now;
                pr.UserId = userInfo.UserId;
                pr.Remarks = pv.SelectedPart.Remarks;
                pr.DelInd = false;
                context.tblPurchases.Add(pr);
                context.SaveChanges();
            }
            return "success";
        }*/

        //End

        //Calling store procedure for mentaining tblPart history
        public string UpdatePart(PartsViewModel pv)
        {
            var response = "";
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ReportsDefaultConnection"].ToString();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("usp_UpdatetblPart", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters if your stored procedure requires any
                        command.Parameters.AddWithValue("@PartId", pv.SelectedPart.PartId);
                        command.Parameters.AddWithValue("@PartName", pv.SelectedPart.PartName!=null? pv.SelectedPart.PartName:"");
                        command.Parameters.AddWithValue("@PartNumber", pv.SelectedPart.PartNumber!=null? pv.SelectedPart.PartNumber:"");
                        command.Parameters.AddWithValue("@SerialNumber", pv.SelectedPart.SerialNo!=null? pv.SelectedPart.SerialNo:"");
                        //command.Parameters.AddWithValue("@Stock", pv.SelectedPart.Stock + pv.SelectedPart.NewStock);
                        command.Parameters.AddWithValue("@NewStock", pv.SelectedPart.NewStock);
                        command.Parameters.AddWithValue("@PurchaseDate", (pv.SelectedPart.PurchaseDate).Date);
                        command.Parameters.AddWithValue("@Remarks", pv.SelectedPart.Remarks!=null? pv.SelectedPart.Remarks:"");
                        command.Parameters.AddWithValue("@UserId", userInfo.UserId);
                        command.Parameters.AddWithValue("@Price", pv.SelectedPart.Price);

                        // Add more parameters as needed
                        connection.Open();
                        command.ExecuteNonQuery();
                        response = "success";
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Error";
                //throw;
            }

            return response;
           
        }

        public string UpdatePartStock(PartsViewModel vm)
        {
            var userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];
            PicannolEntities context = new PicannolEntities();
            var P = context.tblParts.Where(x => x.PartId == vm.SelectedPart.PartId && x.DelInd == false).FirstOrDefault();
            P.Stock = P.Stock + vm.SelectedPart.NewStock;
            context.tblParts.Attach(P);
            context.Entry(P).State = EntityState.Modified;
            context.SaveChanges();
            tblPurchase pr = new tblPurchase();
            pr.PartId = vm.SelectedPart.PartId;
            pr.Quantity = vm.SelectedPart.NewStock;
            pr.PurchaseDate = DateTime.Now.Date;
            pr.DateCreated = DateTime.Now;
            pr.UserId = userInfo.UserId;
            pr.Remarks = vm.SelectedPart.Remarks;
            pr.DelInd = false;
            context.tblPurchases.Add(pr);
            context.SaveChanges();

            //record user activity
            string ActionName = $"Update Part Stock - {vm.SelectedPart.PartId}";
            string TableName = "tblParts; tblPurchases";
            if (ActionName != null)
            {
                new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
            }
            //End

            return "success";
        }


        //internal string InsertOrder(OrderDto item, int custId, int challanId,int Qty)
        internal string InsertOrder(OrderDto item, int custId, int challanId)
        {
            string response = "";
            PicannolEntities context = new PicannolEntities();
            PartDto part = new PartDto();
            part.PartName = item.PartName;
            part.PartNumber = item.PartNo;
            part.SerialNo = item.SerialNo;
            int partId = PartsService.GetPartId(part);
            string loanp = ConstantsHelper.RepairType.Loan.ToString();
            if (context.tblOrders.Any(x => x.PartId == partId && x.RepairType == loanp && x.DelInd == false))
            {
                Guid orderId = new Guid();
                orderId = context.tblOrders.Where(x => x.PartId == partId).Select(x => x.OrderGUID).FirstOrDefault();
                if (context.tblPartLoans.Any(x => x.OrderGUID == orderId && x.DelInd == false && x.Returned == false))
                {
                    if (item.RepairType == ConstantsHelper.RepairType.ReturnLoan.ToString())
                    {
                        OrderHelper oh = new OrderHelper();
                        oh.ReturnLoan(orderId);
                        response = "success";
                    }
                }
                else
                {
                    response = SaveNewOrder(item, custId, challanId, partId);
                    response = "success";
                } 
            }
            else
            {
                response = SaveNewOrder(item, custId, challanId, partId);
            }
            return response;
        }

        public List<WorkOrderImageDto> GetWorkOrderImageList(int id)
        {
            PicannolEntities context = new PicannolEntities();
            var imageList = context.tblWorkOrderImages.Where(x => x.WorkOrderId == id && x.DelInd==false).
                Select(x => new WorkOrderImageDto
                {
                    ImageName=x.ImageName,
                    WorkOrderId=x.WorkOrderId,
                    WorkOrderImageId=x.WorkOrderImageId

                }).ToList();
            return imageList;
        }

        //public string SaveNewOrder(OrderDto item, int custId, int challanId, int partId,int Qty)
        public string SaveNewOrder(OrderDto item, int custId, int challanId, int partId)
        {
            
            string response = "";
            PicannolEntities context = new PicannolEntities();
            try
            {
                tblOrder or = new tblOrder();
                or.OrderGUID = Guid.NewGuid();
                or.RepairType = item.RepairType;
                or.AssignedUserId = item.AssignedUserId;
                or.Remarks = item.Remarks;
                or.PartId = partId;
                or.CustomerId = custId;
                or.ChallanId = challanId;
                or.DateCreated = DateTime.Now;
                or.Qty = 1;
                //or.Qty = Qty;
                or.OrderDate = item.OrderDate;
                or.TrackingNumber = GenerateTrackingNumber(or.OrderDate);
                or.Status = ConstantsHelper.OrderStatus.Open.ToString();
                or.DelInd = false;
                or.Dispatched = false;
                or.Paid = false;
                OrderService.SaveOrder(or);
                if (item.RepairType == ConstantsHelper.RepairType.Loan.ToString())
                {
                    tblPartLoan pl = new tblPartLoan();
                    pl.OrderGUID = or.OrderGUID;
                    pl.DateCreated = DateTime.Now;
                    pl.Returned = false;
                    pl.DelInd = false;
                    context.tblPartLoans.Add(pl);
                    context.SaveChanges();
                }

                //record user activity
                string ActionName = $"Add Inward Material, Tracking No - {or.TrackingNumber}"
                + $", OrderGUID - {or.OrderGUID}"
                + $", ChallanId - {challanId}";
                string TableName = "tblOrder";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                //End


                response = "success";
            }

            catch (Exception ex)
            {
                response = ex.Message;
            }
            return response;
        }

        public bool CheckLoanReturn(OrderDto order)
        {
            bool response = false;
            PicannolEntities context = new PicannolEntities();
            PartDto part = new PartDto();
            part.PartName = order.PartName;
            part.PartNumber = order.PartNo;
            part.SerialNo = order.SerialNo;
            int partId = PartsService.GetPartId(part);
            string loan = ConstantsHelper.RepairType.Loan.ToString();
            if (context.tblOrders.Any(x => x.PartId == partId && x.DelInd == false && x.RepairType == loan))
            {
                Guid orderId = new Guid();
                orderId = context.tblOrders.Where(x => x.PartId == partId && x.DelInd == false && x.RepairType == loan).Select(x => x.OrderGUID).FirstOrDefault();
                if (context.tblPartLoans.Any(x => x.OrderGUID == orderId && x.DelInd == false && x.Returned == false))
                    response = true;
                else if (context.tblPartLoans.Any(x => x.OrderGUID == orderId && x.DelInd == false && x.Returned == true))
                    response = false;
            }
            return response;
        }
        internal void DeleteOrder(OrderDto item)
        {
            PicannolEntities context = new PicannolEntities();
            try
            {
                tblOrder or = context.tblOrders.Where(x => x.OrderGUID == item.OrderGUID).FirstOrDefault();
                or.DelInd = true;
                OrderService.Update(or);

                //record user activity
                string ActionName = $"DeleteOrder, Tracking No - {or.TrackingNumber}"
                + $", OrderGUID - {or.OrderGUID}";
                string TableName = "tblOrders";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                //End
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public long GenerateTrackingNumber(DateTime orderDate)
        {
            long i = 0;
            i = OrderService.GetLastTrackingNumber(orderDate);
            if (i == 0)
            {
                int year = orderDate.Year;
                int month = orderDate.Month;
                int date = orderDate.Day;
                string m = Convert.ToString(month);
                string d = Convert.ToString(date);
                if (m.Length == 1)
                {
                    m = "0" + m;
                }
                if (d.Length == 1)
                    d = "0" + d;
                string y = Convert.ToString(year).Substring(2);
                string a = y + m + d + "01";
                i = Convert.ToInt64(a);
            }
            else
                i++;
            return i;
        }

        public List<PartDto> GetAllPartsList(int type)
        {
            return PartsService.GetAllPartsList(type);
        }
        public List<PartDto> GetAllLowPartsList()
        {
            return PartsService.GetAllLowPartsList();
        }
        public List<PartDto> GetPartsStockList()
        {
            return PartsService.GetPartsStockList();
        }
        public List<PartDto> GetSpecificPart(string part)
        {
            return PartsService.GetSpecificPart(part);
        }
        internal List<OrderDto> GetOrdersList(InwardMaterialViewModel vm)
        {
            List<OrderDto> orders = new List<OrderDto>();

            return orders = OrderService.GetOrdersList(vm.CustomerId);
        }

        internal List<OrderDto> GetOrdersListByChallan(int ChallanId)
        {
            List<OrderDto> orders = new List<OrderDto>();

            return orders = OrderService.GetOrdersListByChallan(ChallanId);
        }



        public ChallanDto GetChallanDetailsByChallanNo(int custId, string challanNo)
        {
            PicannolEntities context = new PicannolEntities();
            var c = (from a in context.tblChallans
                     where a.CustomerId == custId && a.ChallanNumber == challanNo
                     select new ChallanDto
                     {
                         ChallanId = a.ChallanId,
                     }).FirstOrDefault();
            return c;
        }

        public int GetPartIDByPartDetails(string partNo, string partName, string serailNo)
        {
            PicannolEntities context = new PicannolEntities();
            int partId = 0;
            if (context.tblParts.Any(x => x.PartNumber == partNo && x.PartName == partName && x.SerialNo == serailNo && x.DelInd == false))
                partId = context.tblParts.Where(x => x.PartTypeId == 1 && x.PartNumber == partNo && x.PartName == partName && x.SerialNo == serailNo && x.DelInd == false).Select(a => a.PartId).FirstOrDefault();
            else if (context.tblParts.Any(x => x.PartNumber == partNo && x.PartName == partName && x.SerialNo == null && x.DelInd == false))
            {
                var tp = context.tblParts.Where(x => x.PartTypeId == 1 && x.PartNumber == partNo && x.PartName == partName).FirstOrDefault();
                tp.SerialNo = serailNo;
                tp.Stock = 1;
                context.SaveChanges();
                partId = tp.PartId;
            }
            else
            {
                tblPart tp = new tblPart();
                if (tp.PartName == null)
                {
                    tp.PartName = "";
                }
                else
                {
                    tp.PartName = partName;
                }
                if (tp.PartNumber == null)
                {
                    tp.PartNumber = "";
                }
                else
                {
                    tp.PartNumber = partNo;
                }
                if (tp.SerialNo == null)
                {
                    tp.SerialNo = "";
                }
                else
                {
                    tp.SerialNo = serailNo;
                }

                tp.Stock = 1;
                tp.PartTypeId = 1;
                tp.DelInd = false;
                context.tblParts.Add(tp);
                context.SaveChanges();
                partId = tp.PartId;

            }

            return partId;
        }

        public List<ZoneDto> ZoneList()
        {
            
            var ZoneList = new List<ZoneDto>();
            ZoneList = (from a in context.tblZones
                        where (a.DelInd == false)
                        select new ZoneDto
                        {
                            ZoneId = a.ZoneId,
                            Zone = a.Zone,
                        }).ToList();


            return ZoneList;
        }

        public List<PartMovementDto> GetPartMovementDetails(int id)
        {
            PicannolEntities context = new PicannolEntities();
            //var openingStockDate = ConfigurationManager.AppSettings["StockUpdateDate"];
            //DateTime dt = DateTime.Parse(openingStockDate);
            var p = (from a in context.tblOrderParts
                     join b in context.tblOrders on a.OrderGUID equals b.OrderGUID
                     join c in context.tblInvoices on a.OrderGUID equals c.OrderGuid
                     join d in context.tblCustomers on b.CustomerId equals d.CustomerId
                     //orderby a.DateCreated
                     orderby b.DateCreated
                     where a.PartId == id&& c.Cancelled == false && c.DelInd == false && a.DelInd == false 
                     /*&& a.DateCreated > dt*/
                     select new PartMovementDto
                     {
                         Date = a.DateCreated,
                         Particulars = d.CustomerName + "/" + c.InvoiceNo,
                         InvoiceNo = c.InvoiceNo,
                         Quantity = a.Qty,
                         InQuantity = 0
                     }).ToList();
           
            if (context.tblPurchases.Any(x => x.PartId == id))
            {
                var pr = context.tblPurchases.Where(x => x.PartId == id).ToList();
                foreach (var item in pr)
                {
                    PartMovementDto pm = new PartMovementDto();
                    pm.Date = item.PurchaseDate;
                    pm.Particulars = "Purchase";
                    pm.Quantity = 0;
                    pm.InQuantity = item.Quantity;
                    p.Add(pm);
                }

            }
            return p;
        }

        public List<ChallanDto> GetInwardMaterialList()
        {
            PicannolEntities context = new PicannolEntities();
            var ml = (from a in context.tblChallans
                      join b in context.tblCustomers on a.CustomerId equals b.CustomerId
                      orderby a.ChallanId descending
                      select new ChallanDto
                      {
                          ChallanId = a.ChallanId,
                          ChallanNumber = a.ChallanNumber,
                          DateCreated = context.tblOrders.Where(x => x.ChallanId == a.ChallanId).Select(x => x.OrderDate).FirstOrDefault(),
                          CustomerName = b.CustomerName,
                          ChallanDate = a.ChallanDate,
                          CustomerId = a.CustomerId,

                      }).ToList();
            foreach (var item in ml)
            {
                item.CDate = item.ChallanDate == null ? null : item.ChallanDate.Value.ToString("dd/MM/yyyy");
                item.OrderDate = item.DateCreated.Value.ToString("dd/MM/yyyy");
            }
            return ml;
        }

        //new helper method for Challanlist Pagination
        public List<ChallanDto> GetInwardMaterialListV1(InwardMaterialViewModel vm)
        {
            return PartsService.GetInwardMaterialListV1(vm);
        }
        //end Here
        public string DeletePart(int partId)
        {
            PicannolEntities context = new PicannolEntities();
            var p = context.tblParts.Where(x => x.PartId == partId && x.DelInd == false).FirstOrDefault();
            p.DelInd = true;
            string response = "";
            try
            {
                context.tblParts.Attach(p);
                context.Entry(p).State = EntityState.Modified;
                context.SaveChanges();
                response = "success";

                //record user activity
                string ActionName = $"DeletePart, PartID - {p.PartId}, + PartName - {p.PartName}";
                string TableName = "tblPart";
                if (ActionName != null)
                {
                    new UserHelper(this).recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                }
                //End
            }
            catch (Exception ex)
            {
                response = ex.Message;
                throw;
            }
            return response;
        }

        public List<PartExcel> GetInventoryData()
        {
            PicannolEntities context = new PicannolEntities();
            var p = (from a in context.tblParts
                     where a.DelInd == false && a.PartTypeId == 2
                     select new PartExcel
                     {
                       PartNumber = a.PartNumber,
                       PartName = a.PartName,
                       Price = a.Price,
                       Stock = a.Stock,
                       PartId = a.PartId
                     }).ToList();
            foreach (var item in p)
            {
                item.Stock = GetConsumptionOfPart(item.PartId);
            }
            return p;
        }

        public int GetConsumptionOfPart(int PartId)
        {
           return PartsService.GetConsumptionOfPart(PartId);
        }
        #endregion

        
    }
}