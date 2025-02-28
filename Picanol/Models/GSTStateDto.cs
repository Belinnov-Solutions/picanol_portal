using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class GSTStateDto
    {
        public int GSTStateId { get; set; }
        public string StateName { get; set; }
        public string StateInitials { get; set; }
        public string StateCode { get; set; }
        public string StateType { get; set; }
        public bool DelInd { get; set; }
    }
}