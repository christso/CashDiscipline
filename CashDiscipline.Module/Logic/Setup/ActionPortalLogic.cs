using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp;
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
            var act = _ActionPortalList.Where(x => x.ActionName == actionName).FirstOrDefault();
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
            item.ActionName = CashFlowViewController.processCubeRecentCaption;
            item.ExecutableAction = (app, svp) =>
            {
                CashFlowViewController.ProcessCube_Recent();
            };
            _ActionPortalList.Add(item);
        }
    }
}
