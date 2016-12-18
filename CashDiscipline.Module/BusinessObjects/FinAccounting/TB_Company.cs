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
    public class TB_Company : BaseObject
    {
        public TB_Company(Session session) : base(session)
        {

        }

        string fCompany;
        [Size(10)]
        public string Company
        {
            get { return fCompany; }
            set { SetPropertyValue<string>("Company", ref fCompany, value); }
        }

        string fParentCompany;
        [Size(50)]
        public string ParentCompany
        {
            get { return fParentCompany; }
            set { SetPropertyValue<string>("ParentCompany", ref fParentCompany, value); }
        }
    }
}
