using Newtonsoft.Json;
using NLog;
using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using Picanol.ViewModels;
using Picanol.ViewModels.EInvoiceModel;
using Picanol.ViewModels.EInvoiceModel.IRNModel;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows.Forms;

namespace Picanol.Helpers
{
    public class EInvoiceHelper
    {
        ReqPlGenIRN reqPlGenIRN = new ReqPlGenIRN();
        string eInvoiceURL = ConfigurationManager.AppSettings["eInvoiceAPIURL"];
        PicannolEntities context = new PicannolEntities();
        string formatedDate = "";
        private readonly CustomerHelper _customerHelper;
        UserSession userInfo = (UserSession)HttpContext.Current.Session["UserInfo"];


        public EInvoiceHelper()
        {

            _customerHelper = new CustomerHelper(this);
        }


        public ReqPlGenIRN GetEInVoiceTransDtl(CustomerDto customerDetails, InvoiceDto invoiceDetails, List<InvoiceItemDto> items, OrderDto order)
        {

            //Added transaction Dtl

            reqPlGenIRN.Version = "1.1";
            reqPlGenIRN.TranDtls = new ReqPlGenIRN.TranDetails();
            reqPlGenIRN.TranDtls.TaxSch = "GST";
            reqPlGenIRN.TranDtls.SupTyp = "B2B";
            reqPlGenIRN.TranDtls.RegRev = "N";
            reqPlGenIRN.TranDtls.IgstOnIntra = "N";
            //End

            #region  Add Doc Detail
            
            reqPlGenIRN.DocDtls = new ReqPlGenIRN.DocSetails();
            //reqPlGenIRN.DocDtls.Typ = "INV";

            if (invoiceDetails.InvoiceNo.Contains("CN"))
            {
                reqPlGenIRN.DocDtls.Typ = ConfigurationManager.AppSettings["CreditNote"];

            } else if (invoiceDetails.InvoiceNo.Contains("DN"))
            {
                reqPlGenIRN.DocDtls.Typ = ConfigurationManager.AppSettings["DebitNote"];
            }else
            {
                reqPlGenIRN.DocDtls.Typ = ConfigurationManager.AppSettings["Invoice"];
            }
            

            reqPlGenIRN.DocDtls.No = invoiceDetails.InvoiceNo;

            var dtStr = DateTime.Now.ToShortDateString();
            if (dtStr.Contains("-"))
            {
                formatedDate = dtStr.Replace("-", "/");
            }
            reqPlGenIRN.DocDtls.Dt = formatedDate;

            #endregion


            #region Seller Detail
            reqPlGenIRN.SellerDtls = new ReqPlGenIRN.SellerDetails();
            reqPlGenIRN.SellerDtls.Gstin = ConfigurationManager.AppSettings["Gstin"];
            reqPlGenIRN.SellerDtls.LglNm = ConfigurationManager.AppSettings["LglNm"];
            reqPlGenIRN.SellerDtls.TrdNm = ConfigurationManager.AppSettings["TrdNm"];
            reqPlGenIRN.SellerDtls.Addr1 = ConfigurationManager.AppSettings["Addr1"];
            reqPlGenIRN.SellerDtls.Addr2 = ConfigurationManager.AppSettings["Addr2"];
            reqPlGenIRN.SellerDtls.Loc = ConfigurationManager.AppSettings["Loc"];
            reqPlGenIRN.SellerDtls.Pin = Int32.Parse(ConfigurationManager.AppSettings["Pin"]);
            reqPlGenIRN.SellerDtls.Stcd = ConfigurationManager.AppSettings["Stcd"];
            //reqPlGenIRN.SellerDtls.Ph = "9000000000";
            //reqPlGenIRN.SellerDtls.Em = "abc@gmail.com";

            #endregion

            #region Add Buyer Detail

            /*reqPlGenIRN.BuyerDtls = new ReqPlGenIRN.BuyerDetails();
            reqPlGenIRN.BuyerDtls.Gstin = customerDetails.GSTIN.Trim();
            reqPlGenIRN.BuyerDtls.LglNm = customerDetails.CustomerName.Trim();
            reqPlGenIRN.BuyerDtls.TrdNm = customerDetails.CustomerName.Trim();
            //reqPlGenIRN.BuyerDtls.Pos = customerDetails.StateCode.Trim(); 
            reqPlGenIRN.BuyerDtls.Pos = (customerDetails.StateCode != null) ?
                customerDetails.StateCode.Trim() : null;
            reqPlGenIRN.BuyerDtls.Addr1 = (customerDetails.AddressLine1!=null)?
                customerDetails.AddressLine1.Trim():null;
            reqPlGenIRN.BuyerDtls.Addr2 = (customerDetails.AddressLine2!=null) 
                || (customerDetails.AddressLine2 != "") ?
                customerDetails.AddressLine2.Trim(): customerDetails.AddressLine1.Trim();
            reqPlGenIRN.BuyerDtls.Loc = (customerDetails.City!=null)
                ?customerDetails.City.Trim():null;
            reqPlGenIRN.BuyerDtls.Pin = int.Parse(customerDetails.PIN);
            reqPlGenIRN.BuyerDtls.Stcd = customerDetails.StateCode.Trim();*/

            if (customerDetails.SubCustomerId > 0)
            {
                SubCustomerDto subCustomerDto = GetSubCustomerDetail(customerDetails.SubCustomerId);

                reqPlGenIRN.BuyerDtls = new ReqPlGenIRN.BuyerDetails();
                reqPlGenIRN.BuyerDtls.Gstin = subCustomerDto.GSTIN.Trim();
                reqPlGenIRN.BuyerDtls.LglNm = subCustomerDto.SubCustomerName.Trim();
                reqPlGenIRN.BuyerDtls.TrdNm = subCustomerDto.SubCustomerName.Trim();
                //reqPlGenIRN.BuyerDtls.Pos = customerDetails.StateCode.Trim(); 
                reqPlGenIRN.BuyerDtls.Pos = (subCustomerDto.StateCode != null) ?
                    customerDetails.StateCode.Trim() : null;
                reqPlGenIRN.BuyerDtls.Addr1 = (subCustomerDto.AddressLine1 != null) ?
                    customerDetails.AddressLine1.Trim() : null;
                reqPlGenIRN.BuyerDtls.Addr2 = (subCustomerDto.AddressLine2 != null)
                    || (customerDetails.AddressLine2 != "") ?
                    customerDetails.AddressLine2.Trim() : subCustomerDto.AddressLine1.Trim();
                reqPlGenIRN.BuyerDtls.Loc = (subCustomerDto.City != null)
                    ? customerDetails.City.Trim() : null;
                reqPlGenIRN.BuyerDtls.Pin = int.Parse(subCustomerDto.PIN);
                reqPlGenIRN.BuyerDtls.Stcd = subCustomerDto.StateCode.Trim();
            }
            else
            {
                reqPlGenIRN.BuyerDtls = new ReqPlGenIRN.BuyerDetails();
                reqPlGenIRN.BuyerDtls.Gstin = customerDetails.GSTIN.Trim();
                reqPlGenIRN.BuyerDtls.LglNm = customerDetails.CustomerName.Trim();
                reqPlGenIRN.BuyerDtls.TrdNm = customerDetails.CustomerName.Trim();
                //reqPlGenIRN.BuyerDtls.Pos = customerDetails.StateCode.Trim(); 
                reqPlGenIRN.BuyerDtls.Pos = (customerDetails.StateCode != null) ?
                    customerDetails.StateCode.Trim() : null;
                reqPlGenIRN.BuyerDtls.Addr1 = (customerDetails.AddressLine1 != null) ?
                    customerDetails.AddressLine1.Trim() : null;
                reqPlGenIRN.BuyerDtls.Addr2 = (customerDetails.AddressLine2 != null)
                    || (customerDetails.AddressLine2 != "") ?
                    customerDetails.AddressLine2.Trim() : customerDetails.AddressLine1.Trim();
                reqPlGenIRN.BuyerDtls.Loc = (customerDetails.City != null)
                    ? customerDetails.City.Trim() : null;
                reqPlGenIRN.BuyerDtls.Pin = int.Parse(customerDetails.PIN);
                reqPlGenIRN.BuyerDtls.Stcd = customerDetails.StateCode.Trim();
            }

            #endregion




            #region shipping Detail

            /*reqPlGenIRN.ShipDtls = new ReqPlGenIRN.ShippedDetails();
            reqPlGenIRN.ShipDtls.Gstin = customerDetails.GSTIN.Trim();
            reqPlGenIRN.ShipDtls.LglNm = customerDetails.CustomerName.Trim();
            reqPlGenIRN.ShipDtls.TrdNm = customerDetails.CustomerName.Trim();
            reqPlGenIRN.ShipDtls.Addr1 = (customerDetails.ShippingAddressLine1 != null)
                ? customerDetails.ShippingAddressLine1.Trim() : null;
            reqPlGenIRN.ShipDtls.Addr2 = (customerDetails.ShippingAddressLine2 != null) ?
                customerDetails.ShippingAddressLine2.Trim() : null;
            reqPlGenIRN.ShipDtls.Loc = (customerDetails.ShippingCity != null) ?
                customerDetails.ShippingCity.Trim() : customerDetails.City.Trim();
            reqPlGenIRN.ShipDtls.Pin = (customerDetails.ShippingPIN != null) ?
                int.Parse(customerDetails.ShippingPIN.Trim()) : int.Parse(customerDetails.PIN.Trim());
            reqPlGenIRN.ShipDtls.Stcd = (customerDetails.ShippingState != null) ?
               customerDetails.ShippingState.Trim() : customerDetails.State.Trim();
            reqPlGenIRN.ShipDtls.Stcd = (customerDetails.StateCode != null) ?
                customerDetails.StateCode.Trim() : null;*/


            if (customerDetails.SubCustomerId > 0)
            {
                SubCustomerDto subCustomerDto = GetSubCustomerDetail(customerDetails.SubCustomerId);

                reqPlGenIRN.ShipDtls = new ReqPlGenIRN.ShippedDetails();
                reqPlGenIRN.ShipDtls.Gstin = subCustomerDto.GSTIN.Trim();
                reqPlGenIRN.ShipDtls.LglNm = subCustomerDto.SubCustomerName.Trim();
                reqPlGenIRN.ShipDtls.TrdNm = subCustomerDto.SubCustomerName.Trim();
                reqPlGenIRN.ShipDtls.Addr1 = (subCustomerDto.AddressLine1 != null)
                    ? subCustomerDto.AddressLine1.Trim() : null;
                reqPlGenIRN.ShipDtls.Addr2 = (subCustomerDto.AddressLine2 != null) ?
                    subCustomerDto.AddressLine2.Trim() : null;
                reqPlGenIRN.ShipDtls.Loc = (subCustomerDto.City != null) ?
                    subCustomerDto.City.Trim() : subCustomerDto.City.Trim();
                reqPlGenIRN.ShipDtls.Pin = (subCustomerDto.PIN != null) ?
                    int.Parse(customerDetails.ShippingPIN.Trim()) : int.Parse(subCustomerDto.PIN.Trim());
                reqPlGenIRN.ShipDtls.Stcd = (subCustomerDto.State != null) ?
                   customerDetails.ShippingState.Trim() : subCustomerDto.State.Trim();
                reqPlGenIRN.ShipDtls.Stcd = (subCustomerDto.StateCode != null) ?
                    customerDetails.StateCode.Trim() : null;
            }
            else
            {
                reqPlGenIRN.ShipDtls = new ReqPlGenIRN.ShippedDetails();
                reqPlGenIRN.ShipDtls.Gstin = customerDetails.GSTIN.Trim();
                reqPlGenIRN.ShipDtls.LglNm = customerDetails.CustomerName.Trim();
                reqPlGenIRN.ShipDtls.TrdNm = customerDetails.CustomerName.Trim();
                reqPlGenIRN.ShipDtls.Addr1 = (customerDetails.ShippingAddressLine1 != null)
                    ? customerDetails.ShippingAddressLine1.Trim() : null;
                reqPlGenIRN.ShipDtls.Addr2 = (customerDetails.ShippingAddressLine2 != null) ?
                    customerDetails.ShippingAddressLine2.Trim() : null;
                reqPlGenIRN.ShipDtls.Loc = (customerDetails.ShippingCity != null) ?
                    customerDetails.ShippingCity.Trim() : customerDetails.City.Trim();
                reqPlGenIRN.ShipDtls.Pin = (customerDetails.ShippingPIN != null) ?
                    int.Parse(customerDetails.ShippingPIN.Trim()) : int.Parse(customerDetails.PIN.Trim());
                reqPlGenIRN.ShipDtls.Stcd = (customerDetails.ShippingState != null) ?
                   customerDetails.ShippingState.Trim() : customerDetails.State.Trim();
                reqPlGenIRN.ShipDtls.Stcd = (customerDetails.StateCode != null) ?
                    customerDetails.StateCode.Trim() : null;
            }

            #endregion


            #region item list
            reqPlGenIRN.ItemList = GetEItemList(items, invoiceDetails, order);
            #endregion

           
            reqPlGenIRN.ValDtls = new ReqPlGenIRN.ValDetails();
            reqPlGenIRN.ValDtls.AssVal = Decimal.ToDouble(invoiceDetails.AmountBTax);
            reqPlGenIRN.ValDtls.CgstVal = Decimal.ToDouble(invoiceDetails.CGSTTax);
            reqPlGenIRN.ValDtls.SgstVal = Decimal.ToDouble(invoiceDetails.SGSTTax);
            if (invoiceDetails.IGSTTax > 0)
            {
                reqPlGenIRN.ValDtls.IgstVal = Decimal.ToDouble(invoiceDetails.IGSTTax);

            }
            else
            {
                reqPlGenIRN.ValDtls.IgstVal = Decimal.ToDouble(invoiceDetails.TotalTax);
            }

            reqPlGenIRN.ValDtls.CesVal = 0;
            reqPlGenIRN.ValDtls.StCesVal = 0;
            //reqPlGenIRN.ValDtls.Discount = Decimal.ToDouble((decimal)(order.Discount != null ? order.Discount : 0));
            reqPlGenIRN.ValDtls.Discount = 0;
            reqPlGenIRN.ValDtls.OthChrg = 0;
            //reqPlGenIRN.ValDtls.RndOffAmt = 0.3;
            reqPlGenIRN.ValDtls.TotInvVal = Decimal.ToDouble(invoiceDetails.Amount);
            reqPlGenIRN.ValDtls.TotInvValFc = Decimal.ToDouble(invoiceDetails.Amount);
            return reqPlGenIRN;
        }

        


        #region get Items list
        private List<EInvoiceItemList> GetEItemList(List<InvoiceItemDto> items,
            InvoiceDto invoiceDetails, OrderDto order)
        {
            var eInvoiceList = new List<EInvoiceItemList>();
            var componentHSN = ConfigurationManager.AppSettings["ComponentHSN"];
            var unit = ConfigurationManager.AppSettings["Unit"];

            int sn = 0;
            foreach (var a in items)
            {
                sn = sn + 1;
                var itemList = new EInvoiceItemList();
                itemList.SlNo = sn.ToString();
                itemList.PrdDesc = a.Name;
                itemList.Qty = a.Qty;
                itemList.HsnCd = a.HSNCode;
                itemList.IsServc = a.IsService;
                /* if (a.HSNCode == "84484990")
                 {
                     itemList.Unit = "NOS";
                 }
                 else
                 {
                     itemList.Unit = null;
                 }*/

                if (a.HSNCode == componentHSN)
                {
                    itemList.Unit = unit;
                }
                else
                {
                    itemList.Unit = null;
                }

                itemList.UnitPrice = Decimal.ToDouble(a.Rate);
                itemList.TotAmt = Decimal.ToDouble(a.AmountBTax);

                itemList.Discount = 0;
                itemList.PreTaxVal = Decimal.ToDouble(a.AmountBTax);
                itemList.AssAmt = Decimal.ToDouble((decimal)(a.AmountBTax));

                if (a.GSTRate > 0)
                {
                    itemList.GstRt = Decimal.ToDouble(a.GSTRate);
                }
                else
                {
                    itemList.GstRt = 18;
                }

                if (a.IGSTAmount > 0)
                {
                    itemList.IgstAmt = Decimal.ToDouble(a.IGSTAmount);
                }
                else
                {
                    itemList.IgstAmt = Decimal.ToDouble(a.GSTAmount);
                }

                itemList.CgstAmt = Decimal.ToDouble(a.CGSTAmount);
                itemList.SgstAmt = Decimal.ToDouble(a.SGSTAmount);
                itemList.OthChrg = 0;
                if (a.IGSTAmount > 0)
                {
                    itemList.TotItemVal = Decimal.ToDouble(a.AmountBTax + a.IGSTAmount);
                }
                else
                {
                    itemList.TotItemVal = Decimal.ToDouble(a.AmountBTax + a.GSTAmount);
                }
                eInvoiceList.Add(itemList);

            }
            return eInvoiceList;
        }
        #endregion

        #region generate E-Invoice
        public RespPlGenIRNDec GenerateIRN(CustomerDto customerDetails, InvoiceDto invoiceDetails, List<InvoiceItemDto> items, OrderDto order)
        {
            GenerateInvoiceResp generateInvoiceResp = new GenerateInvoiceResp();
            RespPlGenIRNDec respPlGenIRN = new RespPlGenIRNDec();
            var roleId = ConfigurationManager.AppSettings["AllowRoleIdForEInvoice"];
            if (roleId.Contains(userInfo.RoleId.ToString()))
            {
                try
                {
                    AuthResponse authResponse = new AuthResponse();

                    RestClient client = new RestClient(eInvoiceURL + "eivital/dec/v1.04/auth");
                    RestRequest request = new RestRequest(Method.GET);
                    request = GetHeaderParam(request, authResponse);
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IRestResponse response = client.Execute(request);
                    authResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);

                    if (authResponse.Status == 1)
                    {
                        client = new RestClient(eInvoiceURL + "eicore/dec/v1.03/Invoice?QrCodeSize=250");
                        request = new RestRequest(Method.POST);
                        request = GetHeaderParam(request, authResponse);
                        var reqPlGenIR = GetEInVoiceTransDtl(customerDetails, invoiceDetails, items, order);
                        request.AddBody(reqPlGenIR);
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        IRestResponse iRestResponse = client.Execute(request);
                        var str = iRestResponse.Content;
                        try
                        {
                            generateInvoiceResp = JsonConvert.DeserializeObject<GenerateInvoiceResp>(str);
                        }
                        catch (Exception ex)
                        {

                            respPlGenIRN.ErrorDetails[0].ErrorMessage = ex.Message.ToString();
                        }
                        if (generateInvoiceResp.ErrorDetails != null)
                        {
                            respPlGenIRN.ErrorDetails = generateInvoiceResp.ErrorDetails;
                        }
                        if (generateInvoiceResp.InfoDtls != null)
                        {
                            respPlGenIRN.InfoDtls = generateInvoiceResp.InfoDtls;
                        }

                        if (generateInvoiceResp.Status.Equals("1"))
                        {
                            respPlGenIRN = JsonConvert.DeserializeObject<RespPlGenIRNDec>(generateInvoiceResp.Data);
                            string image = invoiceDetails.InvoiceNo;
                            string imagename = invoiceDetails.InvoiceNo.Replace("/", "");
                            string fileName = string.Format("{0}{1}", imagename, ".png");
                            //Image img = Base64ToImage(data.QrCodeImage);
                            byte[] qrImg = Convert.FromBase64String(respPlGenIRN.QrCodeImage);
                            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                            Bitmap bitmap1 = (Bitmap)tc.ConvertFrom(qrImg);
                            string savedFilePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/QrCodeImages/"), fileName);
                            respPlGenIRN.EInvoiceQrCodeImg = fileName;
                            bitmap1.Save(savedFilePath);

                            tblEInvoice eInvoice = new tblEInvoice();
                            eInvoice.IRN = respPlGenIRN.Irn.ToString();
                            eInvoice.Cancelled = false;
                            eInvoice.QRCodeBase64 = respPlGenIRN.QrCodeImage.ToString();
                            eInvoice.QRCode = fileName;
                            eInvoice.InvoiceDate = DateTime.Now;
                            eInvoice.OrderID = order.OrderGUID;
                            eInvoice.IRNGeneratedBy = userInfo.UserId.ToString();
                            eInvoice.DateCreated = DateTime.Now;
                            eInvoice.DelInd = false;
                            eInvoice.InvoiceNo = invoiceDetails.InvoiceNo;
                            eInvoice.ProvisionalBillId = invoiceDetails.ProvisionalBillId;
                           
                            if (invoiceDetails.InvoiceNo.Contains("CN"))
                            {
                                eInvoice.Type = ConfigurationManager.AppSettings["CreditNote"];
                            }else if (invoiceDetails.InvoiceNo.Contains("DN"))
                            {
                                eInvoice.Type = ConfigurationManager.AppSettings["DebitNote"];
                            }
                            else
                            {
                                eInvoice.Type = ConfigurationManager.AppSettings["Invoice"];
                            }

                            context.tblEInvoices.Add(eInvoice);
                            context.SaveChanges();

                        }
                        else
                        {
                            return respPlGenIRN;

                        }
                    }


                    return respPlGenIRN;
                }
                catch (Exception ex)
                {
                    respPlGenIRN.ErrorDetails[0].ErrorMessage = ex.Message.ToString();
                    return respPlGenIRN;
                    //throw ex;
                }
            }
            else
            {
                string msg = "You are not allow to create E-Invoice";

                //respPlGenIRN.ErrorDetails = generateInvoiceResp.ErrorDetails;

                //respPlGenIRN.ErrorDetails[0].ErrorMessage = msg.ToString();
                respPlGenIRN.errorMsg = msg.ToString();
                return respPlGenIRN;
            }

            

        }
        #endregion


        #region Cancel E-Invoice
        public RespPlCancelIRN CancelEInvoice(tblEInvoice eInv)
        {

            GenerateInvoiceResp generateInvoiceResp = new GenerateInvoiceResp();
            ReqPlCancelIRN reqPlCancelIRN = new ReqPlCancelIRN();
            RespPlCancelIRN respPlCancelIRN = new RespPlCancelIRN();
            var roleId = ConfigurationManager.AppSettings["AllowRoleIdForEInvoice"];
            if (roleId.Contains(userInfo.RoleId.ToString()))
            {
                try
                {
                    AuthResponse authResponse = new AuthResponse();
                    RestClient client = new RestClient(eInvoiceURL + "eivital/dec/v1.04/auth");
                    RestRequest request = new RestRequest(Method.GET);

                    request = GetHeaderParam(request, authResponse);
                    reqPlCancelIRN.CnlRem = "Wrong entry";
                    reqPlCancelIRN.CnlRsn = "1";
                    reqPlCancelIRN.Irn = eInv.IRN;
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IRestResponse response = client.Execute(request);
                    authResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);
                    client = new RestClient(eInvoiceURL + "eicore/dec/v1.03/Invoice/Cancel");
                    request = new RestRequest(Method.POST);
                    request = GetHeaderParam(request, authResponse);
                    request.AddBody(reqPlCancelIRN);
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IRestResponse iRestResponse = client.Execute(request);
                    generateInvoiceResp = JsonConvert.DeserializeObject<GenerateInvoiceResp>(iRestResponse.Content);
                    respPlCancelIRN.ErrorDetails = generateInvoiceResp.ErrorDetails;
                    respPlCancelIRN.InfoDtls = generateInvoiceResp.InfoDtls;
                    if (generateInvoiceResp.Status.Equals("1"))
                    {
                        respPlCancelIRN = JsonConvert.DeserializeObject<RespPlCancelIRN>(generateInvoiceResp.Data);

                        eInv.Cancelled = true;
                        eInv.CancelledDate = DateTime.Now;
                        eInv.CancelledBy = userInfo.UserId.ToString();
                        eInv.LastModified = DateTime.Now;
                        context.tblEInvoices.Attach(eInv);
                        context.Entry(eInv).State = EntityState.Modified;
                        context.SaveChanges();

                    }
                    else
                    {
                        return respPlCancelIRN;

                    }
                }
                catch (Exception ex)
                {
                    respPlCancelIRN.ErrorDetails[0].ErrorMessage = ex.Message.ToString();
                    return respPlCancelIRN;
                    //throw ex;
                }

                return respPlCancelIRN;
            }
            else
            {
                string msg = "You are not allow to cancel E-Invoice";
                respPlCancelIRN.errorMsg = msg.ToString();
                return respPlCancelIRN;
            }
                

        }
        #endregion


        #region header parameter
        private RestRequest GetHeaderParam(RestRequest request, AuthResponse authResponse)
        {
            var gstin = ConfigurationManager.AppSettings["ProjectGST"];

            request.AddHeader("Gstin", ConfigurationManager.AppSettings["ProjectGST"]);
            request.AddHeader("user_name", ConfigurationManager.AppSettings["eInvUserName"]);
            if (authResponse.Status == 1)
            {
                request.AddHeader("AuthToken", authResponse.Data.AuthToken);
            }
            request.AddHeader("aspid", ConfigurationManager.AppSettings["aspid"]);
            request.AddHeader("password", ConfigurationManager.AppSettings["eInvoicePassword"]);
            request.AddHeader("eInvPwd", ConfigurationManager.AppSettings["eInvPassword"]);
            request.AddHeader("QrCodeSize", "250");
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.RequestFormat = DataFormat.Json;
            return request;

        }
        #endregion


        #region Get Subcustomer detail
        private SubCustomerDto GetSubCustomerDetail(int subCustomerId)
        {
            PicannolEntities context = new PicannolEntities();
            var scl = context.tblSubCustomers.Where(x => x.SubCustomerId == subCustomerId && x.DelInd == false).
                Select(x => new SubCustomerDto
                {
                    SubCustomerName = x.SubCustomerName,
                    AddressLine1 = x.AddressLine1 != null ? x.AddressLine1 : null,
                    AddressLine2 = x.AddressLine2 != null ? x.AddressLine2 : null,
                    State = x.State,
                    StateCode = x.StateCode,
                    GSTIN = x.GSTIN != null ? x.GSTIN : null,
                    District = x.District != null ? x.District : null,
                    City = x.City != null ? x.City : null,
                    PIN = x.PIN != null ? x.PIN : null
                }).FirstOrDefault();
            return scl;
        }
        #endregion
    }
}