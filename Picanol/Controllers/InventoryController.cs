using Picanol.Helpers;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
    [RedirectingAction]
    public class InventoryController : BaseController
    {
        
        // GET: Inventory
        public ActionResult Index()
        {
            return View();
        }

      
    }
}