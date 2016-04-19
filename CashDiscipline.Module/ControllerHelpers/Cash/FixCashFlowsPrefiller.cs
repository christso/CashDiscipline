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
using Xafology.Spreadsheet.Attributes;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;

namespace CashDiscipline.Module.ControllerHelpers.Cash
{
    public class FixCashFlowsPrefiller
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;

        public FixCashFlowsPrefiller(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
        }

        public void Process(CashFlow cashFlow)
        {
            ProcessFixActivity(cashFlow);
            ProcessFixTag(cashFlow);
        }

        public void ProcessFixActivity(CashFlow cashFlow)
        {

            if (cashFlow.Activity == null
                || cashFlow.Source == null)
                return;

            // update FixActivity
            if (cashFlow.Source.Team.ToUpper() == "AP"
                && cashFlow.Activity != null
                && (
                    cashFlow.Activity.Name.ToUpper() == "Handset Pchse"
                    || cashFlow.Activity.Name.ToUpper() == "iPhone Pchse Pymt"
                    )
                )
            {
                cashFlow.FixActivity = cashFlow.Activity;
            }
            else if (cashFlow.Source.Team.ToUpper() == "AR"
                && cashFlow.Activity.Name == "Foreign Roaming Rcpt")
            {
                cashFlow.FixActivity = cashFlow.Activity;
            }
            else
            {
                cashFlow.FixActivity = cashFlow.Activity.FixActivity;
            }
        }

        public void ProcessFixTag(CashFlow cashFlow)
        {
            if (cashFlow.Fix != null)
                return;

            if (cashFlow.Source != null && cashFlow.Source.Team.ToLower() == "commission")
            {
                cashFlow.FixRank = 3;
            }
        }

    }
}
