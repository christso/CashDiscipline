using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DG2NTT.ExpressApp.SystemModule
{
    public sealed partial class DG2NTTSystemModule
    {
        public override void Setup(XafApplication application)
        {
            base.Setup(application);
            ApplicationHelper.Instance.Initialize(application);
        }
    }
    public class ApplicationHelper
    {
        private const string ValueManagerKey = "ApplicationHelper";
        private static volatile IValueManager<ApplicationHelper> instanceValueManager;
        private static object syncRoot = new object();
        private XafApplication _Application;
        private ApplicationHelper()
        {
        }
        public static ApplicationHelper Instance
        {
            get
            {
                if (instanceValueManager == null)
                {
                    lock (syncRoot)
                    {
                        if (instanceValueManager == null)
                        {
                            instanceValueManager = ValueManager.GetValueManager<ApplicationHelper>(ValueManagerKey);
                        }
                    }
                }
                if (instanceValueManager.Value == null)
                {
                    lock (syncRoot)
                    {
                        if (instanceValueManager.Value == null)
                        {
                            instanceValueManager.Value = new ApplicationHelper();
                        }
                    }
                }
                return instanceValueManager.Value;
            }
        }
        public XafApplication Application { get { return _Application; } }
        internal void Initialize(XafApplication app)
        {
            _Application = app;
        }
    }
}
