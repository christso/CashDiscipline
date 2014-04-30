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
using CTMS.Module.BusinessObjects.Artf;
using CTMS.Module.ParamObjects.Artf;

namespace CTMS.Module.Controllers
{
    // Purpose: Create Journals from Tasks
    // For more information on Controllers and their life cycle, check out the http://documentation.devexpress.com/#Xaf/CustomDocument2621 and http://documentation.devexpress.com/#Xaf/CustomDocument3118 help articles.
    public partial class TaskResultParamController : ViewController
    {
        // Use this to do something when a Controller is instantiated (do not execute heavy operations here!).
        public TaskResultParamController()
        {
            InitializeComponent();
            // For instance, you can specify activation conditions of a Controller or create its Actions (http://documentation.devexpress.com/#Xaf/CustomDocument2622).
            TargetObjectType = typeof(ArtfTask);
            //TargetViewType = ViewType.DetailView;
            //TargetViewId = "DomainObject1_DetailView";
            //TargetViewNesting = Nesting.Root;
            //SimpleAction myAction = new SimpleAction(this, "MyActionId", DevExpress.Persistent.Base.PredefinedCategory.RecordEdit);
        }
        private void InitializeComponent()
        {

            // popUpAction1
            // 
            this.popUpAction1 = new PopupWindowShowAction(this, "popUpAction", "Edit");
            this.popUpAction1.Caption = "Task Results";
            this.popUpAction1.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.popUpAction1_CustomizePopupWindowParams);
            this.popUpAction1.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.popupWindowShowAction1_Execute);
        }

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction popUpAction1;

        // Override to do something before Controllers are activated within the current Frame (their View property is not yet assigned).
        protected override void OnFrameAssigned()
        {
            base.OnFrameAssigned();
            //For instance, you can access another Controller via the Frame.GetController<AnotherControllerType>() method to customize it or subscribe to its events.
        }
        // Override to do something when a Controller is activated and its View is assigned.
        protected override void OnActivated()
        {
            base.OnActivated();
            //For instance, you can customize the current View and its editors (http://documentation.devexpress.com/#Xaf/CustomDocument2729) or manage the Controller's Actions visibility and availability (http://documentation.devexpress.com/#Xaf/CustomDocument2728).
        }
        // Override to access the controls of a View for which the current Controller is intended.
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // For instance, refer to the http://documentation.devexpress.com/Xaf/CustomDocument3165.aspx help article to see how to access grid control properties.
        }
        // Override to do something when a Controller is deactivated.
        protected override void OnDeactivated()
        {
            // For instance, you can unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void popupWindowShowAction1_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            var taskObjects = Application.MainWindow.View.SelectedObjects;
            //if (taskObjects == null) return;
            // TODO: check parent object space is recon object
            // uncomment this if you need to change taskObject which requires the same session
            //IObjectSpace objectSpace = Application.CreateObjectSpace();
            IObjectSpace objectSpace = ObjectSpace;
            ArtfGlJournal gljnl;
            foreach (ArtfGlJournalTask taskObject in taskObjects)
            {
                string glDesc = taskObject.ArtfRecon.BankStmt.TranDescription ?? "";
                var reconDate = taskObject.ArtfRecon.ReconDate;
                string reconDateText = reconDate.ToString("dd-MMM-yy");
                string bankStmtDateText = taskObject.ArtfRecon.BankStmt.TranDate.ToString("dd-MMM-yy");

                // debit
                if (taskObject.GlDebitGlCode != null)
                {
                    gljnl = objectSpace.CreateObject<ArtfGlJournal>();
                    gljnl.ArtfTask = taskObject;
                    gljnl.NetAmount = taskObject.GlNetDebitAmount;
                    gljnl.GlCompany = taskObject.GlDebitGlCode.GlCompany;
                    gljnl.GlAccount = taskObject.GlDebitGlCode.GlAccount;
                    gljnl.GlDate = taskObject.ArtfRecon.ReconDate;
                    gljnl.GlDescription = reconDateText + " " + glDesc;
                }
                // credit
                if (taskObject.GlCreditGlCode != null)
                {
                    gljnl = objectSpace.CreateObject<ArtfGlJournal>();
                    gljnl.ArtfTask = taskObject;
                    gljnl.NetAmount = -taskObject.GlNetDebitAmount;
                    gljnl.GlCompany = taskObject.GlCreditGlCode.GlCompany;
                    gljnl.GlAccount = taskObject.GlCreditGlCode.GlAccount;
                    gljnl.GlDate = taskObject.ArtfRecon.ReconDate;
                    gljnl.GlDescription = reconDateText + " " + glDesc;
                }
            }
            objectSpace.CommitChanges();
        }

        private void popUpAction1_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            // List View
            //IObjectSpace objectSpace = Application.CreateObjectSpace();
            //e.View = Application.CreateListView(Application.FindListViewId(typeof(ArtfSystem)), new CollectionSource(objectSpace, typeof(ArtfSystem)), true);

            // Detail View
            IObjectSpace objectSpace = Application.CreateObjectSpace();
            ArtfCreateGlJournalsParam createJournalObject = new ArtfCreateGlJournalsParam();
            if (createJournalObject != null)
            {
                e.View = Application.CreateDetailView(objectSpace, Application.FindDetailViewId(typeof(ArtfCreateGlJournalsParam)), true, createJournalObject);
                ((DetailView)e.View).ViewEditMode = ViewEditMode.Edit;
            }
        }

    }
}
