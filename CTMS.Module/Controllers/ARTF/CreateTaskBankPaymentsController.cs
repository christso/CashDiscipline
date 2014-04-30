using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using CTMS.Module.BusinessObjects.Artf;
using CTMS.Module.ParamObjects.Artf;
using CTMS.Module.BusinessObjects.Payments;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.Controllers.Artf
{
    public class CreateTaskBankPaymentsController : ViewController
    {
        private DevExpress.ExpressApp.Actions.SimpleAction createPaymentsAction;
        private IObjectSpace _ParamObjectSpace;
        

        public CreateTaskBankPaymentsController()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            createPaymentsAction = new SimpleAction(this, "CreatePayments", "Edit");
            this.createPaymentsAction.Caption = "Create Payments";
            this.createPaymentsAction.Execute += createPaymentsAction_Execute;
            // 
            // CreateTaskBankPaymentsController
            // 
            this.TargetObjectType = typeof(CTMS.Module.BusinessObjects.Artf.ArtfFundsTransferTask);

        }

        void AcceptAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            ArtfCreatePaymentsParam obj = (ArtfCreatePaymentsParam)e.CurrentObject;
            Validator.RuleSet.Validate(_ParamObjectSpace, obj, Constants.AcceptActionContext);
            CreatePayments(obj);
        }

        private void createPaymentsAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            _ParamObjectSpace = Application.CreateObjectSpace();

            // show popup window
            var svp = new ShowViewParameters();
            svp.CreatedView = Application.CreateDetailView(_ParamObjectSpace, new ArtfCreatePaymentsParam());
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.PopupWindow;

            svp.CreateAllControllers = true;
            var dc = Application.CreateController<DialogController>();
            dc.AcceptAction.Execute += AcceptAction_Execute;
            svp.Controllers.Add(dc);

            ((DetailView)svp.CreatedView).ViewEditMode = ViewEditMode.Edit;
            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }

        private void CreatePayments(ArtfCreatePaymentsParam paramObject)
        {
            var selectedTasks = (System.Collections.IList)View.SelectedObjects;

            // order by TfrFromBankAccount so we can combine multiple tasks
            // that use the same DebitAccount into a single payment batch
            var queriedTasks = from a in selectedTasks.OfType<ArtfFundsTransferTask>()
                        orderby a.TfrFromBankAccount
                        select a;

            var session = ((XPObjectSpace)View.ObjectSpace).Session;
            
            PaymentBatch paymentBatch = null;
            foreach (var task in queriedTasks)
            {
                // if DebitAccount is different to previous one, then create new payment batch
                if (paymentBatch == null || task.TfrFromBankAccount != paymentBatch.DebitAccount)
                {
                    paymentBatch = new PaymentBatch(session);
                    paymentBatch.ValueDate = paramObject.ValueDate;
                    paymentBatch.BatchName = paramObject.PaymentBatchName;
                    paymentBatch.DebitAccount = task.TfrFromBankAccount;
                    paymentBatch.Save();
                }
                // create payment
                var payment = new ArtfTaskPayment(session);
                payment.PaymentBatch = paymentBatch;
                payment.CreditAccount = session.FindObject<CreditAccount>(
                    CreditAccount.Fields.LinkedAccount == task.TfrToBankAccount);
                payment.Amount = task.ArtfRecon.Amount;
            }
            ObjectSpace.CommitChanges();
        }

    }
}
