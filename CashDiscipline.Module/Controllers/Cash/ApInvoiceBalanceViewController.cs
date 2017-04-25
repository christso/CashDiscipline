using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ApInvoiceBalanceViewController : ViewController
    {
        private const string importCaption = "Import";
        private const string mapSelectedCaption = "Map Selected";
        private const string mapFilteredCaption = "Map Filtered";

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

            var mapSelectedChoice = new ChoiceActionItem();
            mapSelectedChoice.Caption = mapSelectedCaption;
            mainAction.Items.Add(mapSelectedChoice);

            var mapFilteredChoice = new ChoiceActionItem();
            mapFilteredChoice.Caption = mapFilteredCaption;
            mainAction.Items.Add(mapFilteredChoice);

        }

        private void MainAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case importCaption:
                    ShowImportForm(e.ShowViewParameters);
                    break;
                case mapSelectedCaption:
                    MapSelected();
                    break;
                case mapFilteredCaption:
                    MapFiltered();
                    break;
            }
        }

        private void MapSelected()
        {
            var mapper = new ApInvoiceBalanceMapper((XPObjectSpace)ObjectSpace);
            var objs = View.SelectedObjects;
            mapper.Process(objs);
        }

        private void MapFiltered()
        {
            var mapper = new ApInvoiceBalanceMapper((XPObjectSpace)ObjectSpace);

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
