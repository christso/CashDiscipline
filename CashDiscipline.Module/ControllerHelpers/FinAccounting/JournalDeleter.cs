using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.ControllerHelpers.FinAccounting
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

        private static void DeleteAutoGenLedgerItems(Session session, FinGenJournalParam paramObj)
        {
            var query = (new XPQuery<GenLedger>(session))
                .Where(x => x.EntryType == GenLedgerEntryType.Auto
                    && x.SrcBankStmt.TranDate >= paramObj.FromDate
                    && x.SrcBankStmt.TranDate <= paramObj.ToDate
                    || x.SrcCashFlow.TranDate >= paramObj.FromDate
                    && x.SrcCashFlow.TranDate <= paramObj.ToDate
                    && x.SrcCashFlow.Snapshot.Oid == SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid);
            foreach (var gls in query)
            {
                gls.Delete();
            }
        }
    }
}
