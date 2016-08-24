using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.ParamObjects.Cash;
using Xafology.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;

namespace CashDiscipline.Module.Logic.Cash
{
    public class FixCashFlowsRephaser
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;

        public FixCashFlowsRephaser(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
            paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));
        }

        // adjust date of outflows
        public void Process(CashFlow cashFlow)
        {
            var maxActualDate = CashFlowHelper.GetMaxActualTranDate(objSpace.Session);

            if (cashFlow.Fix.FixTagType == CashForecastFixTagType.ScheduleOut
                && cashFlow.FixRank > 2
                && cashFlow.TranDate <= paramObj.PayrollLockdownDate
                && cashFlow.Activity.ForecastFixTag != CashDiscipline.Common.Constants.PayrollFixTag
                )
            {
                // adjust date of payroll payments
                cashFlow.TranDate = paramObj.PayrollNextLockdownDate;
            }
            else if (cashFlow.Fix.FixTagType == CashForecastFixTagType.ScheduleOut
                && cashFlow.FixRank > 2
                && cashFlow.TranDate <= paramObj.ApayableLockdownDate
                && cashFlow.Activity.ForecastFixTag != CashDiscipline.Common.Constants.PayrollFixTag
                && cashFlow.Activity.ForecastFixTag != CashDiscipline.Common.Constants.BankFeeFixTag
                && cashFlow.Activity.ForecastFixTag != CashDiscipline.Common.Constants.ProgenFixTag
                && cashFlow.Activity.ForecastFixTag != CashDiscipline.Common.Constants.TaxFixTag
                )
            {
                // adjust date of AP payments (exclude Payroll, Progen, Bank Fee, Tax)
                cashFlow.TranDate = paramObj.ApayableNextLockdownDate;
            }
            else if (cashFlow.Fix.FixTagType == CashForecastFixTagType.Allocate
                && cashFlow.FixRank > 2
                && cashFlow.TranDate <= paramObj.ApayableLockdownDate)
            {
                cashFlow.TranDate = paramObj.ApayableLockdownDate;

                if (cashFlow.Activity == paramApReclassActivity)
                {
                    // select the Friday on or before the forecasted payment date
                    cashFlow.TranDate = CashFlowHelper.StartDateOfWeek(
                        paramObj.ApayableNextLockdownDate, 5, 2);
                }
            }

            // ensure date is in forecast period (not actual period)
            if (cashFlow.TranDate <= maxActualDate)
            cashFlow.TranDate = maxActualDate.AddDays(1);

        }

    }
}
