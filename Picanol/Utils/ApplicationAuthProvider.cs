using Microsoft.Owin.Security.OAuth;
using Picanol.Helpers;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Cors;

namespace Picanol.Utils
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ApplicationAuthProvider : OAuthAuthorizationServerProvider
    {
        //comented by Sunit on 2025-01-09
        /*public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string Username = context.Parameters.Where(f => f.Key == "Username").Select(f => f.Value).FirstOrDefault()[0];
            string Otp = context.Parameters.Where(f => f.Key == "Otp").Select(f => f.Value).FirstOrDefault()[0];
            context.OwinContext.Set<string>("UserName", Username);
            context.OwinContext.Set<string>("Otp", Otp);
            context.Validated();
        }*/

        //check the request param from the app for verify OTP
        /*public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            String Username = "";
            String Otp = "";
            //calling from android app
            if (context.Request.Method.ToString() == "POST")
            {
                Username = context.Parameters.Where(f => f.Key == "Username").Select(f => f.Value).FirstOrDefault()[0];
                Otp = context.Parameters.Where(f => f.Key == "Otp").Select(f => f.Value).FirstOrDefault()[0];
            }
            else
            {
                //calling from the flutter app
                Username = context.Parameters.Where(f => f.Key == "Username").Select(f => f.Value).FirstOrDefault()[0];
                Otp = context.Parameters.Where(f => f.Key == "Otp").Select(f => f.Value).FirstOrDefault()[0];
            }


            context.OwinContext.Set<string>("UserName", Username);
            context.OwinContext.Set<string>("Otp", Otp);
            context.Validated();
        }*/

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string username = "";
            string otp = "";

            var request = context.Request;

            // Check Content-Type to confirm it's JSON
            if (context.Request.Method.ToString() == "POST")
            {
                // Read raw body
                request.Body.Seek(0, System.IO.SeekOrigin.Begin); // Ensure stream is at the beginning
                using (var reader = new System.IO.StreamReader(request.Body))
                {
                    var body = await reader.ReadToEndAsync();

                    username = context.Parameters.Where(f => f.Key == "Username").Select(f => f.Value).FirstOrDefault()?[0];
                    otp = context.Parameters.Where(f => f.Key == "Otp").Select(f => f.Value).FirstOrDefault()?[0];
                }
            }
            else
            {
                // Fallback for form-urlencoded data
                username = context.Parameters.Where(f => f.Key == "Username").Select(f => f.Value).FirstOrDefault()?[0];
                otp = context.Parameters.Where(f => f.Key == "Otp").Select(f => f.Value).FirstOrDefault()?[0];
            }

            // Set in OwinContext for further usage
            context.OwinContext.Set<string>("UserName", username);
            context.OwinContext.Set<string>("Otp", otp);

            // You can add validation logic here if needed

            context.Validated();
        }



        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            OneTimePassword authRepository = new OneTimePassword();
            var url2 = ConfigurationManager.AppSettings["authenticateUserPath"].ToString();
            bool Valid = false;
            string Username = context.OwinContext.Get<string>("UserName");
            string Otp = context.OwinContext.Get<string>("Otp");
            if (context.Request.Path.ToString().Equals(url2, StringComparison.OrdinalIgnoreCase))
            {
                Valid = authRepository.GetAccessToken(Username, Otp);
            }
            else
            {
                Valid = authRepository.VerifyOtp(Username, Otp);
            }
           
            if (Valid)
            {   
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim("Username", Username));
                identity.AddClaim(new Claim("Password", Otp));
                context.Validated(identity);
            }
            else
            {
                context.SetError("invalid_grant", "The Otp is incorrect.");
                return;
            }
        }
    }
}