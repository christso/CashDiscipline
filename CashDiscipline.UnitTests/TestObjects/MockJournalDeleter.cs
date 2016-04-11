using CashDiscipline.Module.Interfaces;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.UnitTests.TestObjects
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
