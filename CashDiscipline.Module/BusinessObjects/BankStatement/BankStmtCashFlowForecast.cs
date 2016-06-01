using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.BusinessObjects.BankStatement
{
    [ImageName("BO_List")]
    public class BankStmtCashFlowForecast : BaseObject
    {
        public BankStmtCashFlowForecast()
        {
            
        }
        public BankStmtCashFlowForecast(Session session)
            : base(session)
        {
            
        }

        private BankStmt _BankStmt;
        public BankStmt BankStmt
        {
            get
            {
                return _BankStmt;
            }
            set
            {
                SetPropertyValue("BankStmt", ref _BankStmt, value);
            }
        }

        public DateTime BsTranDate
        {
            get
            {
                return BankStmt == null ? default(DateTime) : BankStmt.TranDate;
            }
        }

        public Account BsAccount
        {
            get
            {
                return BankStmt == null ? null : BankStmt.Account;
            }
        }

        public Activity BsActivity
        {
            get
            {
                return BankStmt == null ? null : BankStmt.Activity;
            }
        }

        public decimal BsTranAmount
        {
            get
            {
                return BankStmt == null ? 0 : BankStmt.TranAmount;
            }
        }

        private CashFlow _CashFlow;
        public CashFlow CashFlow
        {
            get
            {
                return _CashFlow;
            }
            set
            {
                SetPropertyValue("CashFlow", ref _CashFlow, value);
            }
        }

        public DateTime CfTranDate
        {
            get
            {
                return CashFlow == null ? default(DateTime) : CashFlow.TranDate;
            }
        }

        public Account CfAccount
        {
            get
            {
                return CashFlow == null ? null : CashFlow.Account;
            }
        }

        public Activity CfActivity
        {
            get
            {
                return CashFlow == null ? null : CashFlow.Activity;
            }
        }

        public decimal CfTranAmount
        {
            get
            {
                return CashFlow == null ? 0 : CashFlow.AccountCcyAmt;
            }
        }

    }
}
