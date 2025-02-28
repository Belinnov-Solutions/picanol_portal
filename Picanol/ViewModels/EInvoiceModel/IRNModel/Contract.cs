using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels.EInvoiceModel.IRNModel
{
    public class Contract
    {
        /// <summary>
        /// "Receipt Advice No."
        /// </summary>
        public string RecAdvDt { get; set; }
        /// <summary>
        /// "Date of receipt advice"
        /// </summary>
        public string RecAdvRefr { get; set; }
        /// <summary>
        /// "Lot/Batch Reference No."
        /// </summary>
        public string TendRefr { get; set; }
        /// <summary>
        /// "Contract Reference Number"
        /// </summary>
        public string ContrRefr { get; set; }
        /// <summary>
        ///  "Any other reference"
        /// </summary>
        public string ExtRefr { get; set; }
        /// <summary>
        /// "Project Reference Number"
        /// </summary>
        public string ProjRefr { get; set; }
        /// <summary>
        /// "Vendor PO Reference Number"
        /// </summary>
        public string PORefr { get; set; }
        /// <summary>
        /// "Vendor PO Reference date"
        /// </summary>
        public string PORefDt { get; set; }

    }
}