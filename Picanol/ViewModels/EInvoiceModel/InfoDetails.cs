using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;

namespace Picanol.ViewModels.EInvoiceModel
{
    public class InfoDetails
    {
        public string InfCd { get; set; }
        public DecriptionDtl Desc { get; set; }

        public InfoDetails()
        {

            Desc = new DecriptionDtl();
        }
    }

    
}