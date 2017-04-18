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
using Xafology.ExpressApp.SystemModule;

namespace CashDiscipline.Module.ParamObjects.Import
{
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [ModelDefault("ImageName", "BO_List")]
    [NavigationItem("Import")]
    public class ImportApPmtDistnParam : BaseObject
    {
        public ImportApPmtDistnParam(Session session) : base(session)
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


        [Association("ImportApPmtDistnParam-ImportApPmtDistnColumns")]
        public XPCollection<ImportApPmtDistnColumn> ImportApPmtDistnColumns
        {
            get
            {
                return GetCollection<ImportApPmtDistnColumn>("ImportApPmtDistnColumns");
            }
        }

        public static ImportApPmtDistnParam GetInstance(IObjectSpace objectSpace)
        {
            return BaseObjectHelper.GetInstance<ImportApPmtDistnParam>(objectSpace);
        }
    }
}
