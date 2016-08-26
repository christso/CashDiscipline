using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.ParamObjects.Import;
using Xafology.Utils;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using SDP.ParserUtils;
using System;
using System.IO;
using System.Text;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.Logic;
using System.Linq;
using CashDiscipline.Module.ServiceReference1;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ImportBankStmtParamViewController : ViewController
    {
        public ImportBankStmtParamViewController()
        {
            TargetObjectType = typeof(ImportBankStmtParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportBankStmtAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Run Import";
            importAction.Execute += ImportAction_Execute;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                Import();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void Import()
        {
            var paramObj = View.CurrentObject as ImportBankStmtParam;

            var importer = new BankStmtImporter();

            var result = importer.Execute(paramObj.ImportBankStmtParamItems);

            string messagesText = string.Empty;
            foreach (var childResult in result)
            {
                if (!string.IsNullOrEmpty(messagesText))
                    messagesText += "\r\n\r\n";
                messagesText += string.Format("## {0}: {1} ---------------\r\n", 
                    childResult.OperationStatus.ToString().ToUpper(),
                    childResult.PackageName);
                messagesText += CashDiscipline.Module.Logic.SqlServer.SsisUtil.GetMessageText(childResult.SsisMessages);
            }

            int successCount = result.Where(x => x.OperationStatus == SsisOperationStatus.Success).Count();
            int failCount = result.Count - successCount;
            

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
                    failCount > 0 ? string.Format("Import Completed with {0} Failures", failCount) :
                    "Import Completed Successfully"
                );
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
