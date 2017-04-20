using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CashDiscipline.Module.Controllers.Cash
{
    public class AccountBalanceViewController : ViewController
    {
        private const string calcBalCaption = "Calculate";

        public AccountBalanceViewController()
        {
            TargetObjectType = typeof(AccountBalance);

            var mainAction = new SingleChoiceAction(this, "AccountBalanceAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            mainAction.Caption = "Actions";
            mainAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            mainAction.ShowItemsOnClick = true;
            mainAction.Execute += MainAction_Execute;

            var calcBalChoice = new ChoiceActionItem();
            calcBalChoice.Caption = calcBalCaption;
            mainAction.Items.Add(calcBalChoice);
        }

        private void MainAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case calcBalCaption:
                    ShowImportForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ShowImportForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = AccountBalanceParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }
    }
}
