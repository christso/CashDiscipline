using System;
using System.Configuration;
using System.Windows.Forms;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Data.Filtering;

namespace CashDiscipline.Win {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
#if EASYTEST
            DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
			Tracing.LocalUserAppDataPath = Application.LocalUserAppDataPath;
			Tracing.Initialize();
            CashDisciplineWindowsFormsApplication winApplication = new CashDisciplineWindowsFormsApplication();

            if(ConfigurationManager.ConnectionStrings["ConnectionString"] != null) {
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
#if EASYTEST
            if(ConfigurationManager.ConnectionStrings["EasyTestConnectionString"] != null) {
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["EasyTestConnectionString"].ConnectionString;
            }
#endif
            if(System.Diagnostics.Debugger.IsAttached && winApplication.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                winApplication.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
            try {
                CriteriaOperator.RegisterCustomFunction(new CashDiscipline.Module.CustomFunctions.MultiConcatFunction());
                CriteriaOperator.RegisterCustomFunction(new CashDiscipline.Module.CustomFunctions.RegexMatchFunction());
                CriteriaOperator.RegisterCustomFunction(new CashDiscipline.Module.CustomFunctions.BoMonthFunction());
                CriteriaOperator.RegisterCustomFunction(new CashDiscipline.Module.CustomFunctions.EoMonthFunction());

                winApplication.Setup();
                winApplication.Start();
            }
            catch (Exception e)
            {
                winApplication.HandleException(e);
            }
        }
    }
}
