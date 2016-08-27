using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using DG2NTT.AnalysisServicesHelpers;
using Xafology.ExpressApp.SystemModule;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowViewController : ViewController
    {
        public const string processCubeHistCaption = "Process Historical";
        public const string processCubeAllCaption = "Process All";
        public const string processCubeCurrentCaption = "Process Current";
        public const string processCubeCaption = "Process Report";
        public const string processCubeSshotCaption = "Process Snapshots";
        public const string mapSelectedCaption = "Map Selected";
        public const string fixForecastFormCaption = "Fix Forecast";
        public const string ActionId = "RunCashFlowProgramAction";
        public const string saveForecastCaption = "Save Forecast";

        public SingleChoiceAction RunProgramAction;

        public CashFlowViewController()
        {
            TargetObjectType = typeof(CashFlow);

            RunProgramAction = new SingleChoiceAction(this, ActionId, DevExpress.Persistent.Base.PredefinedCategory.Edit);
            RunProgramAction.Caption = "Actions";
            RunProgramAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            RunProgramAction.Execute += runProgramAction_Execute;
            RunProgramAction.ShowItemsOnClick = true;
            //RunProgramAction.ExecuteCompleted += RunProgramAction_ExecuteCompleted;
            
            var dailyUpdateAction = new ChoiceActionItem();
            dailyUpdateAction.Caption = "Daily Update";
            RunProgramAction.Items.Add(dailyUpdateAction);

            var mapAction = new ChoiceActionItem();
            mapAction.Caption = mapSelectedCaption;
            RunProgramAction.Items.Add(mapAction);

            var fixForecastAction = new ChoiceActionItem();
            fixForecastAction.Caption = "Fix Forecast";
            RunProgramAction.Items.Add(fixForecastAction);

            var reloadForexTradesAction = new ChoiceActionItem();
            reloadForexTradesAction.Caption = "Reload Forex Forecast";
            RunProgramAction.Items.Add(reloadForexTradesAction);

            var saveForecastAction = new ChoiceActionItem();
            saveForecastAction.Caption = saveForecastCaption;
            RunProgramAction.Items.Add(saveForecastAction);

            var processCubeAction = new ChoiceActionItem();
            processCubeAction.Caption = processCubeCaption;
            RunProgramAction.Items.Add(processCubeAction);

            var processCubeCurrentAction = new ChoiceActionItem();
            processCubeCurrentAction.Caption = processCubeCurrentCaption;
            processCubeAction.Items.Add(processCubeCurrentAction);

            var processCubeAllAction = new ChoiceActionItem();
            processCubeAllAction.Caption = processCubeAllCaption;
            processCubeAction.Items.Add(processCubeAllAction);

            var processCubeHistAction = new ChoiceActionItem();
            processCubeHistAction.Caption = processCubeHistCaption;
            processCubeAction.Items.Add(processCubeHistAction);

            var processCubeSshotAction = new ChoiceActionItem();
            processCubeSshotAction.Caption = processCubeSshotCaption;
            processCubeAction.Items.Add(processCubeSshotAction);
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
            var choice = e.SelectedChoiceActionItem;
            var captionPath = choice.GetCaptionPath();

            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Daily Update":
                    ShowDailyCashUpdateForm(e.ShowViewParameters);
                    break;
                case fixForecastFormCaption:
                    ShowFixForecastForm(e.ShowViewParameters);
                    break;
                case mapSelectedCaption:
                    MapSelected();
                    break;
                case "Reload Forex Forecast":
                    var uploader = new CashDiscipline.Module.Logic.Forex.ForexToCashFlowUploader((XPObjectSpace)ObjectSpace);
                    uploader.Process();
                    ObjectSpace.CommitChanges();
                    break;
                case "Save Forecast":
                    SaveForecast();
                    break;
                default:
                    if (e.SelectedChoiceActionItem.ParentItem.Caption == processCubeCaption)
                    {
                        ProcessCube(e.SelectedChoiceActionItem.Caption);
                    }
                    break;
            }
        }

        public bool ProcessCube(string caption)
        {
            var tabular = new CashFlowTabular();

            if (ObjectSpace.IsModified)
            {
                new GenericMessageBox(
                    "Please save your changes before processing the Cash Report", "Cash Report Processing Cancelled",
                    (app, svp) => { return; });
                return false;
            }
            switch (caption)
            {
                case processCubeAllCaption:
                    tabular.ProcessAll();
                    new GenericMessageBox(
                         "ACTION COMPLETED : Process Cash Report - All",
                         "ACTION COMPLETED");
                    break;
                case processCubeCurrentCaption:
                    var os = Application.CreateObjectSpace();
                    tabular.ProcessCurrent((XPObjectSpace)os);
                    new GenericMessageBox(
                        "ACTION COMPLETED : Process Cash Report - Current\r\n"
                        + "NOTE : " + tabular.LastReturnMessage,
                        "ACTION COMPLETED");
                    break;
                case processCubeHistCaption:
                    tabular.ProcessHist();
                    new GenericMessageBox(
                        "ACTION COMPLETED : Process Cash Report - Historical",
                        "ACTION COMPLETED");
                    break;
                case processCubeSshotCaption:
                    tabular.ProcessSshot();
                    new GenericMessageBox(
                        "ACTION COMPLETED : Process Cash Report - Snapshots",
                        "ACTION COMPLETED");
                    break;
            }
            return true;
        }
        
        private void ShowFixForecastForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = CashFlowFixParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        private void ShowDailyCashUpdateForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = DailyCashUpdateParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        private void SaveForecast()
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var logic = new SaveForecastSnapshot(objSpace);
            logic.Process();
            new GenericMessageBox(
                 "ACTION COMPLETED : Save Forecast",
                 "ACTION COMPLETED");
        }

        private void MapSelected()
        {
            var mapper = new CashFlowFixMapper((XPObjectSpace)ObjectSpace);
            var cashFlows = View.SelectedObjects;
            mapper.Process(cashFlows);
        }

    }
}
