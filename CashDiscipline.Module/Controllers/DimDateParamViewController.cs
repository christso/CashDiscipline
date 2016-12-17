using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.ParamObjects.Import;
using System.Data.SqlTypes;

namespace CashDiscipline.Module.Controllers
{
    public class DimDateParamViewController : ViewController
    {
        public DimDateParamViewController()
        {
            TargetObjectType = typeof(DimDateParam);

            var generateAction = new SimpleAction(this, "GenerateDatesAction", PredefinedCategory.ObjectsCreation);
            generateAction.Caption = "Generate";
            generateAction.Execute += GenerateAction_Execute;

            var calculateAction = new SimpleAction(this, "CalculateDateColumnsAction", PredefinedCategory.ObjectsCreation);
            calculateAction.Caption = "Calculate";
            calculateAction.Execute += CalculateAction_Execute;

        }

        private void CalculateAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            
        }

        private void GenerateAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            
            var os = Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as DimDateParam;
            if (paramObj == null)
                throw new UserFriendlyException("Error: You must select a Date Parameter");

            var dateValue = paramObj.FromDate;
            if (dateValue <= SqlDateTime.MinValue)
                throw new UserFriendlyException(string.Format(
                    "Error: Date must be greater than '{0:dd-MMM-yy}'", SqlDateTime.MinValue));

            for (dateValue = paramObj.FromDate; 
                dateValue <= paramObj.ToDate; 
                dateValue = dateValue.AddDays(1))
            {
                var dateObj = os.CreateObject<DimDate>();
                dateObj.FullDate = dateValue;
            }

            os.CommitChanges();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    string.Format("Dates generated between {0:dd-MMM-yy} and {1:dd-MMM-yy}.",
                        paramObj.FromDate, paramObj.ToDate),
                    "Date Generation SUCCESSFUL");
        }
    }
}
