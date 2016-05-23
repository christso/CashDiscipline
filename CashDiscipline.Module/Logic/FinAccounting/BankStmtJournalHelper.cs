using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.StringEvaluators;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class BankStmtJournalHelper : IJournalHelper<BankStmt>
    {
        private readonly XPObjectSpace objSpace;
        private readonly FinGenJournalParam paramObj;
        public BankStmtJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
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

                    GenLedger activityGli = CreateActivityJournalItem(bsi, activityMap);
                    GenLedger accountGli = CreateAccountJournalItem(bsi, accountMap, activityMap);

                    // Evaluate Amount Expression
                    accountGli.FunctionalCcyAmt = EvalFunctionalCcyAmt(bsi, activityMap, genLedgerFinActivityJoin); // TODO: may be zero
                    activityGli.FunctionalCcyAmt = accountGli.FunctionalCcyAmt * -1.00M;

                    activityGli.Save();
                    accountGli.Save();

                    // Join GenLedger with FinActivity so you can get the sum of all previous GenLedger.FinActivity.Token
                    // we use accountGli as the FunctionCcyAmt's sign is not reversed like in actvitiyGli
                    genLedgerFinActivityJoin.Add(new GenLedgerFinActivityJoin() { FinActivity = activityMap, GenLedger = accountGli });
                }
            }
        }

        public GenLedger CreateActivityJournalItem(BankStmt bsi, FinActivity activityMap)
        {
            var activityGli = new GenLedger(objSpace.Session);
            activityGli.SrcBankStmt = bsi;

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
            return activityGli;
        }

        public GenLedger CreateAccountJournalItem(BankStmt bsi, FinAccount accountMap, FinActivity activityMap)
        {
            var accountGli = new GenLedger(objSpace.Session);
            accountGli.SrcBankStmt = bsi;
            accountGli.GlDescription = accountMap.GlDescription ?? (string.IsNullOrEmpty(bsi.SummaryDescription) ?
                bsi.TranDescription : bsi.TranDescription + "_" + bsi.SummaryDescription);

            accountGli.EntryType = GenLedgerEntryType.Auto;
            accountGli.JournalGroup = accountMap.JournalGroup;
            accountGli.Activity = activityMap.ToActivity;
            accountGli.GlCompany = accountMap.GlCompany;
            accountGli.GlAccount = accountMap.GlAccount;
            accountGli.GlCostCentre = accountMap.GlCostCentre;
            accountGli.GlCountry = accountMap.GlCountry;
            accountGli.GlIntercompany = accountMap.GlIntercompany;
            accountGli.GlLocation = accountMap.GlLocation;
            accountGli.GlProduct = accountMap.GlProduct;
            accountGli.GlProject = accountMap.GlProject;
            accountGli.GlSalesChannel = accountMap.GlSalesChannel;
            accountGli.IsActivity = false;
            return accountGli;
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
                             select j.GenLedger.FunctionalCcyAmt;
                return result.Sum();
            };
            fp.Add("FA", TokenFAmountHandler);
            var parsed = fp.ParseToCriteria(activityMap.FunctionalCcyAmtExpr.Replace("{FA}", "FunctionalCcyAmt"));

            object exprResult = bsi.Evaluate(CriteriaOperator.Parse(parsed.Criteria, parsed.Parameters));
            decimal famt = 0.00M;
            if (exprResult != null && Decimal.TryParse(exprResult.ToString(), out famt))
            {
                return famt;
            }
            return 0.00M;
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
