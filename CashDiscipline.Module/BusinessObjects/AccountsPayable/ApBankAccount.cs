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
using CashDiscipline.Module.Attributes;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [ModelDefault("IsCloneable", "True")]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    [NavigationItem("Cash Setup")]
    public class ApBankAccount : BaseObject
    {
        public ApBankAccount(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
        // Fields...

        private string _BankAccountName;
        public string BankAccountName
        {
            get
            {
                return _BankAccountName;
            }
            set
            {
                SetPropertyValue("BankAccountName", ref _BankAccountName, value);
            }
        }

        private Account _Account;
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
