using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using CashDiscipline.Module.BusinessObjects.Cash;

namespace CashDiscipline.Module.Win.Controllers
{
    public class BankStmtFilterViewController : ViewController
    {
        public BankStmtFilterViewController()
        {
            TargetObjectType = typeof(BankStmt);
            TargetViewType = ViewType.ListView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            View.ControlsCreated -= View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            var gridListEditor = ((ListView)View).Editor as GridListEditor;
            if (gridListEditor != null)
            {
                GridView view = gridListEditor.GridView;
                // set default filter condition for TranAmount to equals, otherwise it will default to starts with
                var column = view.Columns["TranAmount"];
                if (column != null && column.OptionsFilter != null)
                    column.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals;
            }
        }
    }
}
