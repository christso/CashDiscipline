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

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    public class BankStmtAccountImportMapping : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public BankStmtAccountImportMapping(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        // Fields...
        private Account _Account;
        private string _InputAccountText;

        public string InputAccountText
        {
            get
            {
                return _InputAccountText;
            }
            set
            {
                SetPropertyValue("InputAccountText", ref _InputAccountText, value);
            }
        }

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
