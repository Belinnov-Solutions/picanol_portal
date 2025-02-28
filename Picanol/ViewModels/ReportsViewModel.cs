using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;
namespace Picanol.ViewModels
{
    public class ReportsViewModel
    {
        public ReportsViewModel()
        {
            MonthList = Enum.GetNames(typeof(SelectMonths)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
            RepairList = Enum.GetNames(typeof(RepairType)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
        }
        public IEnumerable<SelectListItem> MonthList { get; set; }
        public IEnumerable<SelectListItem> RepairList { get; set; }
        public string SelectedRepairType { get; set; }
        public string PartName { get; set; }
        public int EngineerName { get; set; }
        public List<UserDto> UserList { get; set; }
        public string SelectedMonth { get; set; }
        public long FromTrackingNo { get; set; }
        public long ToTrackingNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PartDto Part { get; set; }
        public List<PartDto> PartList { get; set; }
        public List<CustomerDto> Customers { get; set; }
        
    }
}