using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Services
{
    public class CustomerService : BaseService<PicannolEntities, tblCustomer>
    {
        #region Constructors
        internal CustomerService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        
        public void SaveStudent(tblCustomer cl)
        {
            AddCustomer(cl);
        }
        public int AddCustomer(tblCustomer cls)
        {
            if (cls == null)
                throw new ArgumentNullException("student", "Null Parameter");
            Add(cls);
            return cls.CustomerId;
        }
        public void UpdateCustomer(tblCustomer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("user", "Null Parameter");
            Update(customer);
        }

        //public List<CustomerContactDto> GetCustomerContacts(int? id)
        //{
        //    var cl = Context.tblCustomerContacts.Where(x => x.CustomerContactId == id)
        //        .Select(x => new CustomerContactDto {
        //            CustomerContactId=x.CustomerContactId,
        //            CustomerId=x.CustomerId,
        //            Department=x.Department,
        //            ContactPersonName=x.ContactPersonName,
        //            Mobile=x.Mobile,
        //            EmailId=x.EmailId
        //                }).ToList();
        //    return cl;
        //}

        public List<CustomerDto> GetCustomersList()
        {
            var st = (from a in Context.tblCustomers
                          //where a.CustomerId > 450 && a.CustomerId <461
                          // contains
                      where a.Delind == false && a.Active == false || a.Active == true
                      select new CustomerDto
                      {
                          CustomerId = a.CustomerId,
                          CustomerName = a.CustomerName,
                          AddressLine1 = a.AddressLine1,
                          AddressLine2 = a.AddressLine2,
                          District = a.District,
                          City = a.City,
                          State = a.State,
                          StateCode = a.StateCode,
                          ContactPerson = a.ContactPerson,
                          PIN = a.PIN,
                          Email = a.Email,
                          Mobile = a.Mobile,
                          GSTIN = a.GSTIN,
                          PAN = a.PAN,
                          BigForwarding = a.BigForwarding,
                          SmallForwarding = a.SmallForwarding,
                          BigPacking = a.BigPacking,
                          SmallPacking = a.SmallPacking,
                          ShippingAddressLine1 = a.ShippingAddressLine1,
                          ShippingAddressLine2 = a.ShippingAddressLine2,
                          ShippingDistrict = a.ShippingDistrict,
                          ShippingState = a.ShippingState,
                          ShippingPIN = a.ShippingPIN,
                          ShippingStateCode = a.StateCode,
                          RepairCharges = a.RepairCharges,
                          InActive = (bool)a.Active,
                          Zone = a.Zone
                      }).ToList();
            foreach (var item in st)
            {
                if (Context.tblCustomerContacts.Any(x => x.CustomerId == item.CustomerId))
                {
                    item.CustomerContacts = (from a in Context.tblCustomerContacts
                                             where a.CustomerId == item.CustomerId
                                             select new CustomerContactDto
                                             {
                                                 CustomerContactId = a.CustomerContactId,
                                                 Department = a.Department,
                                                 Mobile = a.Mobile,
                                                 EmailId = a.EmailId,
                                                 ContactPersonName = a.ContactPersonName
                                             }).ToList();
                }
                else
                    item.CustomerContacts = new List<CustomerContactDto>();
            }
            return st;
        }
        //new method for Delete Inactive  customer

        public List<CustomerDto> GetCustomersListVersion0()
        {
            var st = (from a in Context.tblCustomers
                          //where a.CustomerId > 450 && a.CustomerId <461
                          // contains
                      where a.Delind == false && a.Active == false
                      select new CustomerDto
                      {
                          CustomerId = a.CustomerId,
                          CustomerName = a.CustomerName,
                          AddressLine1 = a.AddressLine1,
                          AddressLine2 = a.AddressLine2,
                          District = a.District,
                          City = a.City,
                          State = a.State,
                          StateCode = a.StateCode,
                          ContactPerson = a.ContactPerson,
                          PIN = a.PIN,
                          Email = a.Email,
                          Mobile = a.Mobile,
                          GSTIN = a.GSTIN,
                          PAN = a.PAN,
                          BigForwarding = a.BigForwarding,
                          SmallForwarding = a.SmallForwarding,
                          BigPacking = a.BigPacking,
                          SmallPacking = a.SmallPacking,
                          ShippingAddressLine1 = a.ShippingAddressLine1,
                          ShippingAddressLine2 = a.ShippingAddressLine2,
                          ShippingDistrict = a.ShippingDistrict,
                          ShippingState = a.ShippingState,
                          ShippingPIN = a.ShippingPIN,
                          ShippingStateCode = a.StateCode,
                          RepairCharges = a.RepairCharges,
                          InActive = (bool)a.Active
                      }).ToList();
            foreach (var item in st)
            {
                if (Context.tblCustomerContacts.Any(x => x.CustomerId == item.CustomerId))
                {
                    item.CustomerContacts = (from a in Context.tblCustomerContacts
                                             where a.CustomerId == item.CustomerId
                                             select new CustomerContactDto
                                             {
                                                 CustomerContactId = a.CustomerContactId,
                                                 Department = a.Department,
                                                 Mobile = a.Mobile,
                                                 EmailId = a.EmailId,
                                                 ContactPersonName = a.ContactPersonName
                                             }).ToList();
                }
                else
                    item.CustomerContacts = new List<CustomerContactDto>();
            }
            return st;
        }
        //End Here
        [OutputCache(Duration = 6000)]
        public List<CustomerDto> GetCustomersListSearchVersion1(string CustomerName)
        {
            //List<CustomerDto> customerList = new List<CustomerDto>();

            var customers = (from a in Context.tblCustomers
                      where a.Delind == false && a.Active == false
                      select new CustomerDto
                      {
                          CustomerId = a.CustomerId,
                          CustomerName = a.CustomerName,
                          AddressLine1 = a.AddressLine1,
                          AddressLine2 = a.AddressLine2,
                          District = a.District,
                          City = a.City,
                          State = a.State,
                          StateCode = a.StateCode,
                          ContactPerson = a.ContactPerson,
                          PIN = a.PIN,
                          Email = a.Email,
                          Mobile = a.Mobile,
                          GSTIN = a.GSTIN,
                          PAN = a.PAN,
                          BigForwarding = a.BigForwarding,
                          SmallForwarding = a.SmallForwarding,
                          BigPacking = a.BigPacking,
                          SmallPacking = a.SmallPacking,
                          ShippingAddressLine1 = a.ShippingAddressLine1,
                          ShippingAddressLine2 = a.ShippingAddressLine2,
                          ShippingDistrict = a.ShippingDistrict,
                          ShippingState = a.ShippingState,
                          ShippingPIN = a.ShippingPIN,
                          ShippingStateCode = a.StateCode,
                          RepairCharges = a.RepairCharges,
                          Zone = a.Zone,
                          InActive = (bool)a.Active

                      }).ToList();


            var scl = (from subCustomer in Context.tblSubCustomers
                       join cust in Context.tblCustomers on subCustomer.CustomerId equals cust.CustomerId
                       where subCustomer.DelInd == false
                       select new CustomerDto
                       {
                           CustomerId = (int)subCustomer.CustomerId,
                           SubCustomerId = subCustomer.SubCustomerId,
                           CustomerName = subCustomer.SubCustomerName,
                           AddressLine1 = subCustomer.AddressLine1,
                           AddressLine2 = subCustomer.AddressLine2,
                           District = subCustomer.District,
                           City = subCustomer.City,
                           State = subCustomer.State,
                           StateCode = subCustomer.StateCode,
                           ContactPerson = subCustomer.ConatctPerson,
                           PIN = subCustomer.PIN,
                           Email = subCustomer.Email,
                           Mobile = subCustomer.Mobile,
                           GSTIN = subCustomer.GSTIN,
                       }).ToList();

            var customerList = customers.Concat(scl).ToList() as List<CustomerDto>;

            foreach (var item in customerList)
            /*foreach (var item in customers)*/
            {
                if (Context.tblCustomerContacts.Any(x => x.CustomerId == item.CustomerId))
                {
                    item.CustomerContacts = (from a in Context.tblCustomerContacts
                                             where a.CustomerId == item.CustomerId
                                             select new CustomerContactDto
                                             {
                                                 CustomerContactId = a.CustomerContactId,
                                                 Department = a.Department,
                                                 Mobile = a.Mobile,
                                                 EmailId = a.EmailId,
                                                 ContactPersonName = a.ContactPersonName
                                             }).ToList();
                }
                else
                    item.CustomerContacts = new List<CustomerContactDto>();
            }




            return customerList as List<CustomerDto>;
            //return customers;
        }


        //End Here//


        //New Method for customer Management Pagination BY Himanshu//

        public List<CustomerDto> GetCustomersListVersion2(int PageSize = 10, int PageNo = 1, int CustomerId = 0, string Inactive = "")
        {
            List<CustomerDto> st = new List<CustomerDto>();
            var query = "select *, statecode as ShippingStateCode, Active as InActive from tblcustomer where delind =0 and   ";
            
            if (CustomerId != 0)
            {
                query += "customerId=" + CustomerId;
            }
            if (Inactive == "CheckActive")
            {
                query += " Active=1";
            }
            if (Inactive != "CheckActive" && CustomerId == 0)
            {
                query += " Active=0";
            }

            PicannolEntities context = new PicannolEntities();
            st = context.Database.SqlQuery<CustomerDto>(query).Skip((PageNo - 1) * PageSize).Take(PageSize).ToList<CustomerDto>();
            foreach (var item in st)
            {
                if (Context.tblCustomerContacts.Any(x => x.CustomerId == item.CustomerId))
                {
                    item.CustomerContacts = (from a in Context.tblCustomerContacts
                                             where a.CustomerId == item.CustomerId
                                             select new CustomerContactDto
                                             {
                                                 CustomerContactId = a.CustomerContactId,
                                                 Department = a.Department,
                                                 Mobile = a.Mobile,
                                                 EmailId = a.EmailId,
                                                 ContactPersonName = a.ContactPersonName
                                             }).ToList();
                }
                else
                    item.CustomerContacts = new List<CustomerContactDto>();
            }
            return st;
        }


        public List<ProformaInvoiceDto> GetProformaList(int PageSize = 10, int PageNo = 1)
        {
            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["NoOfRecordAvailable"].ToString());
            var st = (from a in Context.tblProformaInvoices
                      join b in Context.tblCustomers on a.CustomerId equals b.CustomerId  //where a.CustomerId > 450 && a.CustomerId <461
                                                                                          // where a.CustomerName.StartsWith(CustomerName)
                      where a.DelInd == false
                      select new ProformaInvoiceDto
                      {
                          ProformaInvoiceNo = a.ProformaInvoiceNo,
                          DateCreated = a.DateCreated,
                          Amount = a.Amount,
                          ProformaInvoiceDate = a.ProformaInvoiceDate,
                          CustomerName = b.CustomerName

                      }).OrderByDescending(x => x.ProformaInvoiceNo).Skip((PageNo - 1) * PageSize).Take(PageSize).OrderByDescending(x => x.ProformaInvoiceNo).ToList();
            return st;
        }

        public SubCustomerDto GetSubCustomerListBySubCustomerId(int SubCustomerId)
        {
            var scl = Context.tblSubCustomers.Where(x => x.SubCustomerId == SubCustomerId && x.DelInd == false).
                Select(x => new SubCustomerDto
                {
                    SubCustomerName = x.SubCustomerName,
                    SubCustomerId = x.SubCustomerId,
                    AddressLine1 = x.AddressLine1 != null ? x.AddressLine1 : null,
                    AddressLine2 = x.AddressLine2 != null ? x.AddressLine2 : null,
                    State = x.State,
                    StateCode = x.StateCode,
                    GSTIN = x.GSTIN != null ? x.GSTIN : null,
                    District = x.District != null ? x.District : null,
                    City = x.City != null ? x.City : null,
                    PIN = x.PIN != null ? x.PIN : null
                }).FirstOrDefault();
            return scl;
        }



        public CustomerDto GetCustomerDetails(int id)
        {
            try
            {
                var c = (from a in Context.tblCustomers
                         where a.CustomerId == id
                         where a.Delind == false

                         select new CustomerDto
                         {
                             CustomerId = a.CustomerId,
                             CustomerName = a.CustomerName,
                             AddressLine1 = a.AddressLine1,
                             AddressLine2 = a.AddressLine2,
                             District = a.District,
                             City = a.City,
                             State = a.State,
                             Zone = a.Zone,
                             PIN = a.PIN,
                             SmallForwarding = a.SmallForwarding,
                             SmallPacking = a.SmallPacking,
                             BigForwarding = a.BigForwarding,
                             BigPacking = a.BigPacking,
                             GSTIN = a.GSTIN,
                             ShippingAddressLine1 = a.ShippingAddressLine1,
                             ShippingAddressLine2 = a.ShippingAddressLine2,
                             ShippingCity = a.ShippingCity,
                             //District = a.ShippingDistrict,
                             ShippingDistrict = a.ShippingDistrict,
                             ShippingPIN = a.ShippingPIN,
                             ShippingState = a.ShippingState,
                             StateCode = a.StateCode,
                             Mobile = a.Mobile,
                             Email = a.Email,
                             ContactPerson = a.ContactPerson,
                             RepairCharges = a.RepairCharges,
                             InActive = (bool)a.Active,
                             ShippingStateCode = a.ShippingStateCode

                         }).FirstOrDefault();
                if (c != null)
                {

                    if (Context.tblCustomerContacts.Any(x => x.CustomerId == id))
                    {
                        c.CustomerContacts = (from a in Context.tblCustomerContacts
                                              where a.CustomerId == id


                                              select new CustomerContactDto
                                              {
                                                  CustomerContactId = a.CustomerContactId,
                                                  Department = a.Department,
                                                  Mobile = a.Mobile,
                                                  EmailId = a.EmailId,
                                                  ContactPersonName = a.ContactPersonName
                                              }).ToList();
                    }
                    else
                        c.CustomerContacts = new List<CustomerContactDto>();
                    return c;
                }
                else return c;

                //else
                //    c.CustomerContacts = new List<CustomerContactDto>();
                //    return c;


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void DeleteCustomer(tblCustomer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer", "Null Parameter");
            Delete(customer);

        }

        public decimal GetRepairRate(int id)
        {
            var r = (decimal)Context.tblCustomers.Where(a => a.CustomerId == id).Select(x => x.RepairCharges).SingleOrDefault();
            return r;
        }
        public string GetSubCustomerAddress(int subcustomerId)
        {
            var s = Context.tblSubCustomers.Where(x => x.SubCustomerId == subcustomerId).
                Select(x => new SubCustomerDto
                {
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    District = x.District,
                    City = x.City,
                    State = x.State,
                    PIN = x.PIN,
                }).FirstOrDefault();
            string ab = s.AddressLine1 == null ? "" : (s.AddressLine1 + ",") + ',' + s.AddressLine2 == null ? "" : (s.AddressLine2 + ",");
            ab += s.District == null ? "" : (s.District + ",");
            ab += s.City == null ? "" : (s.City + ",");
            ab += s.State == null ? "" : s.State;
            ab += s.PIN == null ? "" : ("-" + s.PIN);
            return ab;
        }
        public string GetCustomerAddress(int p)
        {
            try
            {
                var f = (from a in Context.tblCustomers
                         where a.CustomerId == p
                         where a.Delind == false
                         //where a.Delind == true
                         select new CustomerDto
                         {
                             AddressLine1 = a.AddressLine1,
                             AddressLine2 = a.AddressLine2,
                             District = a.District,
                             City = a.City,
                             State = a.State,
                             PIN = a.PIN,
                         }).FirstOrDefault();
                if (f != null)
                {
                    string ab = f.AddressLine1 + ',' + f.AddressLine2 + ',';
                    ab += f.District == null ? "" : (f.District + ",");
                    ab += f.City == null ? "" : (f.City + ",");
                    ab += f.State == null ? "" : f.State;
                    ab += f.PIN == null ? "" : ("-" + f.PIN);
                    return ab;

                }
                else return "";

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CustomerDto GetCustomerDetailsByName(string name)
        {
            var c = (from a in Context.tblCustomers
                     where a.CustomerName == name
                     where a.Delind == false
                     select new CustomerDto
                     {
                         CustomerId = a.CustomerId,
                         CustomerName = a.CustomerName,
                         AddressLine1 = a.AddressLine1,
                         AddressLine2 = a.AddressLine2,
                         City = a.City,
                         State = a.State,
                         PIN = a.PIN,
                         SmallForwarding = a.SmallForwarding,
                         SmallPacking = a.SmallPacking,
                         BigForwarding = a.BigForwarding,
                         BigPacking = a.BigPacking,
                         GSTIN = a.GSTIN
                     }).FirstOrDefault();
            return c;
        }

        public CustomerDto GetCustomerDetailsByGST(string gstNo)
        {
            var c = (from a in Context.tblCustomers
                     where a.GSTIN == gstNo
                     where a.Delind == false
                     select new CustomerDto
                     {
                         CustomerName = a.CustomerName,
                         CustomerId = a.CustomerId,
                         Email = a.Email,
                         AddressLine1 = a.AddressLine1,
                         AddressLine2 = a.AddressLine2,
                         City = a.City,
                         District = a.District,
                         State = a.State,
                         StateCode = a.StateCode,
                         ShippingAddressLine1 = a.ShippingAddressLine1,
                         ShippingAddressLine2 = a.ShippingAddressLine2,
                         PIN = a.PIN,
                         ShippingCity = a.ShippingCity,
                         ShippingDistrict = a.ShippingDistrict,
                         ShippingState = a.ShippingState,
                         ShippingPIN = a.ShippingPIN,
                         BigForwarding = a.BigForwarding,
                         BigPacking = a.BigPacking,
                         SmallForwarding = a.SmallForwarding,
                         SmallPacking = a.SmallPacking,
                         ContactPerson = a.ContactPerson,
                         GSTIN = a.GSTIN,
                         Mobile = a.Mobile,
                         RepairCharges = a.RepairCharges,
                         ShippingStateCode = a.StateCode
                     }).FirstOrDefault();
            return c;
        }

        public List<CustomerDto> GetCustomerListByName(string searchString)
        {
            var c = (from a in Context.tblCustomers
                     where a.CustomerName.Contains(searchString)
                     where a.Delind == false
                     select new CustomerDto
                     {
                         CustomerName = a.CustomerName,
                         AddressLine1 = a.ShippingAddressLine1 + "," + a.ShippingAddressLine2 + ","
                                        + a.ShippingCity + "," + a.State,
                         GSTIN = a.GSTIN
                     }).ToList();
            return c;
        }
        public CustomerDto GetCustomerDetailsById(int CustomerId)
        {
            try
            {
                var customerDetail = Context.tblCustomers.Where(x => x.CustomerId == CustomerId && x.Delind == false).
                    Select(x => new CustomerDto
                    {
                        CustomerName = x.CustomerName,
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        District = x.District != null ? x.District : null,
                        City = x.City != null ? x.City : null,
                        State = x.State,
                        StateCode = x.StateCode,
                        PIN = x.PIN != null ? x.PIN : null,
                        Email = x.Email != null ? x.Email : null,
                        Mobile = x.Mobile != null ? x.Mobile : null,
                        ContactPerson = x.ContactPerson != null ? x.ContactPerson : null,
                        GSTIN = x.GSTIN != null ? x.GSTIN : null
                    }).FirstOrDefault();
                return customerDetail;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<SubCustomerDto> GetSubCustomerListByCustomerId(int customerId)
        {
            var subcustomerList = Context.tblSubCustomers.Where(x => x.CustomerId == customerId && x.DelInd == false).
                Select(x => new SubCustomerDto
                {
                    SubCustomerId = x.SubCustomerId,
                    SubCustomerName = x.SubCustomerName,
                }).ToList();
            return subcustomerList;
        }
        public SubCustomerDto GetSubCustomerByProvisionalBillId(int ProvisionalBillId)
        {
            var subcustomerByProvisonalId = Context.tblProvisionalBills.Where(x => x.ProvisionalBillId == ProvisionalBillId).
                Select(x => new SubCustomerDto
                {
                    SubCustomerName = Context.tblSubCustomers.Where(y => y.SubCustomerId == x.SubCustomerId && y.DelInd == false).Select(y => y.SubCustomerName).FirstOrDefault(),
                    //SubCustomerId=(int)x.SubCustomerId,
                }).FirstOrDefault();
            return subcustomerByProvisonalId;
        }

        public CustomerDto GetSubCustomerDetails(int id)
        {
            var c = (from a in Context.tblSubCustomers
                     join b in Context.tblCustomers on a.CustomerId equals b.CustomerId
                     where a.SubCustomerId == id


                     select new CustomerDto
                     {
                         CustomerId = a.SubCustomerId,
                         CustomerName = a.SubCustomerName,
                         AddressLine1 = a.AddressLine1,
                         AddressLine2 = a.AddressLine2,
                         City = a.City,
                         State = a.State,
                         PIN = a.PIN,
                         SmallForwarding = b.SmallForwarding,
                         SmallPacking = b.SmallPacking,
                         BigForwarding = b.BigForwarding,
                         BigPacking = b.BigPacking,
                         GSTIN = a.GSTIN,
                         ShippingAddressLine1 = b.ShippingAddressLine1,
                         ShippingAddressLine2 = b.ShippingAddressLine2,
                         ShippingCity = b.ShippingCity,
                         District = b.ShippingDistrict,
                         ShippingPIN = b.ShippingPIN,
                         ShippingState = b.ShippingState,
                         StateCode = a.StateCode,
                         Mobile = a.Mobile,
                         Email = a.Email,
                         ContactPerson = b.ContactPerson,
                         RepairCharges = b.RepairCharges,
                         InActive = (bool)b.Active,

                     }).FirstOrDefault();
            if (Context.tblCustomerContacts.Any(x => x.CustomerId == id))
            {
                c.CustomerContacts = (from a in Context.tblCustomerContacts
                                      where a.CustomerId == id
                                      select new CustomerContactDto
                                      {
                                          CustomerContactId = a.CustomerContactId,
                                          Department = a.Department,
                                          Mobile = a.Mobile,
                                          EmailId = a.EmailId,
                                          ContactPersonName = a.ContactPersonName
                                      }).ToList();
            }
            else
                c.CustomerContacts = new List<CustomerContactDto>();
            return c;
        }

        //new Service for SubCustomer on Customer management Tab By Himanshu
        public List<SubCustomerDto> GetAllSubCustomerV1(int PageSize = 10, int PageNo = 1, string search = "")
        {
            List<SubCustomerDto> s = new List<SubCustomerDto>();
            var query = "select a.SubCustomerId ,a.AddressLine1,a.AddressLine2 as AddressLine2, " +
                        "a.District as District, a.City as City, " +
                        "a.State as State, a.StateCode as StateCode, " +
                        "a.SubCustomerId as SubCustomerId , " +
                        "a.GSTIN as GSTIN, "+
                        "b.CustomerName as CustomerName," +
                        "a.SubCustomerName as SubCustomerName from tblSubCustomer as a " +
                        "inner join tblCustomer b on a.CustomerId = b.CustomerId " +
                        "where a.DelInd = 0";
            if (search != "")
            {
                query += " and a.SubCustomerName like '%" + search + "%'";
            }
            query += " order by a.CustomerId desc";
            PicannolEntities context = new PicannolEntities();
            s = context.Database.SqlQuery<SubCustomerDto>(query).Skip((PageNo - 1) * PageSize).Take(PageSize).ToList<SubCustomerDto>();

            return s;
        }

        //End Here

        #region Overrides
        public override void Add(tblCustomer student)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(student);
            }
        }

        public override void Update(tblCustomer customer)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                tblCustomer updatecustomer = Context.tblCustomers.Where(p => p.CustomerId == customer.CustomerId).FirstOrDefault();
                updatecustomer.CustomerName = customer.CustomerName;
                updatecustomer.AddressLine1 = customer.AddressLine1;
                updatecustomer.AddressLine2 = customer.AddressLine2;
                updatecustomer.District = customer.District;
                updatecustomer.City = customer.City;
                updatecustomer.State = customer.State;
                updatecustomer.PIN = customer.PIN;
                updatecustomer.Email = customer.Email;
                updatecustomer.Mobile = customer.Mobile;
                updatecustomer.ContactPerson = customer.ContactPerson;
                updatecustomer.GSTIN = customer.GSTIN;
                //updatecustomer.StateCode = customer.StateCode;
                updatecustomer.SmallPacking = customer.SmallPacking;
                updatecustomer.BigPacking = customer.BigPacking;
                updatecustomer.SmallForwarding = customer.SmallForwarding;
                updatecustomer.BigForwarding = customer.BigForwarding;
                updatecustomer.RepairCharges = customer.RepairCharges;
                updatecustomer.ShippingAddressLine1 = customer.ShippingAddressLine1;
                updatecustomer.ShippingAddressLine2 = customer.ShippingAddressLine2;
                updatecustomer.ShippingDistrict = customer.ShippingDistrict;
                updatecustomer.ShippingState = customer.ShippingState;
                updatecustomer.ShippingPIN = customer.ShippingPIN;
                updatecustomer.StateCode = customer.StateCode;
                updatecustomer.Active = customer.Active;
                updatecustomer.Zone = customer.Zone;


                base.Update(updatecustomer);
            }
        }

        public override void Delete(tblCustomer customer)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                var itemToRemove = Context.tblCustomers.SingleOrDefault(x => x.CustomerId == customer.CustomerId); //returns a single item.

                if (itemToRemove != null)
                {
                    itemToRemove.Delind = true;
                    Context.Entry(itemToRemove).State = EntityState.Modified;
                    //Context.tblCustomers.Remove(itemToRemove);
                    Context.SaveChanges();
                }

            }
        }


        #endregion
    }
}