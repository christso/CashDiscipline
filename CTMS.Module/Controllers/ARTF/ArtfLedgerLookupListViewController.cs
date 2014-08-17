using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects.Artf;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Editors;

namespace CTMS.Module.Controllers.Artf
{
    public class ArtfLedgerLookupListViewController : ViewController
    {
        public ArtfLedgerLookupListViewController()
        {
            TargetObjectType = typeof(ArtfLedger);
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

            var ledger = (ArtfLedger)e.CreatedObject;

            if (mainView.Id == Constants.ArtfReconDetailViewId)
            {
                var amountPropertyEditor = mainView.FindItem("Amount") as PropertyEditor;
                ledger.Amount = (decimal)amountPropertyEditor.PropertyValue;
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
