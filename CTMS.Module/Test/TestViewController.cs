#if TESTDATA
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CTMS.Module.Test
{
    public class TestViewController : ViewController
    {
        public TestViewController()
        {
            var testDataAction = new SingleChoiceAction(this, "TestDataAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            testDataAction.ShowItemsOnClick = false;
            testDataAction.Caption = "Test Data";
            testDataAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            testDataAction.Execute += testAction_Execute;

            var cashFlowChoice = new ChoiceActionItem();
            cashFlowChoice.Caption = "Cash Flow";
            testDataAction.Items.Add(cashFlowChoice);
        }

        void testAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AP Pymt";
            activity.Dim_1_1 = ObjectSpace.CreateObject<ActivityTag>();
            activity.Dim_1_1.Text = "Non-Capex Payments";
            activity.Dim_1_2 = ObjectSpace.CreateObject<ActivityTag>();
            activity.Dim_1_2.Text = "Other OPEX";
            activity.Save();

            CashFlow cf;
            cf = ObjectSpace.CreateObject<CashFlow>();
            cf.InitDefaultValues();
            cf.TranDate = new DateTime(2014, 5, 6);
            cf.Activity = activity;
            cf.AccountCcyAmt = 1000;
            cf.FunctionalCcyAmt = 1000;
            cf.CounterCcyAmt = 1000;
            cf.Save();

            var shot = CashFlow.SaveForecast((XPObjectSpace)ObjectSpace, false);
        }
    }
}
#endif