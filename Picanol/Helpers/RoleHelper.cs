using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Helpers
{
    public class RoleHelper : IDisposable
    {
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        protected iValidation validationDictionary { get; set; }
        private RoleManagerService _roleManagerService;
        protected RoleManagerService RoleManagerService
        {
            get
            {
                if (_roleManagerService == null) _roleManagerService = new RoleManagerService(entities, validationDictionary);
                return _roleManagerService;
            }
        }
        #endregion


        #region Ctor
        public RoleHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }
        #endregion




        public List<RoleDto> GetAllRole()
        {
            return RoleManagerService.GetAllRole();
        }


        #region IDisposable Members
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    disposed = true;
                    if (entities != null) entities.Dispose();
                    entities = null;

                    if (_roleManagerService != null) _roleManagerService.Dispose();
                    _roleManagerService = null;


                }
            }
        }
        #endregion
    }
}