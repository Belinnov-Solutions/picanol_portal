using Microsoft.Reporting.WebForms;
using Picanol.DataModel;
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
    public partial class InwardReportViewer : System.Web.UI.Page
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

                CustomerListReportViewer.Reset();
                DataSet ds = new DataSet();
                ds = GetSearchData();
                //if (!string.IsNullOrEmpty(LocationId != null ? LocationId : ""))
                //{
                //    ds = GetSearchData();

                //}


                //ReportViewer1.LocalReport.ReportPath = Server.MapPath("ChineseBirthdayReport.rdlc");
                CustomerListReportViewer.LocalReport.ReportPath = Server.MapPath("InwardReport.rdlc");
                CustomerListReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables[1]);
                CustomerListReportViewer.LocalReport.DataSources.Add(rds);
                CustomerListReportViewer.DataBind();
                CustomerListReportViewer.LocalReport.Refresh();

            }
            catch (SecurityException ex) {
            }


        }
        private DtInwardReport GetSearchData()
        {
            PicannolEntities context = new PicannolEntities();
            DataTable dt = new DataTable();
            dt.Columns.Add("January", typeof(int));
            dt.Columns.Add("February", typeof(int));
            dt.Columns.Add("March", typeof(int));
            dt.Columns.Add("April", typeof(int));
            dt.Columns.Add("May", typeof(int));
            dt.Columns.Add("June", typeof(int));
            dt.Columns.Add("July", typeof(int));
            dt.Columns.Add("August", typeof(int));
            dt.Columns.Add("September", typeof(int));
            dt.Columns.Add("October", typeof(int));
            dt.Columns.Add("November", typeof(int));
            dt.Columns.Add("December", typeof(int));
            dt.Columns.Add("CJanuary", typeof(int));
            DateTime sd = new DateTime(2019, 1, 1);
            DateTime ed = new DateTime(2019, 1, 31);

            var a = context.tblOrders.Where(x => x.DateCreated >= sd && x.DateCreated <= ed).Count();
            var b = context.tblOrders.Where(x => x.DateCreated >= sd && x.DateCreated <= ed && (x.Status == "Completed" || x.Status == "Dispatched")).Count();
            var c = context.tblOrders.Where(x => x.DateCreated >= sd && x.DateCreated <= ed && (x.Status == "Completed" || x.Status == "Dispatched") && x.AssignedUserId == 2).Count();

            DataRow row = dt.NewRow();
            row.SetField<int>("January", a);
            row.SetField<int>("CJanuary", b);
            row.SetField<int>("CJanuary", c);
            dt.Rows.Add(row);

            DtInwardReport ds = new DtInwardReport();
            ds.Tables.Add(dt);
            return ds;

        }
    
    
    }
}