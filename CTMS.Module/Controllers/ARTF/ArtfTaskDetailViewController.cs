using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using CTMS.Module.BusinessObjects.Artf;
using DevExpress.ExpressApp.Model;

namespace CTMS.Module.Controllers.Artf
{
    public partial class ArtfTaskDetailViewController : ViewController
    {
        public ArtfTaskDetailViewController()
        {
            InitializeComponent();
            RegisterActions(components);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            Session session = ((XPObjectSpace)ObjectSpace).Session;
            var taskObject = (ArtfTask)View.CurrentObject;
            var reconObject = (ArtfRecon)taskObject.ArtfRecon;
            if (reconObject != null)
            {
                string fromCustTypeName = "Unknown";
                if (reconObject.BankStmt != null)
                    if (reconObject.BankStmt.Account != null)
                        if (reconObject.BankStmt.Account.ArtfCustomerType != null)
                            fromCustTypeName = reconObject.BankStmt.Account.ArtfCustomerType.Name;

                string toCustTypeName = "Unknown";
                if (reconObject.CustomerType != null)
                    toCustTypeName = reconObject.CustomerType.Name;

                View.Caption = ((IModelDetailView)View.Model).Caption + " - "
                    + fromCustTypeName
                    + " to " + toCustTypeName;
            }
        }
    }
}
