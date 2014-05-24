using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using CTMS.Module.ParamObjects.Setup;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Xpo;
using CTMS.Module.BusinessObjects.Setup;
using DevExpress.Data.Filtering;

namespace CTMS.Module.Controllers.Setup
{
    public class CreateDateDimParamViewController : ViewController
    {
        public CreateDateDimParamViewController()
        {
            TargetObjectType = typeof(CreateDateDimParam);
            TargetViewType = ViewType.DetailView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
            var dc = Frame.GetController<DialogController>();
            if (dc != null)
                dc.AcceptAction.Execute += AcceptAction_Execute;
        }

        void AcceptAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = View.CurrentObject as CreateDateDimParam;
            var objSpace = Application.CreateObjectSpace() as XPObjectSpace;

            // delete DateDims
            objSpace.Session.Delete(objSpace.GetObjects<DateDim>(CriteriaOperator.Parse("DateKey BETWEEN (?,?)", paramObj.FromDate, paramObj.ToDate)));
            objSpace.CommitChanges();

            // TODO: DevExpress to advise how to represent the Date Dimension as you cannot use DateTime as a key.
            // create DateDims
            for (DateTime dt = paramObj.FromDate; dt <= paramObj.ToDate;
                dt = dt.AddDays(1.0))
            {
                var dateDim = objSpace.CreateObject<DateDim>();
                dateDim.DateKey = dt;
                dateDim.MonthName = string.Format("{0:MMM}", dt);
                dateDim.Year = dt.Year;
                dateDim.DayOfWeek = dt.DayOfWeek;
                dateDim.Save();
            }
            objSpace.CommitChanges();
        }
    }
}
