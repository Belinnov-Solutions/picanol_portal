using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            
            role = new List<RoleDto>();
            user = new List<UserDto>();
            User = new UserDto();

        }
        public List<RoleDto> role { get; set; }
        public int SelectedBusinessId { get; set; }
        public int SelectedRoleId { get; set; }
        public List<UserDto> user { get; set; }
        public UserDto User { get; set; }
    }
}