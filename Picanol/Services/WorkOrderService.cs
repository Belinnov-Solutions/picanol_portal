using NLog;
using Picanol.DataModel;
using Picanol.Models;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
//using System.Data.Objects;
using System.Linq;
using System.Web;

namespace Picanol.Services
{
    public class WorkOrderService : BaseService<PicannolEntities, tblWorkOrder>
    {
        #region Constructors
        internal WorkOrderService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public int SaveWorkOrder(tblWorkOrder cl)
        {
            AddWorkOrder(cl);
            return cl.WorkOrderId;
        }
        public void AddWorkOrder(tblWorkOrder cls)
        {
            if (cls == null)
                throw new ArgumentNullException("workOrder", "Null Parameter");
            Add(cls);
        }

        public List<WorkOrderDto> GetWorkOrdersList()
        // public List<WorkOrderDto> GetWorkOrdersListVersion2(WorkOrderViewModel wvm)
        {
            try
            {
                var s = (from a in Context.tblWorkOrders
                         join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                         join c in Context.tblUsers on a.CreatedBy equals c.UserId
                         join d in Context.tblUsers on a.AssignedTo equals d.UserId
                         where a.DelInd == false
                         select new WorkOrderDto
                         {
                             DateCreated = a.DateCreated,
                             WorkOrderId = a.WorkOrderId,
                             WorkOrderNo = a.WorkOrderNo,
                             ContractNumber = a.ContractNumber,
                             StartDate = a.StartDate,
                             EndDate = a.EndDate,
                             CreatorName = c.UserName,
                             AssignedTo = a.AssignedTo,
                             CustomerName = b.CustomerName,
                             AssignedUserName = d.UserName,
                             CustomerId = a.CustomerId,
                             Description = a.Description,
                             Conditions = a.Conditions,
                             Mobile = a.Mobile,
                             Phone = a.Phone,
                             EmailId = a.EmailId,
                             WorkOrderType = a.WorkOrderType,
                             ContactPerson = a.ContactPerson,
                             CallType = a.CallType,
                             CustomerAddress = b.AddressLine1 + " " + b.District + " " + b.State,
                         }).OrderByDescending(x => x.DateCreated).ToList();
                foreach (var item in s)
                {
                    if (Context.tblProvisionalBills.Any(x => x.WorkOrderId == item.WorkOrderId))
                        item.ProvisionalBillCreated = true;
                    else
                        item.ProvisionalBillCreated = false;
                    item.SDate = item.StartDate == null ? "" : item.StartDate.Value.ToShortDateString();
                    item.EDate = item.EndDate == null ? "" : item.EndDate.Value.ToShortDateString();
                    if (Context.tblTimeSheets.Any(x => x.WorkOrderId == item.WorkOrderId))
                        item.TimeSheetFilled = true;
                    else
                        item.TimeSheetFilled = false;
                }

                return s;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in SendEmail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        //new Pagination 170721

        // public List<WorkOrderDto> GetWorkOrdersList()
        public List<WorkOrderDto> GetWorkOrdersListVersion2(WorkOrderViewModel wvm)
        {
            try
            {
                wvm.PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
                Logger logger = LogManager.GetLogger("databaseLogger");
                char[] splitchar = { '/' };
                string sdate = "";
                string edate = "";
                if (wvm.SDate != null)
                {
                    string[] arr = wvm.SDate.Split(splitchar);
                    //var val = arr[2].Split();
                    sdate = arr[2] + "-" + arr[1] + "-" + arr[0];
                }
                if (wvm.EDate != null && wvm.EDate != " ")
                {
                    string[] arr1 = wvm.EDate.Split(splitchar);
                    edate = arr1[2] + "-" + arr1[1] + "-" + arr1[0];
                }
                logger.Info("edate", sdate);

                List<WorkOrderDto> s = new List<WorkOrderDto>();
                string query = " select DateCreated,a.WorkOrderId,a.WorkOrderNo,a.ContractNumber, " +
                                  " a.StartDate,a.EndDate,c.UserName,a.AssignedTo,b.CustomerName,d.UserName, " +
                                  " a.CustomerId,a.Description,a.Conditions,a.Mobile," +
                                 " a.Phone,a.EmailId,a.WorkOrderType,a.ContactPerson,a.CallType,d.UserName as  AssignedUserName, " +
                                 " CONCAT(b.AddressLine1, ' ', b.District, ' ', b.State) as Address from tblWorkOrders as a " +
                                " inner join tblCustomer as b on a.CustomerId = b.CustomerId " +
                                " inner join tblUser as c on a.CreatedBy = c.UserId " +
                                 " inner join tblUser as d on a.AssignedTo = d.UserId " +
                                " where b.Delind = 0 and a.Delind = 0 ";
                if (wvm.CustomerId != null)
                    if (wvm.CustomerId != 0)
                    {
                        query += " and b.CustomerId = " + wvm.CustomerId;
                    }

                if (wvm.SDate != null && wvm.SDate != "")
                    if (wvm.EDate != null && wvm.EDate != "")
                    {
                        query += " and CAST(a.StartDate as date) >= '" + sdate + "' and CAST(a.EndDate as date) <= '" + edate + "' ";
                    }
                if (wvm.AssignedTo != null)
                    if (wvm.AssignedTo != 0)
                    {
                        query += " and a.AssignedTo =" + wvm.AssignedTo;
                    }
                if (wvm.RoleId != 5 && wvm.RoleId != 3 && wvm.RoleId != 6)
                {
                    query += "and a.AssignedTo =" + wvm.UserId;
                }


                //try
                //{
                //    //Change this to sql query 
                //    //like we done in provisionalBill

                //    var s = (from a in Context.tblWorkOrders
                //             join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                //             join c in Context.tblUsers on a.CreatedBy equals c.UserId
                //             join d in Context.tblUsers on a.AssignedTo equals d.UserId
                //             where a.DelInd == false
                //             select new WorkOrderDto
                //             {
                //                 DateCreated = a.DateCreated,
                //                 WorkOrderId = a.WorkOrderId,
                //                 WorkOrderNo = a.WorkOrderNo,
                //                 ContractNumber = a.ContractNumber,
                //                 StartDate = a.StartDate,
                //                 EndDate = a.EndDate,
                //                 CreatorName = c.UserName,
                //                 AssignedTo = a.AssignedTo,
                //                 CustomerName = b.CustomerName,
                //                 AssignedUserName = d.UserName,
                //                 CustomerId = a.CustomerId,
                //                 Description = a.Description,
                //                 Conditions = a.Conditions,
                //                 Mobile = a.Mobile,
                //                 Phone = a.Phone,
                //                 EmailId = a.EmailId,
                //                 WorkOrderType = a.WorkOrderType,
                //                 ContactPerson = a.ContactPerson,
                //                 CallType = a.CallType,
                //                 CustomerAddress = b.AddressLine1 + " " + b.District + " " + b.State,

                //             }).OrderByDescending(x => x.DateCreated).Skip((wvm.PageNo - 1) * wvm.PageSize).Take(wvm.PageSize).ToList();
                query += " order by a.WorkOrderNo desc";
                logger.Info("WorkOrder query", query);
                PicannolEntities context = new PicannolEntities();
                s = context.Database.SqlQuery<WorkOrderDto>(query).Skip((wvm.PageNo - 1) * wvm.PageSize).Take(wvm.PageSize).ToList<WorkOrderDto>();
                foreach (var item in s)
                {
                    if (Context.tblProvisionalBills.Any(x => x.WorkOrderId == item.WorkOrderId))
                        item.ProvisionalBillCreated = true;
                    else
                        item.ProvisionalBillCreated = false;
                    item.SDate = item.StartDate == null ? "" : item.StartDate.Value.ToShortDateString();
                    item.EDate = item.EndDate == null ? "" : item.EndDate.Value.ToShortDateString();
                    if (Context.tblTimeSheets.Any(x => x.WorkOrderId == item.WorkOrderId))
                        item.TimeSheetFilled = true;
                    else
                        item.TimeSheetFilled = false;
                }

                return s;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in SendEmail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }

        //End here

        //new method for pagination work order Himanshu//
        public List<WorkOrderDto> GetWorkOrdersListVersion1(int PageSize = 10, int PageNo = 1)
        {
            try
            {
                var s = (from a in Context.tblWorkOrders
                         join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                         join c in Context.tblUsers on a.CreatedBy equals c.UserId
                         join d in Context.tblUsers on a.AssignedTo equals d.UserId
                         where a.DelInd == false && b.Delind == false
                         select new WorkOrderDto
                         {
                             DateCreated = a.DateCreated,
                             WorkOrderId = a.WorkOrderId,
                             WorkOrderNo = a.WorkOrderNo,
                             ContractNumber = a.ContractNumber,
                             StartDate = a.StartDate,
                             EndDate = a.EndDate,
                             CreatorName = c.UserName,
                             AssignedTo = a.AssignedTo,
                             CustomerName = b.CustomerName,
                             AssignedUserName = d.UserName,
                             CustomerId = a.CustomerId,
                             Description = a.Description,
                             Conditions = a.Conditions,
                             Mobile = a.Mobile,
                             Phone = a.Phone,
                             EmailId = a.EmailId,
                             WorkOrderType = a.WorkOrderType,
                             ContactPerson = a.ContactPerson,
                             CallType = a.CallType,
                             CustomerAddress = b.AddressLine1 + " " + b.District + " " + b.State,

                         }).OrderByDescending(x => x.DateCreated).Skip((PageNo - 1) * PageSize).Take(PageSize).ToList();
                foreach (var item in s)
                {
                    if (Context.tblProvisionalBills.Any(x => x.WorkOrderId == item.WorkOrderId))
                        item.ProvisionalBillCreated = true;
                    else
                        item.ProvisionalBillCreated = false;
                    item.SDate = item.StartDate == null ? "" : item.StartDate.Value.ToShortDateString();
                    item.EDate = item.EndDate == null ? "" : item.EndDate.Value.ToShortDateString();
                    if (Context.tblTimeSheets.Any(x => x.WorkOrderId == item.WorkOrderId))
                        item.TimeSheetFilled = true;
                    else
                        item.TimeSheetFilled = false;
                }

                return s;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in SendEmail!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        //End here//
        public string GetLastWorkOrderNumber()
        {
            string i = "";
            var inv = (from a in Context.tblWorkOrders
                       where a.DelInd == false
                       select a).ToList();
            inv = inv.OrderByDescending(x => x.WorkOrderId).ToList();
            i = inv.Select(x => x.WorkOrderNo).FirstOrDefault();
            return i;
        }

        public WorkOrderDto GetWorkOrderDetails(int? id)
        {
            try
            {
                var w = (from a in Context.tblWorkOrders
                         join b in Context.tblUsers on a.AssignedTo equals b.UserId
                         where a.WorkOrderId == id
                         select new WorkOrderDto
                         {
                             WorkOrderNo = a.WorkOrderNo,
                             WorkOrderType = a.WorkOrderType,
                             ContractNumber = a.ContractNumber,
                             Description = a.Description,
                             Conditions = a.Conditions,
                             StartDate = a.StartDate,
                             EndDate = a.EndDate,
                             AssignedUserName = b.UserName,
                             CustomerId = a.CustomerId
                         }).FirstOrDefault();
                return w;
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetLogger("databaseLogger");
                logger.Error(ex, "Error in GetWorkOrderDetails!");
                logger.Error(ex.InnerException, "Inner exception");
                throw ex;
            }
        }
        //public BusinessDto GetBusinessDetails()
        //{
        //    var b = (from a in Context.tblWorkOrderes
        //             select new BusinessDto
        //             {
        //                 Name = a.Name,
        //                 AddressLine1 = a.AddressLine1,
        //                 AddressLine2 = a.AddressLine2,
        //                 City = a.City,
        //                 State = a.State,
        //                 PIN = a.PIN,
        //                 GSTIN = a.GSTIN,
        //                 AccountNumber = a.AccountNumber,
        //                 IFSCCode = a.IFSCCode,
        //                 BankBranch = a.BankBranch,
        //                 BankName = a.BankName,
        //             }).FirstOrDefault();

        //    return b;
        //}
        #endregion

        #region Overrides
        public override void Add(tblWorkOrder workOrder)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(workOrder);
            }
        }


        #endregion

    }
}