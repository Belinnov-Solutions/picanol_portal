//using Microsoft.Reporting.WebForms;
using Microsoft.Reporting.WebForms;
using Picanol.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Web.UI.WebControls;

namespace Picanol.ReportSet
{
    public partial class StockReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                ReportDetails();
            }
        }
        public void ReportDetails()
        {
            try
            {
                CustomerListReportViewer.Reset();
                DataSet ds = new DataSet();
                ds = GetSearchData();
                CustomerListReportViewer.LocalReport.ReportPath = Server.MapPath("StockReport.rdlc");
                CustomerListReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables[1]);
                CustomerListReportViewer.LocalReport.DataSources.Add(rds);
                CustomerListReportViewer.DataBind();
                CustomerListReportViewer.LocalReport.Refresh();
            }
            catch (SecurityException ex)
            {
            }
        }



        private DtStockReport GetSearchData()
        {
            var stockUpdateDate = ConfigurationManager.AppSettings["StockUpdateDate"];

            string strconnection = ConfigurationManager.ConnectionStrings["ReportsDefaultConnection"].ToString();
            string searchText = string.Empty;
            string series = string.Empty;
            //Taking Dates Data
            searchText = Request.QueryString["Dates"].ToString();
            series = Request.QueryString["series"].ToString();
            var datearray = searchText.Split(';');
            var fdate = datearray[0];
            var farray = fdate.Split('/');
            var fd = farray[2] + '/' + farray[1] + '/' + farray[0];
            var tdate = datearray[1];
            var tarray = tdate.Split('/');
            var td = tarray[2] + '/' + tarray[1] + '/' + tarray[0];
            //Added by Ritu 23Sep20
            DateTime fromDate = DateTime.Parse(fdate);
            DateTime toDate = DateTime.Parse(tdate);
            TimeSpan ts = new TimeSpan(23, 59, 59);
            toDate = toDate + ts;
            if (!string.IsNullOrEmpty(fdate))
            {
                string[] Separate = fdate.Split(' ');
                fdate = Separate[0];
                fdate = fdate.Split('/')[2] + '-' + fdate.Split('/')[1] + '-' + fdate.Split('/')[0];
            }

            if (!string.IsNullOrEmpty(tdate))
            {
                string[] Separate = tdate.Split(' ');
                tdate = Separate[0];
                tdate = tdate.Split('/')[2] + '-' + tdate.Split('/')[1] + '-' + tdate.Split('/')[0];
                //strToDate = strToDate + " " + Separate[1];
            }


            PicannolEntities context = new PicannolEntities();
            DataTable dt = new DataTable();
            dt.Columns.Add("PartName", typeof(string));
            dt.Columns.Add("PartNumber", typeof(string));
            dt.Columns.Add("OpeningBalance", typeof(int));
            dt.Columns.Add("Purchase", typeof(int));
            dt.Columns.Add("Consumption", typeof(int));

            //Added by Ritu 23Sep20
            List<tblPart> p = new List<tblPart>();

            if (!string.IsNullOrEmpty(series))
            {
                p = context.tblParts.Where(x => x.PartTypeId == 2 && x.PartNumber.Contains(series)).ToList();
            }
            else
            {
                p = context.tblParts.Where(x => x.PartTypeId == 2).ToList();

            }
            foreach (var item in p)
            {
                try
                {
                    //DateTime od = Convert.ToDateTime("2020-07-31");
                    DateTime od = Convert.ToDateTime(stockUpdateDate);


                    //Added by Ritu 23Sep20
                    //PurchseCountCode

                    int totalPurchseQty = context.tblPurchases.Where(x => x.PartId == item.PartId && x.DelInd == false && x.DateCreated != null && x.DateCreated >= fromDate && x.DateCreated <= toDate && x.PurchaseDate > od).Sum(x => (int?)x.Quantity) ?? 0;

                    //End

                    //Calculate part consumption

                    int os = 0;
                    int consumption = 0;
                    if (context.tblOrderParts.Any(x => x.PartId == item.PartId && x.DateCreated != null
                     && x.DateCreated >= fromDate && x.DateCreated <= toDate))
                    {
                        using (SqlConnection conn = new SqlConnection(strconnection))
                        {
                            conn.Open();

                            SqlCommand cmd = new SqlCommand(" select COALESCE(Sum(b.qty), 0) from tblOrder as a" +
                               " inner join tblOrderPart as b on a.OrderGUID = b.OrderGUID " +
                               " inner join tblInvoices as c on c.OrderGuid = a.OrderGuid " +
                               " where b.PartId = " + item.PartId + "and a.DelInd = 0 and b.DelInd = 0 and c.Delind = 0 and c.Cancelled = 0  and CAST(b.DateCreated as date) >='" + fdate + "' AND  CAST(b.DateCreated as date) <= '" + tdate + "' ", conn);

                            object result = cmd.ExecuteScalar();
                            result = (result == DBNull.Value) ? null : result;
                            consumption = Convert.ToInt32(result);
                            conn.Close();
                        }
                    }
                    //Em=nd

                    //

                    //Calculate opening stock

                    int OpeningStock = 0;
                    {

                        using (SqlConnection conn = new SqlConnection(strconnection))
                        {
                            conn.Open();

                            SqlCommand command = new SqlCommand("select OpeningStock - (select COALESCE(Sum(Qty),0) as counts from tblInvoices as a " +
                           " inner join tblOrderPart as b on a.OrderGuid = b.OrderGUID" +
                           " where PartId = " + item.PartId + " and  a.Cancelled=0 and a.Delind = 0 and b.Delind = 0 and a.InvoiceDate > '" + stockUpdateDate + "' and  a.InvoiceDate <'" + fdate + "') + " +
                           "(select COALESCE (SUM(Quantity),0) as counts from tblPurchase where DelInd = 0 and PartId = " + item.PartId + " and CAST(DateCreated as date) > '" + stockUpdateDate + "' and  CAST(DateCreated as date)<'" + fdate + "' ) " +
                           " as counts from tblPart where PartId  =" + item.PartId + "", conn);

                            object result = command.ExecuteScalar();
                            result = (result == DBNull.Value) ? null : result;
                            OpeningStock = Convert.ToInt32(result);
                            conn.Close();

                        }
                    }

                    //End

                    DataRow row = dt.NewRow();
                    row.SetField<string>("PartName", item.PartName);
                    row.SetField<string>("PartNumber", item.PartNumber);
                    row.SetField<int>("OpeningBalance", OpeningStock);     // item.OpeningStock == null ? 0 : (int)item.OpeningStock);
                    row.SetField<int>("Purchase", totalPurchseQty == null ? 0 : (int)totalPurchseQty);
                    row.SetField<int>("Consumption", consumption);
                    dt.Rows.Add(row);

                }
                catch (Exception ex)
                {
                    var a = item.PartId;
                    throw ex;
                }

            }
            DtStockReport ds = new DtStockReport();
            ds.Tables.Add(dt);
            return ds;
        }
    }
}

