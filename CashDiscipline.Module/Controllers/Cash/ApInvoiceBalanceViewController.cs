﻿using CashDiscipline.Module.BusinessObjects.AccountsPayable;
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
    public class ApInvoiceBalanceViewController : ViewController
    {
        private const string importCaption = "Import";
        public ApInvoiceBalanceViewController()
        {
            TargetObjectType = typeof(ApInvoiceBalance);

            var mainAction = new SingleChoiceAction(this, "ApInvoiceBalanceAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            mainAction.Caption = "Actions";
            mainAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            mainAction.ShowItemsOnClick = true;
            mainAction.Execute += MainAction_Execute;

            var importChoice = new ChoiceActionItem();
            importChoice.Caption = importCaption;
            mainAction.Items.Add(importChoice);
        }

        private void MainAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case importCaption:
                    ShowImportForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ShowImportForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportApInvoiceBalanceParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }
    }
}
