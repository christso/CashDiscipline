using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp;

namespace CTMS.Web
{
    public partial class SkipLogonForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((CTMSAspNetApplication)WebApplication.Instance).Logon(Request.QueryString["UserName"], "");
            WebApplication.Redirect("Default.aspx");
        }
    }
}