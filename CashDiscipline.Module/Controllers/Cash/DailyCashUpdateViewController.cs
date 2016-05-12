using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class DailyCashUpdateViewController : ViewController
    {
        public DailyCashUpdateViewController()
        {
            TargetObjectType = typeof(DailyCashUpdateParam);
            TargetViewType = ViewType.DetailView;

            var runAction = new SimpleAction(this, "DailyCashUpdateRunAction", PredefinedCategory.ObjectsCreation);
            runAction.Caption = "Run";
            runAction.Execute += RunAction_Execute;

        }

        private void RunAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = (DailyCashUpdateParam)View.CurrentObject;
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();

            var uploader = new CashDiscipline.Module.Logic.Cash.BankStmtToCashFlowAlgorithm(objSpace, paramObj);
            uploader.Process();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
