using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.ParamObjects.FinAccounting;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using DG2NTT.StringEvaluators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.ControllerHelpers.FinAccounting
{
    public class JournalGenerator
    {
        public JournalGenerator(FinGenJournalParam paramObj)
        {
            _ParamObj = paramObj;
        }

        private FinGenJournalParam _ParamObj;
        private List<GenLedgerFinActivityJoin> _GenLedgerFinActivityJoin;

        #region Journal Item
        private static GenLedger CreateActivityJournalItem(Session session, BankStmt bsi, FinActivity activityMap)
        {
            var activityGli = new GenLedger(session);
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

        private static GenLedger CreateActivityJournalItem(Session session, CashFlow cf, FinActivity activityMap)
        {
            var activityGli = new GenLedger(session);
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

        private static GenLedger CreateAccountJournalItem(Session session, BankStmt bsi, FinAccount accountMap, FinActivity activityMap)
        {
            var accountGli = new GenLedger(session);
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

        private static GenLedger CreateAccountJournalItem(Session session, CashFlow cf, FinAccount accountMap, FinActivity activityMap)
        {
            var accountGli = new GenLedger(session);
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
        #endregion

        #region Evaluate Functional Ccy Amt

        private decimal EvalFunctionalCcyAmt(BankStmt bsi, FinActivity activityMap)
        {
            // get maps that have been computed for each bank statement item
            var fp = new FunctionParser();
            ParserDelegate TokenFAmountHandler = delegate(FunctionParseDelegateArgs e)
            {
                // e.FunctionArgs[0] will have value like, for example, 'A'
                var result = from j in _GenLedgerFinActivityJoin
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

        private decimal EvalFunctionalCcyAmt(CashFlow cf, FinActivity activityMap)
        {
            // get maps that have been computed for each bank statement item
            var fp = new FunctionParser();
            ParserDelegate TokenFAmountHandler = delegate(FunctionParseDelegateArgs e)
            {
                // e.FunctionArgs[0] will have value like, for example, 'A'
                var result = from j in _GenLedgerFinActivityJoin
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
        #endregion

        /// <summary>
        ///  Delete existing items in GenLedger excluding Manual Entries that matches a CashFlow or BankStmt
        /// </summary>
        /// <param name="session"></param>
        /// <param name="paramObj"></param>
        private static void DeleteAutoGenLedgerItems(Session session, FinGenJournalParam paramObj)
        {
            // Delete Bank Stmts and Cash Flows
            string sqlDelete = string.Format("DELETE FROM GenLedger WHERE GenLedger.EntryType = @EntryType"
                                + " AND ("
                                + "GenLedger.SrcBankStmt IN"
                                    + " (SELECT BankStmt.Oid FROM BankStmt WHERE BankStmt.TranDate BETWEEN @FromDate AND @ToDate)"
                                + " OR GenLedger.SrcCashFlow IN"
                                    + " (SELECT CashFlow.Oid FROM CashFlow WHERE CashFlow.TranDate BETWEEN @FromDate AND @ToDate"
                                    +" AND CashFlow.Snapshot = @SnapshotOid)"
                                + ")");
            var sqlParamNames = new string[] { "FromDate", "ToDate", "EntryType", "SnapshotOid" };
            var sqlParamValues = new object[] { paramObj.FromDate, paramObj.ToDate,
                                    GenLedgerEntryType.Auto, SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid};
            session.ExecuteNonQuery(sqlDelete, sqlParamNames, sqlParamValues);
        }

        private IList<BankStmt> GetBankStmts(IEnumerable<Activity> activitiesToMap,
            IEnumerable<Account> accountsToMap)
        {
            var paramObj = _ParamObj;
            var session = paramObj.Session;
            var cop = CriteriaOperator.Parse("TranDate Between(?, ?)", paramObj.FromDate, paramObj.ToDate);
            cop = GroupOperator.And(cop, new InOperator("Activity", activitiesToMap));
            cop = GroupOperator.And(cop, new InOperator("Account", accountsToMap));
            return GetObjects<BankStmt>(session, cop);
        }

        /// <summary>
        /// Get Cash Flows excluding ones sourced from Bank Stmt
        /// </summary>
        private IList<CashFlow> GetCashFlows(IEnumerable<Activity> activitiesToMap,
            IEnumerable<Account> accountsToMap)
        {
            var paramObj = _ParamObj;
            var session = paramObj.Session;
            var excludeSource = session.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.BankStmtCashFlowSource.Oid);
            var cop = CriteriaOperator.Parse(
                    string.Format("{0} Between(?, ?) And {1} <> ? And {2} = ?",
                    CashFlow.FieldNames.TranDate,
                    CashFlow.FieldNames.Source,
                    CashFlow.FieldNames.Snapshot),
                paramObj.FromDate, paramObj.ToDate, excludeSource, SetOfBooks.CachedInstance.CurrentCashFlowSnapshot);
            cop = GroupOperator.And(cop, new InOperator("Activity", activitiesToMap));
            cop = GroupOperator.And(cop, new InOperator("Account", accountsToMap));
            return GetObjects<CashFlow>(session, cop);
        }

        public void Execute()
        {
            var paramObj = _ParamObj;
            var session = paramObj.Session;
            if (paramObj == null) throw new UserFriendlyException("Param Object cannot be null.");

            #region Get Objects
            _GenLedgerFinActivityJoin = new List<GenLedgerFinActivityJoin>();

            var jnlGroupKeysInParams = paramObj.JournalGroupParams.Select(p => p.JournalGroup.Oid);
            var jnlGroupsInParams = GetObjects<FinJournalGroup>(session,
                new InOperator("Oid", jnlGroupKeysInParams));

            var activityMaps = new Func<List<FinActivity>>(() =>
            {
                var sortProps = new SortingCollection(null);
                sortProps.Add(new SortProperty("Index", DevExpress.Xpo.DB.SortingDirection.Ascending));
                var result = session.GetObjects(session.GetClassInfo(typeof(FinActivity)),
                                new InOperator("JournalGroup", jnlGroupsInParams),
                                sortProps, 0, false, true).Cast<FinActivity>().ToList();
                return result;
            })();
            var activitiesToMap = activityMaps.GroupBy(m => m.FromActivity).Select(k => k.Key);

            var accountMaps = GetObjects<FinAccount>(session, new InOperator("JournalGroup", jnlGroupsInParams));
            var accountsToMap = accountMaps.Select(k => k.Account); // TODO: investigate why nothing selected
            #endregion

            DeleteAutoGenLedgerItems(session, paramObj);

            #region Bank Stmt
            // Add items to GenLedger
            var bankStmts = GetBankStmts(activitiesToMap, accountsToMap);
            foreach (BankStmt bsi in bankStmts)
            {
                // use the same Account Map for each Bank Statement Item
                var accountMap = accountMaps.FirstOrDefault(m => m.Account == bsi.Account);
                _GenLedgerFinActivityJoin.Clear();
                foreach (var activityMap in activityMaps)
                {
                    if (activityMap.FromActivity != bsi.Activity
                        || !(activityMap.TargetObject == FinJournalTargetObject.BankStmt
                        || activityMap.TargetObject == FinJournalTargetObject.All)) continue;

                    GenLedger activityGli = CreateActivityJournalItem(session, bsi, activityMap);
                    GenLedger accountGli = CreateAccountJournalItem(session, bsi, accountMap, activityMap);

                    // Evaluate Amount Expression
                    accountGli.FunctionalCcyAmt = EvalFunctionalCcyAmt(bsi, activityMap); // TODO: may be zero
                    activityGli.FunctionalCcyAmt = accountGli.FunctionalCcyAmt * -1.00M;

                    activityGli.Save();
                    accountGli.Save();

                    // Join GenLedger with FinActivity so you can get the sum of all previous GenLedger.FinActivity.Token
                    // we use accountGli as the FunctionCcyAmt's sign is not reversed like in actvitiyGli
                    _GenLedgerFinActivityJoin.Add(new GenLedgerFinActivityJoin() { FinActivity = activityMap, GenLedger = accountGli });
                }
            }
            #endregion

            #region Cash Flows

            var cashFlows = GetCashFlows(activitiesToMap, accountsToMap);
            foreach (CashFlow cf in cashFlows)
            {
                // use the same Account Map for each Bank Statement Item
                var accountMap = accountMaps.FirstOrDefault(m => m.Account == cf.Account);
                _GenLedgerFinActivityJoin.Clear();
                foreach (var activityMap in activityMaps)
                {
                    if (activityMap.FromActivity != cf.Activity 
                        || !(activityMap.TargetObject == FinJournalTargetObject.CashFlow
                        || activityMap.TargetObject == FinJournalTargetObject.All)) continue;

                    GenLedger activityGli = CreateActivityJournalItem(session, cf, activityMap);
                    GenLedger accountGli = CreateAccountJournalItem(session, cf, accountMap, activityMap);

                    // Evaluate Amount Expression
                    accountGli.FunctionalCcyAmt = EvalFunctionalCcyAmt(cf, activityMap); // TODO: may be zero
                    activityGli.FunctionalCcyAmt = accountGli.FunctionalCcyAmt * -1.00M;

                    activityGli.Save();
                    accountGli.Save();

                    // Join GenLedger with FinActivity so you can get the sum of all previous GenLedger.FinActivity.Token
                    // we use accountGli as the FunctionCcyAmt's sign is not reversed like in actvitiyGli
                    _GenLedgerFinActivityJoin.Add(new GenLedgerFinActivityJoin() { FinActivity = activityMap, GenLedger = accountGli });
                }
            }
            #endregion

            session.CommitTransaction();
        }

        private static System.Collections.Generic.IList<T> GetObjects<T>(Session session, CriteriaOperator criteria)
        {
            return session.GetObjects(session.GetClassInfo(typeof(T)),
                            criteria,
                            new SortingCollection(null), 0, false, true).Cast<T>().ToList();
        }

        private class GenLedgerFinActivityJoin
        {
            public GenLedger GenLedger;
            public FinActivity FinActivity;
        }
    }
}
