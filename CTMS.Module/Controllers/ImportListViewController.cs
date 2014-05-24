using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using CTMS.Module.ParamObjects.Import;

using D2NXAF.ExpressApp.Xpo;


namespace CTMS.Module.Controllers
{
    public partial class ImportListViewController : ViewController
    {
        public ImportListViewController()
        {
            TargetViewType = ViewType.ListView;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            ChoiceActionItem defaultActionItem = new ChoiceActionItem();
            defaultActionItem.Caption = "Default";

            ChoiceActionItem csvActionItem = new ChoiceActionItem();
            csvActionItem.Caption = "CSV";

            importDataAction = new SingleChoiceAction(this, "ImportData", PredefinedCategory.ObjectsCreation);
            importDataAction.Caption = "Import";
            this.importDataAction.Items.Add(defaultActionItem);
            this.importDataAction.Items.Add(csvActionItem);
            this.importDataAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            this.importDataAction.Execute += new DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventHandler(this.importDataAction_Execute);
        }

        private SingleChoiceAction importDataAction;

        protected override void OnActivated()
        {
            base.OnActivated();
            
            // Disable for non-admin user
            if (SecuritySystem.CurrentUserName != null)
            {
                if (SecuritySystem.CurrentUserName != "admin")
                    this.importDataAction.Active.SetItemValue("Disabled for non-Admin", false);
            }
        }

        private void importDataAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            ChoiceActionItem activeItem = e.SelectedChoiceActionItem;
            var currentTypeInfo = ((ObjectView)View).ObjectTypeInfo;

            var showParamAction = new Action<FieldMaps>((fieldAliases) =>
                {
                    var paramObj = new ImportDelimitedFileDataParam()
                    {
                        ObjectTypeInfo = currentTypeInfo,
                        FieldMaps = fieldAliases
                    };
                    paramObj.CreateTemplate();
                    var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
                    dialog.ShowView(paramObj);
                });


            if (activeItem.Caption == "Default")
            {
                // TODO: how to show different custom form for each parameter object?
                // Using ViewController?
                // Using attributes to show view parameters?
                
                if (currentTypeInfo.Type == typeof(CTMS.Module.BusinessObjects.Market.ForexRate))
                {
                    var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application); 
                    dialog.ShowNonPersistentView(typeof(ImportForexRatesParam));
                }
                else if (currentTypeInfo.Type == typeof(CTMS.Module.BusinessObjects.Cash.BankStmt))
                {
                    var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application); 
                    dialog.ShowNonPersistentView(typeof(ImportBankStmtParam));
                }
                else if (currentTypeInfo.Type == typeof(CTMS.Module.BusinessObjects.Cash.AccountsPayable.ApPmtDistn))
                {
                    var fieldMaps = new FieldMaps();
                    fieldMaps.Add("Actual Payment Date", "PaymentDate");
                    fieldMaps.Add("Invoice Due Date", "InvoiceDueDate");
                    fieldMaps.Add("Source", "Source");
                    fieldMaps.Add("Bank Account Name", "BankAccount");
                    fieldMaps.Add("Pay Group", "PayGroup");
                    fieldMaps.Add("Inv Source", "InvSource");
                    fieldMaps.Add("Vendor Name", "Vendor");
                    fieldMaps.Add("Company", "GlCompany");
                    fieldMaps.Add("Account", "GlAccount");
                    fieldMaps.Add("Cost Centre", "GlCostCentre");
                    fieldMaps.Add("Product", "GlProduct");
                    fieldMaps.Add("Sales Channel", "GlSalesChannel");
                    fieldMaps.Add("Country", "GlCountry");
                    fieldMaps.Add("Intercompany", "GlIntercompany");
                    fieldMaps.Add("Project", "GlProject");
                    fieldMaps.Add("Location", "GlLocation");
                    fieldMaps.Add("Po Num", "PoNum");
                    fieldMaps.Add("Invoice Num", "InvoiceNum");
                    fieldMaps.Add("Actual Payment Amount Fx", "PaymentAmountFx");
                    fieldMaps.Add("Payment Amount Aud", "PaymentAmountAud");
                    fieldMaps.Add("Payment Number", "PaymentNumber");
                    fieldMaps.Add("Payment Batch Name", "PaymentBatchName");
                    fieldMaps.Add("Invoice Currency", "InvoiceCurrency");
                    fieldMaps.Add("Payment Creation Date", "PaymentCreationDate");
                    fieldMaps.Add("Invoice Line Desc", "InvoiceLineDesc");
                    fieldMaps.Add("Payment Currency", "PaymentCurrency");
                    showParamAction(fieldMaps);
                }
                else
                {
                    showParamAction(null);
                }
            }
            else if (activeItem.Caption == "CSV")
            {
                showParamAction(null);
            }
        }
    }
}
