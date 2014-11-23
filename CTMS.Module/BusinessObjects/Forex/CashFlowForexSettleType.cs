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
using CTMS.Module.BusinessObjects.Market;
using CTMS.Module.BusinessObjects.Setup;
using DevExpress.ExpressApp.Xpo;
using CTMS.Module.ParamObjects.Cash;
using System.Diagnostics;
using Xafology.Utils;
using GenerateUserFriendlyId.Module.BusinessObjects;

namespace CTMS.Module.BusinessObjects.Forex
{
    public enum CashFlowForexSettleType
    {
        Exclude,
        In,
        Out
    }
}
