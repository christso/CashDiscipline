using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.Interfaces;

namespace CTMS.Module.Controllers.Forex
{
    public class ForexTradeBatchUploaderImpl : ForexTradeBatchUploader
    {
        private readonly IObjectSpace objectSpace;

        public ForexTradeBatchUploaderImpl(IObjectSpace objectSpace)
        {
            if (objectSpace == null)
                throw new ArgumentException("objectSpace");
            this.objectSpace = objectSpace;
        }

        private DateTime GetMaxActualTranDate()
        {
            var session = ((XPObjectSpace)objectSpace).Session;
            DateTime? res = (DateTime?)session.Evaluate<CashFlow>(CriteriaOperator.Parse("Max(TranDate)"),
                CriteriaOperator.Parse("Status = ?", CashFlowStatus.Actual));
            if (res == null)
                return default(DateTime);
            return (DateTime)res;
        }

        public void UploadToCashFlowForecast()
        {
            DeleteCashFlowForecasts();

            var maxActualDate = GetMaxActualTranDate();

            IEnumerable<CashFlow> ftPriQuery = GroupPrimaryForexTrades(maxActualDate);
            IEnumerable<CashFlow> ftCouQuery = GroupCounterForexTrades(maxActualDate);
    
            objectSpace.CommitChanges();
        }

        public IEnumerable<CashFlow> GroupPrimaryForexTrades(DateTime maxActualDate)
        {

            CriteriaOperator criteria = null;
            if (maxActualDate != null && maxActualDate != default(DateTime))
                criteria = CriteriaOperator.Parse("PrimarySettleDate > ?", maxActualDate);

            var ftQuery = objectSpace.GetObjects<ForexTrade>(criteria);

            return ftQuery
                .GroupBy(ft => new
                {
                    PrimarySettleDate = ft.PrimarySettleDate,
                    CounterCcy = ft.CounterCcy,
                    PrimarySettleAccount = ft.PrimarySettleAccount,
                    SettleGroupId = ft.SettleGroupId,
                    CashFlowCounterparty = ft.Counterparty.CashFlowCounterparty
                })
                .Select(grouped =>
                {
                    var cf = new CashFlow(((XPObjectSpace)objectSpace).Session,
                        false)
                    {
                        TranDate = grouped.Key.PrimarySettleDate,
                        CounterCcy = grouped.Key.CounterCcy,
                        Account = grouped.Key.PrimarySettleAccount,
                        Counterparty = grouped.Key.CashFlowCounterparty,
                        AccountCcyAmt = -grouped.Sum(s => s.PrimaryCcyAmt),
                        FunctionalCcyAmt = -grouped.Sum(s => s.PrimaryCcyAmt),
                        CounterCcyAmt = -grouped.Sum(s => s.CounterCcyAmt)
                    };
                    cf.PrimaryCashFlowForexTrades.AddRange(grouped.AsEnumerable<ForexTrade>());
                    return cf;
                })
                .ToArray(); // ToArray is required, otherwise the XPO association is not updated
        }


        public IEnumerable<CashFlow> GroupCounterForexTrades(DateTime maxActualDate)
        {
            CriteriaOperator criteria = null;
            if (maxActualDate != null && maxActualDate != default(DateTime))
                criteria = CriteriaOperator.Parse("CounterSettleDate > ?", maxActualDate);

            var ftQuery = objectSpace.GetObjects<ForexTrade>(criteria);
            return ftQuery
                .GroupBy(ft => new
                {
                    ft.CounterSettleDate,
                    ft.CounterCcy,
                    ft.CounterSettleAccount,
                    ft.SettleGroupId,
                    ft.Counterparty.CashFlowCounterparty
                })
                .Select(grouped =>
                {
                    var cf = new CashFlow(((XPObjectSpace)objectSpace).Session,
                        false)
                    {
                        TranDate = grouped.Key.CounterSettleDate,
                        CounterCcy = grouped.Key.CounterCcy,
                        Account = grouped.Key.CounterSettleAccount,
                        Counterparty = grouped.Key.CashFlowCounterparty,
                        CounterCcyAmt = grouped.Sum(s => s.CounterCcyAmt),
                        AccountCcyAmt = grouped.Sum(s => s.CounterCcyAmt),
                        FunctionalCcyAmt = grouped.Sum(s => s.PrimaryCcyAmt)
                    };
                    cf.CounterCashFlowForexTrades.AddRange(grouped.AsEnumerable<ForexTrade>());
                    return cf;
                })
                .ToArray(); // ToArray is required, otherwise the XPO association is not updated
        }

        public void DeleteCashFlowForecasts()
        {
            var source = objectSpace.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.ForexSettleCashFlowSource.Oid);
            var criteria = CriteriaOperator.Parse("Source = ? And Status = ?",
                source, CashFlowStatus.Forecast);
            var cashFlows = objectSpace.GetObjects<CashFlow>(criteria);
            objectSpace.Delete(cashFlows);
        }

    }
}
