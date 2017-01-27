using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.Clients;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.SystemModule;

namespace CashDiscipline.Module.Controllers.FinAccounting
{
    public class TrialBalanceViewController : ViewController
    {
        public TrialBalanceViewController()
        {
            TargetObjectType = typeof(TB_TrialBalance);

            var actions = new SingleChoiceAction(this, "TrialBalanceActions", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            actions.Caption = "Actions";
            actions.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            actions.Execute += actions_Execute;
            actions.ShowItemsOnClick = true;

            var importAction = new ChoiceActionItem();
            actions.Items.Add(importAction);
            importAction.Caption = "Import";

            var reportAction = new ChoiceActionItem();
            reportAction.Caption = "Process Report";
            actions.Items.Add(reportAction);
        }

        private void actions_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            string path = e.SelectedChoiceActionItem.GetCaptionPath();
            switch (path)
            {
                case "Process Report":
                    ProcessReport();
                    break;
                case "Import":
                    ShowImportForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ProcessReport()
        {
            var adomdClient = CshDscTabularHelper.CreateAdomdClient();
            adomdClient.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""TrialBalance""
      }
    ]
  }
}");
            new GenericMessageBox(
                 "ACTION COMPLETED : Process Report\r\n",
                 "ACTION COMPLETED");
        }

        private void ShowImportForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportTrialBalanceParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.CreatedView = detailView;
        }

    }
}
