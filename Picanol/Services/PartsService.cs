using NLog;
using Picanol.DataModel;
using Picanol.Models;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Picanol.Services
{
    public class PartsService : BaseService<PicannolEntities, tblPart>
    {
        #region Constructors
        internal PartsService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public void SavePart(tblPart cl)
        {
            if (cl.PartId == 0)
                AddPart(cl);
            else
                UpdatePart(cl);
        }
        public void AddPart(tblPart part)
        {
            if (part == null)
                throw new ArgumentNullException("part", "Null Parameter");
            Add(part);
        }
        public void UpdatePart(tblPart part)
        {
            if (part == null)
                throw new ArgumentNullException("productgroup", "Null Parameter");
            Update(part);
            base.SaveChanges();
        }


        public List<PartDto> GetPartsList(int type)
        {
            var st = (from a in Context.tblParts
                      where a.PartTypeId == type && a.DelInd == false
                      select new PartDto
                      {
                          //PartId = a.PartId,
                          PartName = a.PartName,
                          PartNumber = a.PartNumber
                      }).Distinct().ToList();
            return st;
        }

        public PartDto GetPartDetailsById(int? id)
        {
            var p = (from a in Context.tblParts
                     where a.PartId == id && a.DelInd == false
                     select new PartDto
                     {
                         PartId = a.PartId,
                         PartName = a.PartName,
                         PartNumber = a.PartNumber,
                         SerialNo = a.SerialNo,
                         Stock = a.Stock,
                         Price = a.Price,
                         PartTypeId = a.PartTypeId,

                     }).FirstOrDefault();
            p.Stock = GetConsumptionOfPart(p.PartId);
            return p;
        }

        //changed by shijo for adding prize to cpl
        public List<PartDto> GetCombinedPartsList(int type)
        {
            var ste = Context.tblParts.AsEnumerable().Where(x => x.PartTypeId == type && x.DelInd == false).Select(x => new PartDto
            {
                Stock = x.Stock != null ? (int)x.Stock : 0,
                PartName = getPartName(x.PartName, x.Price, x.Stock, x.PartNumber),
                PartNumber = x.PartNumber,
                Price = x.Price
            }).Distinct().ToList();
            return ste;
        }
        public List<PartDto> GetBoardsList()
        {
            var ste = Context.tblParts.Where(x => x.PartTypeId == 1 && x.DelInd == false)
                .GroupBy(x => new { x.PartName, x.PartNumber })
                .Select(x => new PartDto
                {
                    PartName = x.Key.PartName + ";" + x.Key.PartNumber
                }).ToList();
            return ste;
        }
        public string getPartName(string partName, decimal? price, int? stock, string partnumber)
        {
            string value = partName + ";" + partnumber + ";" + (price == null ? "" : price.ToString()) + ";" + (stock != null ? stock.ToString() : "");

            return value;
        }

        public List<PartDto> GetAllPartsList(int type)
        {
            if (type == 2)
            {
                var st = (from a in Context.tblParts
                          where a.PartTypeId == type && a.DelInd == false
                          select new PartDto
                          {
                              PartId = a.PartId,
                              PartName = a.PartName,
                              PartNumber = a.PartNumber,
                              PartTypeId = a.PartTypeId,
                              Stock = a.Stock,
                              OpeningBalance = a.OpeningStock == null ? 0 : (int)a.OpeningStock,
                          }).Distinct().ToList();

                return st;
            }
            else
            {
                var st = (from a in Context.tblParts
                          where a.PartTypeId == type && a.DelInd == false
                          select new PartDto
                          {
                              PartName = a.PartName,
                              PartNumber = a.PartNumber,
                              PartTypeId = a.PartTypeId,
                              PartId = a.PartId
                          }).Distinct().ToList();
                return st;
            }
        }
        public List<PartDto> GetAllLowPartsList()
        {


            var st = (from a in Context.tblParts
                      where a.PartTypeId == 2 && a.DelInd == false && a.Stock < 10
                      select new PartDto
                      {
                          PartId = a.PartId,
                          PartName = a.PartName,
                          PartNumber = a.PartNumber,
                          PartTypeId = a.PartTypeId,
                          Stock = a.Stock,
                          OpeningBalance = a.OpeningStock == null ? 0 : (int)a.OpeningStock,
                      }).Distinct().ToList();


            return st;
        }

        public List<PartDto> GetPartsStockList()
        {
            var st = (from a in Context.tblParts
                      where a.DelInd == false
                      select new PartDto
                      {
                          PartId = a.PartId,
                          PartName = a.PartName,
                          PartNumber = a.PartNumber,
                          SerialNo = a.SerialNo,
                          PartTypeId = a.PartTypeId,
                          Stock = a.Stock,
                      }).Distinct().ToList();
            return st;
        }

        public List<PartDto> GetSpecificPart(string part)
        {
            var st = (from a in Context.tblParts
                      where (a.PartName.Contains(part) || a.PartNumber.Contains(part)) && a.DelInd == false
                      select new PartDto
                      {
                          PartId = a.PartId,
                          PartName = a.PartName,
                          PartNumber = a.PartNumber,
                          SerialNo = a.SerialNo,
                          PartTypeId = a.PartTypeId,
                          Stock = a.Stock,
                          OpeningBalance = a.OpeningStock == null ? 0 : (int)a.OpeningStock
                      }).OrderBy(m => m.PartNumber).ToList();
            return st;
        }
        public string GetPartNumber(string p)
        {
            var f = (from a in Context.tblParts
                     where a.PartName == p && a.DelInd == false
                     select new PartDto
                     {
                         PartNumber = a.PartNumber,
                     }).Select(x => x.PartNumber).FirstOrDefault();

            return f;

        }

        public int GetPartId(PartDto part)
        {
            int partId = 0;
            if (part.PartNumber != null && part.SerialNo != null)
            {
                if (Context.tblParts.Any(a => a.PartName == part.PartName && a.PartNumber == part.PartNumber && a.SerialNo == part.SerialNo && a.DelInd == false))
                {
                    partId = Context.tblParts.Where(x => x.PartName == part.PartName && x.PartNumber == part.PartNumber && x.SerialNo == part.SerialNo && x.DelInd == false).Select(a => a.PartId).FirstOrDefault();
                }
                else
                    partId = AddNewPart(part);
            }
            else if (part.PartNumber != null && part.SerialNo == null)
            {
                if (Context.tblParts.Any(a => a.PartName == part.PartName && a.PartNumber == part.PartNumber && (a.SerialNo == "" || a.SerialNo == null) && a.DelInd == false))
                {
                    partId = Context.tblParts.Where(x => x.PartName == part.PartName && x.PartNumber == part.PartNumber && (x.SerialNo == "" || x.SerialNo == null) && x.DelInd == false).Select(a => a.PartId).FirstOrDefault();
                }
                else
                    partId = AddNewPart(part);
            }
            else if (part.PartNumber == null && part.SerialNo != null)
            {
                if (Context.tblParts.Any(a => a.PartName == part.PartName && (a.PartNumber == "" || a.PartNumber == null) && a.SerialNo == part.SerialNo && a.DelInd == false))
                {
                    partId = Context.tblParts.Where(x => x.PartName == part.PartName && (x.PartNumber == "" || x.PartNumber == null) && x.SerialNo == part.SerialNo && x.DelInd == false).Select(a => a.PartId).FirstOrDefault();
                }
                else
                    partId = AddNewPart(part);
            }
            else if (part.PartNumber == null && part.SerialNo == null)
            {
                if (Context.tblParts.Any(a => a.PartName == part.PartName && (a.PartNumber == "" || a.PartNumber == null)
                                    && (a.SerialNo == "" || a.SerialNo == null) && a.DelInd == false))
                {
                    partId = Context.tblParts.Where(x => x.PartName == part.PartName && (x.PartNumber == part.PartNumber || x.PartNumber == "" || x.PartNumber == null)
                                        && (x.SerialNo == "" || x.SerialNo == null) && x.DelInd == false).Select(a => a.PartId).FirstOrDefault();
                }
                else
                    partId = AddNewPart(part);
            }
            else
                partId = AddNewPart(part);
            return partId;
        }

        public int AddNewPart(PartDto part)
        {
            tblPart pt = new tblPart();
            pt.PartNumber = part.PartNumber;
            pt.PartName = part.PartName;
            pt.SerialNo = part.SerialNo;
            pt.Stock = 1;
            pt.PartTypeId = 1;
            pt.DelInd = false;
            SavePart(pt);
            return pt.PartId;
        }

        public void UpdateParts(tblPart part)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
            }
        }
        #endregion


        //new service for challan list Pagination by Himanshu
        public List<ChallanDto> GetInwardMaterialListV1(InwardMaterialViewModel vm)
        {
            List<ChallanDto> s = new List<ChallanDto>();
            string query = " select a.ChallanId, a.ChallanNumber as ChallanNumber," +
                            " a.DateCreated as DateCreated,b.CustomerName as CustomerName,a.ChallanDate as ChallanDate," +
                            "a.CustomerId as CustomerId,(select Top 1 TRY_CAST(OrderDate as nvarchar) " +
                            " from tblOrder o where a.ChallanId = o.ChallanId) as OrderDate from tblChallan a " +
                            " inner join tblCustomer as b on a.CustomerId = b.CustomerId  ";
            if (vm.fdate != null)
                if (vm.tdate != null)
                {
                    query += " and CAST(a.DateCreated as date) >= '" + vm.fdate + "' and CAST(a.DateCreated as date) <= '" + vm.tdate + "' ";
                }


            query += " order by a.ChallanId desc";
            PicannolEntities context = new PicannolEntities();
            s = context.Database.SqlQuery<ChallanDto>(query).Skip((vm.PageNo - 1) * vm.PageSize).Take(vm.PageSize).ToList<ChallanDto>();
            return s;
        }
        //End Here
        
        
        public override void Add(tblPart student)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(student);
            }
        }

        public override void Update(tblPart part)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                tblPart currentPart = Context.tblParts.Where(p => p.PartId == part.PartId && p.DelInd == false).FirstOrDefault();
                base.Update(currentPart);
                currentPart.Stock = currentPart.Stock - part.Stock;
            }
        }


        /*public int GetConsumptionOfPart(int PartId)
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            string strconnection = System.Configuration.ConfigurationManager.ConnectionStrings["ReportsDefaultConnection"].ToString();
            try
            {
                string Dateval = "2020-08-17";
                DateTime dt = DateTime.Parse(Dateval);

                PicannolEntities context = new PicannolEntities();
                int? consumption = 0;
                int OpeningStock = 0;
                var item = context.tblParts.Where(x => x.PartId == PartId).FirstOrDefault();
                //if (context.tblPurchases.Any(x => x.PartId == PartId))
                var sk = context.tblPurchases.Where(x => x.PartId == PartId && x.DateCreated > dt).ToList();
                if (sk != null && sk.Count >= 1)
                {
                    OpeningStock = sk.Sum(a => a.Quantity);
                    //OpeningStock = (int)item.OpeningStock + (int)OpeningStock;
                    OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock + (int)OpeningStock;
                }
                else
                {
                    //Comment By Sunit 01-Aug-2022 to handle null pointer exception

                    //OpeningStock = (int)item.OpeningStock;

                    //End

                    OpeningStock = item.OpeningStock==null ? 0 : (int)item.OpeningStock;
                }
                if(sk != null && sk.Count > 0)
                {
                    //if There is only One purchase
                    if(sk.Count == 1)
                    {
                        if(sk[0].Quantity == item.OpeningStock)
                        {
                            //OpeningStock = (int)item.OpeningStock;
                            OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock;
                        }
                        else
                        {
                            //OpeningStock = sk.Sum(x => x.Quantity) + (int)item.OpeningStock;
                            OpeningStock = sk.Sum(x => x.Quantity) + item.OpeningStock==null ? 0 : (int)item.OpeningStock;
                        }
                    }
                    else
                    {
                        if (sk[0].Quantity == item.OpeningStock)
                        {
                            OpeningStock = sk.Sum(x => x.Quantity);
                        }
                        else
                        {
                            OpeningStock = sk.Sum(x => x.Quantity) + (int)item.OpeningStock;
                        }
                    }
                }
                else

                {
                    if(item.OpeningStock == null)
                    {
                        return 0;
                    }
                    //OpeningStock = (int)item.OpeningStock == null?0:(int)item.OpeningStock ;
                    OpeningStock = (item.OpeningStock == null)?0: (int)item.OpeningStock;
                }
                using (SqlConnection conn = new SqlConnection(strconnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" select COALESCE(Sum(b.qty), 0) from tblOrder as a" +
                    " inner join tblOrderPart as b on a.OrderGUID = b.OrderGUID " +
                    " inner join tblInvoices as c on c.OrderGuid = a.OrderGuid " +
                    " where b.PartId = " + PartId + "and a.DelInd = 0 and b.DelInd = 0 and c.Delind = 0 and c.InvoiceDate > '2020-08-17' and c.Cancelled = 0 ", conn);
                    consumption = (int)cmd.ExecuteScalar();
                    conn.Close();
                }

                //return (int)(OpeningStock - consumption);

                int balanceStock = 0;
                if (OpeningStock == 0)
                {
                    balanceStock = (int)(item.Stock != null ? item.Stock : 0);
                }
                else
                {
                    balanceStock = (int)(OpeningStock - consumption);
                }
                return balanceStock;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in PartServices -GetConsumptionOfPart");
                throw ex;
            }
        }*/


        public int GetConsumptionOfPart(int? PartId)
        {
            Logger logger = LogManager.GetLogger("databaseLogger");
            string strconnection = ConfigurationManager.ConnectionStrings["ReportsDefaultConnection"].ToString();
            try
            {
                var openingStockDate = ConfigurationManager.AppSettings["StockUpdateDate"];

                //string Dateval = "2023-03-01";
                DateTime dt = DateTime.Parse(openingStockDate);

                PicannolEntities context = new PicannolEntities();
                int? consumption = 0;
                int OpeningStock = 0;
                var item = context.tblParts.Where(x => x.PartId == PartId).FirstOrDefault();
                var purchaseQty = context.tblPurchases.Where(x => x.PartId == PartId && x.DateCreated > dt).ToList();

                //Comment by Sunit
                /*if (sk != null && sk.Count >= 1)
                {
                    OpeningStock = sk.Sum(a => a.Quantity);
                    OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock + (int)OpeningStock;
                }
                else
                {
                    OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock;
                }*/

                //End



                //Coment by Sunit
                /*if (sk != null && sk.Count > 0)
                {
                    //if There is only One purchase
                    if (sk.Count == 1)
                    {
                        if (sk[0].Quantity == item.OpeningStock)
                        {
                            OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock;
                        }
                        else
                        {
                            OpeningStock = sk.Sum(x => x.Quantity) + item.OpeningStock == null ? 0 : (int)item.OpeningStock;
                        }
                    }
                    else
                    {
                        if (sk[0].Quantity == item.OpeningStock)
                        {
                            OpeningStock = sk.Sum(x => x.Quantity);
                        }
                        else
                        {
                            OpeningStock = sk.Sum(x => x.Quantity) + (int)item.OpeningStock;
                        }
                    }
                    //End
                }
                else{
                    if (item.OpeningStock == null)
                    {
                        return 0;
                    }
                    OpeningStock = (item.OpeningStock == null) ? 0 : (int)item.OpeningStock;
                }*/
                //end

                if (purchaseQty != null && purchaseQty.Count >= 1)
                {
                    OpeningStock = purchaseQty.Sum(a => a.Quantity);
                    OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock + (int)OpeningStock;
                }
                else
                {
                    OpeningStock = item.OpeningStock == null ? 0 : (int)item.OpeningStock;
                }

                OpeningStock = purchaseQty.Sum(x => x.Quantity) + (int)item.OpeningStock;

                using (SqlConnection conn = new SqlConnection(strconnection))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(" select COALESCE(Sum(b.qty), 0) from tblOrder as a" +
                    " inner join tblOrderPart as b on a.OrderGUID = b.OrderGUID " +
                    " inner join tblInvoices as c on c.OrderGuid = a.OrderGuid " +
                    " where b.PartId = " + PartId + "and a.DelInd = 0 and b.DelInd = 0 and c.Delind = 0 and c.InvoiceDate > '" + openingStockDate + "' and c.Cancelled = 0 ", conn);
                    consumption = (int)cmd.ExecuteScalar();
                    conn.Close();
                }

                //if opening stock is zero, opening stock is taken from the stock
                //else we have subtracted opening stock from consumption of part

                int balanceStock = 0;
                if (OpeningStock == 0)
                {
                    balanceStock = (int)(item.Stock != null ? item.Stock : 0);
                }
                else
                {
                    balanceStock = (int)(OpeningStock - consumption);
                }
                //return (int)(OpeningStock - consumption);
                
                //End
                return balanceStock;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in PartServices -GetConsumptionOfPart");
                throw ex;
            }
        }

        internal PartDto GetPartDetails(string partNumber)
        {

            var id = (from a in Context.tblParts
                      where a.PartNumber == partNumber && a.DelInd == false
                      select new PartDto
                      {
                          PartId = a.PartId,
                          PartName = a.PartName,
                          PartNumber = a.PartNumber,
                          Stock = a.Stock
                      }).FirstOrDefault();
            return id;
        }
    }
}