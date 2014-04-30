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
using CTMS.Module.BusinessObjects.Payments;

namespace CTMS.Module.ReportParams
{
    [DomainComponent]
    public class ArtfPaymentReportParameters : ReportParametersObjectBase
    {
        public ArtfPaymentReportParameters(IObjectSpace objectSpace, Type reportDataType) : base(objectSpace, reportDataType) { }
        public override CriteriaOperator GetCriteria()
        {
            // TODO: select ArtfRecon based on selected PaymentBatches
            // ReportDataType is PaymentBatches
            // use InPlaceReporting or custom code?

            return null;
        }
        public override SortProperty[] GetSorting()
        {
            SortProperty[] sorting = new SortProperty[0];
            return sorting;
        }
    }
}
