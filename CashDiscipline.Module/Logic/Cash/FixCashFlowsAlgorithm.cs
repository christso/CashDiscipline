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

namespace CashDiscipline.Module.Logic.Cash
{
    public class FixCashFlowsAlgorithm : IFixCashFlows
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private IFixCashFlows algorithm;

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (objSpace.Session.Connection == null)
                this.algorithm = new MemFixCashFlowsAlgorithm(objSpace, paramObj);
            else
                this.algorithm = new SqlFixCashFlowsAlgorithm(objSpace, paramObj);
        }

        public void Reset()
        {
            algorithm.Reset();
        }

        public void ProcessCashFlows()
        {
            algorithm.ProcessCashFlows();
        }
    }
}
