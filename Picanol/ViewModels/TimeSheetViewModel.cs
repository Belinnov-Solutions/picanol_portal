using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class TimeSheetViewModel
    {
        public TimeSheetViewModel()
        {
            TimeSheetList = new List<TimeSheetDto>();
            TimeSheetDetails = new List<TimeSheetDetailDto>();
            WeekDates = new Dictionary<string, List<TimeSheetDetailDto>>();
        }
        public List<TimeSheetDto> TimeSheetList { get; set; }
        public List<TimeSheetDetailDto> TimeSheetDetails { get; set; }
        public Dictionary<string, List<TimeSheetDetailDto>> WeekDates { get; set; }
        public WorkOrderDto WorkOrder { get; set; }
        public int ProvisionalBillId { get; set; }
    }
}