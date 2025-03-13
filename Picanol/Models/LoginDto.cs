using Picanol.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Models
{
    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public int UserID { get; set; }
        public int type { get; set; }

        public string RoleName { get; set; }
        public string EncryptedUserId { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public string Otp { get; set; }


    }
}