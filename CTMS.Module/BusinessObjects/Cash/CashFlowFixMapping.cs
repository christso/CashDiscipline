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
    public class CashFlowFixMapping : BaseObject, IMappingObject
    {
        public CashFlowFixMapping(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
            object maxIndex = Session.Evaluate<CashFlowFixMapping>(CriteriaOperator.Parse("Max(Index)"), null);
            if (maxIndex != null)
            {
                if ((int)maxIndex >= NextIndex)
                    NextIndex = (int)maxIndex + 1;
            }
            this.Index = NextIndex;
        }

        // singleton
        private string _FixToDateExpr;
        private string _FixFromDateExpr;
        private CashFlowStatus _CriteriaStatus;
        private CashForecastFixTag _Fix;
        private static int NextIndex = 1;

        // Fields...
        private Activity _FixActivity;
        private string _CriteriaExpression;
        private int _Index;

        [ModelDefault("DisplayFormat", "f0")]
        [ModelDefault("SortOrder", "Ascending")]
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

        public CashFlowStatus CriteriaStatus
        {
            get
            {
                return _CriteriaStatus;
            }
            set
            {
                SetPropertyValue("CriteriaStatus", ref _CriteriaStatus, value);
            }
        }

        public Activity FixActivity
        {
            get
            {
                return _FixActivity;
            }
            set
            {
                SetPropertyValue("FixActivity", ref _FixActivity, value);
            }
        }


        public CashForecastFixTag Fix
        {
            get
            {
                return _Fix;
            }
            set
            {
                SetPropertyValue("Fix", ref _Fix, value);
            }
        }


        public string FixFromDateExpr
        {
            get
            {
                return _FixFromDateExpr;
            }
            set
            {
                SetPropertyValue("FixFromDateExpr", ref _FixFromDateExpr, value);
            }
        }


        public string FixToDateExpr
        {
            get
            {
                return _FixToDateExpr;
            }
            set
            {
                SetPropertyValue("FixToDateExpr", ref _FixToDateExpr, value);
            }
        }
    }
}
