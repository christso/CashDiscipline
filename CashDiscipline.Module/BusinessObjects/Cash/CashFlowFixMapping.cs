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
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.Interfaces;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    public class CashFlowFixMapping : BaseObject, IRowMoverObject, IMapping
    {
        private static int NextIndex = 1;

        public CashFlowFixMapping(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();

            object maxIndex = Session.Evaluate<CashFlowFixMapping>(CriteriaOperator.Parse("Max(RowIndex)"), null);
            if (maxIndex != null && maxIndex.GetType() == typeof(int))
            {
                if ((int)maxIndex >= NextIndex)
                    NextIndex = (int)maxIndex + 1;
            }
            this.RowIndex = NextIndex;
            this.MapStep = 1;
        }

        private int _RowIndex;
        [ModelDefault("DisplayFormat", "f0")]
        [ModelDefault("SortOrder", "Ascending")]
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

        private int _MapStep;
        public int MapStep
        {
            get
            {
                return _MapStep;
            }
            set
            {
                SetPropertyValue("MapStep", ref _MapStep, value);
            }
        }

        private string _CriteriaExpression;
        [VisibleInLookupListView(true)]
        [VisibleInListView(true)]
        [Size(SizeAttribute.Unlimited)]
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

        private CashFlowStatus _CriteriaStatus;
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

        private Activity _FixActivity;
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

        private CashForecastFixTag _Fix;
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

        private int _FixRank;
        public int FixRank
        {
            get
            {
                return _FixRank;
            }
            set
            {
                SetPropertyValue("FixRank", ref _FixRank, value);
            }
        }

        private string _FixFromDateExpr;
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

        private string _FixToDateExpr;
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

        private string _Description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                SetPropertyValue("Description", ref _Description, value);
            }
        }

        private CashFlowForexSettleType _ForexSettleType;
        public CashFlowForexSettleType ForexSettleType
        {
            get
            {
                return _ForexSettleType;
            }
            set
            {
                SetPropertyValue("ForexSettleType", ref _ForexSettleType, value);
            }
        }

    }
}
