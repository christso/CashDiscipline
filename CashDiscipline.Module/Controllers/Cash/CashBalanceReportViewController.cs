using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashBalanceReportViewController : CashReportViewController
    {
        public CashBalanceReportViewController()
        {
            TargetViewType = ViewType.ListView;
            TargetViewId = "AccountSummary_PivotGridView";
            TargetObjectType = typeof(AccountSummary);
        }

        protected override void SetupView()
        {
            base.SetupView();

            var objSpace = (XPObjectSpace)ObjectSpace;

            if (ReportParam.SetDefaultParams())
                objSpace.CommitChanges();

            // Calculate Account Summary
            var snapshots = new List<CashFlowSnapshot>() { ReportParam.Snapshot1, ReportParam.Snapshot2 };
            AccountSummary.CalculateCashFlow(objSpace, ReportParam.FromDate.Date, ReportParam.ToDate, snapshots);
            var startDate = AccountSummary.GetUniqueBalanceDate(objSpace.Session, ReportParam.FromDate);
            AccountSummary.CalculateBalance(objSpace, startDate, snapshots);

            // Set View Criteria to only include the opening balance for Balance line types
            var criteria = CriteriaOperator.Parse("TranDate Between (?,?)", startDate, ReportParam.ToDate);
            // exclude balance after the report start date
            var balCriteria = CriteriaOperator.Parse("Not (LineType = ? And TranDate <> ?)", AccountSummaryLineType.Balance, startDate);
            // exclude cash flows at the balance date to avoid double-counting
            var flowCriteria = CriteriaOperator.Parse("Not (LineType = ? And TranDate = ?)", AccountSummaryLineType.Flow, startDate);
            var view = (ListView)View;
            view.CollectionSource.Criteria["Filter1"] = criteria & balCriteria & flowCriteria;
        }

        // Get the first date in AccountSummary that is equal to or greater than fromDate
        // to avoid performing the same calculations (since the result would be the same 
        // for periods that have no cash flow movement.
        // If fromDate is greater than any of the dates in AccountSummary, then this will
        // return Max(TranDate) in Account Summary.
        protected DateTime GetStartDate(DateTime fromDate)
        {
            var session = ((XPObjectSpace)ObjectSpace).Session;
            var startDate =  session.Evaluate<AccountSummary>(
                CriteriaOperator.Parse("Min(TranDate)"),
                CriteriaOperator.Parse("TranDate >= ? And LineType = ?",
                   fromDate, AccountSummaryLineType.Flow));
            // this exception will also be thrown if no cash flows exist
            if (startDate == null)
                startDate = session.Evaluate<AccountSummary>(
                    CriteriaOperator.Parse("Max(TranDate)"), null);
            if (startDate == null)
                startDate = fromDate;
                //throw new ApplicationException("No Dates in CashFlow are greater than the Report Parameter 'FromDate'.");
            return (DateTime)startDate;
        }


    }
}
