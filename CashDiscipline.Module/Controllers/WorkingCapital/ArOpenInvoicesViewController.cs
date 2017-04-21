using CashDiscipline.Module.BusinessObjects.AccountsReceivable;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ArOpenInvoicesViewController : ViewController
    {
        private const string ImportCaption = "Import";

        public ArOpenInvoicesViewController()
        {
            TargetObjectType = typeof(ArOpenInvoices);

            var mainAction = new SingleChoiceAction(this, "ArOpenInvoicesAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            mainAction.Caption = "Actions";
            mainAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            mainAction.Execute += mainAction_Execute;
            mainAction.ShowItemsOnClick = true;

            var importChoice = new ChoiceActionItem();
            importChoice.Caption = ImportCaption;
            mainAction.Items.Add(importChoice);
        }

        private void mainAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case ImportCaption:
                    ShowImportForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ShowImportForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportArOpenInvoicesParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

    }
}
