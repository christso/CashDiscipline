using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects;
using DevExpress.ExpressApp.Actions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using DevExpress.ExpressApp.Utils;
using CTMS.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Xafology.ExpressApp.Reports;
using DevExpress.Data.Filtering;
using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.ParamObjects.Cash;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.Controllers.ExcelReporting
{
    public class ExcelReportViewController : ViewController
    {
        public ExcelReportViewController()
        {
            TargetObjectType = typeof(ExcelReport);
            createXlReportAction = new SimpleAction(this, "CreateXlReportAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            createXlReportAction.Caption = "Create Report";
            createXlReportAction.Execute += createXlReportAction_Execute;

            // Report Creator Names
            _ReportCreatorTypes = new Dictionary<string, Type>();
            _ReportCreatorTypes.Add("Cash Report", typeof(CashReportCreator));
            _ReportCreatorTypes.Add("Gen Ledger", typeof(GenLedgerReportCreator));
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        private readonly Dictionary<string, Type> _ReportCreatorTypes;

        #region Report Creators

        private class CashReportCreator : ExcelReportCreator
        {
            private CashReportParam _ParamObj;

            public override void Execute()
            {
                var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
                _ParamObj = CashReportParam.GetInstance(objSpace);
                var dialog = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
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
                ExcelReportHelper.CopyObjectsToWorksheet(session, cashFlows, ws);
                var reportSheet = Package.Workbook.Worksheets["Report"];
                session.CommitTransaction();
            }
        }

        private class GenLedgerReportCreator : ExcelReportCreator
        {
            private FinGenJournalParam _ParamObj;

            public override void Execute()
            {
                var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var paramObj = FinGenJournalParam.GetInstance(objSpace);
                var dialog = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
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
        }

        #endregion

        public void CreateXlReport()
        {
            var reportObj = View.CurrentObject as ExcelReport;
            if (reportObj.TemplateFile == null) return;

            // create stream for ExcelPackage
            var fileName = reportObj.TemplateFile.FileName;
            var stream = new MemoryStream();
            reportObj.TemplateFile.SaveToStream(stream);

            stream.Position = 0;

            using (ExcelPackage package = new ExcelPackage(stream))
            {
                foreach (var pair in _ReportCreatorTypes)
                {
                    if (reportObj.ReportName != pair.Key)
                        continue;
                    var report = (ExcelReportCreator)Activator.CreateInstance(pair.Value);
                    report.SetEnv(Application, package);
                    report.Execute();
                }

                package.Save();

                package.Stream.Position = 0;
                reportObj.TemplateFile.LoadFromStream(fileName, package.Stream);
                reportObj.Save();
                reportObj.Session.CommitTransaction();
            }
            stream.Close();
        }

        void createXlReportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            CreateXlReport();
        }

        private SimpleAction createXlReportAction;
    }
}
