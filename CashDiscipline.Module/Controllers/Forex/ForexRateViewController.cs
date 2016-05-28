using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
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
        public ForexRateViewController()
        {
            TargetObjectType = typeof(ForexRate);

            var importAction = new SingleChoiceAction(this, "ImportForexRateAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            importAction.Execute += ImportAction_Execute;
            var wbcChoice = new ChoiceActionItem();
            wbcChoice.Caption = "WBC";
            importAction.Items.Add(wbcChoice);
        }

        private void ImportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {

            var es = (SingleChoiceActionExecuteEventArgs)e;
            switch (es.SelectedChoiceActionItem.Caption)
            {
                case "WBC":
                    ShowImportRatesForm(e.ShowViewParameters);
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

    }
}
