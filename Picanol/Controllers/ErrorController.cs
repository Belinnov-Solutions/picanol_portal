using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(InvoiceViewModel vm)
        {
            string data = TempData["ErrorMessage"] as string;
            if (data != null)
            {
                ViewBag.ErrorMessage = data.ToString();
            }
            else
            {
                ViewBag.ErrorMessage = "Something went wrong";
            }
            

            return View("NotFound");
        }

        public ActionResult NotFound()
        {
            return View("NotFound");
        }

        public ActionResult EInvoiceMessage()
        {
            return View();
        }
    }
}