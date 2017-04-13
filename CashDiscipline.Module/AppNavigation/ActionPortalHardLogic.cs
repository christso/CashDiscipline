using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.DatabaseUpdate;
using CashDiscipline.Module.Logic.FinAccounting;
using CashDiscipline.Module.ParamObjects.Cash;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.AppNavigation
{
    public class ActionPortalHardLogic
    {
        public ActionPortalHardLogic(ActionPortalLogic logic)
        {
            this.logic = logic;
        }

        private List<ActionPortalItem> _ActionPortalList;

        public List<ActionPortalItem> ActionPortalList
        {
            get
            {
                if (_ActionPortalList == null)
                {
                    _ActionPortalList = new List<ActionPortalItem>();
                    Populate();
                }
                return _ActionPortalList;
            }
        }

        public void ExecutePortalAction(ActionPortalEventArgs args, string actionName)
        {
            var actionPortal = ActionPortalList.Where(x => x.ActionName == actionName).FirstOrDefault();
            if (actionPortal == null)
                throw new UserFriendlyException(string.Format("Unrecognized action '{0}'", actionName));
            if (actionPortal.ExecutableAction == null)
                throw new UserFriendlyException(string.Format("Action {0} is not implemented.", actionName));

            actionPortal.ExecutableAction(args);
        }

        private ActionPortalLogic logic;

        private void Populate()
        {
            if (ActionPortalList == null)
                throw new InvalidOperationException("ActionPortalList cannot be null");

            #region Fix Forecast
            ActionPortalList.Add(new ActionPortalItem()
            {
                ActionName = CashFlowViewController.fixForecastFormCaption,
                ActionDescription = "Cash Flow Fix Forecast",
                ExecutableAction = (args) =>
                {
                    var nav = new NavigationHelper();
                    logic.OpenNavigationItem(args, typeof(CashFlowFixParam));
                }
            });
            #endregion

            #region Purge
            ActionPortalList.Add(new ActionPortalItem()
            {
                ActionName = "Purge",
                ActionDescription = "Purge Deleted Objects",
                ExecutableAction = (args) =>
                {
                    var objSpace = (XPObjectSpace)args.ObjectSpace;
                    objSpace.Session.PurgeDeletedObjects();
                    DbUtils.ExecuteNonQueryCommand(objSpace, "cashdisc_purge_objects", false);
                }
            });

            #endregion

            //#region Process Cube
            //ActionPortalList.Add(new ActionPortalItem()
            //{
            //    ActionName = "Actions",
            //    ActionDescription = "Cash Report - Process Cube - Current",
            //    ExecutableAction = (args) =>
            //    {
            //        logic.ExecuteChoiceActionByCaptionPath(args,
            //            typeof(CashFlow),
            //            typeof(CashFlowViewController),
            //            CashFlowViewController.ActionId,
            //            CashFlowViewController.processCubeCaption
            //                + "/" + CashFlowViewController.processCubeCurrentCaption);
            //    }
            //});
            //#endregion

            //#region Import Forex Rates
            //ActionPortalList.Add(new ActionPortalItem()
            //{
            //    ActionName = "Import Forex Rates",
            //    ActionDescription = "Import Foreign Exchange Rates",
            //    ExecutableAction = (args) =>
            //    {
            //        var nav = new NavigationHelper();
            //        logic.OpenNavigationItem(args, typeof(ImportForexRatesParam));
            //    }
            //});
            //#endregion

            //#region SQL Test
            //ActionPortalList.Add(new ActionPortalItem()
            //{
            //    ActionName = "GenLedger SQL Test",
            //    ActionDescription = "GenLedger SQL Test",
            //    ExecutableAction = (args) =>
            //    {
            //        var objSpace = args.ObjectSpace;
            //        var paramObj = FinGenJournalParam.GetInstance(objSpace);
            //        var sqlJnlr = new GenLedgerUnpostedCreator((XPObjectSpace)objSpace, paramObj);
            //        sqlJnlr.Process();
            //    }
            //});
            //#endregion
        }
    }
}