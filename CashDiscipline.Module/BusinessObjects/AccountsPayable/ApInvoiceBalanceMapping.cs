using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Interfaces;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.RowMover;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [ModelDefault("IsCloneable", "True")]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    public class ApInvoiceBalanceMapping : BaseObject, IRowMoverObject, IMapping, IMappingCriteriaGenerator
    {
        public ApInvoiceBalanceMapping(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            object maxIndex = Session.Evaluate<ApInvoiceBalanceMapping>(CriteriaOperator.Parse("Max(RowIndex)"), null);
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

        [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
        [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
        [MemberDesignTimeVisibility(false)]
        public Type CriteriaObjectType
        {
            get { return typeof(ApInvoiceBalance); }
        }

        private string _Criteria;
        [CriteriaOptions("CriteriaObjectType"), Size(SizeAttribute.Unlimited)]
        [EditorAlias(EditorAliases.PopupCriteriaPropertyEditor)]
        public string Criteria
        {
            get { return _Criteria; }
            set { SetPropertyValue("Criteria", ref _Criteria, value); }
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

        private string _ActivityExpr;
        public string ActivityExpr
        {
            get
            {
                return _ActivityExpr;
            }
            set
            {
                SetPropertyValue("ActivityExpr", ref _ActivityExpr, value);
            }
        }


    }
}
