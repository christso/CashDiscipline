using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;

using CTMS.Module.ParamObjects.Cash;

namespace CTMS.Module.Controllers.Cash
{
    /// <summary>
    /// Applies a dynamic filter on the Cash Flow View when the view is activated
    /// or when the user changes the parameters (via the popup window action).
    /// The behaviour of the filter depends on the type of Cash Flow View that is activated.
    /// </summary>
    public class CashReportViewController : ViewController
    {
        public CashReportViewController()
        {
            TargetViewType = ViewType.ListView;
            //TargetViewId = "CashFlow_PivotGridView";
            paramAction = new SimpleAction(this, "CashReportParamAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            paramAction.Caption = "Parameters";
            paramAction.Execute += paramAction_Execute;
        }

        public CashReportParam ReportParam;

        private SimpleAction paramAction;

        protected override void OnActivated()
        {
            base.OnActivated();
            _ReportObjSpace = (XPObjectSpace)Application.CreateObjectSpace();
            ReportParam = CashReportParam.GetInstance(_ReportObjSpace);
            if (ReportParam.SetDefaultParams())
                _ReportObjSpace.CommitChanges();
            SetupView();
        }

        private XPObjectSpace _ReportObjSpace;

        void paramAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            _ReportObjSpace = (XPObjectSpace)Application.CreateObjectSpace();
            ReportParam = CashReportParam.GetInstance(_ReportObjSpace);
            if (ReportParam.SetDefaultParams())
                _ReportObjSpace.CommitChanges();
            var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
            dialog.Accepting += dialog_Accepting;
            dialog.ShowView(_ReportObjSpace, ReportParam);
        }

        void dialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            SetupView();
        }

        protected virtual void SetupView()
        {
        }
    }
}
