using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class RespPlGenIRN
    {
        public string AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public string Status { get; set; }
        public string EwbNo { get; set; }
        public string EwbDt { get; set; }
        public string EwbValidTill { get; set; }
        public RespGenIRNInvData ExtractedSignedInvoiceData { get; set; }
        public RespGenIRNQrCodeData ExtractedSignedQrCode { get; set; }
        public Bitmap QrCodeImage { get; set; }
        public string JwtIssuer { get; set; }
    }

    public class RespGenIRNInvData
    {
        public long AckNo { get; set; }
        public string AckDt { get; set; }
        public string Version { get; set; }
        public string Irn { get; set; }
        public TranDetails TranDtls { get; set; }
        public DocSetails DocDtls { get; set; }
        public SellerDetails SellerDtls { get; set; }

        public BuyerDetails BuyerDtls { get; set; }
        public DispatchedDetails DispDtls { get; set; }
        public ShippedDetails ShipDtls { get; set; }
        public ValDetails ValDtls { get; set; }
        public PayDetails PayDtls { get; set; }
        public ExpDetails ExpDtls { get; set; }
        public RefDetails RefDtls { get; set; }
        public List<ItmList> ItemList { get; set; }
        public class ItmList
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
            public string Qty { get; set; }
            /// <summary>
            /// "Free Quantity"
            /// </summary>
            public string FreeQty { get; set; }
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
            public class AttributeDtls
            {
                /// <summary>
                /// "Attribute details of the item"
                /// </summary>
                public string Nm { get; set; }
                /// <summary>
                /// "Attribute value of the item"
                /// </summary>
                public string Val { get; set; }
            }
        }
        public class RefDetails
        {
            /// <summary>
            /// "Remarks/Note"
            /// </summary>
            public string InvRmk { get; set; }
            /// <summary>
            ///Required Parameter - "Invoice Period Start Date"
            /// </summary>
            public string InvStDt { get; set; }
            /// <summary>
            ///Required Parameter - "Invoice Period End Date"
            /// </summary>
            public string InvEndDt { get; set; }
            public PrecDocument PrecDocDtls { get; set; }
            public class PrecDocument
            {
                /// <summary>
                ///Required Parameter - "Reference of original invoice, if any."
                /// </summary>
                public string InvNo { get; set; }
                /// <summary>
                ///Required Parameter - "Date of preceding invoice"
                /// </summary>
                public string InvDt { get; set; }
                /// <summary>
                /// "Other Reference"
                /// </summary>
                public string OthRefNo { get; set; }
            }
            public List<Contract> ContrDtls { get; set; }
            public class Contract
            {
                /// <summary>
                /// "Receipt Advice No."
                /// </summary>
                public string RecAdvDt { get; set; }
                /// <summary>
                /// "Date of receipt advice"
                /// </summary>
                public string RecAdvRefr { get; set; }
                /// <summary>
                /// "Lot/Batch Reference No."
                /// </summary>
                public string TendRefr { get; set; }
                /// <summary>
                /// "Contract Reference Number"
                /// </summary>
                public string ContrRefr { get; set; }
                /// <summary>
                ///  "Any other reference"
                /// </summary>
                public string ExtRefr { get; set; }
                /// <summary>
                /// "Project Reference Number"
                /// </summary>
                public string ProjRefr { get; set; }
                /// <summary>
                /// "Vendor PO Reference Number"
                /// </summary>
                public string PORefr { get; set; }
                /// <summary>
                /// "Vendor PO Reference date"
                /// </summary>
                public string PORefDt { get; set; }

            }
        }
        public class PayDetails
        {
            /// <summary>
            ///  "Payee Name"
            /// </summary>
            public string Nm { get; set; }
            /// <summary>
            /// "Bank account number of payee"
            /// </summary>
            public string AcctDet { get; set; }
            /// <summary>
            /// "Mode of Payment: Cash, Credit, Direct Transfer"
            /// </summary>
            public string Mode { get; set; }
            /// <summary>
            /// "Branch or IFSC code"
            /// </summary>
            public string FinInsBr { get; set; }
            /// <summary>
            /// "Credit Transfer"
            /// </summary>
            public string CrTrn { get; set; }
            /// <summary>
            /// "Payment Instruction"
            /// </summary>
            public string PayInstr { get; set; }
            /// <summary>
            /// "Terms of Payment"
            /// </summary>
            public string PayTerm { get; set; }
            /// <summary>
            /// "Direct Debit"
            /// </summary>
            public string DirDr { get; set; }
            /// <summary>
            /// "Credit Days"
            /// </summary>
            public int CrDay { get; set; }
            /// <summary>
            /// "The sum of amount which have been paid in advance."
            /// </summary>
            public double PaidAmt { get; set; }
            /// <summary>
            /// "Outstanding amount that is required to be paid."
            /// </summary>
            public double PaymtDue { get; set; }
        }

        public class ShippedDetails
        {
            /// <summary>
            ///"GSTIN of entity to whom goods are shipped"
            /// </summary>
            public string Gstin { get; set; }
            /// <summary>
            ///Required Parameter - "Legal Name"
            /// </summary>
            public string LglNm { get; set; }
            /// <summary>
            /// "Tradename"
            /// </summary>
            public string TrdNm { get; set; }
            /// <summary>
            ///Required Parameter - "Building/Flat no, Road/Street"
            /// </summary>
            public string Addr1 { get; set; }
            /// <summary>
            /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
            /// </summary>
            public string Addr2 { get; set; }
            //public string Bno { get; set; }
            //public string Bnm { get; set; }
            //public string Flno { get; set; }
            /// <summary>
            ///Required Parameter - "Location"
            /// </summary>
            public string Loc { get; set; }
            //public string Dst { get; set; }
            /// <summary>
            ///Required Parameter - "Pincode"
            /// </summary>
            public string Pin { get; set; }
            /// <summary>
            ///Required Parameter - "State Code to which supplies are shipped to."
            /// </summary>
            public string Stcd { get; set; }
        }
        public class ValDetails
        {
            /// <summary>
            ///Required Parameter - "Total Assessable value of all items"
            /// </summary>
            public Nullable<double> AssVal { get; set; }
            /// <summary>
            /// "Total CGST value of all items"
            /// </summary>
            public Nullable<double> CgstVal { get; set; }
            /// <summary>
            /// "Total SGST value of all items"
            /// </summary>
            public Nullable<double> SgstVal { get; set; }
            /// <summary>
            /// "Total IGST value of all items"
            /// </summary>
            public Nullable<double> IgstVal { get; set; }
            /// <summary>
            /// "Total CESS value of all items"
            /// </summary>
            public Nullable<double> CesVal { get; set; }
            //public Nullable<double> CesNonAdVal { get; set; }
            /// <summary>
            /// "Total State CESS value of all items"
            /// </summary>
            public Nullable<double> StCesVal { get; set; }
            /// <summary>
            /// "Rounded off amount"
            /// </summary>
            public Nullable<double> RndOffAmt { get; set; }
            /// <summary>
            ///Required Parameter - "Final Invoice value "
            /// </summary>
            public Nullable<double> TotInvVal { get; set; }
            /// <summary>
            /// "Final Invoice value in Additional Currency"
            /// </summary>
            public Nullable<double> TotInvValFc { get; set; }
        }
        public class SellerDetails
        {
            /// <summary>
            ///Required Parameter - "GSTIN"
            /// </summary>
            public string Gstin { get; set; }
            /// <summary>
            ///Required Parameter - "Legal Name"
            /// </summary>
            public string LglNm { get; set; }
            /// <summary>
            /// "Tradename"
            /// </summary>
            public string TrdNm { get; set; }
            /// <summary>
            ///Required Parameter - "Building/Flat no, Road/Street"
            /// </summary>
            public string Addr1 { get; set; }
            /// <summary>
            /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
            /// </summary>
            public string Addr2 { get; set; }
            //public string Bno { get; set; }
            //public string Bnm { get; set; }
            //public string Flno { get; set; }
            /// <summary>
            ///Required Parameter - "Location"
            /// </summary>
            public string Loc { get; set; }
            //public string Dst { get; set; }
            /// <summary>
            ///Required Parameter - "Pincode"
            /// </summary>
            public string Pin { get; set; }
            /// <summary>
            ///Required Parameter - "State Name"
            /// </summary>
            public string State { get; set; }
            //public int Stcd { get; set; }
            /// <summary>
            /// "Phone or Mobile No."
            /// </summary>
            public string Ph { get; set; }
            /// <summary>
            /// "Email-Id"
            /// </summary>
            public string Em { get; set; }
        }
        public class BuyerDetails
        {
            /// <summary>
            ///Required Parameter - "GSTIN"
            /// </summary>
            public string Gstin { get; set; }
            /// <summary>
            ///Required Parameter - "Legal Name"
            /// </summary>
            public string LglNm { get; set; }
            /// <summary>
            /// "Tradename"
            /// </summary>
            public string TrdNm { get; set; }

            /// <summary>
            ///Required Parameter - "State code of Place of supply. If POS lies outside the country, a the code shall be 96."
            /// </summary>
            public string Pos { get; set; }
            /// <summary>
            ///Required Parameter - "Building/Flat no, Road/Street"
            /// </summary>
            public string Addr1 { get; set; }
            /// <summary>
            /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
            /// </summary>
            public string Addr2 { get; set; }
            //public string Bno { get; set; }
            //public string Bnm { get; set; }
            //public string Flno { get; set; }
            /// <summary>
            ///Required Parameter - "Location"
            /// </summary>
            public string Loc { get; set; }
            //public string Dst { get; set; }
            /// <summary>
            ///"Pincode"
            /// </summary>
            public string Pin { get; set; }
            /// <summary>
            ///"State Name"
            /// </summary>
            public string State { get; set; }
            //public int Stcd { get; set; }
            /// <summary>
            /// "Phone or Mobile No."
            /// </summary>
            public string Ph { get; set; }
            /// <summary>
            /// "Email-Id"
            /// </summary>
            public string Em { get; set; }
        }
        public class ExpDetails
        {
            /// <summary>
            /// "Shipping Bill No."
            /// </summary>
            public string ShipBNo { get; set; }
            /// <summary>
            ///  "Shipping Bill Date"
            /// </summary>
            public string ShipBDt { get; set; }
            /// <summary>
            /// "Country Code"
            /// </summary>
            public string CntCode { get; set; }
            /// <summary>
            /// "Additional Currency Code"
            /// </summary>
            public string ForCur { get; set; }
            public Nullable<double> InvForCur { get; set; }
            /// <summary>
            /// "Port Code"
            /// </summary>
            public string Port { get; set; }
            /// <summary>
            ///  "Options for supplier for refund. Y/N"
            /// </summary>
            public string RefClm { get; set; }
        }
        public class DocSetails
        {
            /// <summary>
            /// Required Parameter - "description": "Document Type: INVOICE, CREDIT NOTE, DEBIT NOTE"
            /// </summary>
            public string Typ { get; set; }
            /// <summary>
            ///Required Parameter - "Document Number"
            /// </summary>
            public string No { get; set; }
            /// <summary>
            ///Required Parameter - "Document Date"
            /// </summary>
            public string Dt { get; set; } //
            //public string OrgInvNo { get; set; }
        }
        public class TranDetails
        {
            /// <summary>
            /// Required Parameter - "GST- Goods and Services Tax Scheme"
            /// </summary>
            public string TaxSch { get; set; }
            /// <summary>
            /// Required Parameter - supply Type
            /// </summary>
            public string SupTyp { get; set; }
            //public string Catg { get; set; }
            /// <summary>
            /// "Y- whether the tax liability is payable under reverse charge"
            /// </summary>
            public string RegRev { get; set; }
            //public string Typ { get; set; }
            //public string EcmTrn { get; set; }
            /// <summary>
            /// "GSTIN of e-Commerce operator"
            /// </summary>
            public string EcmGstin { get; set; }
            //public bool EcmTrnSel { get; set; }
        }
        public class DispatchedDetails
        {
            /// <summary>
            ///Required Parameter - "Name of the company from which the goods are dispatched"
            /// </summary>
            public string Nm { get; set; }
            /// <summary>
            ///Required Parameter - "Address 1 of the entity from which goods are dispatched.(Building/Flat no.Road/Street etc.)"
            /// </summary>
            public string Addr1 { get; set; }
            /// <summary>
            /// "Address 2 of the entity from which goods are dispatched. (Floor no., Name of the premises/building)"
            /// </summary>
            public string Addr2 { get; set; }
            /// <summary>
            ///Required Parameter - "Location"
            /// </summary>
            public string Loc { get; set; }
            /// <summary>
            ///Required Parameter - "Pincode"
            /// </summary>
            public string Pin { get; set; }
            /// <summary>
            ///Required Parameter - "State Code"
            /// </summary>
            public string Stcd { get; set; }
        }
    }
    public class RespGenIRNQrCodeData
    {
        public string SellerGstin { get; set; }
        public string BuyerGstin { get; set; }
        public string DocNo { get; set; }
        public string DocTyp { get; set; }
        public string DocDt { get; set; }
        public Nullable<double> TotInvVal { get; set; }
        public string ItemCnt { get; set; }
        public string MainHsnCode { get; set; }
        public string Irn { get; set; }
    }
}
