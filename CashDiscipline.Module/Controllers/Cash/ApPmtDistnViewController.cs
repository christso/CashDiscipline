using CashDiscipline.Common;
using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.BatchDelete;
using Xafology.ExpressApp.Xpo;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ApPmtDistnViewController : ViewController
    {
        private const string mapSelectedCaption = "Map Selected";
        private const string mapFilteredCaption = "Map Filtered";
        private const string processCaption = "Proces Report";
        private const string importCaption = "Import";

        public ApPmtDistnViewController()
        {
            TargetObjectType = typeof(ApPmtDistn);

            var mainAction = new SingleChoiceAction(this, "RunApPmtDistnAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            mainAction.Caption = "Actions";
            mainAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            mainAction.ShowItemsOnClick = true;
            mainAction.Execute += MainAction_Execute;

            var importChoice = new ChoiceActionItem();
            importChoice.Caption = importCaption;
            mainAction.Items.Add(importChoice);

            var mapSelectedChoice = new ChoiceActionItem();
            mapSelectedChoice.Caption = mapSelectedCaption;
            mainAction.Items.Add(mapSelectedChoice);

            var mapFilteredChoice = new ChoiceActionItem();
            mapFilteredChoice.Caption = mapFilteredCaption;
            mainAction.Items.Add(mapFilteredChoice);

            var processChoice = new ChoiceActionItem();
            processChoice.Caption = processCaption;
            mainAction.Items.Add(processChoice);
        }

        private void MainAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case mapSelectedCaption:
                    MapSelected();
                    break;
                case mapFilteredCaption:
                    MapFiltered();
                    break;
                case importCaption:
                    ShowImportForm(e.ShowViewParameters);
                    break;
                case processCaption:
                    ProcessReport();
                    break;
            }
        }

        private void ProcessReport()
        {
            string serverName = Constants.SsasServerName;
            var processor = new AdomdProcessor(serverName);
            processor.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""ApPayments""
      }
    ]
  }
}");

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Process Report Successful",
                "Process Report Successful"
            );
        }

        private void ShowImportForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportApPmtDistnParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        private void MapSelected()
        {
            var mapper = new ApPmtDistnMapper((XPObjectSpace)ObjectSpace);
            var objs = View.SelectedObjects;
            mapper.Process(objs);
        }

        private void MapFiltered()
        {
            var mapper = new ApPmtDistnMapper((XPObjectSpace)ObjectSpace);

            var batchController = Frame.GetController<BatchDeleteListViewController>();
            if (batchController != null)
            {
                var criteria = batchController.ActiveFilterCriteria;
                var filtered = batchController.ActiveFilterEnabled;

                if (Object.ReferenceEquals(null, criteria) || !filtered)
                {
                    var message = new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                        "Filter is empty. Do you wish to continue mapping the ENTIRE table?",
                        "Warning",
                        (sender, svp) => mapper.Process(criteria),
                        (sender, svp) => { return; });
                }
                else
                {
                    mapper.Process(criteria);
                }
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.CustomDeleteObjects += ObjectSpace_CustomDeleteObjects;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.CustomDeleteObjects -= ObjectSpace_CustomDeleteObjects;
            base.OnDeactivated();
        }
        private void ObjectSpace_CustomDeleteObjects(object sender, CustomDeleteObjectsEventArgs e)
        {
            var deleter = new BatchDeleter(ObjectSpace);
            deleter.Delete(e.Objects);
        }
    }
}
