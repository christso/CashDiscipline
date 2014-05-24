using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D2NXAF.Utils;
using D2NXAF.ExpressApp.Xpo;

namespace CTMS.Module.ParamObjects.Import
{
    [NonPersistent]
    [AutoCreatableObject]
    public class ImportBankStmtParam
    {
        public ImportBankStmtParam()
        {
            _File = new OpenFileData();
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

        private OpenFileData _File;

        [DisplayName("Please upload a file")]
        public OpenFileData File
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
        [DevExpress.ExpressApp.DC.XafDisplayName("CBA")]
        CBA,
        [DevExpress.ExpressApp.DC.XafDisplayName("SCB")]
        SCB
    }
}
