using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Xpo;

namespace CTMS.UnitTests.Base
{
    public class TestBase
    {
        public ITest tester;
        private readonly ITest memTestBase;
        private readonly ITest mssqlTestBase;

        public XPObjectSpace ObjectSpace
        {
            get
            {
                return tester.ObjectSpace;
            }

            set
            {
                tester.ObjectSpace = value;
            }
        }

        public TestBase()
        {
            memTestBase = new InMemoryDbTestBase();
            mssqlTestBase = new MSSqlDbTestBase();
            memTestBase.OnSetupObjects += Tester_OnSetupObjects;
            mssqlTestBase.OnSetupObjects += Tester_OnSetupObjects;
            this.tester = memTestBase; // default is inmemory
        }

        public void SetTesterDbType(TesterDbType testerDbType)
        {
            switch (testerDbType)
            {
                case TesterDbType.InMemory:
                    tester = memTestBase;
                    break;
                case TesterDbType.MsSql:
                    tester = mssqlTestBase;
                    break;
            }
        }

        [SetUp]
        public void Setup()
        {
            tester.Setup();
        }

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            tester.SetUpFixture();
        }

        private void Tester_OnSetupObjects(object sender, EventArgs e)
        {
            SetupObjects();
        }

        public virtual void SetupObjects()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            tester.TearDown();
        }

        [TestFixtureTearDown]
        public void TearDownFixture()
        {
            tester.TearDownFixture();
        }
    }
}
