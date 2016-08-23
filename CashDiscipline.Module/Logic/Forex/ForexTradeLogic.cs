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

        public ForexTradePredelivery Predeliver(
            ForexTrade ft,
            decimal counterCcyAmt, DateTime valueDate, decimal rate)
        {
            var pdy = objSpace.CreateObject<ForexTradePredelivery>();

            pdy.FromForexTrade = ft;
            pdy.CounterCcyAmt = counterCcyAmt;
            pdy.ValueDate = valueDate;
            pdy.Rate = rate;

            pdy.AmendForexTrade = ForexTradeLogic.CloneForexTrade(pdy.FromForexTrade);
            pdy.AmendForexTrade.CounterCcyAmt *= -1;

            pdy.ToForexTrade = ForexTradeLogic.CloneForexTrade(pdy.FromForexTrade);
            pdy.ToForexTrade.Rate = rate;
            pdy.ToForexTrade.TradeDate = pdy.TradeDate;
            pdy.ToForexTrade.ValueDate = pdy.ValueDate;

            return pdy;
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
