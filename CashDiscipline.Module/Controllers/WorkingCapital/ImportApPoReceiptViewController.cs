using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.Logic.Import;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;
using CashDiscipline.Common;
using DG2NTT.AnalysisServicesHelpers;
using CashDiscipline.Module.ParamObjects;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ImportApPoReceiptViewController : ViewController
    {
        public ImportApPoReceiptViewController()
        {
            TargetObjectType = typeof(ImportApPoReceiptParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApPoReceiptAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var importer = new ApPoReceiptImporter((XPObjectSpace)ObjectSpace);
            var paramObj = (ImportApPoReceiptParam)View.CurrentObject;
            var messagesText = importer.Execute(paramObj);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
