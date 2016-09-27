using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.StringEvaluators;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class BankStmtActivityOrmJournalHelper : IJournalHelper<BankStmt>
    {
        private readonly XPObjectSpace objSpace;
        private readonly FinGenJournalParam paramObj;
        public BankStmtActivityOrmJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
        }

        public void Process(IEnumerable<FinAccount> accountMaps, IEnumerable<FinActivity> activityMaps)
        {
            var helper = (IJournalHelper<BankStmt>)this;
            OrmJournalHelper.Process<BankStmt>(helper, accountMaps, activityMaps);
        }

        public void Process(IEnumerable<BankStmt> bankStmts, IEnumerable<FinAccount> accountMaps, IEnumerable<FinActivity> activityMaps)
        {
            var genLedgerFinActivityJoin = new List<GenLedgerFinActivityJoin>();

            foreach (BankStmt bsi in bankStmts)
            {
                // use the same Account Map for each Bank Statement Item
                var accountMap = accountMaps.FirstOrDefault(m => m.Account == bsi.Account);
                genLedgerFinActivityJoin.Clear();

                foreach (var activityMap in activityMaps)
                {
                    if (activityMap.FromActivity != bsi.Activity
                        || !(activityMap.TargetObject == FinJournalTargetObject.BankStmt
                        || activityMap.TargetObject == FinJournalTargetObject.All)) continue;

                    #region Create Journal Items
                    GenLedger activityGli = CreateActivityJournalItem(bsi, activityMap);
                    #endregion

                    #region Evaluate Amount

                    // Evaluate Amount Expression
                    var genLedgerKey = new GenLedgerKey() {
                        FunctionalCcyAmt = EvalFunctionalCcyAmt(bsi, activityMap, genLedgerFinActivityJoin) };
                    activityGli.FunctionalCcyAmt = genLedgerKey.FunctionalCcyAmt * -1.00M;

                    // Join GenLedger with FinActivity so you can get the sum of all previous GenLedger.FinActivity.Token
                    genLedgerFinActivityJoin.Add(new GenLedgerFinActivityJoin() { FinActivity = activityMap, GenLedgerKey = genLedgerKey });

                    #endregion
                }
            }
        }


        public GenLedger CreateActivityJournalItem(BankStmt bsi, FinActivity activityMap)
        {
            var activityGli = new GenLedger(objSpace.Session);
            activityGli.SrcBankStmt = bsi;
            if (bsi != null)
            {
                activityGli.GlDate = bsi.TranDate;
            }

            activityGli.EntryType = GenLedgerEntryType.Auto;
            activityGli.JournalGroup = activityMap.JournalGroup;
            activityGli.Activity = activityMap.ToActivity;
            activityGli.GlDescription = activityMap.GlDescription;

            activityGli.GlCompany = activityMap.GlCompany;
            activityGli.GlAccount = activityMap.GlAccount;
            activityGli.GlCostCentre = activityMap.GlCostCentre;
            activityGli.GlCountry = activityMap.GlCountry;
            activityGli.GlIntercompany = activityMap.GlIntercompany;
            activityGli.GlLocation = activityMap.GlLocation;
            activityGli.GlProduct = activityMap.GlProduct;
            activityGli.GlProject = activityMap.GlProject;
            activityGli.GlSalesChannel = activityMap.GlSalesChannel;
            activityGli.IsActivity = true;
            activityGli.IsJournal = true;
            return activityGli;
        }
        
        public decimal EvalFunctionalCcyAmt(BankStmt bsi, FinActivity activityMap, List<GenLedgerFinActivityJoin> genLedgerFinActivityJoin)
        {
            // get maps that have been computed for each bank statement item
            var fp = new FunctionParser();
            ParserDelegate TokenFAmountHandler = delegate (FunctionParseDelegateArgs e)
            {
                // e.FunctionArgs[0] will have value like, for example, 'A'
                var result = from j in genLedgerFinActivityJoin
                             where j.FinActivity.Token == e.FunctionArgs[0]
                             select j.GenLedgerKey.FunctionalCcyAmt;
                return result.Sum();
            };
            fp.Add("FA", TokenFAmountHandler);
            var parsed = fp.ParseToCriteria(activityMap.FunctionalCcyAmtExpr.Replace("{FA}", "FunctionalCcyAmt"));
            try
            {
                object exprResult = bsi.Evaluate(CriteriaOperator.Parse(parsed.Criteria, parsed.Parameters));
                decimal famt = 0.00M;
                if (exprResult != null && Decimal.TryParse(exprResult.ToString(), out famt))
                {
                    return famt;
                }
                return 0.00M;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(
                    "Error parsing map expression = '{1}', at FinActivity.Oid = '{0}', BankStmt.Oid = '{2}'\r\n{3}", 
                    activityMap.Oid, activityMap.FunctionalCcyAmtExpr, bsi.Oid, ex.Message),
                    ex);
            }
        }

        public IList<BankStmt> GetSourceObjects(IEnumerable<Activity> activitiesToMap,
            IEnumerable<Account> accountsToMap)
        {
            var paramObj = this.paramObj;
            var session = paramObj.Session;
            var cop = CriteriaOperator.Parse("TranDate Between(?, ?)", paramObj.FromDate, paramObj.ToDate);
            cop = GroupOperator.And(cop, new InOperator("Activity", activitiesToMap));
            cop = GroupOperator.And(cop, new InOperator("Account", accountsToMap));
            return new XPCollection<BankStmt>(session, cop);
        }

    }
}
