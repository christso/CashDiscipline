using CTMS.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D2NXAF.ExpressApp;
using D2NXAF.ExpressApp.SystemModule;
using System.Threading;

namespace CTMS.Module.Controllers
{
    public class ViewControllerEx : ViewController
    {
        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected CancellationTokenSource CancellationTokenSource;
        private ActionRequest _RequestObj;
        protected RequestStatus? CustomRequestExitStatus;

        protected virtual void SubmitRequest(string requestName, Action job)
        {
            var ts = new CancellationTokenSource();
            CancellationTokenSource = ts;
            
            var currentUser = SecuritySystem.CurrentUser as SecuritySystemUser;
            var objSpace = Application.CreateObjectSpace();
            var requestObj = objSpace.CreateObject<ActionRequest>();
            requestObj.Requestor = objSpace.GetObjectByKey<SecuritySystemUser>(currentUser.Oid);
            requestObj.RequestName = requestName;
            requestObj.RequestStatus = RequestStatus.Processing;
            requestObj.Save();
            requestObj.Session.CommitTransaction();

            requestObj.SetCancellationTokenSource(ts);
            requestObj.Save();
            requestObj.Session.CommitTransaction();
            new GenericMessageBox("Request ID: " + requestObj.RequestId, "Concurrent Request");

            Task t = Task.Factory.StartNew(() =>
            {
                SubmitRequest(job, requestObj);
            }, ts.Token);
        }

        

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }

        protected void WriteLogLine(string text, bool commit = true)
        {
            if (_RequestObj == null) return;
            if (!string.IsNullOrEmpty(_RequestObj.RequestLog))
                _RequestObj.RequestLog += "\r\n";
            _RequestObj.RequestLog += text;
            if (commit)
            {
                _RequestObj.Save();
                _RequestObj.Session.CommitTransaction();
            }
        }

        protected void SubmitRequest(Action job, ActionRequest requestObj)
        {
            CustomRequestExitStatus = null;
            _RequestObj = requestObj;

            try
            {
                job();
                if (CustomRequestExitStatus == null)
                    requestObj.RequestStatus = RequestStatus.Complete;
                else
                    requestObj.RequestStatus = (RequestStatus)CustomRequestExitStatus;
            }
            catch (Exception ex)
            {
                requestObj.RequestStatus = RequestStatus.Error;
                WriteLogLine("Unhandled Error: " + ex.Message + "\r\n" + ex.StackTrace);
            }
            requestObj.Save();
            requestObj.Session.CommitTransaction();
        }

        protected void ShowSingletonPopupDialogDetailView<T>(XafApplication app)
        {
            var objSpace = Application.CreateObjectSpace();
            var svp = new ShowViewParameters();
            svp.CreatedView = app.CreateDetailView(objSpace,
                StaticHelpers.GetInstance<T>(objSpace));
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.PopupWindow;
            svp.CreateAllControllers = true;
            var dc = app.CreateController<DialogController>();
            svp.Controllers.Add(dc);
            app.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }

        public static void ShowNonPersistentPopupDialogDetailView(XafApplication app, Type objType)
        {
            var svp = new ShowViewParameters();
            svp.CreatedView = app.CreateDetailView(ObjectSpaceInMemory.CreateNew(),
                Activator.CreateInstance(objType));
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.PopupWindow;
            svp.CreateAllControllers = true;
            var dc = app.CreateController<DialogController>();
            svp.Controllers.Add(dc);
            app.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }

        public static void ShowPopupDialogDetailView(XafApplication app, object classInstance)
        {
            var svp = new ShowViewParameters();
            svp.CreatedView = app.CreateDetailView(ObjectSpaceInMemory.CreateNew(),
                classInstance);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.PopupWindow;
            svp.CreateAllControllers = true;
            var dc = app.CreateController<DialogController>();
            svp.Controllers.Add(dc);
            app.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }
    }
}
