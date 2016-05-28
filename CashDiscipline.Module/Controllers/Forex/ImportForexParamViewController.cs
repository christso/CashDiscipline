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
using CashDiscipline.Module.Logic.Forex;

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ImportForexParamViewController : ViewController
    {
        public ImportForexParamViewController()
        {
            TargetObjectType = typeof(ImportForexRatesParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportWbcForexRatesACtion", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Run Import";
            importAction.Execute += ImportAction_Execute; ;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            WbcImport();
        }


        public void WbcImport()
        {
            var paramObj = View.CurrentObject as ImportForexRatesParam;

            var importer = new WbcForexRateImporter();
            importer.Execute(paramObj.FileName);

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
