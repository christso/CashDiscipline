using CTMS.Module.Controllers.FinAccounting;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Win.Editors;

namespace CTMS.Module.Win.Controllers.FinAccounting
{
    public class GenLedgerListViewControllerWin : GenLedgerListViewController
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            ((ListView)View).ControlsCreated += GenLedgerListViewControllerWin_ControlsCreated;
        }

        void GenLedgerListViewControllerWin_ControlsCreated(object sender, EventArgs e)
        {
            var editor = ((ListView)View).Editor;
            if (editor is GridListEditor)
            {
                var gridView = ((GridListEditor)editor).GridView;
                gridView.OptionsView.ColumnAutoWidth = false;
            }
        }
    }
}
