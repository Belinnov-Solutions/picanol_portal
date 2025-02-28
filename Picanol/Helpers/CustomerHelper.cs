using Picanol.DataModel;
using Picanol.Models;
using Picanol.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;


namespace Picanol.Helpers
{
    public class CustomerHelper
    {
        #region BL Properites
        PicannolEntities entities = new PicannolEntities();
        private CustomerService _customerService;
        protected CustomerService CustomerService
        {
            get
            {
                if (_customerService == null) _customerService = new CustomerService(entities, validationDictionary);
                return _customerService;
            }
        }



        private PartsService _partsService;
        protected PartsService PartsService
        {
            get
            {
                if (_partsService == null) _partsService = new PartsService(entities, validationDictionary);
                return _partsService;
            }
        }

        private UserService _userService;
        protected UserService UserService
        {
            get
            {
                if (_userService == null) _userService = new UserService(entities, validationDictionary);
                return _userService;
            }
        }
        private ChallanService _challanService;
        protected ChallanService ChalanService
        {
            get
            {
                if (_challanService == null) _challanService = new ChallanService(entities, validationDictionary);
                return _challanService;
            }
        }
        private OrderService _orderService;
        protected OrderService OrderService
        {
            get
            {
                if (_orderService == null) _orderService = new OrderService(entities, validationDictionary);
                return _orderService;
            }
        }
        protected iValidation validationDictionary { get; set; }

        PicannolEntities _context = new PicannolEntities();
        private InvoiceHelper invoiceHelper;
        private EInvoiceHelper einvoiceHelper;

        #endregion

        #region Ctor
        public CustomerHelper(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller", "Error");
            }
            //Localize = controller.Localize;
            validationDictionary = new ModelStateWrapper();
        }

        public CustomerHelper(InvoiceHelper invoiceHelper)
        {
            this.invoiceHelper = invoiceHelper;
        }

        public CustomerHelper(EInvoiceHelper einvoiceHelper)
        {
            this.einvoiceHelper = einvoiceHelper;
        }


        #endregion

        #region Base Methods


        public string InsertCustomer(CustomerDto cs)
        {
            tblCustomer CST = new tblCustomer();
            CST.CustomerName = cs.CustomerName;
            CST.AddressLine1 = cs.AddressLine1;
            CST.AddressLine2 = cs.AddressLine2;
            CST.District = cs.District;
            CST.City = cs.City;
            CST.State = cs.State;
            CST.Zone = cs.Zone;
            CST.PIN = cs.PIN;
            CST.BusinessId = 1;
            CST.Email = cs.Email;
            CST.Mobile = cs.Mobile;
            CST.ContactPerson = cs.ContactPerson;
            // CST.ContactPerson = cs.ContactPerson;
            CST.GSTIN = cs.GSTIN;
            //CST.StateCode = cs.StateCode;
            CST.SmallPacking = cs.SmallPacking;
            CST.BigPacking = cs.BigPacking;
            CST.SmallForwarding = cs.SmallForwarding;
            CST.BigForwarding = cs.BigForwarding;
            CST.RepairCharges = cs.RepairCharges;
            CST.ShippingAddressLine1 = cs.ShippingAddressLine1;
            CST.ShippingAddressLine2 = cs.ShippingAddressLine2;
            CST.ShippingDistrict = cs.ShippingDistrict;
            CST.ShippingState = cs.ShippingState;
            CST.ShippingPIN = cs.ShippingPIN;
            CST.StateCode = cs.ShippingStateCode;
            CST.Active = cs.InActive;
            CST.Delind = false;
            CST.ShippingStateCode = cs.ShippingStateCode;
            //int customerId = CustomerService.AddCustomer(CST);
            _context.tblCustomers.Add(CST);
            _context.SaveChanges();
            return "success";
        }

        public string DeleteCustomer(int id)
        {
            tblCustomer tn = new tblCustomer();
            tn.CustomerId = id;
            CustomerService.DeleteCustomer(tn);

            return "success";

        }
        public string GetSubCustomerAddress(int subcustomerId)
        {
            return CustomerService.GetSubCustomerAddress(subcustomerId);
        }
        public string GetCustomerAddress(int pd)
        {
            return CustomerService.GetCustomerAddress(pd);
        }

        public string UpdateCustomer(CustomerDto cs)
        {
            tblCustomer CST = new tblCustomer();
            CST.CustomerId = cs.CustomerId;
            CST.CustomerName = cs.CustomerName;
            CST.AddressLine1 = cs.AddressLine1;
            CST.AddressLine2 = cs.AddressLine2;
            CST.District = cs.District;
            CST.City = cs.City;
            CST.State = cs.State;
            CST.Zone = cs.Zone;
            CST.PIN = cs.PIN;
            CST.Email = cs.Email;
            CST.Mobile = cs.Mobile;
            CST.ContactPerson = cs.ContactPerson;
            CST.BusinessId = 1;
            //CST.ContactPerson = cs.ContactPerson;
            CST.GSTIN = cs.GSTIN;
            CST.StateCode = cs.StateCode;
            CST.SmallPacking = cs.SmallPacking;
            CST.BigPacking = cs.BigPacking;
            CST.SmallForwarding = cs.SmallForwarding;
            CST.BigForwarding = cs.BigForwarding;
            CST.RepairCharges = cs.RepairCharges;
            CST.ShippingAddressLine1 = cs.ShippingAddressLine1;
            CST.ShippingAddressLine2 = cs.ShippingAddressLine2;
            CST.ShippingDistrict = cs.ShippingDistrict;
            CST.ShippingState = cs.ShippingState;
            CST.ShippingCity = cs.ShippingCity;
            CST.ShippingPIN = cs.ShippingPIN;
            CST.ShippingStateCode = cs.ShippingStateCode;
            //CST.StateCode = cs.ShippingStateCode;
            // CST.Shipp = cs.ShippingStateCode;
            CST.Active = cs.InActive;
            CST.Delind = false;

            _context.Entry(CST).State = EntityState.Modified;
            _context.SaveChanges();
            if (cs.CustomerContacts != null)
            {
                foreach (var item in cs.CustomerContacts)
                {
                    if (_context.tblCustomerContacts.Any(x => x.CustomerId == cs.CustomerId))
                    {
                        var tcc = _context.tblCustomerContacts.Where(x => x.CustomerContactId == item.CustomerContactId).FirstOrDefault();
                       if(tcc != null)
                        {//tcc.Department = item.Department;
                            tcc.ContactPersonName = item.ContactPersonName;
                            tcc.Mobile = item.Mobile;
                            tcc.EmailId = item.EmailId;
                            tcc.CustomerContactId = item.CustomerContactId;
                            tcc.DelInd = false;
                            _context.Entry(tcc).State = EntityState.Modified;
                            _context.SaveChanges();

                        }
                        
                    }
                    else
                    {
                        tblCustomerContact tcc = new tblCustomerContact();
                        tcc.Department = item.Department;
                        tcc.ContactPersonName = item.ContactPersonName;
                        tcc.Mobile = item.Mobile;
                        tcc.EmailId = item.EmailId;
                        tcc.DelInd = false;
                        tcc.CustomerId = cs.CustomerId;
                        _context.tblCustomerContacts.Add(tcc);
                        _context.SaveChanges();
                    }
                }
            }
            return "Updated";
        }

        //public List<CustomerContactDto> GetCustomerContacts(int? id)
        //{
        //    return CustomerService.GetCustomerContacts(id);
        //}
        public CustomerDto GetCustomerDetailsById(int CustomerId)
        {
            return CustomerService.GetCustomerDetailsById(CustomerId);
        }
        public List<SubCustomerDto> GetSubCustomerListByCustomerId(int customerId)
        {
            return CustomerService.GetSubCustomerListByCustomerId(customerId);
        }
        public SubCustomerDto GetSubCustomerByProvisionalBillId(int ProvisionalBillId)
        {
            return CustomerService.GetSubCustomerByProvisionalBillId(ProvisionalBillId);
        }

        public List<CustomerDto> GetCustomersList()
        {
            return CustomerService.GetCustomersList();
        }
        //public List<CustomerDto> GetZoneList()
        //{
        //    return CustomerService.GetZoneList();
        //}

        //new helper for subcustomer added
        public List<CustomerDto> GetCustomersListVersion0()
        {
            return CustomerService.GetCustomersListVersion0();
        }
        //end here
        //new for all invoices list
        //public List<CustomerDto> GetCustomersListV1(InvoiceListViewModel vm)
        //{
        //    return CustomerService.GetCustomersListV1(vm);
        //}
        // end here

        //new method  for Pagination Himanshu//
        public List<CustomerDto> GetCustomersListVersion2(int PageSize, int PageNo, int CustomerId, string Inactive)
        {
            return CustomerService.GetCustomersListVersion2(PageSize, PageNo, CustomerId, Inactive);
        }

        public CustomerDto GetCustomerDetails(int CustomerID)
        {
            return CustomerService.GetCustomerDetails(CustomerID);
        }
        public CustomerDto GetSubCustomerDetails(int CustomerID)
        {
            return CustomerService.GetSubCustomerDetails(CustomerID);
        }

        public SubCustomerDto GetSubCustomerListBySubCustomerId(int SubCustomerId)
        {
            return CustomerService.GetSubCustomerListBySubCustomerId(SubCustomerId);
        }

        public CustomerDto GetCustomerDetailsByName(string name)
        {
            return CustomerService.GetCustomerDetailsByName(name);
        }


        public void SaveCustomerEmail(int customerID, string emailIDs)
        {
            PicannolEntities context = new PicannolEntities();
            var customer = context.tblCustomers.Where(x => x.CustomerId == customerID).FirstOrDefault();
            customer.Email = emailIDs;
            context.tblCustomers.Attach(customer);
            context.Entry(customer).State = EntityState.Modified;
            context.SaveChanges();
        }

        public CustomerDto GetCustomerDetailsByGST(string gstNumber)
        {
            return CustomerService.GetCustomerDetailsByGST(gstNumber);
        }

        public List<GSTStateDto> GetStatesList()
        {
            PicannolEntities context = new PicannolEntities();
            var s = (from a in context.tblGSTStates
                     where a.DelInd == false
                     select new GSTStateDto
                     {
                         StateName = a.StateName + ";" + a.StateCode
                     }).ToList();
            return s;
        }

        //Added by Janesh
        public List<ZoneDto> GetZoneList()
        {
            PicannolEntities context = new PicannolEntities();
            var s = (from a in context.tblZones
                     where a.DelInd == false
                     select new ZoneDto
                     {
                         Zone = a.Zone
                     }).ToList();
            return s;
        }
        //End

        public List<CustomerDto> GetCustomerListByName(string searchString)
        {
            return CustomerService.GetCustomerListByName(searchString);
        }

        public List<SubCustomerDto> GetSubCustomerDetail(int customerDetail)
        {
            PicannolEntities context = new PicannolEntities();
            var query = from a in context.tblSubCustomers
                        where a.CustomerId == customerDetail && a.DelInd == false
                        select new SubCustomerDto
                        {
                            ConatctPerson = a.ConatctPerson,
                            SubCustomerName = a.SubCustomerName,
                            AddressLine1 = a.AddressLine1,
                            AddressLine2 = a.AddressLine2,
                            SubCustomerId = a.SubCustomerId,
                            District = a.District,
                            City = a.City,
                            State = a.State,
                            StateCode = a.StateCode,
                            PIN = a.PIN,
                            Mobile = a.Mobile,
                            Email = a.Email,
                            GSTIN = a.GSTIN,
                            CustomerId = (int)a.CustomerId
                        };
            return query.ToList();
        }
        #endregion


        #region for SubCustomerCRUD
        public SubCustomerDto CheckCustomerDeatils(int ProvisionalBillId, int CustomerId)
        {
            var checkSubCustomerId = _context.tblProvisionalBills.Where(x => x.CustomerId == CustomerId && x.ProvisionalBillId == ProvisionalBillId).
                  Select(x => new SubCustomerDto
                  {
                      CustomerName = _context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                      SubCustomerName = _context.tblSubCustomers.Where(y => y.SubCustomerId == x.SubCustomerId).Select(y => y.SubCustomerName).FirstOrDefault(),
                      SubCustomerId = x.SubCustomerId != null ? (int)x.SubCustomerId : 0
                  }).FirstOrDefault();
            return checkSubCustomerId;
        }

        public string SaveSubCustomerDetails(SubCustomerDto request)
        {
            string response = "";
            try
            {
                var getSubCustomerDetails = getInsertDetails(request);
                _context.tblSubCustomers.Add(getSubCustomerDetails);
                _context.SaveChanges();
                response = "Successs";
            }
            catch (DbEntityValidationException e)
            {

                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
                //response = ex.Message;
            }
            return response;
        }
        public string UpdateSubCustomerDetails(SubCustomerDto request)
        {
            string response = "";
            try
            {
                var customerDeatils = _context.tblSubCustomers.FirstOrDefault(x => x.DelInd == false && x.SubCustomerId == request.SubCustomerId);
                if (customerDeatils != null)
                {
                    customerDeatils.AddressLine1 = request.AddressLine1;
                    customerDeatils.SubCustomerName = request.SubCustomerName;
                    customerDeatils.AddressLine2 = request.AddressLine2;
                    customerDeatils.AddressLine2 = request.City;
                    customerDeatils.ConatctPerson = request.ConatctPerson;
                    customerDeatils.CustomerId = request.CustomerId;
                    customerDeatils.District = request.District;
                    customerDeatils.City = request.City;
                    customerDeatils.GSTIN = request.GSTIN;
                    customerDeatils.Mobile = request.Mobile;
                    customerDeatils.State = request.State;
                    customerDeatils.StateCode = request.StateCode;
                    _context.Entry(customerDeatils).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                response = "Updated";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return response;
        }
        public string DeleteSubCustomer(int subCustomerId)
        {
            var checkSubCustomer = _context.tblSubCustomers.FirstOrDefault(x => x.SubCustomerId == subCustomerId && x.DelInd == false);
            if (checkSubCustomer != null)
            {
                checkSubCustomer.DelInd = true;
                _context.SaveChanges();

            }
            return "Success";
        }
        public IQueryable<SubCustomerDto> GetAllSubCustomer(int PageSize = 10, int PageNo = 1)
        {
            var subCustomerList = _context.tblSubCustomers.Where(x => x.DelInd == false).
                Select(x => new SubCustomerDto
                {

                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    District = x.District,
                    City = x.City,
                    State = x.State,
                    StateCode = x.StateCode,
                    PIN = x.PIN,
                    ConatctPerson = x.ConatctPerson,
                    Mobile = x.Mobile,
                    Email = x.Email,
                    GSTIN = x.GSTIN,
                    SubCustomerId = x.SubCustomerId,
                    CustomerName = _context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault(),
                    SubCustomerName = x.SubCustomerName
                });
            return subCustomerList;
        }
        public IQueryable<SubCustomerDto> SearchSubCustomerList(string searching)
        {
            var sl = _context.tblSubCustomers.Where(x => x.SubCustomerName.ToUpper().Contains(searching.ToUpper())).
                Select(x => new SubCustomerDto
                {
                    SubCustomerId = x.SubCustomerId,
                    SubCustomerName = x.SubCustomerName,
                    State = x.State,
                    StateCode = x.StateCode,
                    GSTIN = x.GSTIN,
                    PIN = x.PIN,
                    AddressLine1 = x.AddressLine1,
                    CustomerName = _context.tblCustomers.Where(y => y.CustomerId == x.CustomerId).Select(y => y.CustomerName).FirstOrDefault()


                });
            return sl;
        }
        private tblSubCustomer getInsertDetails(SubCustomerDto req)
        {
            return new tblSubCustomer
            {
                
                SubCustomerName = req.SubCustomerName,
                AddressLine1 = req.AddressLine1,
                AddressLine2 = req.AddressLine2,
                District = req.District,
                City = req.City,
                State = req.State,
                StateCode = req.StateCode,
                PIN = req.PIN,
                ConatctPerson = req.ConatctPerson,
                Mobile = req.Mobile,
                Email = req.Email,
                GSTIN = req.GSTIN,
                CustomerId = req.CustomerId,
                DelInd = false

            };
        }
        //new Helper for Pagination in SubCustomerList
        public List<SubCustomerDto> GetAllSubCustomerV1(int PageSize = 10, int PageNo = 1, string search = "")
        {
            return CustomerService.GetAllSubCustomerV1(PageSize, PageNo, search);
        }
        #endregion

    }
}