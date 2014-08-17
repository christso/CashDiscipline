using DevExpress.ExpressApp;
using DevExpress.Web.ASPxGridView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DevExpress.ExpressApp.Editors;
using CTMS.Module.Controllers;
using DevExpress.Data.Filtering;

namespace CTMS.Module.Web.Controllers
{
    public class BulkUpdateListViewController : BatchDeleteListViewControllerBase
    {
        protected override CriteriaOperator ActiveFilterCriteria
        {
            get
            {
                ListEditor editor = ((ListView)View).Editor;
                ASPxGridView grid = ((ASPxGridView)editor.Control);
                var filterCriteria = CriteriaOperator.Parse(grid.FilterExpression);
                return filterCriteria;
            }
        }
        protected override bool ActiveFilterEnabled
        {
            get
            {
                ListEditor editor = ((ListView)View).Editor;
                var grid = ((ASPxGridView)editor.Control);
                return grid.FilterEnabled;
            }
        }
    }
}
