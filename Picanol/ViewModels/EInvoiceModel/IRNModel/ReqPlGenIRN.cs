using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class ReqPlGenIRN
    {
        /// <summary>
        ///Required Parameter - "Version of the schema"
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Required Parameter - Transaction Details
        /// </summary>
        public TranDetails TranDtls { get; set; }
        /// <summary>
        /// Required Parameter - Doccument Details
        /// </summary>
        public DocSetails DocDtls { get; set; }
        public ExpDetails ExpDtls { get; set; }
        /// <summary>
        /// Required Parameter - Seller Details
        /// </summary>
        public SellerDetails SellerDtls { get; set; }
        /// <summary>
        ///Required Parameter - Buyer Details
        /// </summary>
        public BuyerDetails BuyerDtls { get; set; }
        public DispatchedDetails DispDtls { get; set; }
        public ShippedDetails ShipDtls { get; set; }
        /// <summary>
        /// Required Parameter
        /// </summary>
        public ValDetails ValDtls { get; set; }
        public PayDetails PayDtls { get; set; }
        public RefDetails RefDtls { get; set; }
        public EwbDetails EwbDtls { get; set; }
        public class EwbDetails
        {
            /// <summary>
            /// "Transin/GSTIN"
            /// </summary>
            public string TransId { get; set; }
            /// <summary>
            /// "Name of the transporter"
            /// </summary>
            public string TransName { get; set; }
            /// <summary>
            ///Required Parameter - "Mode of transport (Road-1, Rail-2, Air-3, Ship-4)"
            /// </summary>
            public string TransMode { get; set; }
            /// <summary>
            ///Required Parameter - "Distance between source and destination PIN codes"
            /// </summary>
            public string Distance { get; set; }
            /// <summary>
            /// "Tranport Document Number"
            /// </summary>
            public string TransDocNo { get; set; }
            /// <summary>
            /// "Transport Document Date"
            /// </summary>
            public string TransDocDt { get; set; }
            /// <summary>
            /// "Vehicle Number"
            /// </summary>
            public string VehNo { get; set; }
            /// <summary>
            /// "Whether O-ODC or R-Regular "
            /// </summary>
            public string VehType { get; set; }
        }
        public List<AddlDocument> AddlDocDtls { get; set; }
        public class AddlDocument
        {
            /// <summary>
            /// "Supporting document URL"
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// "Supporting document in Base64 Format"
            /// </summary>
            public string Docs { get; set; }
            /// <summary>
            /// "Any additional information"
            /// </summary>
            public string Info { get; set; }
        }
        /// <summary>
        /// Required Parameter - Item List Details
        /// </summary>
        public List<EInvoiceItemList> ItemList { get; set; }
        

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
            public int Pin { get; set; }
            /// <summary>
            ///Required Parameter - "State Code to which supplies are shipped to."
            /// </summary>
            public string Stcd { get; set; }
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
            //public List<PrecDocument> PrecDocDtls { get; set; }

            public DocPerdDtl DocPerdDtls { get; set; }


            public class DocPerdDtl
            {
                public string InvRmk { get; set; }
                /// <summary>
                ///Required Parameter - "Invoice Period Start Date"
                /// </summary>
                public string InvStDt { get; set; }

                public string InvEndDt { get; set; }
            }

            
            public List<Contract> ContrDtls { get; set; }
            
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

            public Nullable<double> Discount { get; set; }

            public Nullable<double> OthChrg { get; set; }
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
            public int Pin { get; set; }
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

            public string Stcd { get; set; }

            
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
            public int Pin { get; set; }
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

            public string Stcd { get; set; }

            public string Phone { get; set; }
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

            public string IgstOnIntra { get; set; }
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
            public int Pin { get; set; }
            /// <summary>
            ///Required Parameter - "State Code"
            /// </summary>
            public string Stcd { get; set; }
        }
    }
}