﻿using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Utils;
using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using CashDiscipline.Module.BusinessObjects.ChartOfAccounts;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Xpo;
using CashDiscipline.UnitTests.TestObjects;
using Xafology.ExpressApp.Xpo.Import.Parameters;

namespace CashDiscipline.UnitTests
{
    public class CashDisciplineTestHelper
    {
        public static void AddExportedTypes(ModuleBase module)
        {
            module.AdditionalExportedTypes.Add(typeof(SetOfBooks));
            module.AdditionalExportedTypes.Add(typeof(BankStmt));
            module.AdditionalExportedTypes.Add(typeof(CashFlow));
            module.AdditionalExportedTypes.Add(typeof(CashFlowDefaults));
            module.AdditionalExportedTypes.Add(typeof(Account));
            module.AdditionalExportedTypes.Add(typeof(FinActivity));
            module.AdditionalExportedTypes.Add(typeof(FinAccount));
            module.AdditionalExportedTypes.Add(typeof(FinJournalGroup));
            module.AdditionalExportedTypes.Add(typeof(FinGenJournalParam));
            module.AdditionalExportedTypes.Add(typeof(GenLedger));
            module.AdditionalExportedTypes.Add(typeof(GlAccount));
            module.AdditionalExportedTypes.Add(typeof(GlCompany));
            module.AdditionalExportedTypes.Add(typeof(GlCountry));
            module.AdditionalExportedTypes.Add(typeof(GlIntercompany));
            module.AdditionalExportedTypes.Add(typeof(GlLocation));
            module.AdditionalExportedTypes.Add(typeof(GlProduct));
            module.AdditionalExportedTypes.Add(typeof(GlProject));
            module.AdditionalExportedTypes.Add(typeof(GlSalesChannel));
            module.AdditionalExportedTypes.Add(typeof(FinAccountingDefaults));
            module.AdditionalExportedTypes.Add(typeof(ForexTrade));
            module.AdditionalExportedTypes.Add(typeof(ForexCounterparty));
            module.AdditionalExportedTypes.Add(typeof(CashFlowFixParam));
            module.AdditionalExportedTypes.Add(typeof(ForexRate));
            module.AdditionalExportedTypes.Add(typeof(BankStmtCashFlowForecast));
            module.AdditionalExportedTypes.Add(typeof(ForexStdSettleAccount));
            module.AdditionalExportedTypes.Add(typeof(ForexTradePredelivery));
            module.AdditionalExportedTypes.Add(typeof(AustPostSettle));
            module.AdditionalExportedTypes.Add(typeof(CashReportParam));
            module.AdditionalExportedTypes.Add(typeof(AccountSummary));
            module.AdditionalExportedTypes.Add(typeof(ForexSettleLink));
            module.AdditionalExportedTypes.Add(typeof(MockLookupObject1));
            module.AdditionalExportedTypes.Add(typeof(MockLookupObject2));
            module.AdditionalExportedTypes.Add(typeof(MockFactObject));
            module.AdditionalExportedTypes.Add(typeof(ImportParamBase));
            module.AdditionalExportedTypes.Add(typeof(ImportHeadersParam));
            module.AdditionalExportedTypes.Add(typeof(ImportOrdinalsParam));
            module.AdditionalExportedTypes.Add(typeof(CashForecastFixTag));

        }
    }
}