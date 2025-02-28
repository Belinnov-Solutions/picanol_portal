using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel
{
    public class AuthResponse
    {

        public int Status { get; set; }
        public AuthData Data { get; set; }
        public List<AuthErrorDetail> ErrorDetails { get; set; }
        public List<InfoDetails> InfoDtls { get; set; }

        public AuthResponse()
        {
            Data = new AuthData();
            ErrorDetails = new List<AuthErrorDetail>();
            InfoDtls = new List<InfoDetails>();
        }

    }

}