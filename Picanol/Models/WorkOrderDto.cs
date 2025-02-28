using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class WorkOrderDto
    {
        public WorkOrderDto()
        {
            Customer = new CustomerDto();
            workOrderImageList = new List<WorkOrderImageDto>();
        }
        public int WorkOrderId { get; set; }
        public string WorkOrderNo { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<int> AssignedTo { get; set; }
        public int CustomerId { get; set; }
        public string WorkOrderType { get; set; }
        public string ContractNumber { get; set; }
        public string ContactPerson { get; set; }
        public string EmailId { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string GstIn { get; set; }
        public string Conditions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public bool DelInd { get; set; }

        public CustomerDto Customer { get; set; }
        public string WorkOrder { get; set; }
        public string CDailyAllowance { get; set; }
        public string CBoarding { get; set; }
        public string CServiceCharge { get; set; }
        public string DailyAllowance { get; set; }
        public string Boarding { get; set; }
        public string ServiceCharge { get; set; }
        public int Transport { get; set; }
        public int Fare { get; set; }

        public String CreatorName { get; set; }
        public String AssignedUserName { get; set; }
        public string FullAddress { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CallType { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public string TimeSheetDate { get; set; }
        public int TimeSheetHours { get; set; }
        public string TimeSheetRemarks { get; set; }

        public bool TimeSheetFilled { get; set; }
        public bool ProvisionalBillCreated { get; set; }
        public string ImageName { get; set; }
        public bool? InActive { get; set; }
        public List<WorkOrderImageDto> workOrderImageList { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
    }
}