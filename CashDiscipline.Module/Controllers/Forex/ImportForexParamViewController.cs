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
using CashDiscipline.Module.Logic;
using CashDiscipline.Module.CashDisciplineServiceReference;

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
            var result = importer.Execute(paramObj.FileName);
            var messageText = CashDiscipline.Module.Logic.SqlServer.SsisUtil.GetMessageText(result.SsisMessages);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
               messageText,
               result.OperationStatus == SsisOperationStatus.Success ?
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
