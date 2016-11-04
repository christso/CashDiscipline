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
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Utils;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
    [NavigationItem("Cash Setup")]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    public class BankStmtMapping : BaseObject, IRowMoverObject, IMapping, IMappingCriteriaGenerator
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

        [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
        [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
        [MemberDesignTimeVisibility(false)]
        public Type CriteriaObjectType
        {
            get { return typeof(BankStmt); }
        }

        private string _Criteria;
        [CriteriaOptions("CriteriaObjectType"), Size(SizeAttribute.Unlimited)]
        [EditorAlias(EditorAliases.PopupCriteriaPropertyEditor)]
        public string Criteria
        {
            get { return _Criteria; }
            set { SetPropertyValue("Criteria", ref _Criteria, value); }
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

        private ActionOwner _ActionOwner;
        public ActionOwner ActionOwner
        {
            get
            {
                return _ActionOwner;
            }
            set
            {
                SetPropertyValue("ActionOwner", ref _ActionOwner, value);
            }
        }

   
    }
}
