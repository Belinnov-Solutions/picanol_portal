using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class GenerateInvoiceResp
    {

        public string Status { get; set; }
        /*public RespPlGenIRNDec Data { get; set; }*/

        public string Data { get; set; }
        public List<AuthErrorDetail> ErrorDetails { get; set; }

        public List<InfoDetails> InfoDtls { get; set; }

        public string JwtIssue;

        public GenerateInvoiceResp()
        {
            
            //Data = new RespPlGenIRNDec();
            ErrorDetails = new List<AuthErrorDetail>();
            InfoDtls = new List<InfoDetails>();
        }

    }

    

    public class InVoiceResponseData
    {
        public string AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public string EwbNo { get; set; }
        public string EwbDt { get; set; }
        public string EwbValidTill { get; set; }

        public RespGenIRNInvData ExtractedSignedInvoiceData { get; set; }
        public RespGenIRNQrCodeData ExtractedSignedQrCode { get; set; }
        public Bitmap QrCodeImage { get; set; }
        public string JwtIssuer { get; set; }
    }


}