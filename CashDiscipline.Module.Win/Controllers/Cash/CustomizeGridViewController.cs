using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Win.Editors;
using System;

namespace CashDiscipline.Module.Win.Controllers.Cash
{
    public class CustomizeGridViewController : ViewController
    {
        public CustomizeGridViewController()
        {
            TargetViewType = ViewType.ListView;
        }

        private bool IsTargetObject
        {
            get
            {
                return (View.ObjectTypeInfo.Type == typeof(CashFlow)
                || View.ObjectTypeInfo.Type == typeof(ForexTrade));
            }
        }

        protected override void OnActivated()
        {
            if (!IsTargetObject) return;
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            View.ControlsCreated -= View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            if (((ListView)View).Editor is GridListEditor)
            {
                ((GridListEditor)((ListView)View).Editor).GridView.OptionsView.ColumnAutoWidth = false;
            }
        }
    }
}
