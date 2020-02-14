using DataAccess;
using Oracle.ManagedDataAccess.Client;
using PTLDistributeWebAPI.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using System.Web.SessionState;

namespace PTLDistributeWebAPI
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(PTLDistributeWebAPIConfig.Register);

            //System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            System.Configuration.Configuration configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            ConnectionStringSettings connectionStringSettings = configuration.ConnectionStrings.ConnectionStrings["GeelyPtlEntities"];
            //使用数据库登录名作为 Oracle 架构名
            if (connectionStringSettings.ProviderName.Contains("Oracle"))
            {
                OracleConnectionStringBuilder oracleConnectionStringBuilder = new OracleConnectionStringBuilder(connectionStringSettings.ConnectionString);
                GeelyPtlEntities.SchemaName = oracleConnectionStringBuilder.UserID;
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}