using CashDiscipline.Module.BusinessObjects;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using Xafology.ExpressApp.Xpo;
using System.Diagnostics;
using CashDiscipline.Module.ParamObjects.Forex;

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ForexSettleLinkViewController : ViewController
    {
        public ForexSettleLinkViewController()
        {
            TargetObjectType = typeof(ForexSettleLink);
            runProgramAction = new SingleChoiceAction(this, "ForexSettleLinkAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            runProgramAction.Caption = "Actions";
            runProgramAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            runProgramAction.Execute += runProgramAction_Execute;

            var linkFifoAction = new ChoiceActionItem();
            linkFifoAction.Caption = "Link with FIFO";
            runProgramAction.Items.Add(linkFifoAction);

        }

        private SingleChoiceAction runProgramAction;

        void runProgramAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Link with FIFO":
                    ShowFifoForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ShowFifoForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ForexSettleFifoParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

    }
}
