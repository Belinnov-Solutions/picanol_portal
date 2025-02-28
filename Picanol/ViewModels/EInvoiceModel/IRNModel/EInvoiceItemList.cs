using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class EInvoiceItemList
    {
        
            //public string PrdNm { get; set; }
            /// <summary>
            ///Required Parameter - "Serial No. of Item"
            /// </summary>
            public string SlNo { get; set; }
            /// <summary>
            /// "Product Description"
            /// </summary>
            public string PrdDesc { get; set; }
            /// <summary>
            ///Required Parameter - "Specify whether the supply is service or not. Specify Y-for Service"
            /// </summary>
            public string IsServc { get; set; }
            /// <summary>
            ///Required Parameter - "HSN Code"
            /// </summary>
            public string HsnCd { get; set; }
            public BatchDetails BchDtls { get; set; }
            public class BatchDetails
            {
                /// <summary>
                ///Required Parameter - "Batch name"
                /// </summary>
                public string Nm { get; set; }
                /// <summary>
                /// "Batch Expiry Date"
                /// </summary>
                public string ExpDt { get; set; }
                /// <summary>
                /// "Warranty Date"
                /// </summary>
                public string WrDt { get; set; }

            }
            /// <summary>
            ///  "Bar Code"
            /// </summary>
            public string Barcde { get; set; }
            /// <summary>
            /// "Quantity"
            /// </summary>
            public double Qty { get; set; }
            /// <summary>
            /// "Free Quantity"
            /// </summary>
            public int FreeQty { get; set; }
            /// <summary>
            /// "Unit"
            /// </summary>
            public string Unit { get; set; }
            /// <summary>
            ///Required Parameter - "Unit Price - Rate"
            /// </summary>
            public double UnitPrice { get; set; }
            /// <summary>
            ///Required Parameter - "Gross Amount Amount (Unit Price * Quantity)"
            /// </summary>
            public double TotAmt { get; set; }
            /// <summary>
            /// "Discount"
            /// </summary>
            public double Discount { get; set; }
            /// <summary>
            /// "Pre tax value"
            /// </summary>
            public double PreTaxVal { get; set; }
            /// <summary>
            ///Required Parameter - "Taxable Value (Total Amount -Discount)"
            /// </summary>
            public double AssAmt { get; set; }
            /// <summary>
            ///Required Parameter - "The GST rate, represented as percentage that applies to the invoiced item. It will IGST rate only."
            /// </summary>
            public double GstRt { get; set; }
            /// <summary>
            /// " Amount of IGST payable."
            /// </summary>
            public double IgstAmt { get; set; }
            /// <summary>
            /// " Amount of CGST payable."
            /// </summary>
            public double CgstAmt { get; set; }
            /// <summary>
            /// " Amount of SGST payable."
            /// </summary>
            public double SgstAmt { get; set; }
            /// <summary>
            /// "Cess Rate"
            /// </summary>
            public double CesRt { get; set; }
            /// <summary>
            /// "Cess Amount(Advalorem) on basis of rate and quantity of item"
            /// </summary>
            public double CesAmt { get; set; }
            /// <summary>
            /// "Cess Non-Advol Amount"
            /// </summary>
            public double CesNonAdvlAmt { get; set; }
            /// <summary>
            /// "State CESS Rate"
            /// </summary>
            public double StateCesRt { get; set; }
            /// <summary>
            /// "State CESS Amount"
            /// </summary>
            public double StateCesAmt { get; set; }
            /// <summary>
            ///  "State CESS Non Adval Amount"
            /// </summary>
            public double StateCesNonAdvlAmt { get; set; }
            /// <summary>
            /// "Other Charges"
            /// </summary>
            public double OthChrg { get; set; }
            /// <summary>
            ///TotItemVal "Total Item Value = Assessable Amount + CGST Amt + SGST Amt + Cess Amt + CesNonAdvlAmt + StateCesAmt + StateCesNonAdvlAmt+Otherchrg"
            /// </summary>
            public double TotItemVal { get; set; }
            /// <summary>
            /// "Order line referencee"
            /// </summary>
            public string OrdLineRef { get; set; }
            /// <summary>
            /// "Orgin Country"
            /// </summary>
            public string OrgCntry { get; set; }
            /// <summary>
            /// "Serial number in case of each item having a unique number."
            /// </summary>
            public string PrdSlNo { get; set; }
            public List<AttributeDtls> AttribDtls { get; set; }
            
        
    }
}