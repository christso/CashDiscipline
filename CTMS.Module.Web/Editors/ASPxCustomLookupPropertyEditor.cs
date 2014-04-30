using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.ExpressApp.Model;
using System.Web.UI.WebControls;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Editors;

namespace CTMS.Module.Web.Editors
{
    [PropertyEditor(typeof(IXPSimpleObject), "CustomLookupPropertyEditor", false)]
    public class ASPxCustomLookupPropertyEditor : ASPxLookupPropertyEditor
    {
        public ASPxCustomLookupPropertyEditor(Type objectType, DevExpress.ExpressApp.Model.IModelMemberViewItem model)
            : base(objectType, model)
        {
            
        }
        protected override void SetupControl(WebControl control)
        {
            base.SetupControl(control);
            if (DropDownEdit != null)
            {
                
                DropDownEdit.PreRender += DropDownEdit_PreRender;
                DropDownEdit.Init += DropDownEdit_Init;
            }
        }

        void DropDownEdit_Init(object sender, EventArgs e)
        {

        }
        void DropDownEdit_PreRender(object sender, EventArgs e)
        {

        }
    }
}
