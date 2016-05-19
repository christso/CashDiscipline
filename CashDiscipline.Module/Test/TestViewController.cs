using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CashDiscipline.Module.BusinessObjects;
using DevExpress.Data.Filtering;
using Xafology.ExpressApp.Xpo.Import.Parameters;
using CashDiscipline.Module.BusinessObjects.Forex;

namespace CashDiscipline.Module.Test
{
    public class TestViewController : ViewController
    {
        private const string cashFlowChoiceCaption = "Cash Flow";
        private const string importParamChoiceCaption = "Import Param";
        private const string forexChoiceCaption = "Convert Currency";
        private const string pasteChoiceCaption = "Paste";

        public TestViewController()
        {
            this.Active["Test"] = true;

            var testDataAction = new SingleChoiceAction(this, "TestDataAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            testDataAction.ShowItemsOnClick = false;
            testDataAction.Caption = "Test";
            testDataAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            testDataAction.Execute += testAction_Execute;

            var cashFlowChoice = new ChoiceActionItem();
            cashFlowChoice.Caption = "Create Cash Flow Test Data";
            testDataAction.Items.Add(cashFlowChoice);

            var importParamChoice = new ChoiceActionItem();
            importParamChoice.Caption = "Create Import Param Test Data";
            testDataAction.Items.Add(importParamChoice);

            var forexChoice = new ChoiceActionItem();
            forexChoice.Caption = "Convert Currency";
            testDataAction.Items.Add(forexChoice);

            var pasteChoice = new ChoiceActionItem();
            pasteChoice.Caption = pasteChoiceCaption;
            testDataAction.Items.Add(pasteChoice);

            // active
            cashFlowChoice.Active["Test"] = false;
            importParamChoice.Active["Test"] = false;
            forexChoice.Active["Test"] = false;
            pasteChoice.Active["Test"] = true;
        }

        private void testAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case cashFlowChoiceCaption:
                    CreateCashFlows();
                    break;
                case importParamChoiceCaption:
                    CreateCashFlowImportParams();
                    break;
                case forexChoiceCaption:
                    ConvertCurrency();
                    break;
                case pasteChoiceCaption:
                    PasteTest();
                    break;
            }
        }

        protected virtual void PasteTest()
        {

        }

        private void ConvertCurrency()
        {
            var cf = (CashFlow)View.CurrentObject;
            if (cf.Account == null)
                throw new UserFriendlyException("Account must be specified");

            var functionalCurrency = ObjectSpace.GetObjectByKey<Currency>(SetOfBooks.CachedInstance.FunctionalCurrency.Oid);

            var rateObj = ForexRate.GetForexRateObject(((XPObjectSpace)ObjectSpace).Session, 
                cf.Account.Currency, functionalCurrency, (DateTime)cf.TranDate);

            string message = string.Format("rate = {0}", rateObj.ConversionRate);

            var messageBox = new Xafology.ExpressApp.SystemModule.GenericMessageBox(message);
        }

        public void CreateCashFlows()
        {
            var activity1 = ObjectSpace.FindObject<Activity>(CriteriaOperator.Parse(
            "Name = ?", "ANZ Bank Fee"));
            if (activity1 == null)
            {
                activity1 = ObjectSpace.CreateObject<Activity>();
                activity1.Name = "ANZ Bank Fee";
                activity1.ActivityL1 = ObjectSpace.CreateObject<ActivityTag>();
                activity1.ActivityL1.Text = "Non-Capex Payments";
                activity1.ActivityL2 = ObjectSpace.CreateObject<ActivityTag>();
                activity1.ActivityL2.Text = "OPEX";
                activity1.ActivityL3 = ObjectSpace.CreateObject<ActivityTag>();
                activity1.ActivityL3.Text = "Finance";
                activity1.Save();
            }

            var activity2 = ObjectSpace.FindObject<Activity>(CriteriaOperator.Parse(
                "Name = ?", "Interconnect Rcpt"));
            if (activity2 == null)
            {
                activity2 = ObjectSpace.CreateObject<Activity>();
                activity2.Name = "Interconnect Rcpt";
                activity2.ActivityL1 = ObjectSpace.CreateObject<ActivityTag>();
                activity2.ActivityL1.Text = "Receipts";
                activity2.ActivityL2 = ObjectSpace.CreateObject<ActivityTag>();
                activity2.ActivityL2.Text = "Interconnect & Roaming";
                activity2.ActivityL3 = ObjectSpace.CreateObject<ActivityTag>();
                activity2.ActivityL3.Text = "Interconnect";
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

        public void CreateCashFlowImportParams()
        {
            var map1 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map1.SourceName = "TranDate";
            map1.TargetName = map1.SourceName;

            var map2 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map2.SourceName = "Account";
            map2.TargetName = map2.SourceName;
            map2.CreateMember = true;

            var map3 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map3.SourceName = "Activity";
            map3.TargetName = map3.SourceName;
            map3.CreateMember = true;

            var map4 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map4.SourceName = "Counterparty";
            map4.TargetName = map4.SourceName;
            map4.CreateMember = true;

            var map5 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map5.SourceName = "CounterCcyAmt";
            map5.TargetName = map5.SourceName;

            var map6 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map6.SourceName = "CounterCcy";
            map6.TargetName = map6.SourceName;
            map6.CreateMember = true;

            var map7 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map7.SourceName = "Description";
            map7.TargetName = map7.SourceName;

            var map8 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map8.SourceName = "Source";
            map8.TargetName = map8.SourceName;
            map8.CreateMember = true;

            var param = ObjectSpace.CreateObject<ImportHeadersParam>();

            param.HeaderToFieldMaps.Add(map1);
            param.HeaderToFieldMaps.Add(map2);
            param.HeaderToFieldMaps.Add(map3);
            param.HeaderToFieldMaps.Add(map4);
            param.HeaderToFieldMaps.Add(map5);
            param.HeaderToFieldMaps.Add(map6);
            param.HeaderToFieldMaps.Add(map7);
            param.HeaderToFieldMaps.Add(map8);

            param.ObjectTypeName = "CashFlow";

            ObjectSpace.CommitChanges();
        }
    }
}