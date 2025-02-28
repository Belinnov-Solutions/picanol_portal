using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class PartsViewModel
    {
        public PartsViewModel()
        {
            
            PartsList = new List<PartDto>();
            PartsStockList = new List<PartDto>();
            newPartList = new List<PartDto>();
            PartTypeList = Enum.GetNames(typeof(PartType)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name

            });
            //SelectSupplierList = Enum.GetNames(typeof(SelectSupplier)).Select(name => new SelectListItem()
            //{
            //	Text = name,
            //	Value = name

            //});
        }
        public bool IsAddStock { get; set; }
        public List<PartDto> PartsList { get; set; }
        
        public List<PartDto> PartsStockList { get; set; }
        public List<PartDto> newPartList { get; set; }
        //Add New Part Getter & Setter 
        //public string PartName { get; set; }
        //public string PartNumber { get; set; }
        //public string SerialNumber { get; set; }
        //public string Stock { get; set; }
        //public string Price { get; set; }
        //public PartType PartTypeList { get; set; }
        public IEnumerable<SelectListItem> PartTypeList { get; set; }
        //public SelectSupplier SelectSupplier { get; set; }
        public IEnumerable<SelectListItem> SelectSupplierList { get; set; }
        public string SelectedPartType { get; set; }
        public PartDto SelectedPart { get; set; }
        public string LastPartNo { get; set; }

    }
}
	
	
