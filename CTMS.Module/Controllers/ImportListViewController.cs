using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Cash.AccountsPayable;
using CTMS.Module.BusinessObjects.Market;
using CTMS.Module.ParamObjects.Import;
using d2import = D2NXAF.ExpressApp.Xpo.Import;

namespace CTMS.Module.Controllers
{
    public class ImportListViewController : d2import.Controllers.ImportListViewController
    {
        private bool IsHandled = false;

        protected override void ImportDataActionExecute(object sender, DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventArgs e)
        {
            IsHandled = false;
            if (e.SelectedChoiceActionItem.Caption == "Default")
                DefaultImportDataActionExecute();
            if (!IsHandled)
                base.ImportDataActionExecute(sender, e);
        }

        #region Default Import
        private void DefaultImportDataActionExecute()
        {
            if (View.ObjectTypeInfo.Type == typeof(ForexRate))
            {
                var paramObj = new ImportForexRatesParam();
                ShowParamView(paramObj);
            }
            else if (View.ObjectTypeInfo.Type == typeof(BankStmt))
            {
                var paramObj = new ImportBankStmtParam();
                ShowParamView(paramObj);
            }
            else if (View.ObjectTypeInfo.Type == typeof(ApPmtDistn))
            {
                var paramObj = new ImportApPmtDistnParam();
                ShowParamView(paramObj);
            }
        }

        private void ShowParamView(object paramObj)
        {
            var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
            dialog.CanCloseWindow = false;
            dialog.ShowNonPersistentView(paramObj);
            IsHandled = true;
        }
        #endregion
    }
}
