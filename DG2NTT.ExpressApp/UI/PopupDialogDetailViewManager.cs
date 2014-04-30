using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.HelperClasses.UI
{
    public class PopupDialogDetailViewManager
    {
        private XafApplication _app;
        private ShowViewParameters _svp;
        private DialogController _dc;

        public event EventHandler<DialogControllerAcceptingEventArgs> Accepting;

        public PopupDialogDetailViewManager(XafApplication app)
        {
            _app = app;
            _dc = app.CreateController<DialogController>();
        }

        public void ShowNonPersistentView(IObjectSpace objSpace, Type objType)
        {
            if (objType == null) 
                throw new NullReferenceException("Object Type must not be null if you want to call ShowNonPersistentView");
            objSpace = objSpace ?? ObjectSpaceInMemory.CreateNew();
            object obj = Activator.CreateInstance(objType);
            ShowView(objSpace, obj);
        }

        public void ShowNonPersistentView(Type objType)
        {
            ShowNonPersistentView(null, objType);
        }

        public void ShowView(IObjectSpace objSpace, object obj)
        {
            _svp = new ShowViewParameters();
            _svp.CreatedView = _app.CreateDetailView(objSpace, obj);
            _svp.TargetWindow = TargetWindow.NewModalWindow;
            _svp.Context = TemplateContext.PopupWindow;
            _svp.CreateAllControllers = true;

            if (Accepting != null)
                _dc.Accepting += DialogController_Accepting;

            _svp.Controllers.Add(_dc);
            _app.ShowViewStrategy.ShowView(_svp, new ShowViewSource(null, null));
        }

        void DialogController_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            Accepting(sender, e);
        }
    }
}
