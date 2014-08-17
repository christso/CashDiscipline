//http://documentation.devexpress.com/#xaf/CustomDocument2778

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

namespace CTMS.Module.ParamObjects
{
    [DomainComponent]
    public class ReportParams : ReportParametersObjectBase
    {
        public Bank Bank { get; set; }

        public ReportParams(IObjectSpace objectSpace, Type reportDataType) : base(objectSpace, reportDataType) { }
        public override CriteriaOperator GetCriteria()
        {
            return new OperandProperty("DebitAccount.Bank") == Bank;
        }
        public override SortProperty[] GetSorting()
        {
            SortProperty[] sorting = new SortProperty[0];
            return sorting;
        }
    }
}
