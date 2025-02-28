using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Services
{
    public class ChallanService : BaseService<PicannolEntities, tblChallan>
    {
        #region Constructors
        internal ChallanService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public int SaveChallan(tblChallan cl)
        {
            int i = AddChallan(cl);
            return i;
        }
        public int AddChallan(tblChallan cls)
        {
            int i = 0;
            if (cls == null)
                throw new ArgumentNullException("challan", "Null Parameter");
            Add(cls);
            i = cls.ChallanId;
            return i;
        }
        

        public int GetChallanDetails(ChallanDto ch)
        {
            int b = 0;
            if (Context.tblChallans.Any(a => a.ChallanNumber == ch.ChallanNumber && a.CustomerId == ch.CustomerId && a.ChallanDate == ch.ChallanDate))
               b=  Context.tblChallans.Where(a => a.ChallanNumber == ch.ChallanNumber && a.CustomerId == ch.CustomerId && a.ChallanDate == ch.ChallanDate).Select(c => c.ChallanId).FirstOrDefault();
            return b;
        }
        //public List<ChallanDto> GetChallansList()
        //{
        //    var st = (from a in Context.tblChallans
        //              select new CustomerDto
        //              {
        //                  CustomerId = a.CustomerId,
        //                  CustomerName = a.CustomerName,
        //              }).ToList();
        //    return st;
        //}

        #endregion

        #region Overrides
        public override void Add(tblChallan ch)
        {
            bool validationStatus = ValidationDictionary.isValid;
            int i = 0;
            if (validationStatus)
            {
                base.Add(ch);
                i = ch.ChallanId;
            }
        }


        #endregion
    }
}