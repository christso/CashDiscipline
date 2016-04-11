using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Validation;
using DevExpress.Data.Filtering;
using CashDiscipline.Module.BusinessObjects.FinAccounting;

namespace CashDiscipline.Module.Validation
{
    [CodeRule]
    public class GenLedgerRule : RuleBase<GenLedger>
    {
        protected GenLedgerRule(string id, ContextIdentifiers targetContextIDs)
            : base(id, targetContextIDs)
        {
            
        }
        protected GenLedgerRule(string id, ContextIdentifiers targetContextIDs, Type targetType)
            : base(id, targetContextIDs, targetType)
        {
            
        }
        public GenLedgerRule() : base("", "Save")
        {
            
        }
        public GenLedgerRule(IRuleBaseProperties properties)
            : base(properties)
        {
            
        }
        protected override bool IsValidInternal(GenLedger target, out string errorMessageTemplate)
        {
            errorMessageTemplate = "Either but not both Src Cash Flow and Src Bank Stmt must be specified";
            if (target.SrcBankStmt == null && target.SrcCashFlow == null
                || target.SrcBankStmt != null && target.SrcCashFlow != null)
                return false;
            return true;
        }
    }
}
