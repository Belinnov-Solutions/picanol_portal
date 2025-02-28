using Picanol.Helpers;
using Picanol.Utils;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
    [SessionCheck]
    public class ReportsController : BaseController
    {
        // GET: Reports
        private readonly UserHelper _userHelper;
        private readonly MaterialHelper _materialHelper;
        private readonly MaterialHelper _partsHelper;
        public ReportsController()
        {
            _materialHelper = new MaterialHelper(this);
            _userHelper = new UserHelper(this);
            _partsHelper = new MaterialHelper(this);
        }
        public ActionResult StockReport()
        {
            ReportsViewModel vm = new ReportsViewModel();
            return View(vm);
        }

        public ActionResult InwardReports()
        {
            ReportsViewModel vm = new ReportsViewModel();
            return View(vm);
        }
        public ActionResult RepairSummary()
        {
            ReportsViewModel vm = new ReportsViewModel();
            vm.UserList = _userHelper.GetAllUsersById();
           
            return View(vm);
        }
        public ActionResult PCBBoardSummary()
        {
            ReportsViewModel vm = new ReportsViewModel();
            vm.PartList = _partsHelper.GetAllPartsList(1);
            return View(vm);
        }
        public ActionResult PCBNameWiseReport()
        {
            ReportsViewModel vm = new ReportsViewModel();
            vm.Customers = _materialHelper.GetCustomersList();

            return View(vm);
        }
        public ActionResult PartReportByTrackingNo()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

    }
}