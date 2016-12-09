using CashDiscipline.Module.Attributes;
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
    [DefaultProperty("PaymentDate")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [ModelDefault("ImageName", "BO_List")]
    [BatchDelete(isVisible: true)]
    [DeferredDeletion(false)]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    //[Persistent("Treasury.dbo.ApPmtDtParam")]
    public class ApPmtDateSched : BaseObject
    {
        public ApPmtDateSched(Session session)
            : base(session)
        {
        }

        DateTime _PaymentDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime PaymentDate
        {
            get { return _PaymentDate; }
            set { SetPropertyValue<DateTime>("PaymentDate", ref _PaymentDate, value); }
        }


        DateTime _StartDueDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime StartDueDate
        {
            get { return _StartDueDate; }
            set { SetPropertyValue<DateTime>("StartDueDate", ref _StartDueDate, value); }
        }

        DateTime _EndDueDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime EndDueDate
        {
            get { return _EndDueDate; }
            set { SetPropertyValue<DateTime>("EndDueDate", ref _EndDueDate, value); }
        }

    }
}
