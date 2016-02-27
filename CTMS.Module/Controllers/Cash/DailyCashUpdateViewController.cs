using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.Controllers.Cash
{
    public class DailyCashUpdateViewController : ViewController
    {
        public DailyCashUpdateViewController()
        {
            TargetObjectType = typeof(DailyCashUpdateParam);
            TargetViewType = ViewType.DetailView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
            var dc = Frame.GetController<DialogController>();
            if (dc != null)
                dc.AcceptAction.Execute += AcceptAction_Execute;
        }



        private void AcceptAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            var paramObj = (DailyCashUpdateParam)View.CurrentObject;
            var objSpace = (XPObjectSpace) Application.CreateObjectSpace();

            var deleter = new CashFlowDeleter(objSpace.Session, paramObj.TranDate, paramObj.TranDate);
            var uploader = new BankStmtToCashFlow(objSpace, paramObj.TranDate, paramObj.TranDate, deleter);
            uploader.Process();
            //BankStmt.UploadToCashFlow(objSpace, paramObj.TranDate);
        }
    }
}
