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
using Xafology.ExpressApp.RowMover;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [ModelDefault("ImageName", "BO_List")]
    public class BankStmtMapping : BaseObject, IRowMoverObject
    {
        public BankStmtMapping(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            object maxIndex = Session.Evaluate<BankStmtMapping>(CriteriaOperator.Parse("Max(RowIndex)"), null);
            if (maxIndex != null)
            {
                if ((int)maxIndex >= NextIndex)
                    NextIndex = (int)maxIndex + 1;
            }
            this.RowIndex = NextIndex;
        }

        // singleton
        private static int NextIndex = 1;

        // Fields...
        private Account _Account;
        private Activity _Activity;
        private string _CriteriaExpression;
        private int _RowIndex;

        [ModelDefault("DisplayFormat","f0")]
        [ModelDefault("SortOrder","Ascending")]
        public int RowIndex
        {
            get
            {
                return _RowIndex;
            }
            set
            {
                SetPropertyValue("RowIndex", ref _RowIndex, value);
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
