using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Editors;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;

using CTMS.Module.BusinessObjects.Artf;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Model;
using CTMS.Module.Model;
using DevExpress.ExpressApp.Validation;
using DevExpress.Persistent.Validation;

namespace CTMS.Module.Controllers.Artf
{
    public partial class ArtfReconDetailViewController : ViewController
    {

        public ArtfReconDetailViewController()
        {
            InitializeComponent();
            RegisterActions(components);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Ledger
            PropertyEditor ledgerPropertyEditor = ((DetailView)View).FindItem("Ledger") as PropertyEditor;
            if (ledgerPropertyEditor != null)
                ledgerPropertyEditor.ControlValueChanged += LedgerPropertyEditor_ControlValueChanged;

            PropertyEditor custTypePropertyEditor = ((DetailView)View).FindItem("CustomerType") as PropertyEditor;
            if (custTypePropertyEditor != null)
                custTypePropertyEditor.ControlValueChanged += custTypePropertyEditor_ControlValueChanged;
            View.ControlsCreated += View_ControlsCreated;
        }

        void custTypePropertyEditor_ControlValueChanged(object sender, EventArgs e)
        {
            UpdateViewCaption();
        }

        private void UpdateViewCaption()
        {
            // get From Customer Type
            Session session = ((XPObjectSpace)ObjectSpace).Session;
            var reconObject = (ArtfRecon)View.CurrentObject;

            string fromCustTypeName = "N/A";
            if (reconObject.BankStmt != null)
                if (reconObject.BankStmt.Account != null)
                    if (reconObject.BankStmt.Account.ArtfCustomerType != null)
                        fromCustTypeName = reconObject.BankStmt.Account.ArtfCustomerType.Name;

            // get To Customer Type
            string toCustTypeName = "N/A";
            var custTypePropertyEditor = ((DetailView)View).FindItem("CustomerType") as PropertyEditor;
            if (custTypePropertyEditor != null)
            {
                var custType = (ArtfCustomerType)custTypePropertyEditor.ControlValue;
                if (custType == null)
                    custType = (ArtfCustomerType)custTypePropertyEditor.PropertyValue;
                if (custType != null)
                    toCustTypeName = custType.Name;
            }

            // change caption
            View.Caption = ((IModelDetailView)View.Model).Caption + " - "
                + fromCustTypeName
                + " to " + toCustTypeName;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            UpdateViewCaption();

  
            //Session session = ((XPObjectSpace)ObjectSpace).Session;
            //var reconObject = (ArtfRecon)View.CurrentObject;



            //string fromCustTypeName = "N/A";
            //if (reconObject.BankStmt != null)
            //    if (reconObject.BankStmt.Account != null)
            //        if (reconObject.BankStmt.Account.ArtfCustomerType != null)
            //            fromCustTypeName = reconObject.BankStmt.Account.ArtfCustomerType.Name;

            //string toCustTypeName = "N/A";
            //if (reconObject.CustomerType != null)
            //    toCustTypeName = reconObject.CustomerType.Name;

            //View.Caption = ((IModelDetailView)View.Model).Caption + " - "
            //    + fromCustTypeName
            //    + " to " + toCustTypeName;

            
        }

        // calculate default Amount based on selected Ledger
        void LedgerPropertyEditor_ControlValueChanged(object sender, EventArgs e)
        {
            var ledgerPropertyEditor = (PropertyEditor)sender;
            var oldLedger = (ArtfLedger)((PropertyEditor)sender).PropertyValue;
            var ledger = (ArtfLedger)((PropertyEditor)sender).ControlValue;
            var amountPropertyEditor = ((DetailView)View).FindItem("Amount") as PropertyEditor;
            if (oldLedger != ledger & (decimal)amountPropertyEditor.PropertyValue == 0)
            {
                // change Amount
                amountPropertyEditor.PropertyValue = ledger.Amount;
                // update Ledger with new value
                 ledgerPropertyEditor.PropertyValue = ledger;
            }
        }

    }
}
