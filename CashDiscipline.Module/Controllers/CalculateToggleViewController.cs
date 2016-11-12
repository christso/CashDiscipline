using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers
{
    public class CalculateToggleViewController : ViewController
    {
        public CalculateToggleViewController()
        {
            TargetObjectType = typeof(ICalculateToggleObject);

            CalculateAction = new SingleChoiceAction(this, "CalculateToggleAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            CalculateAction.ItemType = SingleChoiceActionItemType.ItemIsMode;
            CalculateAction.Caption = "";
            CalculateAction.Execute += calculateAction_Execute;

            calculateOnChoice = new ChoiceActionItem();
            calculateOnChoice.Caption = "Calculate ON";
            CalculateAction.Items.Add(calculateOnChoice);

            calculateOffChoice = new ChoiceActionItem();
            calculateOffChoice.Caption = "Calculate OFF";
            CalculateAction.Items.Add(calculateOffChoice);
        }

        public SingleChoiceAction CalculateAction;
        private ChoiceActionItem calculateOnChoice;
        private ChoiceActionItem calculateOffChoice;
        private bool IsCalculateEnabled
        {
            get
            {
                return CalculateAction.SelectedItem == calculateOnChoice;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            CalculateAction.SelectedItem = calculateOnChoice;

            View.SelectionChanged += View_SelectionChanged;
        }

        void View_SelectionChanged(object sender, EventArgs e)
        {
            var objs = View.SelectedObjects;
            foreach (ICalculateToggleObject obj in objs)
            {
                obj.CalculateEnabled = IsCalculateEnabled;
            }
        }

        void calculateAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var objs = View.SelectedObjects;
            
            foreach (ICalculateToggleObject obj in objs)
            {
                obj.CalculateEnabled = IsCalculateEnabled;
            }
        }
        private struct CalculateEnabledCaption
        {
            public const string On = "Calculate ON";
            public const string Off = "Calculate OFF";
        }

    }
}
