using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.ParamObjects.Forex;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ForexSettleFifoParamViewController : ViewController
    {
        public ForexSettleFifoParamViewController()
        {
            TargetObjectType = typeof(ForexSettleFifoParam);

            var runAction = new SimpleAction(this, "ForexSettleFifoRunAction", PredefinedCategory.ObjectsCreation);
            runAction.Caption = "Run";
            runAction.Execute += RunAction_Execute;

        }

        private void RunAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as ForexSettleFifoParam;
            if (paramObj != null)
            {
                var algo = new ForexSettleFifoAlgorithm(os, paramObj);
                algo.Process();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
            var batchDeleteController = Frame.GetController<Xafology.ExpressApp.BatchDelete.BatchDeleteListViewController>();
            batchDeleteController.DeleteFilteredChoiceEnabled = false;
        }
    }
}
