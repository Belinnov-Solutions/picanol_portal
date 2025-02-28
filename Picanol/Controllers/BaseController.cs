using Picanol.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Picanol.Controllers
{
    
    public class BaseController : Controller
    {
        protected bool CheckSession()
        {
            var o = (UserSession)Session["UserInfo"];
            if (o == null)
            {
                return false;
            }
            else
                return true;
        }




        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            var o = (UserSession)Session["UserInfo"];
            if (Session == null)
            {
                
                requestContext.HttpContext.Response.Clear();
                requestContext.HttpContext.Response.Redirect(Url.Action("Login", "Home"));
                requestContext.HttpContext.Response.End();
            }
        }

     

        protected UserSession GetUserSession()
        {
            if (this.Session["UserInfo"] != null)
            {
                ReturnToLogin();
            }

            return (UserSession)Session["UserInfo"];
        }

        protected bool IsUserLoggedIn
        {
            get { return GetUserSession() != null; }
        }

        protected ActionResult ReturnToLogin()
        {
            return RedirectToAction("Login", "Home");
        }
        protected ActionResult Pdf()
        {
            return Pdf(null, null, null);
        }

        protected ActionResult Pdf(string fileDownloadName)
        {
            return Pdf(fileDownloadName, null, null);
        }

        protected ActionResult Pdf(string fileDownloadName, string viewName)
        {
            return Pdf(fileDownloadName, viewName, null);
        }

        protected ActionResult Pdf(object model)
        {
            return Pdf(null, null, model);
        }

        protected ActionResult Pdf(string fileDownloadName, object model)
        {
            return Pdf(fileDownloadName, null, model);
        }

        protected ActionResult Pdf(string fileDownloadName, string viewName, object model)
        {
            // Based on View() code in Controller base class from MVC
            if (model != null)
            {
                ViewData.Model = model;
            }
            //PdfResult pdf = new PdfResult()
            //{
            //    FileDownloadName = fileDownloadName,
            //    ViewName = viewName,
            //    ViewData = ViewData,
            //    TempData = TempData,
            //    ViewEngineCollection = ViewEngineCollection
            //};
            //return pdf;
            return Pdf(fileDownloadName, null, model); ;
        }

    }
}