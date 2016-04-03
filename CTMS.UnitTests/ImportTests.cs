using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.UnitTests.TestObjects;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo.Import;
using Xafology.ExpressApp.Xpo.Import.Logic;
using Xafology.ExpressApp.Xpo.Import.Parameters;
using Xafology.ExpressApp.Xpo.ValueMap;
using Xafology.TestUtils;

namespace CTMS.UnitTests
{
    public class ImportTests : TestBase
    {
        #region Config

        public ImportTests()
        {
            SetTesterDbType(TesterDbType.InMemory);
            
            var tester = Tester as MSSqlDbTestBase;
            if (tester != null)
                tester.DatabaseName = "CTMS_Test";
        }

        #endregion

        #region Tests

        [Test]
        public void InsertCashFlow()
        {
            #region Arrange

            var csvText = @"TranDate,Account,Activity,Counterparty,CounterCcyAmt,CounterCcy,Description,Source
21/03/2016,VHA ANZ 70086,AP Pymt,APPLE PTY LTD,-10000,AUD,Test transaction,Chris Tso";

            var map1 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map1.SourceName = "TranDate";
            map1.TargetName = map1.SourceName;

            var map2 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map2.SourceName = "Account";
            map2.TargetName = map2.SourceName;
            map2.CreateMember = true;

            var map3 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map3.SourceName = "Activity";
            map3.TargetName = map3.SourceName;
            map3.CreateMember = true;
            map3.CacheObject = true;

            var map4 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map4.SourceName = "Counterparty";
            map4.TargetName = map4.SourceName;
            map4.CreateMember = true;
            map4.CacheObject = true;

            var map5 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map5.SourceName = "CounterCcyAmt";
            map5.TargetName = map5.SourceName;

            var map6 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map6.SourceName = "CounterCcy";
            map6.TargetName = map6.SourceName;
            map6.CreateMember = true;
            //map6.CacheObject = true;

            var map7 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map7.SourceName = "Description";
            map7.TargetName = map7.SourceName;

            var map8 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map8.SourceName = "Source";
            map8.TargetName = map8.SourceName;
            map8.CreateMember = true;
            map8.CacheObject = true;

            var param = ObjectSpace.CreateObject<ImportHeadersParam>();

            param.HeaderToFieldMaps.Add(map1);
            param.HeaderToFieldMaps.Add(map2);
            param.HeaderToFieldMaps.Add(map3);
            param.HeaderToFieldMaps.Add(map4);
            param.HeaderToFieldMaps.Add(map5);
            param.HeaderToFieldMaps.Add(map6);
            param.HeaderToFieldMaps.Add(map7);
            param.HeaderToFieldMaps.Add(map8);

            param.ObjectTypeName = "CashFlow";

            ObjectSpace.CommitChanges();

            #endregion

            #region Act

            var csvStream = ConvertToCsvStream(csvText);
            var xpoMapper = new XpoFieldMapper();
            ICsvToXpoLoader loader = new HeadCsvToXpoInserter(param, csvStream, xpoMapper, null);
            loader.Execute();
            ObjectSpace.CommitChanges();
            
            #endregion

            #region Assert

            var inserted = new XPQuery<CashFlow>(ObjectSpace.Session);
            Assert.AreEqual(1, inserted.Count());
            var obj1 = inserted.FirstOrDefault();
            Assert.NotNull(obj1.Account);
            Assert.NotNull(obj1.Activity);
            Assert.NotNull(obj1.Counterparty);
            Assert.NotNull(obj1.Source);
            Assert.NotNull(obj1.CounterCcy);

            #endregion
        }

        [Test]
        public void InsertApPmtDistn()
        {

        }

        [Test]
        public void InsertMock()
        {
            #region Arrange

            var csvText = @"Description,Amount,MockLookupObject1,MockLookupObject2
Hello 1,10,Parent 1,Parent B1
Hello 2,11,Parent 2,Parent B2
Hello 3,12,Parent 3,Parent B3
Hello 4,13,Parent 4,Parent B4
";

            var map1 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map1.SourceName = "Description";
            map1.TargetName = map1.SourceName;

            var map2 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map2.SourceName = "Amount";
            map2.TargetName = map2.SourceName;

            var map3 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map3.SourceName = "MockLookupObject1";
            map3.TargetName = map3.SourceName;
            map3.CreateMember = true;
            map3.CacheObject = true;

            var map4 = ObjectSpace.CreateObject<HeaderToFieldMap>();
            map4.SourceName = "MockLookupObject2";
            map4.TargetName = map4.SourceName;
            map4.CreateMember = true;
            map4.CacheObject = true;

            var param = ObjectSpace.CreateObject<ImportHeadersParam>();

            param.HeaderToFieldMaps.Add(map1);
            param.HeaderToFieldMaps.Add(map2);
            param.HeaderToFieldMaps.Add(map3);
            param.HeaderToFieldMaps.Add(map4);

            param.ObjectTypeName = "MockFactObject";

            ObjectSpace.CommitChanges();

            #endregion

            #region Act

            var csvStream = ConvertToCsvStream(csvText);
            var xpoMapper = new XpoFieldMapper();
            ICsvToXpoLoader loader = new HeadCsvToXpoInserter(param, csvStream, xpoMapper, null);
            loader.Execute();
            ObjectSpace.CommitChanges();

            #endregion

            #region Assert

            var inserted = new XPQuery<MockFactObject>(ObjectSpace.Session);
            Assert.AreEqual(4, inserted.Count());
            var obj1 = inserted.Where(x => x.Description == "Hello 1").FirstOrDefault();
            var obj2 = inserted.Where(x => x.Description == "Hello 2").FirstOrDefault();
            var obj3 = inserted.Where(x => x.Description == "Hello 3").FirstOrDefault();
            var obj4 = inserted.Where(x => x.Description == "Hello 4").FirstOrDefault();

            Assert.NotNull(obj1.MockLookupObject1);
            Assert.NotNull(obj1.MockLookupObject2);
            Assert.NotNull(obj2.MockLookupObject1);
            Assert.NotNull(obj2.MockLookupObject2);
            Assert.NotNull(obj3.MockLookupObject1);
            Assert.NotNull(obj3.MockLookupObject2);
            Assert.NotNull(obj4.MockLookupObject1);
            Assert.NotNull(obj4.MockLookupObject2);

            #endregion
        }

        #endregion

        #region Helpers

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CTMSTestHelper.AddExportedTypes(module);
        }

        protected Stream ConvertToCsvStream(string csvText)
        {
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvText);
            return new MemoryStream(csvBytes);
        }

        public override void OnSetup()
        {
            CTMS.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CTMS.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }

        #endregion

    }
}
