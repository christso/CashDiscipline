﻿using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
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

            #region Process Cube
            ActionPortalList.Add(new ActionPortalItem()
            {
                ActionName = "Actions",
                ActionDescription = "Cash Report - Process Cube - Current",
                ExecutableAction = (args) =>
                {
                    logic.ExecuteChoiceActionByCaptionPath(args,
                        typeof(CashFlow),
                        typeof(CashFlowViewController),
                        CashFlowViewController.ActionId,
                        CashFlowViewController.processCubeCaption
                            + "/" + CashFlowViewController.processCubeCurrentCaption);
                }
            });
            #endregion

            #region Import Forex Rates
            ActionPortalList.Add(new ActionPortalItem()
            {
                ActionName = "Import Forex Rates",
                ActionDescription = "Import Foreign Exchange Rates",
                ExecutableAction = (args) =>
                {
                    var nav = new NavigationHelper();
                    logic.OpenNavigationItem(args, typeof(ImportForexRatesParam));
                }
            });
            #endregion

        }
    }
}