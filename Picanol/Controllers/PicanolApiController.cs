using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using System.Text;
using Picanol.ViewModels;
using Picanol.Models;
using System.Web.Http.Results;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Net.Http;

namespace Picanol.Controllers
{
    public class PicanolApiController : ApiController
    {

        ServiceRequestController SRC = new ServiceRequestController();
        HomeController HM = new HomeController();


        [AllowAnonymous]
        [System.Web.Http.Route("ServiceRequest")]
        [HttpPost]
        public IHttpActionResult ServiceRequest(int? ProvisionalBillId, int? CustomerId, string CallType, int? WorkOrderId)
        {

            return Ok(SRC.V1ServiceRequest(ProvisionalBillId, CustomerId, CallType, WorkOrderId));
        }

         [System.Web.Http.Route("TimeSheet")]
        [HttpPost]
        public IHttpActionResult TimeSheet(int ProvisionalBillId, int workOrderId, int? type)
        {

            return Ok(SRC.V1TimeSheet(ProvisionalBillId, workOrderId, type));
        }

         [System.Web.Http.Route("ProvisionalBillDetails")]
        [HttpPost]
        public IHttpActionResult ProvisionalBillDetails(int ProvisionalBillId)
        {

            return Ok(SRC.V1ProvisionalBillDetails(ProvisionalBillId));
        }

         [System.Web.Http.Route("ServiceRequestsList")]
        [HttpPost]
        public IHttpActionResult ServiceRequestsList(ServiceRequestViewModel svm)
        {

            return Ok(SRC.V1ServiceRequestsList(svm));
        }


         [System.Web.Http.Route("ProvisionalBillList")]
        [HttpPost]
        public IHttpActionResult ProvisionalBillList(int WorkOrderId, int? Type)
        {

            return Ok(SRC.V1ProvisionalBillList(WorkOrderId, Type));
        }

         [System.Web.Http.Route("Login")]
       // [HttpPost]
        //public IHttpActionResult Login(LoginDto ld)
        //{

        //    return Ok(HM.V1Login(ld));
        //}

        ////new api made by ritu 3 oct
        // [System.Web.Http.Route("VarifyOtp")]
        //[HttpPost]
        //public IHttpActionResult VarifyOtp(LoginDto ld)
        //{

        //    return Ok(HM.V1VarifyOtp(ld));
        //}

         [System.Web.Http.Route("TimeSheetAuthorization")]
        [HttpPost]
        public IHttpActionResult TimeSheetAuthorization(TimeSheetDto vm)
        {

            return Ok(SRC.V1TimeSheetAuthorization(vm));
        }



        [System.Web.Http.Route("GetSubCustomerList")]
        [HttpGet]
        public IHttpActionResult GetSubCustomerList(int CustomerId)
        {
            return Ok(SRC.V1GetSubCustomerList(CustomerId));
        }

        //[System.Web.Http.Route("GetCustomersList")]
       // [HttpGet]
        //public IHttpActionResult GetCustomersList( )
        //{

        //   // var customers = HM.V1GetCustomersList();
        //   // var actual = ResponseResultExtractor.Extract<List<CustomerDto>>(customers);

        //    return Ok(HM.V1GetCustomersList());
        //   // return Request.CreateResponse(HttpStatusCode.OK, customers, Configuration.Formatters.JsonFormatter);

        //    //return new HttpResponseMessage()
        //    //{
        //    //    Content = new StringContent(JsonConvert.SerializeObject(customers), Encoding.UTF8, "application/json")
        //    //};
        //    //List<CustomerDto> customers = new List<CustomerDto>();
        //    //  HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, HM.GetCustomersList());
        //    //  IEnumerable<CustomerDto> customers= HM.GetCustomersList().;

        //    // response.Content = HM.GetCustomersList();
        //    // var customers = HM.GetCustomersList();
        //    // return base.Content(HttpStatusCode.OK, new { }, new JsonMediaTypeFormatter(), response.Content.ReadAsStringAsync().Result);

        //    //HttpResponseMessage Res = await client.GetAsync(customers);
        //    //if (response.IsSuccessStatusCode)
        //    //{

        //    // var ObjResponse = response.Content.ReadAsStringAsync().Result;
        //    //  customers = JsonConvert.DeserializeObject<List<CustomerDto>>(ObjResponse);

        //    //}
        //    // return Ok(ObjResponse);
        //    // return Ok(response.Content.ReadAsStringAsync().Result);
        //}


        [System.Web.Http.Route("WorkOrdersList")]
        [HttpGet]
        public IHttpActionResult WorkOrdersList(WorkOrderViewModel wvm )
        {
           

            return Ok(SRC.V1WorkOrdersList(wvm));
        }

        [System.Web.Http.Route("GetServiceRequestDetails")]
        [HttpGet]
        public IHttpActionResult GetServiceRequestDetails(int WorkOrderId)
        {
            return Ok(SRC.V1GetServiceRequestDetails(WorkOrderId));
        }

        [System.Web.Http.Route("SendServiceRequestEmail")]
        [HttpGet]
        public IHttpActionResult SendServiceRequestEmail(int ProvisionalBillId, int? WorkOrderId, string EmailID, string CName, string MailType, string UserName, int? type)
        {
            return Ok(SRC.V1SendEmail(ProvisionalBillId, WorkOrderId, EmailID, CName, MailType, UserName,type));
        }

         [System.Web.Http.Route("UpdateServiceRequest")]
        [HttpGet]
        public IHttpActionResult UpdateServiceRequest(int ProvisionalBillId, int? WorkOrderId, string EmailID, string CName, string MailType, string UserName, int? type)
        {
            return Ok(SRC.V1SendEmail( ProvisionalBillId, WorkOrderId, EmailID, CName, MailType, UserName,type));
        }

         [System.Web.Http.Route("GetProvisionalBillName")]
        [HttpGet]
        public IHttpActionResult GetProvisionalBillName(int ProvisionalBillId)
        {
            return Ok(SRC.V1GetProvisionalBillName( ProvisionalBillId));
        }





    }
}
