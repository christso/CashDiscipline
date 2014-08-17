using CTMS.Module.Controllers.Cash;
using DevExpress.ExpressApp.PivotGrid.Win;
using D2NXAF.ExpressApp.PivotGrid.Win.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Win.Controllers
{
    public class CTMSPivotGridDrillDownController : PivotGridDrillDownController
    {
        public CTMSPivotGridDrillDownController()
        {
        }
        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected override void OnPivotGridControlCellDoubleClick(object sender, DevExpress.XtraPivotGrid.PivotCellEventArgs e)
        {
            if (View.Id == "CashFlow_PivotGridView")
            {
                // do not drill down if field is not a CashFlowNote as you want to show the note detail view
                if (e.DataField.Caption != CashFlowPivotGridController.fieldNoteCaption)
                {
                    ProcessAction(e);
                }
            }
            else
            {
                ProcessAction(e);
            }
        }
    }
}
