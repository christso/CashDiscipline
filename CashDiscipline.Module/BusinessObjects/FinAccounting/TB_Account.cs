using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    [DeferredDeletion(false)]
    public class TB_Account : BaseObject
    {
        public TB_Account(Session session) : base(session)
        {

        }

        string fAccount;
        [Size(10)]
        public string Account
        {
            get { return fAccount; }
            set { SetPropertyValue<string>("Account", ref fAccount, value); }
        }

        string fModelL1;
        [Size(50)]
        public string ModelL1
        {
            get { return fModelL1; }
            set { SetPropertyValue<string>("ModelL1", ref fModelL1, value); }
        }
    }
}
