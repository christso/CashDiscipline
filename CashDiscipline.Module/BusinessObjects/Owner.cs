using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using System.ComponentModel;
using DevExpress.ExpressApp;

namespace CashDiscipline.Module.BusinessObjects
{
    [ImageName("BO_List")]
    [DefaultProperty("ShortName")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    public class Owner : BaseObject
    {
        public Owner(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private string _ShortName;

        public string ShortName
        {
            get
            {
                return _ShortName;
            }
            set
            {
                SetPropertyValue("ShortName", ref _ShortName, value);
            }
        }
    }
}

