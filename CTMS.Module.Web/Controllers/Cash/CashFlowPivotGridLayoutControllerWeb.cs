using CTMS.Module.ControllerHelpers.Cash;
using D2NXAF.ExpressApp.PivotGridLayout.Web.Controllers;
using DevExpress.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTMS.Module.Web.Controllers.Cash
{
    public class CashFlowPivotGridLayoutControllerWeb : PivotGridLayoutControllerWeb
    {
        private readonly CashFlowPivotGridSetup _PivotSetup;

        public CashFlowPivotGridLayoutControllerWeb()
        {
            TargetViewId = "CashFlow_PivotGridView";
            _PivotSetup = new CashFlowPivotGridSetup();
            PivotGridSetupObject = _PivotSetup;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            PivotGridFieldsMapped += CashFlow_PivotGridFieldsMapped;
        }

        private void CashFlow_PivotGridFieldsMapped(object sender)
        {
            PivotGridControl.OptionsDataField.ColumnValueLineCount = 2;

            PivotGridControl.Styles.CellStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.FieldValueStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.HeaderStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.RowAreaStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.ColumnAreaStyle.Wrap = DefaultBoolean.True;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            PivotGridFieldsMapped -= CashFlow_PivotGridFieldsMapped;
        }
    }
}
