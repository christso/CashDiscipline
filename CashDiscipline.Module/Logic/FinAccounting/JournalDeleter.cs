using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class JournalDeleter : CashDiscipline.Module.Interfaces.IJournalDeleter
    {
        private readonly FinGenJournalParam paramObj;

        public JournalDeleter(FinGenJournalParam paramObj)
        {
            this.paramObj = paramObj;
        }

        public void DeleteAutoGenLedgerItems()
        {
            DeleteAutoGenLedgerItems(paramObj.Session, paramObj);
        }

        private void DeleteAutoGenLedgerItems(Session session, FinGenJournalParam paramObj)
        {
            // Delete Bank Stmts and Cash Flows


            var sqlParamNames = new string[] { "FromDate", "ToDate", "EntryType", "SnapshotOid" };
            var sqlParamValues = new object[] { paramObj.FromDate, paramObj.ToDate,
                                    GenLedgerEntryType.Auto, SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid};
            session.ExecuteNonQuery(DeleteCommandText, sqlParamNames, sqlParamValues);
        }

        public string DeleteCommandText
        {
            get
            {
                return @"
UPDATE GenLedger SET GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
FROM GenLedger
WHERE GenLedger.EntryType = @EntryType
AND (
    GenLedger.SrcBankStmt IN 
    (
        SELECT BankStmt.Oid FROM BankStmt WHERE BankStmt.TranDate BETWEEN @FromDate AND @ToDate
    )
    OR GenLedger.SrcCashFlow IN
    (
        SELECT CashFlow.Oid FROM CashFlow WHERE CashFlow.TranDate BETWEEN @FromDate AND @ToDate
        AND CashFlow.Snapshot = @SnapshotOid
    )
)";
            }
        }
    }
}
