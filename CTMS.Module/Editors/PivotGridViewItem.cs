using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Editors
{
    public interface IModelPivotGridViewItem : IModelViewItem
    {

    }
    [ViewItem(typeof(IModelCustomUserControlViewItem))]
    public abstract class PivotGridViewItem : CustomUserControlViewItem
    {
        public PivotGridViewItem(IModelViewItem model, Type objectType)
            : base(model, objectType)
        {
            
        }
    }
}
