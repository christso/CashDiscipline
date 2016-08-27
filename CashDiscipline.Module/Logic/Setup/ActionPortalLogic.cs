using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Setup
{
    public delegate void ExecutablePortalAction(XafApplication app, ShowViewParameters svp);

    public class ActionPortalLogic
    {
        private static List<ActionPortalItem> _ActionPortalList;

        public static List<ActionPortalItem> ActionPortalList
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

        public static void Execute(XafApplication app, ShowViewParameters svp, string actionName)
        {
            var act = ActionPortalList.Where(x => x.ActionName == actionName).FirstOrDefault();
            if (act == null)
                throw new UserFriendlyException(string.Format("Unrecognized action '{0}'", actionName));
            act.ExecutableAction(app, svp);
        }

        private static void Populate()
        {
            if (_ActionPortalList == null)
                throw new InvalidOperationException("ActionPortalList cannot be null");

            ActionPortalItem item = null;

            item = new ActionPortalItem();
            item.ActionName = CashFlowViewController.fixForecastFormCaption;
            item.ExecutableAction = CashFlowViewController.ShowFixForecastForm;
            _ActionPortalList.Add(item);

            item = new ActionPortalItem();
            item.ActionName = CashFlowViewController.processCubeCurrentCaption;
            item.ExecutableAction = (app, svp) =>
            {
                var tabular = new CashFlowTabular();
                tabular.ProcessCurrent();
            };

            item = new ActionPortalItem();
            item.ActionName = "Import Forex Rates";

            _ActionPortalList.Add(item);
        }

    }
}
