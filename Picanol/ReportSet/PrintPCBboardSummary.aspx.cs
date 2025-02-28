using Microsoft.Reporting.WebForms;
using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Picanol.ReportSet
{
    public partial class PrintPCBboardSummary : System.Web.UI.Page
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
                PrintPCBBoardSummaryViewer.Reset();
                PrintPCBBoardSummaryViewer.LocalReport.ReportPath = Server.MapPath("PrintRepairSummary.rdlc");
                PrintPCBBoardSummaryViewer.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("PrintPCBboard", ds.Tables[1]);
                PrintPCBBoardSummaryViewer.LocalReport.DataSources.Add(rds);
                PrintPCBBoardSummaryViewer.DataBind();
                PrintPCBBoardSummaryViewer.LocalReport.Refresh();
            }
            catch (SecurityException ex)
            {
                throw ex;
            }
        }

        private PrintPCBboard GetSearchData()
        {
            
            PicannolEntities context = new PicannolEntities();
            DataTable dt = new DataTable();
            dt.Columns.Add("Month", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Total", typeof(int));
            dt.Columns.Add("Chargeable", typeof(int));
            dt.Columns.Add("RepairWarranty", typeof(int));
            dt.Columns.Add("UnRepairedBoards", typeof(int));
            dt.Columns.Add("FOC", typeof(int));
            dt.Columns.Add("NoRepairWarranty", typeof(int));
            dt.Columns.Add("Loan", typeof(int));
            var boardlist = context.tblParts.Where(x => x.PartId == 1 && x.DelInd == false).ToList();
            foreach (var item in boardlist)
            {
                int total = GetPartCountByPartId(item.PartId);

            }
            PrintPCBboard pr = new PrintPCBboard();
            pr.Tables.Add(dt);
            return pr;

        }
        private int GetPartCountByPartId(int pid)
        {
            PicannolEntities context = new PicannolEntities();
            var partTotalCount = context.tblOrders.Where(x => x.PartId == pid).ToList();
            int totalCount = partTotalCount.Count();
            return totalCount;
        }

    }
}