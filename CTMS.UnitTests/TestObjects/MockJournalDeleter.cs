using CTMS.Module.Interfaces;
using CTMS.Module.ParamObjects.FinAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.UnitTests.TestObjects
{
    public class MockJournalDeleter : IJournalDeleter
    {
        public MockJournalDeleter(FinGenJournalParam paramObj)
        {

        }

        public void DeleteAutoGenLedgerItems()
        {

            // not implemented
        }
    }
}
