using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class RespPlCancelIRN
    {
        public string Irn { get; set; }
        public string CancelDate { get; set; }

        public string errorMsg { get; set; }

        public List<AuthErrorDetail> ErrorDetails { get; set; }

        public List<InfoDetails> InfoDtls { get; set; }
    }
}