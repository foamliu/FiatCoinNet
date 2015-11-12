using FiatCoinNet.Common;
using FiatCoinNet.Domain;
using FiatCoinNetWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FiatCoinNetWeb
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            IssuerApiController issuerapi = new IssuerApiController();

            //Start the backend job for Issuer1
            System.Timers.Timer IssuerTimer1 = new System.Timers.Timer(10000);
            IssuerTimer1.Elapsed += new System.Timers.ElapsedEventHandler(issuerapi.bankapi.bankService.issuerService.CreateLowerLevelBlockForIssuer1);
            IssuerTimer1.Enabled = true;
            IssuerTimer1.AutoReset = true;

            //Start the backend job for Issuer2
            System.Timers.Timer IssuerTimer2 = new System.Timers.Timer(10000);
            IssuerTimer2.Elapsed += new System.Timers.ElapsedEventHandler(issuerapi.bankapi.bankService.issuerService.CreateLowerLevelBlockForIssuer2);
            IssuerTimer2.Enabled = true;
            IssuerTimer2.AutoReset = true;

            //Start the backend job for Bank
            System.Timers.Timer BankTimer = new System.Timers.Timer(60000);
            BankTimer.Elapsed += new System.Timers.ElapsedEventHandler(issuerapi.bankapi.bankService.CreateHigherLevelBlock);
            BankTimer.Enabled = true;
            BankTimer.AutoReset = true;

        }

        void Session_End(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(60000);
            //TaskAction.SetContent();
            string url = "http://localhost:17801";
            System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
            System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();

        }
    }
}
