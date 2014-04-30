using CTMS.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers
{
    public class CalculateToggleViewController : ViewController
    {
        public CalculateToggleViewController()
        {
            TargetObjectType = typeof(ICalculateToggleObject);

            calculateAction = new SingleChoiceAction(this, "CalculateToggleAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            calculateAction.ItemType = SingleChoiceActionItemType.ItemIsMode;
            calculateAction.Caption = "";
            calculateAction.Execute += calculateAction_Execute;

            calculateOnChoice = new ChoiceActionItem();
            calculateOnChoice.Caption = "Calculate ON";
            calculateAction.Items.Add(calculateOnChoice);

            calculateOffChoice = new ChoiceActionItem();
            calculateOffChoice.Caption = "Calculate OFF";
            calculateAction.Items.Add(calculateOffChoice);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            calculateAction.SelectedItem = calculateOnChoice;

            View.SelectionChanged += View_SelectionChanged;
        }

        void View_SelectionChanged(object sender, EventArgs e)
        {
            var objs = View.SelectedObjects;
            foreach (ICalculateToggleObject obj in objs)
            {
                obj.CalculateEnabled = true;
            }
        }

        private SingleChoiceAction calculateAction;
        private ChoiceActionItem calculateOnChoice;
        private ChoiceActionItem calculateOffChoice;

        void calculateAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var obj = View.CurrentObject as ICalculateToggleObject;
            if (obj == null) return;
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case CalculateEnabledCaption.Off:
                    obj.CalculateEnabled = false;
                    break;
                case CalculateEnabledCaption.On:
                    obj.CalculateEnabled = true;
                    break;
            }
        }
        private struct CalculateEnabledCaption
        {
            public const string On = "Calculate ON";
            public const string Off = "Calculate OFF";
        }

    }
}
