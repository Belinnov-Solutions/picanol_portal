using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels
{
    public class PartsMovementViewModel
    {
        public PartsMovementViewModel()
        {
            movementList = new List<PartMovementDto>();
        }

        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public int PartStock { get; set; }
        public int OpeningStock { get; set; }
        public List<PartMovementDto> movementList { get; set; }
    }
}