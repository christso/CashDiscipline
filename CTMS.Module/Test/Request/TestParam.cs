using CTMS.Module.ParamObjects.Import;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Test
{
    [NonPersistent]
    [AutoCreatableObject]
    [FileAttachment("File")]
    public class TestParam
    {
        public TestParam()
        {
            _File = new OpenFileData();
        }

        public string Name { get; set; }

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
