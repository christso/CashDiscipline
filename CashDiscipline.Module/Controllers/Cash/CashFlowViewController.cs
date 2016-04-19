using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ControllerHelpers.Cash;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowViewController : ViewController
    {

        public SingleChoiceAction RunProgramAction;

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
            var cashFlow = e.Object as CashFlow;
            if (cashFlow != null && !cashFlow.IsDeleted)
            {
                cashFlow.IsFixeeSynced = false;
                cashFlow.IsFixerSynced = false;
            }
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
                    ShowFixForecastForm(e.ShowViewParameters);
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

        private void ShowFixForecastForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = CashFlowFixParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        private void SaveForecast()
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            CashFlow.SaveForecast(objSpace);
        }

        private void ExecuteMapping()
        {
            var mapper = new CashFlowFixMapper((XPObjectSpace)ObjectSpace);
            var cashFlows = View.SelectedObjects;
            mapper.Process(cashFlows);
        }

    }
}
