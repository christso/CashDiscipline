using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;


namespace CashDiscipline.Module.ParamObjects.Import
{
    [ModelDefault("ImageName", "BO_List")]
    [NavigationItem("Import")]
    [ModelDefault("IsFooterVisible", "True")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    public class ImportBankStmtParamItem : BaseObject
    {
        public ImportBankStmtParamItem(Session session) : base(session)
        {
            session.LockingOption = LockingOption.None;
        }

        private bool _Enabled;
        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                SetPropertyValue("Enabled", ref _Enabled, value);
            }
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

        private string _PackageName;
        public string PackageName
        {
            get
            {
                return _PackageName;
            }
            set
            {
                SetPropertyValue("PackageName", ref _PackageName, value);
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

        private ImportBankStmtParam importBankStmtParam;
        [Association("ImportBankStmtParam-ImportBankStmtParamItems")]
        public ImportBankStmtParam ImportBankStmtParam
        {
            get { return importBankStmtParam; }
            set { SetPropertyValue("ImportBankStmtParam", ref importBankStmtParam, value); }
        }
    }
}
