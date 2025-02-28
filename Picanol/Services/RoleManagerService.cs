using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Services
{
    public class RoleManagerService : BaseService<PicannolEntities, tblRole>
    {
        
        internal RoleManagerService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        public List<RoleDto> GetAllRole()
        {
            List<RoleDto> role = new List<RoleDto>();
            role = (from r in Context.tblRoles
                    select new RoleDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName
                    }).ToList();
            return role;
        }
    }
}