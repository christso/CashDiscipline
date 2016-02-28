using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Utils;


namespace CTMS.UnitTests.Base
{
    public class InMemoryDbTestBase : ITest
    {
        private XPObjectSpaceProvider ObjectSpaceProvider;
        public XPObjectSpace ObjectSpace { get; set; }
        protected TestApplication Application;
        private readonly ModuleBase module;

        public event EventHandler<EventArgs> SetupEvent;
        public event EventHandler<AddExportedTypesEventArgs> AddExportedTypesEvent;

        public InMemoryDbTestBase()
        {
            module = new ModuleBase();
        }

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            InitializeImageLoader();

            ObjectSpaceProvider = CreateObjectSpaceProvider();

            Application = new TestApplication();

            // add base module
            AddExportedTypes(module);
            Application.Modules.Add(module);

            Application.Setup("", ObjectSpaceProvider);
            Application.CheckCompatibility();
            ObjectSpace = (XPObjectSpace)ObjectSpaceProvider.CreateObjectSpace();
        }


        [SetUp]
        public void Setup()
        {
            if (SetupEvent != null)
                SetupEvent(this, EventArgs.Empty);
        }

        private void InitializeImageLoader()
        {
            var classType = GetType();
            if (!ImageLoader.IsInitialized)
            {
                ImageLoader.Init(new AssemblyResourceImageSource(classType.Assembly.FullName, "Images"));
            }
        }

        private XPObjectSpaceProvider CreateObjectSpaceProvider()
        {
            return new XPObjectSpaceProvider(new MemoryDataStoreProvider());
        }

        public void AddExportedTypes(ModuleBase module)
        {
            if (AddExportedTypesEvent != null)
            {
                AddExportedTypesEvent(this, new AddExportedTypesEventArgs(module));
            }
        }

        [TearDown]
        public void TearDown()
        {
            DeleteExportedObjects(module, ObjectSpace.Session);
        }

        [TestFixtureTearDown]
        public void TearDownFixture()
        {
        }

        public static void DeleteExportedObjects(ModuleBase module, Session session)
        {
            if (module == null)
                throw new InvalidOperationException("module cannot be null");

            foreach (var type in module.AdditionalExportedTypes)
            {
                DeleteObjects(session, type);
            }
        }

        public static void DeleteObjects(Session session, Type type)
        {
            session.Delete(new XPCollection(session, type));
        }

    }
}
