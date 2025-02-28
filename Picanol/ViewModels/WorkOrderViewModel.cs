using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class WorkOrderViewModel
    {
        public WorkOrderViewModel()
        {
            Customers = new List<CustomerDto>();
            WorkOrdersList = new List<WorkOrderDto>();
            WorkOrder = new WorkOrderDto();
            CustomerDetails = new CustomerDto();
            TimeSheet = new TimeSheetDto();
             Users = new List<UserDto>();
            CallTypeList = new List<ServiceCallTypeDto>();
            TimeSheetDetails = new List<TimeSheetDetailDto>();
            workOrderImageList = new List<WorkOrderImageDto>();
            WeekDates = new Dictionary<string, List<TimeSheetDetailDto>>();
            WorkOrderTypeList = Enum.GetNames(typeof(WorkOrderType)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
        }
        public List<WorkOrderImageDto> workOrderImageList { get; set; }
        public IEnumerable<SelectListItem> WorkOrderTypeList { get; set; }
        public List<ServiceCallTypeDto> CallTypeList { get; set; }
        public ServiceCallTypeDto CallType { get; set; }
        public List<WorkOrderDto> WorkOrdersList { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public List<UserDto> Users { get; set; }
        public WorkOrderDto WorkOrder { get; set; }
        public CustomerDto CustomerDetails { get; set; }
        public string SelectedWorkOrderType { get; set; }
        public string CustomerAddress { get; set; }
        public TimeSheetDto TimeSheet { get; set; }
        public List<TimeSheetDetailDto> TimeSheetDetails { get; set; }
        public Dictionary<string, List<TimeSheetDetailDto>> WeekDates { get; set; }
        public bool TimeSheetFilled { get; set; }
        public bool ProvisionalBillGenerated { get; set; }
        public string ErrorMessage { get; set; }
        public int AssignedTo { get; set; }
   
        public int Edit { get; set; }
        //Parameters for WorkOrder Filter//
        public int? CustomerId { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public int? TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CurrnetPageNo { get;set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; } 
        public int TotalNumberRecord { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }


        //Parameters for WorkOrder Filter//
    }
}