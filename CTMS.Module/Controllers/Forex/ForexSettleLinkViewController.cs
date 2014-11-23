using CTMS.Module.BusinessObjects;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using Xafology.ExpressApp.Xpo;
using System.Diagnostics;

namespace CTMS.Module.Controllers.Forex
{
    public class ForexSettleLinkViewController : ViewController
    {
        public ForexSettleLinkViewController()
        {
            TargetObjectType = typeof(ForexSettleLink);
            runProgramAction = new SingleChoiceAction(this, "RunForexProgramAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            runProgramAction.Caption = "Run Program";
            runProgramAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            runProgramAction.Execute += runProgramAction_Execute;

            var linkFifoAction = new ChoiceActionItem();
            linkFifoAction.Caption = "Link with FIFO";
            runProgramAction.Items.Add(linkFifoAction);

        }

        void runProgramAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Link with FIFO":
                    ForexLinkFifo(ObjectSpace);
                    break;
            }
        }

        #region Link FIFO

        public static void ForexLinkFifo(IObjectSpace objSpace, int maxSteps = 0)
        {
            var session = ((XPObjectSpace)objSpace).Session;
            // get unlinked forex settles for foreign currency accounts

            var sortProps = new SortingCollection(null);
            sortProps.Add(new SortProperty("SequentialNumber", DevExpress.Xpo.DB.SortingDirection.Ascending));

            var cfOutCop = CriteriaOperator.Parse(
                "ForexLinkIsClosed = ? And ForexSettleType = ? And CounterCcy.Oid <> ?",
                false, CashFlowForexSettleType.Out, SetOfBooks.CachedInstance.FunctionalCurrency.Oid);
            var cashFlowOuts = XpoExtensions.GetObjects<CashFlow>(session, cfOutCop, sortProps);

            var cfInCop = CriteriaOperator.Parse(
                "ForexLinkIsClosed = ? And ForexSettleType = ? And CounterCcy.Oid <> ?",
                false, CashFlowForexSettleType.In, SetOfBooks.CachedInstance.FunctionalCurrency.Oid);
            var cashFlowIns = XpoExtensions.GetObjects<CashFlow>(session, cfInCop, sortProps);

            ForexLinkFifoWork(objSpace, cashFlowOuts, cashFlowIns, maxSteps);

            objSpace.CommitChanges();
        }

        private static void ForexLinkFifoWork(IObjectSpace objSpace, IList<CashFlow> cashFlowOuts, IList<CashFlow> cashFlowIns, int maxSteps)
        {
            int iOut = 0;
            int iIn = 0;
            int step = 0;

            while (iOut < cashFlowOuts.Count)
            {
                var cfOut = cashFlowOuts[iOut];

                if (iIn < 0)
                    iIn = 0;
                while (iIn >= 0 && iIn < cashFlowIns.Count)
                {
                    if (maxSteps != 0 && step > maxSteps)
                        return;
                    step++;

                    var cfIn = cashFlowIns[iIn];

                    // go to next Outflow if current Outflow is closed
                    if (cfOut.ForexUnlinkedAccountCcyAmt == 0) break;

                    // go to next Inflow if current Inflow is closed and the Outflow has negative sign
                    // (negative sign means it requires linking)
                    if (cfIn.ForexUnlinkedAccountCcyAmt == 0 & cfOut.AccountCcyAmt < 0) continue;

                    CreateForexSettleFifoLink(objSpace, cfOut, cfIn, ref iIn, step);
                }
                iOut++;
            }
        }

        // iIn: cursor position in inflows. Can decrease or increase by 1.
        private static ForexSettleLink CreateForexSettleFifoLink(IObjectSpace objSpace, CashFlow cfOut, CashFlow cfIn, ref int iIn, int step)
        {
            // create link
            var fsl = objSpace.CreateObject<ForexSettleLink>();
            fsl.CashFlowIn = cfIn;
            fsl.CashFlowOut = cfOut;
            fsl.Step = step;

            if (cfOut.AccountCcyAmt > 0)
            {
                // limit negative link amount by the positive linked amount of the inflow
                fsl.AccountCcyAmt = -Math.Min(cfIn.ForexLinkedAccountCcyAmt, cfOut.ForexUnlinkedAccountCcyAmt);
                if (cfOut.ForexUnlinkedAccountCcyAmt != 0)
                    // if Outflow is not closed, then go to previous Inflow
                    iIn--;
            }
            else
            {
                fsl.AccountCcyAmt = Math.Min(cfIn.ForexUnlinkedAccountCcyAmt, -cfOut.ForexUnlinkedAccountCcyAmt);
                if (cfOut.ForexUnlinkedAccountCcyAmt != 0)
                    // if Outflow is not closed, then go to next Inflow
                    iIn++;
            }
            fsl.Save();
            return fsl;
        }
        #endregion


        private SingleChoiceAction runProgramAction;
    }
}
