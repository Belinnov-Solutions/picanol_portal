using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class ReqPlCancelIRN
    {
        public string Irn { get; set; }
        public string CnlRsn { get; set; }
        public string CnlRem { get; set; }
    }
}