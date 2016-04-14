using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;

namespace CashDiscipline.Module.Controllers.Cash
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

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.ObjectSaving += ObjectSpace_ObjectSaving;
        }

        private void ObjectSpace_ObjectSaving(object sender, ObjectManipulatingEventArgs e)
        {
            //if (IsFixerUpdated)
            //    IsFixerUpdated = false;
            //if (IsFixeeUpdated)
            //    IsFixeeUpdated = false;
        }

        void runProgramAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Daily Update":
                    var dialog1 = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
                    dialog1.ShowNonPersistentView(typeof(DailyCashUpdateParam));
                    break;
                case "Fix Forecast":
                    {
                        var os = Application.CreateObjectSpace();
                        var paramObj = CashFlowFixParam.GetInstance(os);
                        var detailView = Application.CreateDetailView(os, paramObj);
                        var svp = e.ShowViewParameters;
                        svp.TargetWindow = TargetWindow.NewModalWindow;
                        svp.CreatedView = detailView;
                    }
                    break;
                case "Map":
                    ExecuteMapping();
                    break;
                case "Reload Forex Forecast":
                    var uploader = new ForexTradeBatchUploaderImpl(ObjectSpace);
                    uploader.UploadToCashFlowForecast();
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
