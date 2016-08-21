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
            importer.Execute(paramObj);

            // show log message
            string messagesText = string.Empty;
            foreach (var message in importer.SSISMessagesList)
            {
                if (messagesText != string.Empty)
                    messagesText += "\r\n";
                messagesText += message;
            }

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                Application,
                messagesText.Replace("\r\n\r\n", "\r\n")
                );
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
