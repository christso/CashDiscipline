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
using System.Data.SqlClient;
using SmartFormat;

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
            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var conn = (SqlConnection)os.Connection;

            var paramObj = View.CurrentObject as DimDateParam;
            if (paramObj == null)
                throw new UserFriendlyException("Error: You must select a Date Parameter");

            try
            {
                CalculateDateColumns(conn, paramObj);

                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    "Date Column Calculation Successful",
                   "ACTION SUCCESSFUL"
                    );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void GenerateAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var conn = (SqlConnection)os.Connection;
  
            var paramObj = View.CurrentObject as DimDateParam;
            if (paramObj == null)
                throw new UserFriendlyException("Error: You must select a Date Parameter");

            DeleteExistingDates(conn, paramObj);

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

            CalculateDateColumns(conn, paramObj);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    string.Format("Dates generated between {0:dd-MMM-yy} and {1:dd-MMM-yy}.",
                        paramObj.FromDate, paramObj.ToDate),
                    "Date Generation SUCCESSFUL");
        }

        private int CalculateDateColumns(SqlConnection conn, DimDateParam paramObj)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("FromDate", string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date));
            dic.Add("ToDate", string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date));

            string commandText = Smart.Format(
@"DECLARE @FromDate datetime = '{FromDate}';
DECLARE @ToDate datetime = '{ToDate}';
EXEC sp_CalculateDateColumns @FromDate, @ToDate;", dic);
            using (var cmd = new SqlCommand(commandText, conn))
            {
                return cmd.ExecuteNonQuery();
            }
        }
   
        private int DeleteExistingDates(SqlConnection conn, DimDateParam paramObj)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("FromDate", string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date));
            dic.Add("ToDate", string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date));

            string commandText = Smart.Format(
@"DECLARE @FromDate datetime = '{FromDate}';
DECLARE @ToDate datetime = '{ToDate}';
DELETE FROM DimDate WHERE FullDate BETWEEN @FromDate AND @ToDate;",
dic);
            using (var cmd = new SqlCommand(commandText, conn))
            {
                return cmd.ExecuteNonQuery();
            }
        }

    }
}
