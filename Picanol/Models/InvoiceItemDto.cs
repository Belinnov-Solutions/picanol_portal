using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class InvoiceItemDto
    {
        public int InvoiceItemID { get; set; }
        public string Name { get; set; }
        public string HSNCode { get; set; }
        public string UnitOfMeasurement { get; set; }
        public int Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountBTax { get; set; }
        public decimal OrgComponentsCost { get; set; }
        public decimal OrgLabourCost { get; set; }
        public int TotalComponentQty { get; set; }
        public decimal ComponentsCost { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal CGSTAmount   { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal Total { get; set; }
		//Dispatch Detail
		public string TrackingNo { get; set; }
		public string Company { get; set; }
		public string DisDate { get; set; }
		public string TrackingRef { get; set; }
        public decimal Packing { get; set; }
        public decimal Forwarding { get; set; }
        public decimal GSTRate { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal RepairCharges { get; set; }

        public string IsService { get; set; }


    }
   

}


                 