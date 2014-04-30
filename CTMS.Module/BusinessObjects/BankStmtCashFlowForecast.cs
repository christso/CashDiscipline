﻿using CTMS.Module.BusinessObjects.Cash;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.BusinessObjects
{
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
                return BankStmt.TranDate;
            }
        }

        public Account BsAccount
        {
            get
            {
                return BankStmt.Account;
            }
        }

        public Activity BsActivity
        {
            get
            {
                return BankStmt.Activity;
            }
        }

        public decimal BsTranAmount
        {
            get
            {
                return BankStmt.TranAmount;
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
                return CashFlow.TranDate;
            }
        }

        public Account CfAccount
        {
            get
            {
                return CashFlow.Account;
            }
        }

        public Activity CfActivity
        {
            get
            {
                return CashFlow.Activity;
            }
        }

        public decimal CfTranAmount
        {
            get
            {
                return CashFlow.AccountCcyAmt;
            }
        }

    }
}
