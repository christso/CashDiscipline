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
using System.Data.SqlTypes;
using CashDiscipline.Module.Logic.SqlServer;
using CashDiscipline.Module.Attributes;
using Xafology.ExpressApp.Xpo.Import;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ModelDefault("ImageName", "BO_List")]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("DefaultListViewAllowEdit", "True")]
    [AutoColumnWidth(false)]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    public class AustPostSettle : BaseObject, IXpoImportable
    {
        public AustPostSettle(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

            var sqlUtil = new SqlQueryUtil(Session);
            DateTimeCreated = sqlUtil.GetDate();
        }

        private BankStmt _BankStmt;
        private decimal _GrossAmount;
        private decimal _DishonourChequeFee;
        private decimal _NegativeCorrections;
        private decimal _DishonourChequeReversal;
        private decimal _GrossCommission;
        private decimal _CommissionGST;


        [Association("BankStmt-AustPostSettle")]
        public BankStmt BankStmt
        {
            get
            {
                return _BankStmt;
            }
            set
            {
                if (SetPropertyValue("BankStmt", ref _BankStmt, value))
                {

                }
            }
        }

        private DateTime _BankStmtDate;

        [ModelDefault("AllowEdit", "false")]
        public DateTime BankStmtDate
        {
            get
            {
                if (BankStmt != null)
                    return BankStmt.TranDate;
                else
                    return SqlDateTime.MinValue.Value;
            }
            set
            {
                SetPropertyValue("BankStmtDate", ref _BankStmtDate, value);
            }
        }

        [ModelDefault("AllowEdit", "false")]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal BankStmtAmount
        {
            get
            {
                if (BankStmt != null)
                    return BankStmt.TranAmount;
                else
                    return 0.00M;
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal GrossAmount
        {
            get
            {
                return _GrossAmount;
            }
            set
            {
                SetPropertyValue("GrossAmount", ref _GrossAmount, value);
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal DishonourChequeFee
        {
            get
            {
                return _DishonourChequeFee;
            }
            set
            {
                SetPropertyValue("DishonourChequeFee", ref _DishonourChequeFee, value);
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal NegativeCorrections
        {
            get
            {
                return _NegativeCorrections;
            }
            set
            {
                SetPropertyValue("NegativeCorrections", ref _NegativeCorrections, value);
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal DishonourChequeReversal
        {
            get
            {
                return _DishonourChequeReversal;
            }
            set
            {
                SetPropertyValue("DishonourChequeReversal", ref _DishonourChequeReversal, value);
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal GrossCommission
        {
            get
            {
                return _GrossCommission;
            }
            set
            {
                if (SetPropertyValue("GrossCommission", ref _GrossCommission, value))
                {
                    OnChanged("NetCommission");
                }
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal CommissionGST
        {
            get
            {
                return _CommissionGST;
            }
            set
            {
                if (SetPropertyValue("CommissionGST", ref _CommissionGST, value))
                {
                    OnChanged("NetCommission");
                }
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("AllowEdit", "false")]
        public decimal NetCommission
        {
            get
            {
                return GrossCommission - CommissionGST;
            }
        }

        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal Difference
        {
            get
            {
                return BankStmtAmount - (GrossAmount - DishonourChequeFee - NegativeCorrections
                    - DishonourChequeReversal - GrossCommission);
            }
        }

        private DateTime _DateTimeCreated;
        [ModelDefault("DisplayFormat", "dd-MMM-yy hh:mm:ss")]
        public DateTime DateTimeCreated
        {
            get
            {
                return _DateTimeCreated;
            }
            set
            {
                SetPropertyValue("DateTimeCreated", ref _DateTimeCreated, value);
            }
        }

    }
}
