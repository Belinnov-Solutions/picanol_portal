using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Helpers
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }
        public int RoleId { get; set; }

        public string RoleName { get; set; }
        public string MobileNo { get; set; }
        //public List<RoleDto> Roles { get; set; }
        //public List<ClassSectionDto> Classes { get; set; }
        //public List<SubjectDto> Subjects { get; set; }
        //public List<ApplicationDto> Applications { get; set; }
        public string alertmessage { get; set; }
    }
}