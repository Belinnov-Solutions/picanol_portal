using Picanol.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Picanol.Models;
//using System.Data.Objects;
using Picanol.ViewModels;
using System.Data.Objects;
using System.Configuration;

namespace Picanol.Services
{
    public class OrderService : BaseService<PicannolEntities, tblOrder>
    {
        #region Constructors
        internal OrderService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public void SaveOrder(tblOrder cl)
        {
            AddOrder(cl);
        }
        public void AddOrder(tblOrder cls)
        {
            if (cls == null)
                throw new ArgumentNullException("challan", "Null Parameter");
            Add(cls);
        }

        internal long GetLastTrackingNumber(DateTime date)
        {
            long i = 0;
            i = Context.tblOrders.Where(x => EntityFunctions.TruncateTime(x.OrderDate) == EntityFunctions.TruncateTime(date) && x.DelInd == false).Max(p => (long?)p.TrackingNumber) ?? 0;
            return i;
        }

        public List<OrderDto> GetOrdersList(int custId)
        {
            List<OrderDto> orders = new List<OrderDto>();
            if (custId == 0)
            {
                orders = (from a in Context.tblOrders
                              //join b in Context.tblUsers on a.AssignedUserId equals b.UserId
                          join c in Context.tblParts on a.PartId equals c.PartId
                          join d in Context.tblCustomers on a.CustomerId equals d.CustomerId
                          join e in Context.tblChallans on a.ChallanId equals e.ChallanId
                          where a.DelInd == false
                          select new OrderDto
                          {
                              OrderGUID = a.OrderGUID,
                              TrackingNumber = a.TrackingNumber,
                              DateCreated = a.DateCreated,
                              Status = a.Status,
                              AssignedUserId = a.AssignedUserId,
                              //UserName = b.UserName,
                              PartName = c.PartName,
                              RepairType = a.RepairType,
                              CustomerName = d.CustomerName,
                              CustomerRef = e.ChallanNumber,
                              Dispatched = a.Dispatched,
                              Paid = a.Paid
                          }).ToList();
            }
            else
            {
                orders = (from a in Context.tblOrders
                          join b in Context.tblUsers on a.AssignedUserId equals b.UserId
                          join c in Context.tblParts on a.PartId equals c.PartId
                          where a.CustomerId == custId && a.DelInd == false
                          select new OrderDto
                          {
                              OrderGUID = a.OrderGUID,
                              TrackingNumber = a.TrackingNumber,
                              DateCreated = a.DateCreated,
                              Status = a.Status,
                              AssignedUserId = a.AssignedUserId,
                              UserName = b.UserName,
                              PartName = c.PartName,
                              RepairType = a.RepairType,
                              Dispatched = a.Dispatched,
                              Paid = a.Paid
                          }).ToList();
            }
            return orders;
        }

        public List<OrderDto> GetFilteredOrdersList(OrdersViewModel vm)
        {
            vm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            char[] splitchar = { '/' };
            string fdate = "";
            string tdate = "";
            if (vm.FromDate != null)
            {
                string[] arr = vm.FromDate.Split(splitchar);
                fdate = arr[2] + "-" + arr[1] + "-" + arr[0];
            }
            if (vm.ToDate != null)
            {
                string[] arr1 = vm.ToDate.Split(splitchar);
                tdate = arr1[2] + "-" + arr1[1] + "-" + arr1[0];
            }
            List<OrderDto> orders = new List<OrderDto>();
            /*string query = "Select o.OrderGUID,o.DateCreated,o.TrackingNumber,o.Status,p.PartName, u.UserName, o.RepairType, " +
                            "p.PartNumber As PartNo, SerialNo As SerialNo, i.InvoiceNo, " +
                            "c.CustomerId, c.Email As CustomerEmail, c.CustomerName, ch.ChallanNumber As CustomerRef from tblOrder o " +
                            "inner join tblCustomer c on c.CustomerId = o.CustomerId " +
                            "inner join tblPart p on p.PartId = o.PartId " +
                            "inner join tblChallan ch on ch.ChallanId = o.ChallanId " +
                            "left join tblInvoices i on i.OrderGuid =    o.OrderGUID  and i.Cancelled = 0 and i.DelInd = 0" +
                            "left join tblUser u on u.UserId = o.AssignedUserId ";*/

            string query = "Select o.OrderGUID,o.DateCreated,o.TrackingNumber,o.Status,p.PartName, u.UserName, o.RepairType, " +
                            "p.PartNumber As PartNo, SerialNo As SerialNo, i.InvoiceNo, " +
                            "c.CustomerId, c.Email As CustomerEmail, c.CustomerName, ch.ChallanNumber As CustomerRef from tblOrder o " +
                            "inner join tblCustomer c on c.CustomerId = o.CustomerId " +
                            "inner join tblPart p on p.PartId = o.PartId " +
                            "inner join tblChallan ch on ch.ChallanId = o.ChallanId " +
                            "left join tblInvoices i on i.OrderGuid =    o.OrderGUID  and i.Cancelled = 0 and i.DelInd = 0" +
                            "left join tblProformaInvoices pi on pi.OrderGuid =    o.OrderGUID and pi.DelInd = 0" +
                            "left join tblUser u on u.UserId = o.AssignedUserId ";

            //"where CAST(o.DateCreated as date) BETWEEN '" + vm.FromDate.ToString("yyyy-MM-dd") + "' and '" + vm.ToDate.ToString("yyyy-MM-dd") + "'";
            //Tracking No cases
            if (vm.TrackingNo != null && vm.SelectedStatus == null && vm.SelectedRepairType == null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
                query += "where o.DelInd = 0 and o.TrackingNumber = " + vm.TrackingNo;
            //new  logic for traCKING NUMBER  gainst from and to date hIMANSHU//
            else if (vm.TrackingNo != null && fdate != null && tdate != null)
            {
                query += "where o.DelInd = 0 and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <= '" + tdate +"' and o.TrackingNumber =" + vm.TrackingNo;
            }


            else if (vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType == null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
                    {
                        query += "where o.DelInd=0 and o.TrackingNumber='" + vm.TrackingNo + "' and o.Status='" + vm.SelectedStatus + "'";
                    }
                    else if (vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType != null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
                    {
                        query += "where o.DelInd=0 and o.TrackingNumber='" + vm.TrackingNo + "' and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "' ";
                    }
                    else if (vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType != null && fdate != "" && tdate != "" && vm.SelecetCustomerId == 0)
                    {
                        query += "where o.DelInd=0 and o.TrackingNumber='" + vm.TrackingNo + "' and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "' and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'";
                    }
                    else if (vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType != null && vm.SelecetCustomerId != 0 && fdate != "" && tdate != "")
                    {
                        query += "where o.DelInd=0 and o.TrackingNumber='" + vm.TrackingNo + "' and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "' and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "' and  o.CustomerId = '" + vm.SelecetCustomerId + "' ";
                    }
                    else if (vm.SelectedStatus != null && fdate != "" && tdate != "" && vm.TrackingNo == null && vm.SelectedRepairType == null && vm.SelecetCustomerId == 0)
                    {
                        query += "where o.DelInd=0 and o.Status='" + vm.SelectedStatus + "' and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "' ";
                    }

                    //Repaired Type cases
                    else if (vm.SelectedRepairType != null && vm.TrackingNo == null && vm.SelectedStatus == null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
                    {
                        query += "where o.RepairType = '" + vm.SelectedRepairType + "'";
                    }
                    else if (vm.SelectedRepairType != null && vm.TrackingNo != null && vm.SelectedStatus == null && vm.SelectedRepairType == null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)

                    {
                        query += "where o.RepairType = '" + vm.SelectedRepairType + "' and o.TrackingNumber='" + vm.TrackingNo + "'";
                    }
                    else if (vm.SelectedRepairType != null && vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType != null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
                    {
                        query += "where o.RepairType = '" + vm.SelectedRepairType + "' and o.TrackingNumber='" + vm.TrackingNo + "'and o.Status='" + vm.SelectedStatus + "'and o.RepairType='" + vm.SelectedRepairType + "'";
                    }
                    else if (vm.SelectedRepairType != null && vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType != null && fdate != "" && tdate != "" && vm.SelecetCustomerId == 0)

                    {
                        query += "where o.RepairType = '" + vm.SelectedRepairType + "' and o.TrackingNumber='" + vm.TrackingNo + "'and o.Status='" + vm.SelectedStatus + "'and o.RepairType='" + vm.SelectedRepairType + "'and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'";
                    }
                //Status based cases
            if (vm.SelectedStatus != null && vm.TrackingNo == null && vm.SelectedRepairType == null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
            { 
                query += "where o.DelInd = 0 and o.Status = '" + vm.SelectedStatus+"'";
            }

            else if (vm.SelectedStatus != null && vm.TrackingNo != null && vm.SelectedRepairType != null && fdate != "" && tdate != "" && vm.SelecetCustomerId == 0)
            {
                query += "where o.DelInd=0 and o.TrackingNumber='" + vm.TrackingNo + "' and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "' and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'";
            }
            else if (vm.SelectedStatus != null && vm.TrackingNo != null && vm.SelectedRepairType != null && fdate != "" && tdate != "" && vm.SelecetCustomerId != 0)
            {
                query += "where o.DelInd=0 and o.TrackingNumber='" + vm.TrackingNo + "' and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "' and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "' and  o.CustomerId = '" + vm.SelecetCustomerId + "' ";
            }
            else if (vm.SelectedStatus != null && vm.SelectedRepairType != null && vm.TrackingNo == null && fdate == "" && tdate == "" && vm.SelecetCustomerId == 0)
            {
                query += "where o.DelInd=0 and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "'";
            }

            else if (fdate != "" && tdate != ""   && vm.TrackingNo == null && vm.SelectedStatus != null && vm.SelectedRepairType != null && vm.SelecetCustomerId == 0)
            { 
                query += "where o.DelInd = 0 and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "' and o.RepairType='" +vm.SelectedRepairType+"'and o.Status='"+vm.SelectedStatus+"' ";
            }
            else if (fdate != "" && tdate != "" && vm.TrackingNo == null && vm.SelectedStatus == null && vm.SelectedRepairType == null && vm.SelecetCustomerId == 0)
            { 
                query += "where o.DelInd = 0 and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'";
            }
            else if (vm.SelecetCustomerId != 0  && vm.SelectedRepairType != null && vm.SelectedStatus != null && fdate == "" && tdate == "" && vm.TrackingNo == null  )
            {
                query += "where o.DelInd = 0  and o.CustomerId = '" + vm.SelecetCustomerId + "'and o.Status='" + vm.SelectedStatus + "'and o.RepairType='" + vm.SelectedRepairType + "'";
                
            }
            //new logic//
            else if(vm.SelecetCustomerId !=0 && vm.SelectedRepairType==null && vm.SelectedStatus==null && vm.TrackingNo != null && fdate=="" && tdate=="")
            {
                query +="where o.DelInd=0 and o.CustomerId='" + vm.SelecetCustomerId + "'and o.TrackingNumber='" + vm.TrackingNo +"'";
            }
            else if (vm.SelecetCustomerId != 0 && vm.SelectedRepairType != null && vm.SelectedStatus == null && fdate == "" && tdate == "" && vm.TrackingNo == null)
            { 
                string r = "REPAIR WARRANTY";
                if (vm.SelectedRepairType == r)
                {
                    vm.SelectedRepairType.Replace(" ", "");
                    query += "where o.DelInd = 0  and o.CustomerId = '" + vm.SelecetCustomerId + "'and o.RepairType='" + vm.SelectedRepairType + "'";
                }
                else
                {
                    query += "where o.DelInd = 0  and o.CustomerId = '" + vm.SelecetCustomerId + "'and o.RepairType='" + vm.SelectedRepairType + "'";
                }
            }
            else if (vm.SelecetCustomerId != 0 && fdate == "" && tdate == "" && vm.TrackingNo == null && vm.SelectedStatus == null && vm.SelectedRepairType == null)
            {
                query += "where o.DelInd = 0  and o.CustomerId = '" + vm.SelecetCustomerId + "'";

            }
             else if (vm.SelecetCustomerId != 0 && vm.SelectedStatus != null && vm.SelectedRepairType == null  && fdate == "" && tdate == "" && vm.TrackingNo == null)
            {
                query += " where o.DelInd = 0  and o.CustomerId = '" + vm.SelecetCustomerId + "'and o.Status='" + vm.SelectedStatus + "'";

            }
            else if (vm.SelecetCustomerId != 0 && vm.SelectedStatus != null && vm.SelectedRepairType == null && fdate == "" && tdate == "" && vm.TrackingNo == null)
            {
                query += "where o.DelInd = 0  and o.CustomerId = '" + vm.SelecetCustomerId + "'and o.Status='" + vm.SelectedStatus + "'";

            }
            else if (fdate != "" && tdate != "" && vm.SelecetCustomerId != 0 && vm.TrackingNo == null && vm.SelectedStatus == null && vm.SelectedRepairType == null )
            {
                query += "where o.DelInd = 0 and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'and o.CustomerId = '" + vm.SelecetCustomerId + "'";
            }
            else if (fdate != "" && tdate != "" && vm.SelecetCustomerId != 0 && vm.TrackingNo != null && vm.SelectedStatus != null && vm.SelectedRepairType != null)
            {
                query += "where o.DelInd = 0 and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'and o.CustomerId = '" + vm.SelecetCustomerId + "' and o.TrackingNumber='" + vm.TrackingNo + "'and o.Status='" + vm.SelectedStatus + "'and o.RepairType='" + vm.SelectedRepairType + "'";
            }
            else if (vm.SelectedStatus == null && vm.SelectedRepairType != null && vm.TrackingNo == null && fdate != "" && tdate != "" && vm.SelecetCustomerId == 0)
            {
                query += "where o.DelInd = 0 and CAST(o.OrderDate as date) >= '" + fdate + "' and CAST(o.OrderDate as date) <=  '" + tdate + "'and o.RepairType = '" + vm.SelectedRepairType + "'";
                //query += "where o.DelInd=0 and o.Status='" + vm.SelectedStatus + "' and o.RepairType='" + vm.SelectedRepairType + "'";
            }
            query += " order by o.TrackingNumber desc";
            PicannolEntities context = new PicannolEntities();
            var result = context.Database.SqlQuery<OrderDto>(query).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList<OrderDto>();
         foreach (var item in result)
            {
                if (Context.tblProformaInvoices.Any(x => x.OrderGuid == item.OrderGUID && x.DelInd == false))
                {
                    var pi = Context.tblProformaInvoices.Where(x => x.OrderGuid == item.OrderGUID && x.DelInd==false).FirstOrDefault();
                    item.ProformaInvoiceNo = pi.ProformaInvoiceNo;
                    item.ProformaInvoiceDate = pi.ProformaInvoiceDate;
                }

                    if (Context.tblInvoices.Any(x => x.OrderGuid == item.OrderGUID))
                {
                    item.InvoiceGenerated = true;
                    var i = Context.tblInvoices.Where(x => x.OrderGuid == item.OrderGUID).FirstOrDefault();
                    if (i.Cancelled == true)
                        item.InvoiceCancelled = true;
                    else
                        item.InvoiceCancelled = false;
                }
                else
                {
                    item.InvoiceGenerated = false;
                    item.InvoiceCancelled = false;
                }
            }


            return result;
        }

       
        public List<OrderDto> GetOrdersListByChallan(int challanId)
        {
            List<OrderDto> orders = new List<OrderDto>();
            orders = (from a in Context.tblOrders
                      join c in Context.tblParts on a.PartId equals c.PartId
                      join d in Context.tblCustomers on a.CustomerId equals d.CustomerId
                      //join u in Context.tblUsers on a.AssignedUserId equals u.UserId
                      where a.ChallanId == challanId && a.DelInd == false
                      select new OrderDto
                      {
                          OrderGUID = a.OrderGUID,
                          TrackingNumber = a.TrackingNumber,
                          DateCreated = a.DateCreated,
                          Status = a.Status,
                          AssignedUserId = a.AssignedUserId,
                          //User = u.UserName,
                          PartName = c.PartName,
                          PartNo = c.PartNumber,
                          SerialNo = c.SerialNo,
                          RepairType = a.RepairType,
                          CustomerName = d.CustomerName,
                          Remarks = a.Remarks,
                          Qty = a.Qty,
                      }).ToList();

            return orders;
        }

        public List<OrderPartDto> GetConsumedParts(Guid orderId)
        {
            var parts = (from a in Context.tblOrderParts
                         join b in Context.tblParts on a.PartId equals b.PartId
                         where a.OrderGUID == orderId && a.DelInd == false
                         select new OrderPartDto
                         {
                             OrderPartId = a.OrderPartId,
                             PartId = a.PartId,
                             PartName = b.PartName,
                             PartNumber = b.PartNumber,
                             Price = a.Price == null ? b.Price:a.Price,
                             Qty = a.Qty,
                             Total = a.Qty * (a.Price == null ? b.Price:a.Price)
                         }).ToList();
            return parts;
        }

        public OrderDto GetOrderDetails(Guid orderId)
        {
            OrderDto order = new OrderDto();
            var o = (from a in Context.tblOrders
                     join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                     join c in Context.tblParts on a.PartId equals c.PartId
                     join d in Context.tblChallans on a.ChallanId equals d.ChallanId
                     where a.OrderGUID == orderId && a.DelInd == false
                     select new OrderDto
                     {
                         RepairType = a.RepairType,
                         ChallanId=a.ChallanId,
                         OrderGUID = a.OrderGUID,
                         ChallanDate = d.ChallanDate,
                         CustomerId = a.CustomerId,
                         CustomerName = b.CustomerName,
                         TrackingNumber = a.TrackingNumber,
                         DateCreated = a.DateCreated,
                         PartName = c.PartName,
                         PartNo = c.PartNumber,
                         SerialNo = c.SerialNo,
                         Status = a.Status,
                         CustomerRef = d.ChallanNumber,
                         TimeTaken = a.RepairTime,
                         PackingType = a.PackingType,
                         Remarks = a.Remarks,
                         AssignedUserId = a.AssignedUserId,
                         Dispatched = a.Dispatched,
                         Paid = a.Paid,
                         OrderDate = a.OrderDate,
                         ExemptLabourCost = a.ExemptLabourCost ?? false,
                         ExemptComponentCost = a.ExemptComponentCost ?? false,
                         Discount=a.Discount !=null?a.Discount:null,
                         PerformaInvoiceNumber = Context.tblProformaInvoices.Where(x => x.OrderGuid == orderId && x.DelInd==false).Select(x => x.ProformaInvoiceNo).FirstOrDefault() ?? "",

                         //AddedNew 
                         Qty = a.Qty
                         //AssignedUserName = e.UserName

                     }).FirstOrDefault();

            if (o.AssignedUserId != null)
                o.AssignedUserName = Context.tblUsers.Where(x => x.UserId == o.AssignedUserId).Select(x => x.UserName).FirstOrDefault();
            return o;
        }
        public DispatchDetailsDto GetDispatchDetailsByOrderId(Guid orderId)
        {
            var d = (from a in Context.tblDispatchDetails
                     where a.OrderGUID == orderId && a.DelInd == false
                     select new DispatchDetailsDto
                     {
                         DispatchDate = a.DispatchDate,
                         DispatchDetails = a.DispatchDetails,
                         DocketNumber = a.DocketNo
                     }).FirstOrDefault();

            return d;
        }

        public decimal GetRepairTime(Guid orderId)
        {
            var r = (decimal)Context.tblOrders.Where(a => a.OrderGUID == orderId).Select(x => x.RepairTime).SingleOrDefault();
            return r;
        }
        #endregion

        #region Overrides
        public override void Add(tblOrder ch)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(ch);
            }
        }

        public override void Update(tblOrder order)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                tblOrder updateorder = Context.tblOrders.Where(p => p.OrderGUID == order.OrderGUID).FirstOrDefault();
                updateorder.PackingType = order.PackingType;
                updateorder.LastModified = order.LastModified;
                updateorder.Status = order.Status;
                updateorder.Remarks = order.Remarks;
                updateorder.RepairTime = order.RepairTime;
                updateorder.RepairType = order.RepairType;
                updateorder.AssignedUserId = order.AssignedUserId;
                updateorder.ExemptComponentCost = order.ExemptComponentCost;
                updateorder.ExemptLabourCost = order.ExemptLabourCost;
                updateorder.Discount = order.Discount;
                updateorder.EditedBy = order.EditedBy;
                updateorder.DateEdited = order.DateEdited;
                base.Update(updateorder);
            }
        }

        #endregion
    }
}