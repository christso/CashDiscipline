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
using CTMS.Module.BusinessObjects;
using DevExpress.Data.Filtering;

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
            var activity1 = ObjectSpace.FindObject<Activity>(CriteriaOperator.Parse(
                "Name = ?", "ANZ Bank Fee"));
            if (activity1 == null)
            {
                activity1 = ObjectSpace.CreateObject<Activity>();
                activity1.Name = "ANZ Bank Fee";
                activity1.Dim_1_1 = ObjectSpace.CreateObject<ActivityTag>();
                activity1.Dim_1_1.Text = "Non-Capex Payments";
                activity1.Dim_1_2 = ObjectSpace.CreateObject<ActivityTag>();
                activity1.Dim_1_2.Text = "OPEX";
                activity1.Dim_1_3 = ObjectSpace.CreateObject<ActivityTag>();
                activity1.Dim_1_3.Text = "Finance";
                activity1.Save();
            }

            var activity2 = ObjectSpace.FindObject<Activity>(CriteriaOperator.Parse(
                "Name = ?", "Interconnect Rcpt"));
            if (activity2 == null)
            {
                activity2 = ObjectSpace.CreateObject<Activity>();
                activity2.Name = "Interconnect Rcpt";
                activity2.Dim_1_1 = ObjectSpace.CreateObject<ActivityTag>();
                activity2.Dim_1_1.Text = "Receipts";
                activity2.Dim_1_2 = ObjectSpace.CreateObject<ActivityTag>();
                activity2.Dim_1_2.Text = "Interconnect & Roaming";
                activity2.Dim_1_3 = ObjectSpace.CreateObject<ActivityTag>();
                activity2.Dim_1_3.Text = "Interconnect";
                activity2.Save();
            }

            CashFlow cf;
            cf = ObjectSpace.CreateObject<CashFlow>();
            cf.InitDefaultValues();
            cf.TranDate = new DateTime(2014, 5, 6);
            cf.Account = ObjectSpace.FindObject<Account>(null);
            cf.Counterparty = ObjectSpace.FindObject<Counterparty>(null);
            cf.Activity = activity1;
            cf.AccountCcyAmt = -1000;
            cf.FunctionalCcyAmt = -1000;
            cf.CounterCcyAmt = -1000;
            cf.Save();

            cf = ObjectSpace.CreateObject<CashFlow>();
            cf.InitDefaultValues();
            cf.TranDate = new DateTime(2014, 5, 6);
            cf.Account = ObjectSpace.FindObject<Account>(null);
            cf.Counterparty = ObjectSpace.FindObject<Counterparty>(null);
            cf.Activity = activity2;
            cf.AccountCcyAmt = 1000;
            cf.FunctionalCcyAmt = 1000;
            cf.CounterCcyAmt = 1000;
            cf.Save();

            ObjectSpace.CommitChanges();
            var shot = CashFlow.SaveForecast((XPObjectSpace)ObjectSpace);
        }
    }
}
#endif