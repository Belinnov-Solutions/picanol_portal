using Microsoft.Reporting.WebForms;
using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Picanol.ReportSet
{
    public partial class PrintRepairSummary : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
            }

            if (!IsPostBack)
            {
                ReportDetails();
            }
        }
        public void ReportDetails()
        {
            try
            {
                DataSet ds = new DataSet();
                ds = GetSearchData();
                PrintRepairSummaryReportViewr.Reset();
                PrintRepairSummaryReportViewr.LocalReport.ReportPath = Server.MapPath("PrintRepairSummary.rdlc");
                PrintRepairSummaryReportViewr.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("PrintRepair", ds.Tables[1]);
                PrintRepairSummaryReportViewr.LocalReport.DataSources.Add(rds);
                PrintRepairSummaryReportViewr.DataBind();
                PrintRepairSummaryReportViewr.LocalReport.Refresh();
            }
            catch (SecurityException ex)
            {
                throw ex;
            }
        }

        private PrintRepair GetSearchData()
        {
            string searchText = string.Empty;
            int? enginnerName = 0;
            var repairType = string.Empty;
            searchText = Request.QueryString["Dates"].ToString();
            var datearray = searchText.Split(';');
            if (datearray[3] != null)
                repairType = datearray[3];
            if (datearray[2] != null)
                enginnerName = ConvertStringToInt(datearray[2]);
            var fdate = datearray[0];
            var farray = fdate.Split('-');
            var fd = farray[2] + '-' + farray[1] + '-' + farray[0];
            var tdate = datearray[1];
            var tarray = tdate.Split('-');
            var cmpDate = ConvertStringToInt(tarray[2]);
            var td = tarray[2] + '-' + tarray[1] + '-' + tarray[0];

            PicannolEntities context = new PicannolEntities();
            DataTable dt = new DataTable();
            dt.Columns.Add("Inward", typeof(int));
            dt.Columns.Add("Month", typeof(string));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("RepairType", typeof(string));
            dt.Columns.Add("Count", typeof(int));
            dt.Columns.Add("Mth", typeof(int));
            dt.Columns.Add("InvoiceCount", typeof(int));
            
            //fetch PI list
            var query = "Select DateName(month,i.DateCreated) As Month,u.UserName, " +
                        "o.RepairType, month(i.datecreated) As Mth, Count(*) As InvoiceCount" +
                        " from tblProformaInvoices i inner join tblOrder o on o.OrderGUID = i.OrderGuid " +
                        "and o.RepairType!='FOC' and o.RepairType != 'REPAIR WARRANTY' and RepairType!='RepairWarranty'" +
                        " inner join tblUser u on u.UserId = o.AssignedUserId where i.DelInd = 0 " +
                        "and  cast(i.DateCreated as date) >='" + fd + "'and cast(i.DateCreated as date) <='" + td + "'";


            if (repairType != null && repairType != "")
                query += " and o.RepairType = '" + repairType + "'";
            if (enginnerName != 0 && enginnerName != null)
                query += " and o.AssignedUserId = " + enginnerName;

            query += " group by DateName(month, i.DateCreated), u.UserName, " +
                     "o.RepairType,month(i.datecreated) order by month(i.datecreated) asc";

            var piFilterList = context.Database.SqlQuery<RepairSummaryDto>(query).ToList();

            //End

            //fetch Invoice List count
            var invQuery = "Select DateName(month,i.DateCreated) As Month,u.UserName, o.RepairType," +
                           " month(i.datecreated) As Mth, Count(*) As Count" +
                           " from tblInvoices i inner join tblOrder o on o.OrderGUID = i.OrderGuid" +
                           " inner join tblUser u on u.UserId = o.AssignedUserId where i.DelInd = 0 " +
                           "and  cast(i.DateCreated as date) >='" + fd + "'and cast(i.DateCreated as date) <='" + td + "'";

            if (repairType != null && repairType != "")
                invQuery += " and o.RepairType = '" + repairType + "'";
            if (enginnerName != 0 && enginnerName != null)
                invQuery += " and o.AssignedUserId = " + enginnerName;

            invQuery += " group by DateName(month, i.DateCreated), u.UserName, o.RepairType," +
                          "month(i.datecreated) order by month(i.datecreated) asc";
            
            var invoiceFilterList = context.Database.SqlQuery<RepairSummaryDto>(invQuery).ToList();
            
            //End


            List<RepairSummaryDto> result = new List<RepairSummaryDto>();
            List<RepairSummaryDto> allList = new List<RepairSummaryDto>();
            result = invoiceFilterList.Concat(piFilterList).ToList();
            List<RepairSummaryDto> res = result.Where(f => !invoiceFilterList.Any(t => t.RepairType == f.RepairType && t.UserName == f.UserName)).ToList();
            if (res.Count > 0)
            {
                allList = invoiceFilterList.Concat(res).ToList();
            }
            else
            {
                allList = invoiceFilterList;
            }

            /*foreach (var item in filterList)*/
            foreach (var item in allList)
            {
                var inward = context.tblOrders.Where(x => x.OrderDate.Month == item.Mth 
                             && x.OrderDate.Year == cmpDate && x.DelInd == false).Count();
                
                int invoicePartCount = 0;

                DataRow row = dt.NewRow();
                row.SetField<int>("Inward", inward);
                row.SetField<string>("Month", item.Month);
                row.SetField<string>("UserName", item.UserName);
                row.SetField<string>("RepairType", item.RepairType);
                row.SetField<int>("Mth", item.Mth);
                if (piFilterList.Any(x => x.UserName == item.UserName && x.RepairType == item.RepairType))
                {
                    invoicePartCount = piFilterList.Where(x => x.UserName == item.UserName && x.RepairType == item.RepairType)
                        .Select(x => x.InvoiceCount).FirstOrDefault();

                }
                row.SetField<int>("InvoiceCount", item.Count);
                row.SetField<int>("Count", invoicePartCount);
                dt.Rows.Add(row);
            }
            //dt.DefaultView.Sort = " Mth ASC";
            PrintRepair pr = new PrintRepair();
            pr.Tables.Add(dt);
            return pr;
        }

        private int? ConvertStringToInt(string intString)
        {
            int i = 0;
            return (Int32.TryParse(intString, out i) ? i : (int?)null);
        }
    }
}