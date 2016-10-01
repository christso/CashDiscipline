using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;

namespace CashDiscipline.Module.BusinessObjects.BankStatement
{
    [ModelDefault("ImageName", "BO_List")]
    [NavigationItem("Cash Setup")]
    public class BankStmtDefaults : BaseObject
    {
        public BankStmtDefaults(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        private static BankStmtDefaults _CachedInstance;
        public static BankStmtDefaults CachedInstance
        {
            get
            {
                return _CachedInstance;
            }
            set
            {
                _CachedInstance = value;
            }
        }

        private BankStmtTranCode _TranCode;

        public BankStmtTranCode TranCode
        {
            get
            {
                return _TranCode;
            }
            set
            {
                SetPropertyValue("TranCode", ref _TranCode, value);
            }
        }

        public static BankStmtDefaults GetInstance(IObjectSpace objectSpace)
        {
            return BankStmtDefaults.GetInstance(((XPObjectSpace)objectSpace).Session);
        }

        public static BankStmtDefaults GetInstance(Session session)
        {
            CachedInstance = (BankStmtDefaults)BaseObjectHelper.PullCachedInstance(
                session, CachedInstance, typeof(BankStmtDefaults));
            return CachedInstance;
        }

        protected override void OnSaving()
        {
            var objs = new XPCollection<SetOfBooks>(PersistentCriteriaEvaluationBehavior.InTransaction, Session, null);
            if (objs.Count > 1)
                throw new InvalidOperationException("You cannot create more than one BankStmtDefaults instance");
            base.OnSaving();
        }
    }
}
