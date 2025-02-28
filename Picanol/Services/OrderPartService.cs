using Picanol.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Services
{
    public class OrderPartService : BaseService<PicannolEntities, tblOrderPart>
    {
        #region Constructors
        internal OrderPartService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public void SaveOrderPart(tblOrderPart cl)
        {
            AddOrderPart(cl);
        }
        public void AddOrderPart(tblOrderPart cls)
        {
            if (cls == null)
                throw new ArgumentNullException("challan", "Null Parameter");
            Add(cls);
        }
        #endregion

        #region Overrides
        public override void Add(tblOrderPart ch)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(ch);
            }
        }

        #endregion
    }
}