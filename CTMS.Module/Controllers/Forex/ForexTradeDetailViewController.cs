using CTMS.Module.BusinessObjects.Forex;
using D2NXAF.ExpressApp.Editors;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;

namespace CTMS.Module.Controllers.Forex
{
    public class ForexTradeDetailViewController : ViewController<DetailView>
    {
        public ForexTradeDetailViewController()
        {
            TargetObjectType = typeof(ForexTrade);
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var viewItems = View.GetItems<IActionPropertyEditor>();
            foreach (var viewItem in viewItems)
            {
                var editor = ((PropertyEditor)viewItem);
                if (editor.Id == ForexTrade.FieldNames.CounterCcyAmt)
                    viewItem.ButtonClick += counterCcyAmtCalculator;
                else if (editor.Id == ForexTrade.FieldNames.PrimaryCcyAmt)
                    viewItem.ButtonClick += primaryCcyAmtCalculator;
                else if (editor.Id == ForexTrade.FieldNames.Rate)
                    viewItem.ButtonClick += rateCalculator;

            }
        }

        #region Action Property Calculators

        private void primaryCcyAmtCalculator(PropertyEditor sender, ActionPropertyClickEventArgs e)
        {
            var obj = (ForexTrade)View.CurrentObject;
            obj.UpdatePrimaryCcyAmt();
            sender.Refresh();
        }

        private void counterCcyAmtCalculator(PropertyEditor sender, ActionPropertyClickEventArgs e)
        {
            var obj = (ForexTrade)View.CurrentObject;
            obj.UpdateCounterCcyAmt();
            sender.Refresh();
        }

        private void rateCalculator(PropertyEditor sender, ActionPropertyClickEventArgs e)
        {
            var obj = (ForexTrade)View.CurrentObject;
            obj.UpdateRate();
            sender.Refresh();
        }

        #endregion
    }
}
