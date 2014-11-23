using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.Utils;
using Xafology.ExpressApp.Xpo;
using Xafology.ExpressApp.IO;

namespace CTMS.Module.ParamObjects.Import
{
    [NonPersistent]
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [FileAttachment("File")]
    public class ImportDelimitedFileDataParam
    {
        public ImportDelimitedFileDataParam()
        {
            _File = new Xafology.ExpressApp.SystemModule.OpenFileData();
            _ImportLibrary = ImportLibrary.FastCsvReader;
            _Delimiter = ",";
        }

        private ImportActionType _ImportActionType;
        private bool _CacheLookupObjects;
        private ImportLibrary _ImportLibrary;
        private string _Delimiter;
        private bool _CreateMembers;
        private Xafology.ExpressApp.SystemModule.OpenFileData _File;
        private Xafology.ExpressApp.SystemModule.OpenFileData _TemplateFile;

        public ImportLibrary ImportLibrary
        {
            get
            {
                return _ImportLibrary;
            }
            set
            {
                _ImportLibrary = value;
            }
        }


        public bool CacheLookupObjects
        {
            get
            {
                return _CacheLookupObjects;
            }
            set
            {
                _CacheLookupObjects = value;
            }
        }

        public bool CreateMembers
        {
            get
            {
                return _CreateMembers;
            }
            set
            {
                _CreateMembers = value;
            }
        }

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

        public string Delimiter
        {
            get
            {
                return _Delimiter;
            }
            set
            {
                _Delimiter = value;
            }
        }

        [DisplayName("Template File")]
        public Xafology.ExpressApp.SystemModule.OpenFileData TemplateFile
        {
            get
            {
                return _TemplateFile;
            }
            set
            {
                _TemplateFile = value;
            }
        }


        private ITypeInfo _ObjectTypeInfo;
        [MemberDesignTimeVisibility(false)]
        public ITypeInfo ObjectTypeInfo
        {
            get
            {
                return _ObjectTypeInfo;
            }
            set
            {
                _ObjectTypeInfo = value;
            }
        }


        private FieldMaps _FieldMaps;
        [MemberDesignTimeVisibility(false)]
        public FieldMaps FieldMaps
        {
            get
            {
                return _FieldMaps;
            }
            set
            {
                _FieldMaps = value;
            }
        }

        public ImportActionType ImportActionType
        {
            get
            {
                return _ImportActionType;
            }
            set
            {
                _ImportActionType = value;
            }
        }

        public void CreateTemplate()
        {
            var paramObj = this;
            var objTypeInfo = paramObj.ObjectTypeInfo;
            var templateMemberNames = new List<string>();
            foreach (var m in objTypeInfo.Members)
            {
                if (m.IsVisible && m.IsPersistent && !m.IsReadOnly)
                {
                    templateMemberNames.Add(m.Name);
                }
            }

            byte[] buffer = string.Join(",", templateMemberNames).ToBytes();
            paramObj.TemplateFile = new Xafology.ExpressApp.SystemModule.OpenFileData();
            paramObj.TemplateFile.Load("Template.csv", buffer);
        }

    }
}
