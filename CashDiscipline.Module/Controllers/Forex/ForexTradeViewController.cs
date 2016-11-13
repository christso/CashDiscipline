using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Logic.Forex;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ForexTradeViewController : ViewController
    {
        private const string pdyCaption = "Predeliver";
        public ForexTradeViewController()
        {
            TargetObjectType = typeof(ForexTrade);
            var ftAction = new SingleChoiceAction(this, "ForexTradeAction", PredefinedCategory.ObjectsCreation);
            ftAction.Caption = "Actions";
            ftAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            ftAction.ShowItemsOnClick = true;
            ftAction.Execute += ftAction_Execute;
            var myActionItem = new ChoiceActionItem();
            myActionItem.Caption = pdyCaption;
            ftAction.Items.Add(myActionItem);
        }

        private int maxSettleGroupId = 0;

        private void ftAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case pdyCaption:
                    ShowAmendForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ShowAmendForm(ShowViewParameters svp)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            ForexTrade origOrigFt = View.CurrentObject as ForexTrade;
            ForexTrade origFt = os.GetObjectByKey<ForexTrade>(origOrigFt.Oid);

            ForexTrade newFt = ForexTradeLogic.Initialize(origFt);
            var detailView = Application.CreateDetailView(os, newFt);

            var controller = new ForexTradeDetailViewController();
            svp.Controllers.Add(controller);
            svp.TargetWindow = TargetWindow.NewWindow;
            svp.CreatedView = detailView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            this.View.ObjectSpace.Committed += ObjectSpace_Committed;
            var newController = Frame.GetController<NewObjectViewController>();
            newController.ObjectCreated += NewController_ObjectCreated;
        }

        private void NewController_ObjectCreated(object sender, ObjectCreatedEventArgs e)
        {
            var obj = (ForexTrade)e.CreatedObject;
            maxSettleGroupId++;
            obj.SettleGroupId = maxSettleGroupId;
        }

        private void ObjectSpace_Committed(object sender, EventArgs e)
        {
            var uploader = new CashDiscipline.Module.Logic.Forex.ForexToCashFlowUploader((XPObjectSpace)ObjectSpace);
            uploader.Process();
        }

        protected override void OnDeactivated()
        {
            this.View.ObjectSpace.Committed -= ObjectSpace_Committed;
            base.OnDeactivated();
        }

    }
}
