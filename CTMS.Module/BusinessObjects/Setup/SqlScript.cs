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

namespace CTMS.Module.BusinessObjects.Setup
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class SqlScript : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public SqlScript(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        // Fields...
        private SqlScriptFolder _SqlScriptFolder;
        private string _ScriptContent;
        private string _ScriptName;

        public string ScriptName
        {
            get
            {
                return _ScriptName;
            }
            set
            {
                SetPropertyValue("ScriptName", ref _ScriptName, value);
            }
        }

        [Size(SizeAttribute.Unlimited)]
        public string ScriptContent
        {
            get
            {
                return _ScriptContent;
            }
            set
            {
                SetPropertyValue("ScriptContent", ref _ScriptContent, value);
            }
        }


        [Association("SqlScriptFolder-SqlScripts")]
        public SqlScriptFolder SqlScriptFolder
        {
            get
            {
                return _SqlScriptFolder;
            }
            set
            {
                SetPropertyValue("SqlScriptFolder", ref _SqlScriptFolder, value);
            }
        }
    }
}
