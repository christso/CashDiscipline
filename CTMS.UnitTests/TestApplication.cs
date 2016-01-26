using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Layout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CTMS.UnitTests
{
    public class TestApplication : XafApplication
    {
        public TestApplication()
        {

        }

        //protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args)
        //{
        //    args.ObjectSpaceProvider = new XPObjectSpaceProvider(args.ConnectionString, args.Connection);
        //}

        protected override void OnDatabaseVersionMismatch(DatabaseVersionMismatchEventArgs e)
        {
            base.OnDatabaseVersionMismatch(e);
            e.Updater.Update();
            e.Handled = true;
        }
        protected override LayoutManager CreateLayoutManagerCore(bool simple)
        {
            return null;
        }

    }
}
