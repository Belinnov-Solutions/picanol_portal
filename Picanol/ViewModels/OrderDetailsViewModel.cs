using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;
using static Picanol.Helpers.ConstantsHelper;

namespace Picanol.ViewModels
{
    public class OrderDetailsViewModel
    {
        public OrderDetailsViewModel(){
            OrderDetails = new OrderDto();
            ConsumedParts = new List<OrderPartDto>();
            NewConsumedParts = new List<OrderPartDto>();
            Parts = new List<PartDto>();
            OrderStatusList = Enum.GetNames(typeof(OrderProgress)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
            PackingTypeList = Enum.GetNames(typeof(PackingTypes)).Select(name => new SelectListItem()
            {
                Text = name,
                Value = name
            });
        }
        public OrderStatus OrderStatus { get; set; }
        public IEnumerable<SelectListItem> OrderStatusList { get; set; }
        public string Status { get; set; }
        public string PackingTypes { get; set; }
        public IEnumerable<SelectListItem> PackingTypeList { get; set; }
        public List<OrderPartDto> ConsumedParts { get; set; }
        public List<PartDto> Parts { get; set; }
        public OrderDto OrderDetails { get; set; }
        public List<OrderPartDto> NewConsumedParts { get; set; }
        public OrderPartDto orderPart { get;  set;}
        public Guid SelectedOrderGuid { get; set; }
        public string PartNo { get; set; }
        public int Quantity { get; set; }

        public int PartId { get; set; }
        public List<UserDto> Users { get; set; }

        public string message { get; set; }
    }
   
   
}