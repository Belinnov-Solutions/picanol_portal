using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
    public class SupplierController : Controller
    {
        // GET: Supplier
        public ActionResult Index()
        {
            return View();
        }
		//public ActionResult AddSupplier(SupplierViewModel data)
		//{
		//	return View();
		//}
		public ActionResult Demo()
		{
			return View();
		}
		//public void PostClick(List<Vendor> model)
		//{
		//	/*Some Code */
		//}
	//	public ActionResult EditSupplier()
	//	{
	//		SupplierViewModel sv = new SupplierViewModel
	//		{
	//			SupplierName = "Lalita",
	//			AddressLine1="Laxman Vihar",
	//			AddressLine2="Narnaul",
	//			 City ="Gurgaon",
	//			 District="Gurgaon",
	//			 State="Haryana",
	//			 PIN="122001",
	//			 ContactPerson="Gaurav",
	//			 Mobile="723932888",
	//			 Email="Lalita_agarwal21",
	//			 GSTIN="25",
	//			 StateCode="83898",
	//			 PAN="ADD987998",
	//};
	//		return View("EditSupplier",sv);
	//	}
		
	}
}
