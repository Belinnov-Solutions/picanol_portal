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
    public class BusinessHelper
    {
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        private BusinessService _businessService;
        protected BusinessService BusinessService
        {
            get
            {
                if (_businessService == null) _businessService = new BusinessService(entities, validationDictionary);
                return _businessService;
            }
        }

        protected iValidation validationDictionary { get; set; }



        #endregion

        #region Ctor
        public BusinessHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }
        #endregion

        #region Base Methods
        public BusinessDto GetBusinessDetails()
        {
            return BusinessService.GetBusinessDetails();
        }

        #endregion
    }
}