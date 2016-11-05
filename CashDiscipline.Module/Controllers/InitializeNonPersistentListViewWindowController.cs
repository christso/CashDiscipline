using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers
{
    public class InitializeNonPersistentListViewWindowController : WindowController
    {
        public InitializeNonPersistentListViewWindowController() : base()
        {
            TargetWindowType = WindowType.Main;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            Application.ListViewCreating += Application_ListViewCreating;
        }
        private void Application_ListViewCreating(Object sender, ListViewCreatingEventArgs e)
        {
            if ((e.CollectionSource.ObjectTypeInfo.Type == typeof(Welcome)) && (e.CollectionSource.ObjectSpace is NonPersistentObjectSpace))
            {
                ((NonPersistentObjectSpace)e.CollectionSource.ObjectSpace).ObjectsGetting += ObjectSpace_ObjectsGetting;
            }
        }
        private void ObjectSpace_ObjectsGetting(Object sender, ObjectsGettingEventArgs e)
        {
            BindingList<Welcome> objects = new BindingList<Welcome>();

            objects.Add(new Welcome() { Category = "Important Links", Name = "Working Capital Reports", TextValue = @"\\vhacorp07.vha.internal\vodashare$\Finance\Cash Reports\Working Capital" });
            objects.Add(new Welcome() { Category = "Important Links", Name = "Customer Reports", TextValue = @"\\vhacorp07.vha.internal\vodashare$\Finance\Cash Reports\Customer Investment" });
            objects.Add(new Welcome() { Category = "Important Links", Name = "Accounting Reports", TextValue = @"\\vhacorp07.vha.internal\vodashare$\Finance\Cash Reports\Accounting" });

            objects.Add(new Welcome() { Category = "Session Details", Name = "Username", TextValue = Environment.UserName });
            objects.Add(new Welcome() { Category = "Session Details", Name = "Database Connection", TextValue = Application.ConnectionString });
            objects.Add(new Welcome() { Category = "Session Details", Name = "Application Path", TextValue = System.Reflection.Assembly.GetExecutingAssembly().Location });

            e.Objects = objects;
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Application.ListViewCreating -= Application_ListViewCreating;
        }
    }
}
