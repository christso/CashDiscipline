using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.ParamObjects.FinAccounting;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.ControllerHelpers.FinAccounting
{
    public class JournalDeleter : CTMS.Module.Interfaces.IJournalDeleter
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

        private static void DeleteAutoGenLedgerItems(Session session, FinGenJournalParam paramObj)
        {
            // Delete Bank Stmts and Cash Flows
            string sqlDelete = string.Format("DELETE FROM GenLedger WHERE GenLedger.EntryType = @EntryType"
                                + " AND ("
                                + "GenLedger.SrcBankStmt IN"
                                    + " (SELECT BankStmt.Oid FROM BankStmt WHERE BankStmt.TranDate BETWEEN @FromDate AND @ToDate)"
                                + " OR GenLedger.SrcCashFlow IN"
                                    + " (SELECT CashFlow.Oid FROM CashFlow WHERE CashFlow.TranDate BETWEEN @FromDate AND @ToDate"
                                    + " AND CashFlow.Snapshot = @SnapshotOid)"
                                + ")");
            var sqlParamNames = new string[] { "FromDate", "ToDate", "EntryType", "SnapshotOid" };
            var sqlParamValues = new object[] { paramObj.FromDate, paramObj.ToDate,
                                    GenLedgerEntryType.Auto, SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid};
            session.ExecuteNonQuery(sqlDelete, sqlParamNames, sqlParamValues);
        }
    }
}
