using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class TimeSheetDetailDto
    {
        public int TimeSheetDetailId { get; set; }
        public int TimeSheetId { get; set; }
        public string WorkDate { get; set; }
        public string TotalHours { get; set; }
        public string Description { get; set; }
        public bool isWeekend { get; set; }
        public bool DelInd { get; set; }
        public string WeekNo { get; set; }
    }
}