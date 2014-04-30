using CTMS.Module.Editors;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CTMS.Web.Controls
{
    public partial class BankStmtAnalysisControl : System.Web.UI.UserControl, IXpoSessionAwareControl 
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void UpdateDataSource(DevExpress.Xpo.Session session)
        {

        }

    }
}