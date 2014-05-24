using System;
using System.Collections.Generic;
using System.Linq;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Actions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using CTMS.Module.ParamObjects.Cash;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp;

namespace CTMS.Module.Controllers.Cash
{
    public class CashFlowViewController : ViewController
    {
        public CashFlowViewController()
        {
            TargetObjectType = typeof(CashFlow);
         
            RunProgramAction = new SingleChoiceAction(this, "RunCashFlowProgramAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            RunProgramAction.Caption = "Run Program";
            RunProgramAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            RunProgramAction.Execute += runProgramAction_Execute;
            RunProgramAction.ShowItemsOnClick = true;

            var dailyUpdateAction = new ChoiceActionItem();
            dailyUpdateAction.Caption = "Daily Update";
            RunProgramAction.Items.Add(dailyUpdateAction);

            var mapAction = new ChoiceActionItem();
            mapAction.Caption = "Map";
            RunProgramAction.Items.Add(mapAction);

            var fixForecastAction = new ChoiceActionItem();
            fixForecastAction.Caption = "Fix Forecast";
            RunProgramAction.Items.Add(fixForecastAction);

            var reloadForexTradesAction = new ChoiceActionItem();
            reloadForexTradesAction.Caption = "Reload Forex Forecast";
            RunProgramAction.Items.Add(reloadForexTradesAction);

            var saveForecastAction = new ChoiceActionItem();
            saveForecastAction.Caption = "Save Forecast";
            RunProgramAction.Items.Add(saveForecastAction);
        }

        void runProgramAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Daily Update":
                    var dialog1 = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
                    dialog1.ShowNonPersistentView(typeof(DailyCashUpdateParam));
                    break;
                case "Fix Forecast":
                    var dialog2 = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
                    dialog2.ShowSingletonView<CashFlowFixParam>((IObjectSpace)Application.CreateObjectSpace());
                    break;
                case "Map":
                    ExecuteMapping();
                    break;
                case "Reload Forex Forecast":
                    ForexTrade.UploadToCashFlowForecast(((XPObjectSpace)ObjectSpace).Session);
                    ObjectSpace.CommitChanges();
                    break;
                case "Save Forecast":
                    SaveForecast();
                    break;
            }
        }

        public SingleChoiceAction RunProgramAction;

        private void SaveForecast()
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            CashFlow.SaveForecast(objSpace);
        }

        private void ExecuteMapping()
        {
            var objSpace = (XPObjectSpace)ObjectSpace;
            var mappings = objSpace.GetObjects<CashFlowFixMapping>();

            foreach (CashFlow cf in View.SelectedObjects)
            {
                foreach (var mapping in mappings)
                {
                    if (cf.Fit(mapping.CriteriaExpression))
                    {
                        if (mapping.FixActivity != null)
                            cf.FixActivity = mapping.FixActivity;
                        if (mapping.Fix != null)
                            cf.Fix = mapping.Fix;
                        if (mapping.FixFromDateExpr != null)
                            cf.FixFromDate = (DateTime)cf.Evaluate(CriteriaOperator.Parse(mapping.FixFromDateExpr));
                        if (mapping.FixToDateExpr != null)
                            cf.FixToDate = (DateTime)cf.Evaluate(CriteriaOperator.Parse(mapping.FixToDateExpr));
                        break;
                    }
                }
                cf.Save();
            }
            objSpace.CommitChanges();
        }
        
    }
}
