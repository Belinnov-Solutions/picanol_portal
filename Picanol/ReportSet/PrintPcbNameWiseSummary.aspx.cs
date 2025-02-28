using Microsoft.Reporting.WebForms;
using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Picanol.ReportSet
{
    public partial class PrintPcbNameWiseSummary : System.Web.UI.Page
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
                PCBNameWiseReport.Reset();
                PCBNameWiseReport.LocalReport.ReportPath = Server.MapPath("PrintPcbNameSummary.rdlc");
                PCBNameWiseReport.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("PrintPcbName", ds.Tables[1]);
                PCBNameWiseReport.LocalReport.DataSources.Add(rds);
                PCBNameWiseReport.DataBind();
                PCBNameWiseReport.LocalReport.Refresh();
            }
            catch (SecurityException ex)
            {
                throw ex;
            }
        }
        private PrintPcbName GetSearchData()
        {
            string searchText = string.Empty;
            int? enginnerName = 0;
            var repairType = string.Empty;
            searchText = Request.QueryString["Dates"].ToString();
            var datearray = searchText.Split(';');

            if (datearray[2] != null && datearray[2] !="")
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
            dt.Columns.Add("PartName", typeof(string));
            dt.Columns.Add("RepairType", typeof(string));
            dt.Columns.Add("Count", typeof(int));
           
            var query = "Select p.PartName, o.RepairType, SUM(o.Qty) As Qty" + 
                " from tblOrder o inner join tblPart p on p.PartId = o.PartId where cast(o.OrderDate as date) >='" + fd + "'and cast(o.OrderDate as date) <='" + td + "'";

            if (enginnerName != 0 && enginnerName != null)
                query += " and o.CustomerId = " + enginnerName;

            query += " group by p.PartName,o.RepairType";
            var filterList = context.Database.SqlQuery<PcbNameSummaryDto>(query).ToList();
            filterList.RemoveAll(x => x.RepairType == "Loan");
            foreach (var item in filterList)
            {
                DataRow row = dt.NewRow();
                row.SetField<string>("PartName", item.PartName);
               
                row.SetField<string>("RepairType", item.RepairType);
                
                row.SetField<int>("Count", item.Qty);
                
                dt.Rows.Add(row);
            }
           
            PrintPcbName pr = new PrintPcbName();
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