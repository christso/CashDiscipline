using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Forex
{
    public class ForexTradeLogic
    {
        public ForexTradeLogic(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        private XPObjectSpace objSpace;

        public static ForexTrade Initialize(ForexTrade origFt)
        {
            ForexTrade newFt = ForexTradeLogic.CloneForexTrade(origFt);
            Initialize(origFt, newFt);
            return newFt;
        }

        public static ForexTrade Initialize(ForexTrade origFt, ForexTrade newFt)
        {
            newFt.EventType = ForexEventType.Predeliver;
            newFt.CreationDate = (DateTime)newFt.Session.ExecuteScalar("SELECT GETDATE()");
            newFt.TradeDate = newFt.CreationDate;

            ForexTrade revFt = ForexTradeLogic.CloneForexTrade(origFt);
            revFt.Rate = origFt.Rate;
            revFt.CreationDate = newFt.CreationDate;
            revFt.TradeDate = revFt.CreationDate;

            newFt.ReverseTrade = revFt;
            UpdateReverseTrade(newFt);
            return newFt;
        }

        public static void UpdateReverseTrade(ForexTrade newFt)
        {
            if (newFt.ReverseTrade == null) return;
            newFt.ReverseTrade.CounterCcyAmt = -newFt.CounterCcyAmt;
            newFt.ReverseTrade.PrimaryCcyAmt = -newFt.PrimaryCcyAmt;
        }

        public static ForexTrade CloneForexTrade(ForexTrade fromFt)
        {
            Cloner cloner = new Cloner();
            var amendFt = (ForexTrade)cloner.CloneTo(fromFt, typeof(ForexTrade));
            amendFt.EventType = ForexEventType.Predeliver;
            amendFt.OrigTrade = fromFt;
            return amendFt;
        }
    }
}
