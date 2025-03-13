using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Picanol.Utils;

[assembly: OwinStartup("PicanolConfig",typeof(Picanol.App_Start.Startup))]

namespace Picanol.App_Start
{
    public class Startup
    { 
        
        public void Configuration(IAppBuilder app)
        {
            try
            {
                var url = ConfigurationManager.AppSettings["OtpVerifyPath"].ToString();
                var url2 = ConfigurationManager.AppSettings["authenticateUserPath"].ToString();
                app.UseCors(CorsOptions.AllowAll);
                HttpConfiguration config = new HttpConfiguration();
                OAuthAuthorizationServerOptions option = new OAuthAuthorizationServerOptions
                {

                    TokenEndpointPath = new PathString(url),
                    Provider = new ApplicationAuthProvider(),
                    AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                    AllowInsecureHttp = true
                };

                OAuthAuthorizationServerOptions option1 = new OAuthAuthorizationServerOptions
                {

                    TokenEndpointPath = new PathString(url2),
                    Provider = new ApplicationAuthProvider(),
                    AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                    AllowInsecureHttp = true
                };

                app.UseOAuthAuthorizationServer(option);
                app.UseOAuthAuthorizationServer(option1);
                app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
                app.UseWebApi(config);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
