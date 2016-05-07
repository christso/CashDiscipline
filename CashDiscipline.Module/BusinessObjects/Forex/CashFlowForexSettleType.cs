using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

using CashDiscipline.Module.BusinessObjects.Setup;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.ParamObjects.Cash;
using System.Diagnostics;
using Xafology.Utils;

namespace CashDiscipline.Module.BusinessObjects.Forex
{
    public enum CashFlowForexSettleType
    {
        Exclude,
        In,
        Out,
        Auto,
        [DevExpress.ExpressApp.DC.XafDisplayName("Out Reclass")]
        OutReclass
    }
}
