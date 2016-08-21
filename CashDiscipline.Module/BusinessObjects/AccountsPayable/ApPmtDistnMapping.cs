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
using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.Interfaces;
using CashDiscipline.Module.BusinessObjects.Cash;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [ModelDefault("IsCloneable", "True")]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    [NavigationItem("Cash Setup")]
    public class ApPmtDistnMapping : BaseObject, IRowMoverObject, IMapping
    {
        public ApPmtDistnMapping(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            object maxIndex = Session.Evaluate<ApPmtDistnMapping>(CriteriaOperator.Parse("Max(RowIndex)"), null);
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

        private string _CriteriaExpression;
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

        private Activity _Activity;
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

        private Counterparty _Counterparty;
        public Counterparty Counterparty
        {
            get
            {
                return _Counterparty;
            }
            set
            {
                SetPropertyValue("Counterparty", ref _Counterparty, value);
            }
        }
    }
}
