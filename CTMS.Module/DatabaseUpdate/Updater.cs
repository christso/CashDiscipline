using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Strategy;
using CTMS.Module.BusinessObjects;
using Artf = CTMS.Module.BusinessObjects.Artf;
using Cash = CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;
using System.Data.SqlClient;
using System.Diagnostics;
using DevExpress.Persistent.Base;
using CTMS.Module.ParamObjects.FinAccounting;
using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.HelperClasses;

namespace CTMS.Module.DatabaseUpdate
{
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) 
        {
        }

        private Version _PreviousDBVersion = null;

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            _PreviousDBVersion = CurrentDBVersion;

            base.UpdateDatabaseBeforeUpdateSchema();

            #region Database Schema
            #endregion

        }

        protected new void DropColumn(string tableName, string columnName)
        {
            this.ExecuteNonQueryCommand("ALTER TABLE " + tableName + " DROP COLUMN [" + columnName + "]", true);
        }
        protected new void DropConstraint(string tableName, string constraintName)
        {
            Tracing.Tracer.LogText("DropConstraint {0}, {1}", new object[] { tableName, constraintName });
            this.ExecuteNonQueryCommand("alter table " + tableName + " drop constraint " + constraintName, true);
        }

        // Set up the minimum objects for this application to function correctly
        public static void SetupObjects(IObjectSpace objSpace)
        {
            CreateCurrencies(objSpace); // prerequisite for the below
            InitSetOfBooks(objSpace);
            CreateFinAccountingDefaults(objSpace);
            CreateCashFlowDefaults(objSpace);
            StaticHelpers.GetInstance<CTMS.Module.ParamObjects.Cash.CashFlowFixParam>(objSpace);

        }

        public static void CreateFinAccountingDefaults(IObjectSpace objSpace)
        {
            StaticHelpers.GetInstance<FinGenJournalParam>(objSpace);
            StaticHelpers.GetInstance<FinAccountingDefaults>(objSpace);
        }

        public static void InitSetOfBooks(IObjectSpace objSpace)
        {
            var setOfBooks = SetOfBooks.GetInstance(objSpace); 

            if (setOfBooks.ForexSettleActivity == null)
            {
                var activity = objSpace.FindObject<Activity>(CriteriaOperator.Parse("Name = ?", "Transfer"));
                if (activity == null)
                {
                    activity = objSpace.CreateObject<Activity>();
                    activity.Name = "Transfer";
                    activity.Save();
                }
                setOfBooks.ForexSettleActivity = activity;
            }

            // requires CreateCurrencies
            setOfBooks.FunctionalCurrency = objSpace.FindObject<Currency>(
               CriteriaOperator.Parse(Currency.FieldNames.Name + " = ?", "AUD"));

            if (setOfBooks.ForexSettleCashFlowSource == null)
            {
                var source = objSpace.FindObject<CashFlowSource>(CriteriaOperator.Parse("Name = ?", "Fx Trade"));
                if (source == null)
                {
                    source = objSpace.CreateObject<CashFlowSource>();
                    source.Name = "Fx Trade";
                    source.Save();
                }
                setOfBooks.ForexSettleCashFlowSource = source;
            }
            if (setOfBooks.BankStmtCashFlowSource == null)
            {
                var source = objSpace.FindObject<CashFlowSource>(CriteriaOperator.Parse("Name = ?", "Stmt"));
                if (source == null)
                {
                    source = objSpace.CreateObject<CashFlowSource>();
                    source.Name = "Stmt";
                    source.Save();
                }
                setOfBooks.BankStmtCashFlowSource = source;
            }
            if (setOfBooks.CurrentCashFlowSnapshot == null)
            {
                var snapshot = objSpace.CreateObject<CashFlowSnapshot>();
                snapshot.Name = "Current";
                snapshot.Save();
                setOfBooks.CurrentCashFlowSnapshot = snapshot;
            }

            setOfBooks.CashFlowPivotLayoutName = Constants.CashFlowPivotLayoutMonthly;

            objSpace.CommitChanges();
        }

        public static void CreateCurrencies(IObjectSpace objSpace)
        {
            var ccy = objSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            if (ccy == null)
            {
                ccy = objSpace.CreateObject<Currency>();
                ccy.Name = "AUD";
                ccy.Save();
            }
            ccy = objSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));
            if (ccy == null)
            {
                ccy = objSpace.CreateObject<Currency>();
                ccy.Name = "USD";
                ccy.Save();
            }
        }

        public static void CreateCashFlowDefaults(IObjectSpace objSpace)
        {
            var counterparty = objSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "UNDEFINED"));
            if (counterparty == null)
            {
                counterparty = objSpace.CreateObject<Counterparty>();
                counterparty.Name = "UNDEFINED";
            }
            var activity = objSpace.FindObject<Activity>(CriteriaOperator.Parse("Name = ?", "UNDEFINED"));
            if (activity == null)
            {
                activity = objSpace.CreateObject<Activity>();
                activity.Name = "UNDEFINED";
            }
            var account = objSpace.FindObject<Account>(CriteriaOperator.Parse("Name = ?", "UNDEFINED"));
            if (account == null)
            {
                account = objSpace.CreateObject<Account>();
                account.Name = "UNDEFINED";
                account.Currency = objSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            }

            var cfDef = StaticHelpers.GetInstance<CashFlowDefaults>(objSpace);
            if (cfDef.Counterparty == null)
                cfDef.Counterparty = counterparty;
            if (cfDef.Activity == null)
                cfDef.Activity = activity;
            if (cfDef.Account == null)
                cfDef.Account = account;
        }

        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();

            SetupObjects(ObjectSpace);

#if TESTDATA
            #region Cash Flow
            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AP Pymt";
            activity.Dim_1_1 = ObjectSpace.CreateObject<ActivityTag>();
            activity.Dim_1_1.Text = "Non-Capex Payments";
            activity.Dim_1_2 = ObjectSpace.CreateObject<ActivityTag>();
            activity.Dim_1_2.Text = "Other OPEX";

            CashFlow cf;
            cf = ObjectSpace.CreateObject<CashFlow>();
            cf.InitDefaultValues();
            cf.TranDate = new DateTime(2014, 5, 6);
            cf.Activity = activity;
            cf.AccountCcyAmt = 1000;
            cf.FunctionalCcyAmt = 1000;
            cf.CounterCcyAmt = 1000;
            cf.Save();
            ObjectSpace.CommitChanges();

            var shot = CashFlow.SaveForecast((XPObjectSpace)ObjectSpace);

            #endregion
#endif
            #region Security System
            // Administrative role
            SecuritySystemRole adminRole = ObjectSpace.FindObject<SecuritySystemRole>(
               new BinaryOperator("Name", SecurityStrategy.AdministratorRoleName));
            if (adminRole == null)
            {
                adminRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                adminRole.Name = SecurityStrategy.AdministratorRoleName;
                adminRole.IsAdministrative = true;
            }
            // Administrator users
            SecuritySystemUser adminUser;
            adminUser = ObjectSpace.FindObject<SecuritySystemUser>(
                new BinaryOperator("UserName", "admin"));

            if (adminUser == null)
            {
                adminUser = ObjectSpace.CreateObject<SecuritySystemUser>();
                adminUser.UserName = "admin";
                adminUser.Roles.Add(adminRole);
            }

            // Accounts Receivable role
            SecuritySystemRole arRole = ObjectSpace.FindObject<SecuritySystemRole>(
                new BinaryOperator("Name", "Accounts Receivable"));

            if (arRole == null)
            {
                arRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                arRole.Name = "Accounts Receivable";
                arRole.IsAdministrative = false;
            }

            // Accounts Receivable Permissions

            // BankStmt
            GrantFullAcccess(typeof(Cash.BankStmt), arRole);
            GrantFullAcccess(typeof(Artf.ArtfRecon), arRole);
            GrantFullAcccess(typeof(Artf.ArtfCustomerType), arRole);
            GrantFullAcccess(typeof(Artf.ArtfGlCode), arRole);
            GrantFullAcccess(typeof(Artf.ArtfLedger), arRole);
            GrantFullAcccess(typeof(Artf.ArtfReceipt), arRole);
            GrantFullAcccess(typeof(Artf.ArtfSystem), arRole);
            GrantFullAcccess(typeof(Artf.ArtfTask), arRole);
            #endregion
      }

        // Grant full access if permission object not found for the owner
        // note that if you want to grant full access, you need to delete the existing permissions object
        private void GrantFullAcccess(Type targetType, SecuritySystemRole role)
        {
            var opArArtfRecon = CriteriaOperator.Parse("TargetType = ? And Owner = ?",
                targetType, role);
            SecuritySystemTypePermissionObject typePermission = ObjectSpace.FindObject<SecuritySystemTypePermissionObject>(opArArtfRecon);
            if (typePermission == null)
            {
                typePermission = ObjectSpace.CreateObject<SecuritySystemTypePermissionObject>();
                typePermission.TargetType = targetType;
                typePermission.AllowCreate = true;
                typePermission.AllowDelete = true;
                typePermission.AllowNavigate = true;
                typePermission.AllowRead = true;
                typePermission.AllowWrite = true;
                role.TypePermissions.Add(typePermission);
            }
        }
    }
}
