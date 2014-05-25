using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.ParamObjects.Artf
{
    [NonPersistent]
    [D2NXAF.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    public class ArtfCreateGlJournalsParam // : INotifyPropertyChanged
    {
        //private SimpleAction properties = new SimpleAction();

        //[EditorAlias(EditorAliases.DetailPropertyEditor)]
        //public SimpleAction SimpleAction
        //{
        //    get { return properties; }
        //}
        public TaskComponentEnum TaskComponent;
        public DateTime FromDate;
        public DateTime ToDate;

        private string log;
        [VisibleInListView(false), VisibleInLookupListView(false), Size(SizeAttribute.Unlimited)]
        [ModelDefault("AllowEdit", "False")]
        public string Log
        {
            get { return log; }
            set
            {
                if (log != value)
                {
                    log = value;
                    //OnPropertyChanged("Log");
                }
            }
        }

        //#region INotifyPropertyChanged Members
        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string propertyName)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}
        //#endregion

    }

    public enum TaskComponentEnum
    {
        Transfer,
        [DevExpress.ExpressApp.DC.XafDisplayName("Gl Journal")]
        GlJournal,
        [DevExpress.ExpressApp.DC.XafDisplayName("Activity Reclass")]
        ActivityReclass
    }
}
