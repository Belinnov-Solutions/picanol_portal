using NLog;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.Utils;
using Picanol.ViewModels;
using Rotativa.MVC;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
    public class CustomerSearchController : Controller
    {
        // GET: CustomerSearch
        private readonly MaterialHelper _materialHelper;

        public CustomerSearchController()
        {
            _materialHelper = new MaterialHelper(this);
        }
       [OutputCache(Duration =120)]
        public JsonResult ProvisionalCustomerSearch( string CustomerName )
       // public JsonResult ProvisionalCustomerSearch()
        {
            var userInfo = (UserSession)Session["UserInfo"];
            ServiceRequestViewModel model = new ServiceRequestViewModel();
            model.RoleId = userInfo.RoleId;
            model.UserId = userInfo.UserId;
            model.Customers = _materialHelper.GetCustomersListSearchVersion1(CustomerName);
            return Json(model,JsonRequestBehavior.AllowGet);
        }
    }
}