using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel
{
    public class AuthData
    {

        public string ClientId { get; set; }
        public string  UserName { get; set; }
        public string AuthToken { get; set; }
        public string Sek { get; set; }
        public string TokenExpiry { get; set; }
    }
}