using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Editors;
using CTMS.Module.BusinessObjects.Forex;


namespace CTMS.Module.Controllers.Forex
{
    public class ForexTradePredeliveryListViewController : ViewController
    {
        public ForexTradePredeliveryListViewController()
        {
            TargetObjectType = typeof(ForexTradePredelivery);
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

            var predelivery = (ForexTradePredelivery)e.CreatedObject;

            var valueDatePropertyEditor = mainView.FindItem(ForexTradePredelivery.FieldNames.ValueDate) as PropertyEditor;
            predelivery.ValueDate = (DateTime)valueDatePropertyEditor.PropertyValue;
            var counterCcyAmtPropertyEditor = mainView.FindItem(ForexTradePredelivery.FieldNames.CounterCcyAmt) as PropertyEditor;
            predelivery.CounterCcyAmt = (decimal)counterCcyAmtPropertyEditor.PropertyValue;
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
