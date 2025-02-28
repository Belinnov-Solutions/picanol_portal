using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
	public class PartDto
	{
		public int PartId { get; set; }
		public string PartNumber { get; set; }
		public string PartName { get; set; }
		public string SerialNo { get; set; }
		public Nullable<decimal> Price { get; set; }
		public Nullable<decimal> GST { get; set; }
		public int? Stock { get; set; }
		public int PartTypeId { get; set; }
        public string PartType { get; set; }
		public int ItemDescription { get; set; }
		//parts Movements Getters & Setters
		public string Date { get; set; }
		public string TrackingNumber { get; set; }
		public string CustomerRefernce { get; set; }
		public string Particuler { get; set; }
		public string In { get; set; }
		public string Out { get; set; }
        public int AccountsStock { get; set; }
        public int PortalStock { get; set; }
        public int StockDifference { get; set; }
        public int NewStock { get; set; }
        public string Remarks { get; set; }
        public int OpeningBalance { get; set; }
        public int Consumption { get; set; }
        public int Balance { get; set; }
        public DateTime PurchaseDate { get; set; }
        

    }

	public class PartExcel
	{
		public string PartName { get; set; }
		public string PartNumber { get; set; }
		public int? Stock { get; set; }
		public Nullable<decimal> Price { get; set; }
		public int PartId { get; set; }
		
	}
}


			