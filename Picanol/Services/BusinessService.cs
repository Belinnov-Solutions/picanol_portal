using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Services
{
    public class BusinessService : BaseService<PicannolEntities, tblBusiness>
    {
        #region Constructors
        internal BusinessService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public void SaveStudent(tblBusiness cl)
        {
            AddCustomer(cl);
        }
        public void AddCustomer(tblBusiness cls)
        {
            if (cls == null)
                throw new ArgumentNullException("student", "Null Parameter");
            Add(cls);
        }

        public BusinessDto GetBusinessDetails()
        {
            /*var businessDto = new BusinessDto();
            try
            {
                businessDto = (from a in Context.tblBusinesses
                         select new BusinessDto
                         {
                             Name = a.Name,
                             AddressLine1 = a.AddressLine1,
                             AddressLine2 = a.AddressLine2,
                             City = a.City,
                             State = a.State,
                             PIN = a.PIN,
                             GSTIN = a.GSTIN,
                             AccountNumber = a.AccountNumber,
                             IFSCCode = a.IFSCCode,
                             BankBranch = a.BankBranch,
                             BankName = a.BankName,
                         }).FirstOrDefault();
            }
            catch (Exception)
            {
                

                //throw;
            }*/

            var businessDto = new BusinessDto();
            try
            {

                businessDto = (from a in Context.tblBusinesses
                               select new BusinessDto
                               {
                                   Name = a.Name,
                                   AddressLine1 = a.AddressLine1,
                                   AddressLine2 = a.AddressLine2,
                                   City = a.City,
                                   State = a.State,
                                   PIN = a.PIN,
                                   GSTIN = a.GSTIN,
                                   AccountNumber = a.AccountNumber,
                                   IFSCCode = a.IFSCCode,
                                   BankBranch = a.BankBranch,
                                   BankName = a.BankName,
                               }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Optionally, you can log other details if needed
                // throw; // Uncomment if you want to rethrow the exception after logging
            }


            return businessDto;
        }
        #endregion

        #region Overrides
        public override void Add(tblBusiness student)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(student);
            }
        }


        #endregion
    }
}