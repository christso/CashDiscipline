using CashDiscipline.Module.Logic;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.CashDisciplineServiceReference;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ImportApPmtDistnParamViewController : ViewController
    {
        public ImportApPmtDistnParamViewController()
        {
            TargetObjectType = typeof(ImportApPmtDistnParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApPmtDistnAction", PredefinedCategory.ObjectsCreation);
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
            var paramObj = View.CurrentObject as ImportApPmtDistnParam;

            var importer = new ApPmtDistnImporter();
            var result = importer.Execute(paramObj.FilePath);

            // show log message
            string messagesText = CashDiscipline.Module.Logic.SqlServer.SsisUtil.GetMessageText(result.SsisMessages);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
                result.OperationStatus == CashDisciplineServiceReference.SsisOperationStatus.Success ?
                    "Import Successful" : "Imported Failed"
                );
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
