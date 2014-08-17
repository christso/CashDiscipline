using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.ParamObjects.Cash;
using D2NXAF.ExpressApp.MsoExcel.Attributes;
using D2NXAF.ExpressApp.MsoExcel.Reports;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using OfficeOpenXml;
using System.IO;

namespace CTMS.Module.ExcelReportCreators
{
    [ReportName("Cash Report")]
    public class CashReportCreator : ExcelReportCreator
    {
        private CashReportParam _ParamObj;

        public CashReportCreator()
        {
            FileName = "Cash Report.xlsx";
        }
        public CashReportCreator(XafApplication app, ExcelPackage package)
            : base(app, package)
        {

        }

        public override void Execute()
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            _ParamObj = CashReportParam.GetInstance(objSpace);
            var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
            dialog.Accepting += dialog_Accepting;
            dialog.ShowView(objSpace, _ParamObj);
        }

        void dialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            var session = _ParamObj.Session;

            var sortProps = new SortingCollection(null);
            var cop = CriteriaOperator.Parse("TranDate Between(?,?)",
                              _ParamObj.FromDate, _ParamObj.ToDate);
            var cashFlows = session.GetObjects(session.GetClassInfo(typeof(CashFlow)),
                cop, sortProps, 0, false, true);

            ExcelWorksheet ws = Package.Workbook.Worksheets["Data"];
            if (ws == null)
                throw new UserFriendlyException("Worksheet 'Data' not found in workbook.");

            ExcelReportHelper.CopyObjectsToWorksheet(session, cashFlows, ws);

            session.CommitTransaction();
        }

        public override Stream CreateReportTemplateStream()
        {
            return GetType().Assembly.GetManifestResourceStream(
                   "CTMS.Module.EmbeddedExcelTemplates.Cash Report.xlsx");
        }
    }
}
