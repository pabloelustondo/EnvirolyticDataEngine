using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebMatrix.WebData;

namespace JassWeather
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
/*
            System.Data.Entity.Database.SetInitializer(
                new JassWeather.Models.JassWeatherContextInitializer());

 */
            
            WebSecurity.InitializeDatabaseConnection(
                 connectionStringName: "DefaultConnection",
                 userTableName: "UserProfile",
                 userIdColumn: "UserID",
                 userNameColumn: "UserName",
                 autoCreateTables: true);

            if (!Roles.RoleExists("Admin")) {
                Roles.CreateRole("Admin") ;
            }
            if (!Roles.RoleExists("Envirolytic"))
            {
                Roles.CreateRole("Envirolytic");
            }

            if (Roles.GetRolesForUser("pablo").Length <1)
            {
                Roles.AddUsersToRole(new string[1] { "pablo" }, "Admin");
            }
 

        }
    }
}