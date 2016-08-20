using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;


namespace CashDiscipline.Module.ParamObjects.Import
{
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultClassOptions]
    public class ImportBankStmtParam : BaseObject
    {
        public ImportBankStmtParam(Session session) : base(session)
        {
            session.LockingOption = LockingOption.None;
        }

        private bool _AnzEnabled;
        public bool AnzEnabled
        {
            get
            {
                return _AnzEnabled;
            }
            set
            {
                SetPropertyValue("AnzEnabled", ref _AnzEnabled, value);
            }
        }

        private string _AnzFilePath;
        public string AnzFilePath
        {
            get
            {
                return _AnzFilePath;
            }
            set
            {
                SetPropertyValue("AnzFilePath", ref _AnzFilePath, value);
            }
        }

        private bool _WbcEnabled;
        public bool WbcEnabled
        {
            get
            {
                return _WbcEnabled;
            }
            set
            {
                SetPropertyValue("WbcEnabled", ref _WbcEnabled, value);
            }
        }

        private string _WbcFilePath;
        public string WbcFilePath
        {
            get
            {
                return _WbcFilePath;
            }
            set
            {
                SetPropertyValue("WbcFilePath", ref _WbcFilePath, value);
            }
        }


        private bool _CbaBosEnabled;
        public bool CbaBosEnabled
        {
            get
            {
                return _CbaBosEnabled;
            }
            set
            {
                SetPropertyValue("CbaBosEnabled", ref _CbaBosEnabled, value);
            }
        }

        private string _CbaBosFilePath;
        public string CbaBosFilePath
        {
            get
            {
                return _CbaBosFilePath;
            }
            set
            {
                SetPropertyValue("CbaBosFilePath", ref _CbaBosFilePath, value);
            }
        }

        private bool _CbaOpEnabled;
        public bool CbaOpEnabled
        {
            get
            {
                return _CbaOpEnabled;
            }
            set
            {
                SetPropertyValue("CbaOpEnabled", ref _CbaOpEnabled, value);
            }
        }

        private string _CbaOpFilePath;
        public string CbaOpFilePath
        {
            get
            {
                return _CbaOpFilePath;
            }
            set
            {
                SetPropertyValue("CbaOpFilePath", ref _CbaOpFilePath, value);
            }
        }

        public static ImportBankStmtParam GetInstance(IObjectSpace objectSpace)
        {
            return BaseObjectHelper.GetInstance<ImportBankStmtParam>(objectSpace);
        }
    }
}