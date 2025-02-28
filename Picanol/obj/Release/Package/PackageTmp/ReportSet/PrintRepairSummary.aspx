<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PrintRepairSummary.aspx.cs" Inherits="Picanol.ReportSet.PrintRepairSummary" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" ScriptMode="Release"></asp:ScriptManager>  
    <rsweb:ReportViewer ID="PrintRepairSummaryReportViewr" runat="server" Width="100%"></rsweb:ReportViewer> 
    </div>
    </form>
</body>
</html>
