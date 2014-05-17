using CTMS.Module.Controllers.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.PivotGrid.Win;
using DevExpress.XtraPivotGrid;
using D2NXAF.PivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CTMS.Module.BusinessObjects.Cash;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Actions;
using CTMS.Module.ParamObjects.Cash;
using CTMS.Module.HelperClasses.UI;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Data.Filtering;
using D2NXAF.ExpressApp.PivotGridLayout.Controllers;
using CTMS.Module.ControllerHelpers.Cash;

namespace CTMS.Module.Win.Controllers.Cash
{
    public class CashFlowPivotGridLayoutControllerWin : PivotGridLayoutControllerWin
    {
        private readonly CashFlowPivotGridSetup _PivotSetup;

        public CashFlowPivotGridLayoutControllerWin()
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

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            PivotGridFieldsMapped -= CashFlow_PivotGridFieldsMapped;
        }

        void CashFlow_PivotGridFieldsMapped(object sender, D2NXAF.ExpressApp.PivotGridLayout.PivotGridLayoutEventArgs e)
        {
            PivotGridControl.OptionsCustomization.CustomizationFormStyle
                    = DevExpress.XtraPivotGrid.Customization.CustomizationFormStyle.Excel2007;
            PivotGridControl.OptionsDataField.ColumnValueLineCount = 2;
            PivotGridControl.Appearance.Cell.Options.UseTextOptions = true;
            PivotGridControl.Appearance.Cell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            PivotGridControl.Appearance.FieldValue.Options.UseTextOptions = true;
            PivotGridControl.Appearance.FieldValue.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            PivotGridControl.Appearance.FieldHeader.Options.UseTextOptions = true;
            PivotGridControl.Appearance.FieldHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            PivotGridControl.Appearance.DataHeaderArea.Options.UseTextOptions = true;
            PivotGridControl.Appearance.DataHeaderArea.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            PivotGridControl.Appearance.RowHeaderArea.Options.UseTextOptions = true;
            PivotGridControl.Appearance.RowHeaderArea.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

            foreach (PivotGridField field in PivotGridControl.Fields)
            {
                field.Appearance.Header.Options.UseTextOptions = true;
                field.Appearance.Header.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            }

            switch (e.Layout.Name)
            {
                case Constants.CashFlowPivotLayoutMonthlyVariance:

                    break;
            }
        }

    }
}
