using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Picanol
{
    public class RouteConfig

    {
        #region original Route
        //***************************Original ***********************************************
        // public static void RegisterRoutes(RouteCollection routes)
        // {
        //     routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        //     RouteTable.Routes.MapHttpRoute(
        //         name: "DefaultApi",
        //         routeTemplate: "api/{controller}/{action}/{id}",
        //         defaults: new { id = System.Web.Http.RouteParameter.Optional }
        //         );
        //     routes.MapRoute(
        //       name: "ServiceRequest",
        //       url: "ServiceRequest",
        //       defaults: new { controller = "ServiceRequest", action = "ServiceRequest" }
        //   );


        //     routes.MapRoute(
        //     name: "TimeSheet",
        //     url: "TimeSheet",
        //     defaults: new { controller = "ServiceRequest", action = "TimeSheet" }
        // );

        //     routes.MapRoute(
        //      name: "ProvisionalBillDetails",
        //      url: "ProvisionalBillDetails",
        //      defaults: new { controller = "ServiceRequest", action = "ProvisionalBillDetails" }
        //  );



        //     routes.MapRoute(
        //       name: "ServiceRequestsList",
        //       url: "ServiceRequestsList",
        //       defaults: new { controller = "ServiceRequest", action = "ServiceRequestsList" }
        //   );




        //     routes.MapRoute(
        //       name: "ProvisionalBillList",
        //       url: "ProvisionalBillList",
        //       defaults: new { controller = "ServiceRequest", action = "ProvisionalBillList" }
        //   );


        //     routes.MapRoute(
        //       name: "Login",
        //       url: "Login",
        //       defaults: new { controller = "Home", action = "Login" }
        //   );

        //     routes.MapRoute(
        //     name: "TimeSheetAuthorization",
        //     url: "TimeSheetAuthorization",
        //     defaults: new { controller = "ServiceRequest", action = "TimeSheetAuthorization" }
        // );
        //     routes.MapRoute(
        //       name: "GetSubCustomerList",
        //       url: "GetSubCustomerList",
        //       defaults: new { controller = "ServiceRequest", action = "GetSubCustomerList" }
        //   );

        //     routes.MapRoute(
        //        name: "GetCustomersList",
        //        url: "GetCustomersList",
        //        defaults: new { controller = "Home", action = "GetCustomersList" }
        //    );
        //     routes.MapRoute(
        //       name: "WorkOrdersList",
        //       url: "WorkOrdersList",
        //       defaults: new { controller = "ServiceRequest", action = "WorkOrdersList" }
        //   );
        //     routes.MapRoute(
        //      name: "GetServiceRequestDetails",
        //      url: "GetServiceRequestDetails",
        //      defaults: new { controller = "ServiceRequest", action = "GetServiceRequestDetails" }
        //  );
        //     routes.MapRoute(
        //     name: "SendServiceRequestEmail",
        //     url: "SendServiceRequestEmail",
        //     defaults: new { controller = "ServiceRequest", action = "SendEmail" }
        // );
        //     routes.MapRoute(
        //    name: "UpdateServiceRequest",
        //    url: "UpdateServiceRequest",
        //    defaults: new { controller = "ServiceRequest", action = "SendEmail" }
        //);
        //     routes.MapRoute(
        //    name: "GetProvisionalBillName",
        //    url: "GetProvisionalBillName",
        //    defaults: new { controller = "ServiceRequest", action = "GetProvisionalBillName" }
        //);

        //     routes.MapRoute(
        //         name: "Default",
        //         url: "{controller}/{action}/{id}",
        //         defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
        //     );
        // }
        #endregion
        #region update Route config
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional }
                );
            routes.MapRoute(
              name: "ServiceRequest",
              url: "ServiceRequest",
              defaults: new { controller = "MobileServiceRequest", action = "ServiceRequest" }
          );


            routes.MapRoute(
            name: "TimeSheet",
            url: "TimeSheet",
            defaults: new { controller = "MobileServiceRequest", action = "TimeSheet" }
        );

            routes.MapRoute(
             name: "ProvisionalBillDetails",
             url: "ProvisionalBillDetails",
             defaults: new { controller = "MobileServiceRequest", action = "ProvisionalBillDetails" }
         );



            routes.MapRoute(
              name: "ServiceRequestsList",
              url: "ServiceRequestsList",
              defaults: new { controller = "MobileServiceRequest", action = "ServiceRequestsList" }
          );




            routes.MapRoute(
              name: "ProvisionalBillList",
              url: "ProvisionalBillList",
              defaults: new { controller = "MobileServiceRequest", action = "ProvisionalBillList" }
          );


            routes.MapRoute(
              name: "Login",
              url: "Login",
              defaults: new { controller = "MobileHome", action = "Login" }
          );

            routes.MapRoute(
            name: "TimeSheetAuthorization",
            url: "TimeSheetAuthorization",
            defaults: new { controller = "MobileServiceRequest", action = "TimeSheetAuthorization" }
        );
            routes.MapRoute(
              name: "GetSubCustomerList",
              url: "GetSubCustomerList",
              defaults: new { controller = "MobileServiceRequest", action = "GetSubCustomerList" }
          );

            routes.MapRoute(
               name: "GetCustomersList",
               url: "GetCustomersList",
               defaults: new { controller = "MobileHome", action = "GetCustomersList" }
           );
            routes.MapRoute(
              name: "WorkOrdersList",
              url: "WorkOrdersList",
              defaults: new { controller = "MobileServiceRequest", action = "WorkOrdersList" }
          );
            routes.MapRoute(
             name: "GetServiceRequestDetails",
             url: "GetServiceRequestDetails",
             defaults: new { controller = "MobileServiceRequest", action = "GetServiceRequestDetails" }
         );
            routes.MapRoute(
            name: "SendServiceRequestEmail",
            url: "SendServiceRequestEmail",
            defaults: new { controller = "MobileServiceRequest", action = "SendEmail" }
        );
            routes.MapRoute(
           name: "UpdateServiceRequest",
           url: "UpdateServiceRequest",
           defaults: new { controller = "MobileServiceRequest", action = "SendEmail" }
       );
            routes.MapRoute(
           name: "GetProvisionalBillName",
           url: "GetProvisionalBillName",
           defaults: new { controller = "MobileServiceRequest", action = "GetProvisionalBillName" }
       );
            routes.MapRoute(
           name: "SendOtp",
           url: "SendOtp",
           defaults: new { controller = "MobileHome", action = "SendOtp" }
       );
            routes.MapRoute(
           name: "SaveTimeSheet",
           url: "SaveTimeSheet",
           defaults: new { controller = "MobileServiceRequest", action = "SaveTimeSheet" }
       );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );
        }
        #endregion
    }
}
