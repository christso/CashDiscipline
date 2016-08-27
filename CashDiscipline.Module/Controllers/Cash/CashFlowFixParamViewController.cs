using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowFixParamViewController : ViewController
    {
        public CashFlowFixParamViewController()
        {
            TargetObjectType = typeof(CashFlowFixParam);
            //TargetViewType = ViewType.DetailView;

            var runAction = new SimpleAction(this, "CashFlowFixRunAction", PredefinedCategory.ObjectsCreation);
            runAction.Caption = "Run";
            runAction.Execute += RunAction_Execute;

            var resetAction = new SimpleAction(this, "CashFlowFixResetAction", PredefinedCategory.ObjectsCreation);
            resetAction.Caption = "Reset";
            resetAction.Execute += ResetAction_Execute;

            var mapAction = new SimpleAction(this, "CashFlowMapAction", PredefinedCategory.ObjectsCreation);
            mapAction.Caption = "Map";
            mapAction.Execute += MapAction_Execute;
        }

        private void MapAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Map();
            }
        }

        private void ResetAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Reset();
            }
        }

        private void RunAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.ProcessCashFlows();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            var detailView = View as DetailView;
            if (detailView != null)
                detailView.ViewEditMode = ViewEditMode.Edit;
        }
    }
}
