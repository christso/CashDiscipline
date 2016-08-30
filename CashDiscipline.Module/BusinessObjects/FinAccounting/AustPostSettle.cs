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

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ModelDefault("ImageName", "BO_List")]
    public class AustPostSettle : BaseObject
    {
        public AustPostSettle(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
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

        [Persistent("BankStmtDate")]
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
        }

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

        [ModelDefault("AllowEdit", "false")]
        public decimal NetCommission
        {
            get
            {
                return GrossCommission - CommissionGST;
            }
        }

    }
}
