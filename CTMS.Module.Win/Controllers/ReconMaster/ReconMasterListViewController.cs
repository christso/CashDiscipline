using System;
using System.ComponentModel;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Artf;

namespace CTMS.Module.Controllers.ReconMaster
{
    public class ReconMasterListViewController : ViewController
    {
        private void ObjectSpace_Committed(Object sender, EventArgs e)
        {
            ObjectSpace.Refresh();
        }
        private void Application_DetailViewCreated(Object sender, DetailViewCreatedEventArgs e)
        {
            if (e.View.ObjectTypeInfo.Type == typeof(ArtfRecon))
            {
                e.View.ObjectSpace.Committed += ObjectSpace_Committed;
            }
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            Application.DetailViewCreated += Application_DetailViewCreated;
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Application.DetailViewCreated -= Application_DetailViewCreated;
        }
        public ReconMasterListViewController()
        {
            TypeOfView = typeof(ListView);
            TargetViewNesting = Nesting.Root;
            TargetObjectType = typeof(IReconMaster);
            
        }
    }
}
