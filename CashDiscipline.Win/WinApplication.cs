﻿using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Win;
using System.Collections.Generic;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Xpo;
using Xafology.ExpressApp.Xpo;

namespace CashDiscipline.Win {
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/DevExpressExpressAppWinWinApplicationMembersTopicAll.aspx
    public partial class CashDisciplineWindowsFormsApplication : WinApplication {
        public CashDisciplineWindowsFormsApplication() {
            InitializeComponent();
            //LinkNewObjectToParentImmediately = false;
            DelayedViewItemsInitialization = true;
            this.CustomCheckCompatibility += CashDisc_CustomCheckCompatibility;
        }

        protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args)
        {
            args.ObjectSpaceProvider = new ExtObjectSpaceProvider(args.ConnectionString, args.Connection);
            args.ObjectSpaceProviders.Add(new NonPersistentObjectSpaceProvider(TypesInfo, null));
        }
    
        private void CashDisc_CustomCheckCompatibility(object sender, DevExpress.ExpressApp.CustomCheckCompatibilityEventArgs e)
        {
            // do not check compatibility for release version
            // so we avoid throwing error in the app when we make minor changes to the database
            //var pubVer = CashDiscipline.Module.AssemblyInfo.Version;
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = true;
            }
        }

        private void CashDisciplineWindowsFormsApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
            e.Updater.Update();
            e.Handled = true;
#else
            if(System.Diagnostics.Debugger.IsAttached) {
                e.Updater.Update();
                e.Handled = true;
            }
            else {
				string message = "The application cannot connect to the specified database, " +
					"because the database doesn't exist, its version is older " +
					"than that of the application or its schema does not match " +
					"the ORM data model structure. To avoid this error, use one " +
					"of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

				if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
					message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
				}
				throw new InvalidOperationException(message);
            }
#endif
        }

        private void CashDisciplineWindowsFormsApplication_CustomizeLanguagesList(object sender, CustomizeLanguagesListEventArgs e)
        {
            string userLanguageName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (userLanguageName != "en-US" && e.Languages.IndexOf(userLanguageName) == -1)
            {
                e.Languages.Add(userLanguageName);
            }
        }
    }
}
