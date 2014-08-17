using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.ParamObjects.FinAccounting;
using D2NXAF.ExpressApp.MsoExcel.Attributes;
using D2NXAF.ExpressApp.MsoExcel.Reports;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using OfficeOpenXml;
using System.Linq;
using DevExpress.ExpressApp;

namespace CTMS.Module.ExcelReportCreators
{
    [ReportName("Gen Ledger Report")]
    public class GenLedgerReportCreator : ExcelReportCreator
    {
        private FinGenJournalParam _ParamObj;

        public GenLedgerReportCreator()
        {
            FileName = "Gen Ledger Report.xlsx";
        }
        public GenLedgerReportCreator(DevExpress.ExpressApp.XafApplication app, ExcelPackage package)
            : base(app, package)
        {
            
        }

        public override void Execute()
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = FinGenJournalParam.GetInstance(objSpace);
            var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
            dialog.Accepting += dialog_Accepting;
            _ParamObj = paramObj;
            dialog.ShowView(objSpace, paramObj);
        }

        void dialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            var session = _ParamObj.Session;
            var jnlGroupKeysInParams = _ParamObj.JournalGroupParams.Select(p => p.JournalGroup.Oid);

            var sortProps = new SortingCollection(null);
            sortProps.Add(new SortProperty("SrcDate", DevExpress.Xpo.DB.SortingDirection.Ascending));

            var cop = CriteriaOperator.Parse("SrcDate Between(?,?)",
                              _ParamObj.FromDate, _ParamObj.ToDate);
            var copGenLedgerInJnlGroups = new InOperator("JournalGroup.Oid", jnlGroupKeysInParams);
            cop = GroupOperator.And(cop, copGenLedgerInJnlGroups);
            var genLedgers = session.GetObjects(session.GetClassInfo(typeof(GenLedger)),
                cop, sortProps, 0, false, true);
            ExcelWorksheet ws = Package.Workbook.Worksheets["Data"];
            ExcelReportHelper.CopyObjectsToWorksheet(session, genLedgers, ws);
            session.CommitTransaction();
        }

        public override System.IO.Stream CreateReportTemplateStream()
        {
            return GetType().Assembly.GetManifestResourceStream(
                   "CTMS.Module.EmbeddedExcelTemplates.Gen Ledger Report.xlsx");
        }
    }
}
