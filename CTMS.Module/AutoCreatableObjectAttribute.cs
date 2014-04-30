using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class AutoCreatableObjectAttribute : Attribute
    {
        private bool autoCreatable = true;
        private ViewEditMode viewEditMode = ViewEditMode.Edit;
        public AutoCreatableObjectAttribute()
        {
        }
        public AutoCreatableObjectAttribute(bool autoCreatable)
        {
            this.autoCreatable = autoCreatable;
        }
        public bool AutoCreatable
        {
            get { return autoCreatable; }
        }
        public ViewEditMode ViewEditMode
        {
            get { return viewEditMode; }
            set { viewEditMode = value; }
        }

    }
    //public class AutoCreatableObjectController : ViewController<DetailView>
    //{
    //    protected override void OnViewChanging(View view)
    //    {
    //        base.OnViewChanging(view);
    //        Active.SetItemValue("AutoCreatableObject", false);
    //        if (view != null && view is ObjectView)
    //        {
    //            AutoCreatableObjectAttribute attribute = ((ObjectView)view).ObjectTypeInfo.FindAttribute<AutoCreatableObjectAttribute>(true);
    //            if (attribute != null)
    //            {
    //                Active.SetItemValue("AutoCreatableObject", true);
    //            }
    //        }
    //    }
    //}
}
