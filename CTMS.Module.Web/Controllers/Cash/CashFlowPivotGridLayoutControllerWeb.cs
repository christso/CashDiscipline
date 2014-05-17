using CTMS.Module.ControllerHelpers.Cash;
using D2NXAF.ExpressApp.PivotGridLayout.Web.Controllers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.PivotGrid.Web;
using DevExpress.Utils;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

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

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            PivotGridControl.Width = Unit.Pixel(200);
        }

        private void CashFlow_PivotGridFieldsMapped(object sender, D2NXAF.ExpressApp.PivotGridLayout.PivotGridLayoutEventArgs e)
        {
 
            PivotGridControl.OptionsCustomization.CustomizationFormStyle
                    = DevExpress.XtraPivotGrid.Customization.CustomizationFormStyle.Excel2007;
            PivotGridControl.OptionsView.ShowColumnHeaders = false;
            PivotGridControl.OptionsView.ShowRowHeaders = false;
            PivotGridControl.OptionsView.ShowDataHeaders = false;
            PivotGridControl.OptionsView.ShowFilterHeaders = false;
            PivotGridControl.OptionsView.RowTotalsLocation = PivotRowTotalsLocation.Tree;
            PivotGridControl.Styles.FieldValueStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.RowAreaStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.FilterAreaStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.ColumnAreaStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.CellStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.Styles.AreaStyle.Wrap = DefaultBoolean.True;
            PivotGridControl.OptionsDataField.ColumnValueLineCount = 2;

            foreach (PivotGridField field in PivotGridControl.Fields)
            {
                field.HeaderStyle.Wrap = DevExpress.Utils.DefaultBoolean.True;
                field.ValueStyle.Wrap = DevExpress.Utils.DefaultBoolean.True;
                field.CellStyle.Wrap = DevExpress.Utils.DefaultBoolean.True;
            }

            switch (e.Layout.Name)
            {
                case Constants.CashFlowPivotLayoutMonthlyVariance:
                    
                    break;
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            PivotGridFieldsMapped -= CashFlow_PivotGridFieldsMapped;
        }
    }
}
