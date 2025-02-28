using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace Picanol.Helpers
{
    public class GMailer
    {
        public static string GmailUsername { get; set; }
        public static string GmailPassword { get; set; }
        public static string GmailHost { get; set; }
        public static int GmailPort { get; set; }
        public static bool GmailSSL { get; set; }

        public string ToEmail { get; set; }

        public string CcEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public string AttachmentPath { get; set; }


        static GMailer()
        {
            GmailUsername = "noreply.picanol@gmail.com";
            GmailPassword = "wqhxhsfsehewnpei";
            GmailHost = "smtp.gmail.com";
            //GmailPort = 25; // Gmail can use ports 25, 465 & 587; but must be 25 for medium trust environment.
            GmailPort = 587; // Gmail can use ports 25, 465 & 587; but must be 25 for medium trust environment.
            GmailSSL = true;
        }

        public void Send()
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = GmailHost;
            smtp.Port = GmailPort;
            smtp.EnableSsl = GmailSSL;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(GmailUsername, GmailPassword);

            //using (var message = new MailMessage(GmailUsername, ToEmail))
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(GmailUsername);
                foreach (var address in ToEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    message.To.Add(address); 
                }


                if (CcEmail != null & CcEmail != "")
                { 
                foreach (var address in CcEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    message.CC.Add(address);
                }
                }


                message.Subject = Subject;
                message.Body = Body;
                message.IsBodyHtml = IsHtml;

                if (AttachmentPath != null && AttachmentPath != "")
                {
                    foreach (var attachment in AttachmentPath.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        
                        message.Attachments.Add(new Attachment(attachment));
                    }
                    
                }
                smtp.Send(message);
            }
        }

    }
}