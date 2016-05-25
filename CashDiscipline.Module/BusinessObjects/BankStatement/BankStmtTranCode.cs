using System;
using System.ComponentModel;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Model;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.ParamObjects.Cash;
using System.Linq;
using System.Collections.Generic;

namespace CashDiscipline.Module.BusinessObjects.BankStatement
{
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    [NavigationItem("Cash Setup")]
    public class BankStmtTranCode : BaseObject
    {
        public BankStmtTranCode(Session session) : base(session)
        {
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                SetPropertyValue("Name", ref _Name, value);
            }
        }

        private string _Description;
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                SetPropertyValue("Description", ref _Description, value);
            }
        }
    }
}
