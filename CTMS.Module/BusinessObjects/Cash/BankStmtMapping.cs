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

namespace CTMS.Module.BusinessObjects.Cash
{
    public class BankStmtMapping : BaseObject, IMappingObject
    {
        public BankStmtMapping(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
            object maxIndex = Session.Evaluate<BankStmtMapping>(CriteriaOperator.Parse("Max(Index)"), null);
            if (maxIndex != null)
            {
                if ((int)maxIndex >= NextIndex)
                    NextIndex = (int)maxIndex + 1;
            }
            this.Index = NextIndex;
        }

        // singleton
        private static int NextIndex = 1;

        // Fields...
        private Account _Account;
        private Activity _Activity;
        private string _CriteriaExpression;
        private int _Index;

        [ModelDefault("DisplayFormat","f0")]
        [ModelDefault("SortOrder","Ascending")]
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                SetPropertyValue("Index", ref _Index, value);
            }
        }


        public string CriteriaExpression
        {
            get
            {
                return _CriteriaExpression;
            }
            set
            {
                SetPropertyValue("CriteriaExpression", ref _CriteriaExpression, value);
            }
        }

        public Activity Activity
        {
            get
            {
                return _Activity;
            }
            set
            {
                SetPropertyValue("Activity", ref _Activity, value);
            }
        }

        public Account Account
        {
            get
            {
                return _Account;
            }
            set
            {
                SetPropertyValue("Account", ref _Account, value);
            }
        }
    }
}
