using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [BatchDelete(isVisible: true)]
    [ModelDefault("ImageName", "BO_List")]
    public class GlCostCentreActivityMap : BaseObject
    {
        public GlCostCentreActivityMap(Session session) : base(session) { }

        string _Code;
        [Size(255)]
        public string Code
        {
            get { return _Code; }
            set { SetPropertyValue("Code", ref _Code, value); }
        }

        string _Description;
        [Size(255)]
        public string Description
        {
            get { return _Description; }
            set { SetPropertyValue("Description", ref _Description, value); }
        }

        string _Dept;
        [Size(255)]
        public string Dept
        {
            get { return _Dept; }
            set { SetPropertyValue("Dept", ref _Dept, value); }
        }

        Activity _Activity;
        public Activity Activity
        {
            get { return _Activity; }
            set { SetPropertyValue("Activity", ref _Activity, value); }
        }

        string _FinActivity;
        [Size(255)]
        public string FinActivity
        {
            get { return _FinActivity; }
            set { SetPropertyValue("FinActivity", ref _FinActivity, value); }
        }
    }
}
