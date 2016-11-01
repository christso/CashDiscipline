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

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ImportApInvoiceHeaderViewController : ViewController
    {
        public ImportApInvoiceHeaderViewController()
        {
            TargetObjectType = typeof(ImportApInvoiceHeaderParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApInvoiceHeaderAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
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
            var paramObj = (ImportApInvoiceHeaderParam)View.CurrentObject;

            var objSpace = (XPObjectSpace)ObjectSpace;
            var importer = new ApInvoiceHeaderImporter(objSpace);

            var messagesText = importer.Execute(paramObj);
            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
