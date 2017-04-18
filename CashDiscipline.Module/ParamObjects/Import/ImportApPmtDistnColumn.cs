using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using System.ComponentModel;
using DevExpress.ExpressApp;
using CashDiscipline.Module.ParamObjects.Import;

namespace CashDiscipline.Module.ParamObjects.Import
{
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    //[NavigationItem("Import")]
    public class ImportApPmtDistnColumn : BaseObject
    {
        public ImportApPmtDistnColumn(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private ImportApPmtDistnParam _ImportApPmtDistnParam;
        [Association("ImportApPmtDistnParam-ImportApPmtDistnColumns")]
        public ImportApPmtDistnParam ImportApPmtDistnParam
        {
            get {
                return _ImportApPmtDistnParam;
            }
            set
            {
                SetPropertyValue("ImportApPmtDistnParam", ref _ImportApPmtDistnParam, value);
            }
        }

        private int _Ordinal;
        [ModelDefault("DisplayFormat", "0")]
        [ModelDefault("EditMask", "0")]
        public int Ordinal
        {
            get
            {
                return _Ordinal;
            }
            set
            {
                SetPropertyValue("Ordinal", ref _Ordinal, value);
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

        private string _TypeName;
        public string TypeName
        {
            get
            {
                return _TypeName;
            }
            set
            {
                SetPropertyValue("TypeName", ref _TypeName, value);
            }
        }
    }
}
