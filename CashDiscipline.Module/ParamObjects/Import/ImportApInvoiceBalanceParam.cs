using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.ParamObjects.Import
{
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [ModelDefault("ImageName", "BO_List")]
    [NavigationItem("Import")]
    public class ImportApInvoiceBalanceParam : BaseObject
    {
        public ImportApInvoiceBalanceParam(Session session) : base(session)
        {
            session.LockingOption = LockingOption.None;
        }

        private string _FilePath;
        [Size(SizeAttribute.Unlimited)]
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                SetPropertyValue("FilePath", ref _FilePath, value);
            }
        }

        private DateTime _FromDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime FromDate
        {
            get
            {
                return _FromDate;
            }
            set
            {
                SetPropertyValue("FromDate", ref _FromDate, value);
            }
        }

        private DateTime _ToDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime ToDate
        {
            get
            {
                return _ToDate;
            }
            set
            {
                SetPropertyValue("ToDate", ref _ToDate, value);
            }
        }

        private string _CreateSql;
        [Size(SizeAttribute.Unlimited)]
        public string CreateSql
        {
            get
            {
                return _CreateSql;
            }
            set
            {
                SetPropertyValue("CreateSql", ref _CreateSql, value);
            }
        }

        private string _PersistSql;
        [Size(SizeAttribute.Unlimited)]
        public string PersistSql
        {
            get
            {
                return _PersistSql;
            }
            set
            {
                SetPropertyValue("PersistSql", ref _PersistSql, value);
            }
        }

        public static ImportApInvoiceBalanceParam GetInstance(IObjectSpace objectSpace)
        {
            return BaseObjectHelper.GetInstance<ImportApInvoiceBalanceParam>(objectSpace);
        }
    }
}
