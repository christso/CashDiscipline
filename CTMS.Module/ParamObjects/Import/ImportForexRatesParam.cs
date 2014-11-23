using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace CTMS.Module.ParamObjects.Import
{
    [NonPersistent]
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [FileAttachment("File")]
    [DefaultClassOptions]
    public class ImportForexRatesParam
    {
        public ImportForexRatesParam()
        {
            _File = new Xafology.ExpressApp.SystemModule.OpenFileData();
        }

        private Xafology.ExpressApp.SystemModule.OpenFileData _File;

        [DisplayName("Please upload a file")]
        public Xafology.ExpressApp.SystemModule.OpenFileData File
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
