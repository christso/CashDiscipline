using CashDiscipline.Module.BusinessObjects.Cash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using Xafology.StringEvaluators;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using CashDiscipline.Module.BusinessObjects;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class CashFlowJournalHelper : IJournalHelper<CashFlow>
    {
        private readonly XPObjectSpace objSpace;
        private readonly FinGenJournalParam paramObj;
        public CashFlowJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
        }

        public void Process(IEnumerable<CashFlow> cashFlows, IEnumerable<FinAccount> accountMaps, IEnumerable<FinActivity> activityMaps)
        {
            var genLedgerFinActivityJoin = new List<GenLedgerFinActivityJoin>();

            // TODO: fix error "The 'CashDiscipline.Module.BusinessObjects.Cash.CashFlowSnapshot' object belongs to a different session."
            foreach (CashFlow cf in cashFlows)
            {
                // use the same Account Map for each Bank Statement Item
                var accountMap = accountMaps.FirstOrDefault(m => m.Account == cf.Account);
                genLedgerFinActivityJoin.Clear();
                foreach (var activityMap in activityMaps)
                {
                    if (activityMap.FromActivity != cf.Activity
                        || !(activityMap.TargetObject == FinJournalTargetObject.CashFlow
                        || activityMap.TargetObject == FinJournalTargetObject.All)) continue;

                    GenLedger activityGli = CreateActivityJournalItem(cf, activityMap);
                    GenLedger accountGli = CreateAccountJournalItem(cf, accountMap, activityMap);

                    // Evaluate Amount Expression
                    accountGli.FunctionalCcyAmt = EvalFunctionalCcyAmt(cf, activityMap, genLedgerFinActivityJoin); // TODO: may be zero
                    activityGli.FunctionalCcyAmt = accountGli.FunctionalCcyAmt * -1.00M;

                    activityGli.Save();
                    accountGli.Save();

                    // Join GenLedger with FinActivity so you can get the sum of all previous GenLedger.FinActivity.Token
                    // we use accountGli as the FunctionCcyAmt's sign is not reversed like in actvitiyGli
                    genLedgerFinActivityJoin.Add(new GenLedgerFinActivityJoin() { FinActivity = activityMap, GenLedger = accountGli });
                }
            }
        }

        public GenLedger CreateAccountJournalItem(CashFlow cf, FinAccount accountMap, FinActivity activityMap)
        {
            var accountGli = new GenLedger(objSpace.Session);
            accountGli.SrcCashFlow = cf;
            accountGli.GlDescription = accountMap.GlDescription ?? cf.Description;

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

        public GenLedger CreateActivityJournalItem(CashFlow cf, FinActivity activityMap)
        {
            var activityGli = new GenLedger(objSpace.Session);
            activityGli.SrcCashFlow = cf;

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

        public decimal EvalFunctionalCcyAmt(CashFlow cf, FinActivity activityMap, List<GenLedgerFinActivityJoin> genLedgerFinActivityJoin)
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

            object exprResult = cf.Evaluate(CriteriaOperator.Parse(parsed.Criteria, parsed.Parameters));
            decimal famt = 0.00M;
            if (exprResult != null && Decimal.TryParse(exprResult.ToString(), out famt))
            {
                return famt;
            }
            return 0.00M;
        }

        public IList<CashFlow> GetSourceObjects(IEnumerable<Activity> activitiesToMap, IEnumerable<Account> accountsToMap)
        {
            var paramObj = this.paramObj;
            var session = paramObj.Session;
            var excludeSource = SetOfBooks.GetInstance(objSpace).BankStmtCashFlowSource;
            var currentSnapshot = SetOfBooks.GetInstance(objSpace).CurrentCashFlowSnapshot;

            var cop = CriteriaOperator.Parse(
                    string.Format("{0} Between(?, ?) And {1} <> ? And {2} = ?",
                    CashFlow.Fields.TranDate.PropertyName,
                    CashFlow.Fields.Source.PropertyName,
                    CashFlow.Fields.Snapshot.PropertyName),
                paramObj.FromDate, paramObj.ToDate, excludeSource, currentSnapshot);
            cop = GroupOperator.And(cop, new InOperator("Activity", activitiesToMap));
            cop = GroupOperator.And(cop, new InOperator("Account", accountsToMap));
            return new XPCollection<CashFlow>(session, cop);
        }

    }
}
