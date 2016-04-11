using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace CashDiscipline.Module.BusinessObjects.Setup
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class SqlScriptFolder : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public SqlScriptFolder(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }
        // Fields...
        private SqlScriptFolder _ParentFolder;
        private string _FolderName;

        public string FolderName
        {
            get
            {
                return _FolderName;
            }
            set
            {
                SetPropertyValue("FolderName", ref _FolderName, value);
            }
        }

        [Association("SqlScriptFolder-SqlScripts")]
        public XPCollection<SqlScript> SqlScripts
        {
            get
            {
                return GetCollection<SqlScript>("SqlScripts");
            }
        }

        [Association("ParentFolder-ChildFolders")]
        public SqlScriptFolder ParentFolder
        {
            get
            {
                return _ParentFolder;
            }
            set
            {
                SetPropertyValue("ParentFolder", ref _ParentFolder, value);
            }
        }

        [Association("ParentFolder-ChildFolders")]
        public XPCollection<SqlScriptFolder> ChildFolders
        {
            get
            {
                return GetCollection<SqlScriptFolder>("ChildFolders");
            }
        }

    }
}
