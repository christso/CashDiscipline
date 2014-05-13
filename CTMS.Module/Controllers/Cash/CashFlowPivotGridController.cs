using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.ExpressApp.SystemModule;
using CTMS.Module.HelperClasses.UI;
using CTMS.Module.HelperClasses;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Xpo;
using System.Diagnostics;
using DG2NTT.ExpressApp.PivotGrid.Controllers;
using CTMS.Module.ParamObjects.Cash;
using System.Collections;
using CTMS.Module.BusinessObjects;

namespace CTMS.Module.Controllers.Cash
{
    public class CashFlowPivotGridController : CashReportViewController
    {
        public CashFlowPivotGridController()
        {
            TargetObjectType = typeof(CashFlow);
            TargetViewType = ViewType.ListView;
            TargetViewId = "CashFlow_PivotGridView";
        }

        protected override void OnActivated()
        {
            var cashFlowController = Frame.GetController<CashFlowViewController>();
            if (cashFlowController != null)
                cashFlowController.RunProgramAction.Active["PivotGrid"] = false;
            var calculateToggleController = Frame.GetController<CalculateToggleViewController>();
            if (calculateToggleController != null)
                calculateToggleController.CalculateAction.Active["PivotGrid"] = false;
            base.OnActivated();
        }

        protected override void SetupView()
        {
            base.SetupView();
            var listView = (ListView)View;

            CriteriaOperator dateCriteria = null;
            if (ReportParam.SetDefaultParams())
                dateCriteria = CriteriaOperator.Parse("TranDate Between (?,?)",
                    ReportParam.FromDate, ReportParam.ToDate);

            CriteriaOperator snapshotCriteria = null;
            if (ReportParam.Snapshot1 != null)
                snapshotCriteria = snapshotCriteria | CriteriaOperator.Parse("Snapshot.Oid = ?", ReportParam.Snapshot1.Oid);
            if (ReportParam.Snapshot2 != null)
                snapshotCriteria = snapshotCriteria | CriteriaOperator.Parse("Snapshot.Oid = ?", ReportParam.Snapshot2.Oid);
            listView.CollectionSource.Criteria["Filter1"] = dateCriteria & snapshotCriteria;
        }

        public const string fieldNoteCaption = "Note";

        private void AddCriteriaText(ref string outputText, string text, string op = "And")
        {
            if (!string.IsNullOrWhiteSpace(outputText))
                outputText += " " + op + " ";
            outputText += text;
        }

        protected PivotCellEventArgs DoubleClickPivotCellEventArgs;

        protected CashFlowNote GetNoteObject(PivotCellBaseEventArgs e, IObjectSpace objSpace, 
            bool autoCreate)
        {
            if (e.DataField.Caption != fieldNoteCaption)
                throw new InvalidOperationException("DataField must be of type CashFlowNote");

            var rowFields = e.GetRowFields();
            var colFields = e.GetColumnFields();
            string criteriaText = "";

            #region Add Time Criteria
            int year = 0;
            int month = 0;
            foreach (var field in colFields)
            {
                if (field.Caption == "Tran Month")
                {
                    month = (int)e.GetFieldValue(field);
                }
                else if (field.Caption == "Tran Year")
                {
                    year = (int)e.GetFieldValue(field);
                }
            }

            if (year != 0)
                AddCriteriaText(ref criteriaText, "TranYear = " + year, "And");
            else
                AddCriteriaText(ref criteriaText, "TranYear Is Null", "And");
            if (month != 0)
                AddCriteriaText(ref criteriaText, "TranMonth = " + month, "And");
            else
                AddCriteriaText(ref criteriaText, "TranMonth Is Null", "And");
            #endregion

            #region Add Activity Criteria
            ActivityTag dim_1_1 = null;
            ActivityTag dim_1_2 = null;
            ActivityTag dim_1_3 = null;
            foreach (var field in rowFields)
            {
                if (field.Caption == "Dim_1_1")
                {
                    dim_1_1 = (ActivityTag)e.GetFieldValue(field);
                }
                else if (field.Caption == "Dim_1_2")
                {
                    dim_1_2 = (ActivityTag)e.GetFieldValue(field);
                }
                else if (field.Caption == "Dim_1_3")
                {
                    dim_1_3 = (ActivityTag)e.GetFieldValue(field);
                }
            }

            if (dim_1_1 != null)
                AddCriteriaText(ref criteriaText, string.Format("Dim_1_1.Text = '{0}'", dim_1_1));
            else
                AddCriteriaText(ref criteriaText, "Dim_1_1 Is Null");
            if (dim_1_2 != null)
                AddCriteriaText(ref criteriaText, string.Format("Dim_1_2.Text = '{0}'", dim_1_2));
            else
                AddCriteriaText(ref criteriaText, "Dim_1_2 Is Null");
            if (dim_1_3 != null)
                AddCriteriaText(ref criteriaText, string.Format("Dim_1_3.Text = '{0}'", dim_1_3));
            else
                AddCriteriaText(ref criteriaText, "Dim_1_3 Is Null");
            #endregion

            var criteria = CriteriaOperator.Parse(criteriaText);
            var noteObj = objSpace.FindObject<CashFlowNote>(criteria);
            if (noteObj == null && autoCreate)
            {
                noteObj = objSpace.CreateObject<CashFlowNote>();
                noteObj.TranYear = year;
                noteObj.TranMonth = month;
                noteObj.Dim_1_1 = dim_1_1;
                noteObj.Dim_1_2 = dim_1_2;
                noteObj.Dim_1_3 = dim_1_3;
                noteObj.Save();
                objSpace.CommitChanges();
            }
            return noteObj;
        }


    }
}
