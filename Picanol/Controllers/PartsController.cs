using NLog;
using OfficeOpenXml;
using Picanol.DataModel;
using Picanol.Helpers;
using Picanol.Models;
using Picanol.Utils;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Picanol.Controllers
{
    //[RedirectingAction]
    [SessionCheck]
    public class PartsController : BaseController
    {
        private readonly MaterialHelper _partsHelper;
        private readonly UserHelper _userHelper;

        public PartsController()
        {
            _userHelper = new UserHelper(this);
            _partsHelper = new MaterialHelper(this);
        }
        // GET: Parts
        public ActionResult Index(PartsViewModel pvm)
        {
            //PartsViewModel vm = new PartsViewModel();
            if (pvm.PartsList == null || pvm.PartsList.Count == 0)
                pvm.PartsList = _partsHelper.GetAllPartsList(2);
            return View(pvm);
        }
        Logger logger = LogManager.GetLogger("databaseLogger");
        public ActionResult AddNewPart(int? partId)
        {
            if (partId > 0)
            {
                PartsViewModel pv = new PartsViewModel();
                pv.SelectedPart = _partsHelper.GetPartDetails(partId);
                if (pv.SelectedPart.PartTypeId == 1)
                {
                    pv.SelectedPart.PartType = ConstantsHelper.PartType.Board.ToString();

                }
                else
                {
                    pv.SelectedPart.PartType = ConstantsHelper.PartType.Component.ToString();
                }
                pv.LastPartNo = _partsHelper.GetLastPartNo();
                return View(pv);
            }
            PartsViewModel v = new PartsViewModel();
            v.LastPartNo = _partsHelper.GetLastPartNo();
            return View(v);
            
        }
       
        [HttpPost]
        public ActionResult AddNewPart(PartsViewModel pv)
        {
            string response = "";
            if (pv.SelectedPart.PartId == 0)
            {
                if (pv.SelectedPart.PartType == ConstantsHelper.PartType.Board.ToString())
                {
                    pv.SelectedPart.PartTypeId = 1;
                }
                else
                {
                    pv.SelectedPart.PartTypeId = 2;
                }
                response = _partsHelper.InsertPart(pv);
                if (response == "success")
                {
                    string ActionName = $"add New Part - {pv.SelectedPart.PartName + "Part Number" + pv.SelectedPart.PartNumber}"; ;
                    string TableName = "TblPart; tblPurchases";
                    if (ActionName != null)
                    {
                        var userInfo = (UserSession)Session["UserInfo"];

                        _userHelper.recordUserActivityHistory(userInfo.UserId, ActionName, TableName);
                    }
                }

            }
            else
            {
                
                response = _partsHelper.UpdatePart(pv);
                var userInfo = (UserSession)HttpContext.Session["UserInfo"];
                int userId = userInfo.UserId;
                string ActionName = $"Edit Part - {pv.SelectedPart.PartName + "PartNumber" + pv.SelectedPart.PartNumber + "Price" + pv.SelectedPart.Price + "Stock" + pv.SelectedPart.Stock}"; ;
                string TableName = "TblPart";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userId, ActionName, TableName);

                }
            }
            

            return Json("success");
        }

        public ActionResult PartsMovement(int partId, string partName, string partNumber, int PartStock, int OpeningStock)
        {
            try
            {
                PartsMovementViewModel pmvm = new PartsMovementViewModel();
                pmvm.movementList = _partsHelper.GetPartMovementDetails(partId);
                pmvm.PartName = partName;
                pmvm.PartNumber = partNumber;
                //pmvm.PartStock = partStock;
                pmvm.PartStock = _partsHelper.GetConsumptionOfPart(partId);
                //pmvm.OpeningStock = PartStock;
                pmvm.OpeningStock = 0;
                return View(pmvm);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error in GenerateInvoice");
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult GetFilteredParts(string type, string searchString)
        {
            PartsViewModel vm = new PartsViewModel();
            int partType = 1;
            if (type == "Components")
                partType = 2;
            if (searchString == null || searchString == "")
                vm.PartsList = _partsHelper.GetAllPartsList(partType);
            else
                vm.PartsList = _partsHelper.GetSpecificPart(searchString);
            vm.PartsList = vm.PartsList.Where(x => x.PartTypeId == partType).ToList();
            //return View(vm);
            //return RedirectToAction("Index", "Part", new { pvm = vm });
            return PartialView("_PartsList", vm);
        }

        //changes made by Shijo for View Low Stock(<10)

        public ActionResult AddStock (int id)
        {
            PartsViewModel vm = new PartsViewModel();
            if(id ==2)
            {
                vm.PartsList = _partsHelper.GetAllPartsList(2);
                vm.IsAddStock = true;
            }
            else
            {
                vm.PartsList = _partsHelper.GetAllLowPartsList();
                vm.IsAddStock = false;
            }
           
            //vm.newPartList = _partsHelper.GetAllPartsList(2);
            return View(vm);
        }
        //public ActionResult AddLowStock()
        //{
        //    PartsViewModel vm = new PartsViewModel();
        //    vm.PartslowList = _partsHelper.GetAllLowPartsList();
        //    return View();
        //}

        public ActionResult GetPartByName(string PartName)
        {
            PartsViewModel vm = new PartsViewModel();
            var plist = _partsHelper.GetAllPartsList(2);
            foreach (var item in plist)
            {
                if(item.PartName.ToLower().Contains(PartName.ToLower()))
                {
                    vm.PartsList.Add(item);
                }
                else if(item.PartNumber.ToLower().Contains(PartName.ToLower()))
                {
                    vm.PartsList.Add(item);
                }
            }
            //vm.PartsList = vm.PartsList.Where(x=>x.PartName.Contains(PartName)).ToList();
            //if(vm.PartsList.Count<=0)
            //    vm.PartsList = vm.PartsList.Where(x => x.PartNumber.Contains(PartName)).ToList();
            //vm.newPartList = _partsHelper.GetAllPartsList(2);
            return PartialView("_AddStock", vm);
            //return View(vm);
        }


        public ActionResult GetPartStock(string PartName)
        {
            PartsViewModel vm = new PartsViewModel();
            vm.PartsList = _partsHelper.GetSpecificPart(PartName);
            return PartialView("_AddStock", vm);
        }
        public ActionResult UpdatePartStock(PartsViewModel vm)
        {
            string response = "";
            response = _partsHelper.UpdatePartStock(vm);
            return Json(response);
        }
        [HttpGet]
        public ActionResult UpdatePrice()
        {
            return View();
        }


        [HttpPost]
        public ActionResult UpdatePrice(HttpPostedFileBase files)
        {
            try
            {
                PicannolEntities context = new PicannolEntities();

                var PartList = new List<PartDto>();

                if (Request != null)
                {
                    //HttpPostedFileBase file = Request.Files["UploadedFile"];
                    HttpPostedFileBase file = files;
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;

                            for (int rowIterator =
                                2; rowIterator <= noOfRow; rowIterator++)
                            {
                                var store = new PartDto();


                                if (workSheet.Cells[rowIterator, 1].Value != null)
                                {
                                    store.PartName = (workSheet.Cells[rowIterator, 1].Value.ToString());
                                }
                                if (workSheet.Cells[rowIterator, 2].Value != null)
                                {
                                    store.PartNumber = (workSheet.Cells[rowIterator, 2].Value.ToString());
                                }
                                if (workSheet.Cells[rowIterator, 3].Value != null)
                                {
                                    store.Stock = Convert.ToInt32(workSheet.Cells[rowIterator, 3].Value.ToString());
                                }
                                else
                                    store.Stock = 0;
                                if (workSheet.Cells[rowIterator, 4].Value != null)
                                {
                                    store.Price = Convert.ToDecimal(workSheet.Cells[rowIterator, 4].Value.ToString());
                                }
                                else
                                    store.Price = 0;
                                PartList.Add(store);
                            }
                        }
                    }



                }

                List<PartDto> response = CreateStore(PartList);


                //PartDto model = new PartDto();


                PartsViewModel VM = new PartsViewModel();
                VM.PartsList = response;
                var userInfo = (UserSession)HttpContext.Session["UserInfo"];
                int userId = userInfo.UserId;
                string ActionName = $"Update Price And Stock  - {PartList.Select(x => x.Price) + "Price" + PartList.Select(x => x.Stock) + "Stock"}";
                string TableName = "TblPart";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(userId, ActionName, TableName);
                }
                //return View("UpdatePrice", VM);
                return PartialView("_StockDifference", VM);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static List<PartDto> CreateStore(List<PartDto> PartList)
        {
            try
            {
                PartsViewModel vm = new PartsViewModel();
                PicannolEntities context = new PicannolEntities();
                List<PartDto> newPartList = new List<PartDto>();
                foreach (var item in PartList)
                {
                    PartDto newPart = new PartDto();
                    var part = context.tblParts.Where(x =>x.PartNumber == item.PartNumber).FirstOrDefault();
                    if (part.Stock != null)
                        newPart.PortalStock = (int)part.Stock;
                    else
                        newPart.PortalStock = 0;

                    if (item.Price != 0)
                    {
                        part.Price = item.Price;
                    }
                    //context.tblParts.Attach(part);
                    //context.Entry(part).State = EntityState.Modified;
                    //context.SaveChanges();
                    context.Entry(part).Property(x => x.Price).IsModified = true;
                    context.SaveChanges();
                    newPart.PartId = part.PartId;
                    newPart.PartName = part.PartName;
                    newPart.PartNumber = part.PartNumber;
                    newPart.AccountsStock = (int)item.Stock;
                    newPart.StockDifference = newPart.AccountsStock - newPart.PortalStock;
                    newPartList.Add(newPart);

                }
                return newPartList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult DeletePart(int partId)
        {
            string response = _partsHelper.DeletePart(partId);
            PartsViewModel vm = new PartsViewModel();
            return RedirectToAction("Index");
        }

        public ActionResult GetCurrentInventory()
        {
            var gv = new GridView();
            gv.DataSource = _partsHelper.GetInventoryData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=InventoryExcel.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View("index");
        }
    }
}