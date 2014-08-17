using System;
using System.Linq;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Templates;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.SystemModule;
using CTMS.Module.BusinessObjects.Artf;
using DevExpress.ExpressApp.Model.NodeGenerators;
using System.Diagnostics;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.Controllers.Artf
{
    // For more information on Controllers and their life cycle, check out the http://documentation.devexpress.com/#Xaf/CustomDocument2621 and http://documentation.devexpress.com/#Xaf/CustomDocument3118 help articles.
    public partial class ArtfReceiptLookupListViewController : ViewController
    {

        // Use this to do something when a Controller is instantiated (do not execute heavy operations here!).
        public ArtfReceiptLookupListViewController()
        {
            TargetObjectType = typeof(ArtfReceipt);
            TargetViewType = ViewType.ListView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            newObjectViewController.ObjectCreated += ReceiptLookupController_ObjectCreated;
        }

        // This is fired when the New button is clicked on a LookupEditor of type Receipt
        void ReceiptLookupController_ObjectCreated(object sender, ObjectCreatedEventArgs e)
        {
            NestedFrame nestedFrame = (NestedFrame)Frame;
            var mainView = nestedFrame.ViewItem.View;

            var receipt = (ArtfReceipt)e.CreatedObject;

            if (mainView.Id == Constants.ArtfReconDetailViewId)
            {
                var amountPropertyEditor = mainView.FindItem("Amount") as PropertyEditor;
                receipt.Amount = (decimal)amountPropertyEditor.PropertyValue;

                if (View.Id == Constants.ArtfFromReceiptLookupListViewId)
                {
                    var bankStmtPropEditor = mainView.FindItem("BankStmt") as PropertyEditor;
                    var bankStmt = (BankStmt)bankStmtPropEditor.PropertyValue;
                    var custType = bankStmt.Account.ArtfCustomerType;

                    if (custType != null)
                    {
                        if (custType.System != null)
                        {
                            receipt.System = e.ObjectSpace.GetObjectByKey<ArtfSystem>(ObjectSpace.GetKeyValue(custType.System));
                        }
                    }
                    if (bankStmt != null)
                        receipt.RemittanceAccount = bankStmt.Account;
                }
                else if (View.Id == Constants.ArtfToReceiptLookupListViewId)
                {
                    var custTypePropEditor = mainView.FindItem("CustomerType") as PropertyEditor;
                    var custType = (ArtfCustomerType)custTypePropEditor.PropertyValue;

                    if (custType != null)
                    {
                        var account = ObjectSpace.FindObject<Account>(Account.Fields.ArtfCustomerType == custType
                            & Account.Fields.IsArtfDefault == new OperandValue(true));
                        if (account != null)
                        {
                            account = e.ObjectSpace.GetObjectByKey<Account>(ObjectSpace.GetKeyValue(account));
                            receipt.RemittanceAccount = account;
                        }
                    }
                    if (custType != null && custType.System != null)
                    {
                        receipt.System = e.ObjectSpace.GetObjectByKey<ArtfSystem>(ObjectSpace.GetKeyValue(custType.System));
                    }
                }
            }
        }
        // Override to do something when a Controller is deactivated.
        protected override void OnDeactivated()
        {
            // For instance, you can unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
            Frame.GetController<NewObjectViewController>().ObjectCreated -= ReceiptLookupController_ObjectCreated;
        }
    }
}
