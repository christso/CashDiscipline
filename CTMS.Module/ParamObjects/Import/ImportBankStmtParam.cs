using DevExpress.Xpo;

namespace CTMS.Module.ParamObjects.Import
{
    [NonPersistent]
    [D2NXAF.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    public class ImportBankStmtParam
    {
        public ImportBankStmtParam()
        {
            _File = new D2NXAF.ExpressApp.SystemModule.OpenFileData();
        }
        private BankStmtImportType _ImportType;

        public BankStmtImportType ImportType
        {
            get
            {
                return _ImportType;
            }
            set
            {
                _ImportType = value;
            }
        }

        private D2NXAF.ExpressApp.SystemModule.OpenFileData _File;

        [DisplayName("Please upload a file")]
        public D2NXAF.ExpressApp.SystemModule.OpenFileData File
        {
            get
            {
                return _File;
            }
            set
            {
                _File = value;
            }
        }
    }

    public enum BankStmtImportType
    {
        [DevExpress.ExpressApp.DC.XafDisplayName("ANZ")]
        ANZ,
        [DevExpress.ExpressApp.DC.XafDisplayName("WBC")]
        WBC,
        [DevExpress.ExpressApp.DC.XafDisplayName("CBA OP")]
        CBAOP,
        [DevExpress.ExpressApp.DC.XafDisplayName("CBA BOS")]
        CBABOS,
    }
}
