using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Reports;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.ReportParams
{
    [DomainComponent]
    public class CashReportParameters : ReportParametersObjectBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public CashReportParameters(IObjectSpace objectSpace, Type reportDataType) : base(objectSpace, reportDataType)
        {
            var currentUser = CTMS.Module.HelperClasses.StaticHelpers.GetCurrentUser(objectSpace);
            var paramObj = objectSpace.FindObject<CashReportParameters>(
                CriteriaOperator.Parse("ReportUser = ?", currentUser));
            FromDate = paramObj.FromDate;
            ToDate = paramObj.ToDate;
        }

        public override CriteriaOperator GetCriteria()
        {
            return new OperandProperty("TranDate") >= FromDate 
                & new OperandProperty("TranDate") <= ToDate;
        }
        public override SortProperty[] GetSorting()
        {
            SortProperty[] sorting = new SortProperty[1];
            sorting[0] = new SortProperty("TranDate", SortingDirection.Ascending);
            return sorting;
        }
    }
}
