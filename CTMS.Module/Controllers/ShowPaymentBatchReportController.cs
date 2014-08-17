using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects.Payments;
using CTMS.Module.BusinessObjects.Artf;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Reports;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Data.Filtering;
using System.Collections;

namespace CTMS.Module.Controllers
{
    public class ShowPaymentBatchReportController : ViewController
    {
        private class Captions
        {
            public const string PaymentBatch = "Payment Batch";
            public const string ArTransferRecon = "AR Transfer Recon";
        }

        private SingleChoiceAction ShowReportAction;
        public ShowPaymentBatchReportController()
        {
            TargetObjectType = typeof(PaymentBatch);

            ChoiceActionItem paymentBatchChoice = new ChoiceActionItem();
            paymentBatchChoice.Caption = Captions.PaymentBatch;
            ChoiceActionItem reconChoice = new ChoiceActionItem();
            reconChoice.Caption = Captions.ArTransferRecon;

            ShowReportAction = new SingleChoiceAction(this, "ShowReportAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            ShowReportAction.ItemType = DevExpress.ExpressApp.Actions.SingleChoiceActionItemType.ItemIsOperation;
            ShowReportAction.Caption = "Show Report";
            ShowReportAction.ImageName = "BO_Report";
            ShowReportAction.Execute += ShowReportAction_Execute;

            ShowReportAction.Items.Add(paymentBatchChoice);
            ShowReportAction.Items.Add(reconChoice);
        }

        void ShowReportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            if (e.SelectedChoiceActionItem.Caption == Captions.PaymentBatch)
            {
                ReportData reportData = ObjectSpace.FindObject<ReportData>(new BinaryOperator("ReportName", "Payment Batch"));
                if (reportData == null)
                    throw new UserFriendlyException("Report 'Payment Batch' was not found");
                ArrayList paymentBatchKeys = new ArrayList();
                foreach (PaymentBatch paymentBatch in e.SelectedObjects)
                {
                    paymentBatchKeys.Add(ObjectSpace.GetKeyValue(paymentBatch));
                }
                // get PaymentRequests
                Frame.GetController<ReportServiceController>().ShowPreview(reportData, new InOperator("Oid", paymentBatchKeys));
            }
            else if (e.SelectedChoiceActionItem.Caption == Captions.ArTransferRecon)
            {
                ReportData reportData = ObjectSpace.FindObject<ReportData>(new BinaryOperator("ReportName", "AR Transfer Recon"));
                if (reportData == null)
                    throw new UserFriendlyException("Report 'AR Transfer Recon' was not found");
                ArrayList reconKeys = new ArrayList();
                foreach (PaymentBatch paymentBatch in e.SelectedObjects)
                {
                    foreach (ArtfTaskPayment taskPayment in paymentBatch.Payments)
                    {
                        if (!(taskPayment is ArtfTaskPayment)) continue;
                        // TODO: fix null exception below
                        var keyValue = ObjectSpace.GetKeyValue(taskPayment.ArtfFundsTransferTask.ArtfRecon);
                        if (!reconKeys.Contains(keyValue))
                            reconKeys.Add(keyValue);
                    }
                }
                // get PaymentRequests
                Frame.GetController<ReportServiceController>().ShowPreview(reportData, new InOperator("Oid", reconKeys));
            }
        }
    }
}
