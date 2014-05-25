using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D2NXAF.ExpressApp.SystemModule;

namespace CTMS.Module.ParamObjects.Import
{
    [NonPersistent]
    [D2NXAF.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [FileAttachment("File")]
    public class ImportApPmtDistnParam
    {
        public ImportApPmtDistnParam()
        {
            _File = new OpenFileData();
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

        [MemberDesignTimeVisibility(false)]
        public string FileName
        {
            get
            {
                return _File.FileName;
            }
        }
        [MemberDesignTimeVisibility(false)]
        public int Size
        {
            get { return _File.Size; }
        }

        [MemberDesignTimeVisibility(false)]
        public byte[] Content
        {
            get
            {
                return _File.Content;
            }
        }
    }
}
