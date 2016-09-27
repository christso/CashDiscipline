using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ForexRateViewController : ViewController
    {
        private const string wbcImportCaption = "Import WBC";
        private const string dedupeCaption = "De-duplicate";
        public ForexRateViewController()
        {
            TargetObjectType = typeof(ForexRate);

            var frAction = new SingleChoiceAction(this, "ForexRateActions", PredefinedCategory.ObjectsCreation);
            frAction.Caption = "Actions";
            frAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            frAction.ShowItemsOnClick = true;
            frAction.Execute += ImportAction_Execute;

            var wbcChoice = new ChoiceActionItem();
            wbcChoice.Caption = wbcImportCaption;
            frAction.Items.Add(wbcChoice);

            var dedupeChoice = new ChoiceActionItem();
            dedupeChoice.Caption = dedupeCaption;
            frAction.Items.Add(dedupeChoice);
        }

        private void ImportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {

            var es = (SingleChoiceActionExecuteEventArgs)e;
            switch (es.SelectedChoiceActionItem.Caption)
            {
                case wbcImportCaption:
                    ShowImportRatesForm(e.ShowViewParameters);
                    break;
                case dedupeCaption:
                    DedupeForexRates();
                    break;
            }
        }

        private void ShowImportRatesForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportForexRatesParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        private void DedupeForexRates()
        {
            var os = Application.CreateObjectSpace();
            var deduper = new ForexRateDeduplicator((XPObjectSpace)os);
            deduper.Process();
        }

    }
}
