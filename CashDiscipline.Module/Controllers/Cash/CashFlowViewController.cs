﻿using CashDiscipline.Module.BusinessObjects.Cash;
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

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowViewController : ViewController
    {
        private const string processCubeCaption = "Process Cube";
        private const string processCubeHistCaption = "Process Historical";
        private const string processCubeAllCaption = "Process All";
        private const string processCubeRecentCaption = "Process Recent";
        private const string processCubeSshotCaption = "Process Snapshots";

        public SingleChoiceAction RunProgramAction;

        public CashFlowViewController()
        {
            TargetObjectType = typeof(CashFlow);

            RunProgramAction = new SingleChoiceAction(this, "RunCashFlowProgramAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            RunProgramAction.Caption = "Run Program";
            RunProgramAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            RunProgramAction.Execute += runProgramAction_Execute;
            RunProgramAction.ShowItemsOnClick = true;
            RunProgramAction.ExecuteCompleted += RunProgramAction_ExecuteCompleted;

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

            var processCubeAction = new ChoiceActionItem();
            processCubeAction.Caption = processCubeCaption;
            RunProgramAction.Items.Add(processCubeAction);

            var processCubeAllAction = new ChoiceActionItem();
            processCubeAllAction.Caption = processCubeAllCaption;
            processCubeAction.Items.Add(processCubeAllAction);

            var processCubeHistAction = new ChoiceActionItem();
            processCubeHistAction.Caption = processCubeHistCaption;
            processCubeAction.Items.Add(processCubeHistAction);

            var processCubeRecentAction = new ChoiceActionItem();
            processCubeRecentAction.Caption = processCubeRecentCaption;
            processCubeAction.Items.Add(processCubeRecentAction);

            var processCubeSshotAction = new ChoiceActionItem();
            processCubeSshotAction.Caption = processCubeSshotCaption;
            processCubeAction.Items.Add(processCubeSshotAction);
        }

        private void RunProgramAction_ExecuteCompleted(object sender, ActionBaseEventArgs e)
        {
            var es = (SingleChoiceActionExecuteEventArgs)e;
            switch (es.SelectedChoiceActionItem.Caption)
            {
                case processCubeCaption:
                case "Save Forecast":
                    new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                        Application,
                        es.SelectedChoiceActionItem.Caption + " completed.", e.Action.Caption);
                    break;
            }

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
                case processCubeCaption:
                    break;
                case processCubeAllCaption:
                    ProcessCube_All();
                    break;
                case processCubeRecentCaption:
                    ProcessCube_Recent();
                    break;
                case processCubeHistCaption:
                    ProcessCube_Hist();
                    break;
                case processCubeSshotCaption:
                    ProcessCube_Sshot();
                    break;
            }
        }

        #region Process Cube

        public ServerProcessor CreateSsasClient()
        {
            return new ServerProcessor("FINSERV01", "CashFlow");
        }


        public void ProcessCube_All()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessDatabase();
        }

        public void ProcessCube_Recent()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessPartition("Model", "CashFlow", "CashFlow_Current_Recent");
        }

        public void ProcessCube_Hist()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessPartition("Model", "CashFlow", "CashFlow_Current_Hist");
        }

        public void ProcessCube_Sshot()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessPartition("Model", "CashFlow", "CashFlow_Snapshot");
        }
        #endregion

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
