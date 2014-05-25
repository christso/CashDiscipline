using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.Validation;
using D2NXAF.ExpressApp.PivotGridLayout;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Validation;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;

namespace CTMS.Module
{
    public sealed partial class CTMSModule : ModuleBase
    {
        public CTMSModule()
        {
            InitializeComponent();
        }

        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
        {
            ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
            return new ModuleUpdater[] { updater };
        }
        public override void Setup(ApplicationModulesManager moduleManager)
        {
            base.Setup(moduleManager);

            // register my code rules
            ValidationRulesRegistrator.RegisterRule(moduleManager, typeof(GenLedgerRule), typeof(IRuleBaseProperties));
            ValidationRulesRegistrator.RegisterRule(moduleManager, typeof(ForexTradeRule), typeof(IRuleBaseProperties));

        }
        public override void Setup(XafApplication application)
        {
            base.Setup(application);

            // PivotGridSavedLayout
            PivotGridSavedLayout.ReservedLayoutNames.Add(Constants.CashFlowPivotLayoutMonthlyVariance, typeof(CashFlow).Name);
            PivotGridSavedLayout.ReservedLayoutNames.Add(Constants.CashFlowPivotLayoutDaily, typeof(CashFlow).Name);
            PivotGridSavedLayout.ReservedLayoutNames.Add(Constants.CashFlowPivotLayoutFixForecast, typeof(CashFlow).Name);
            PivotGridSavedLayout.ReservedLayoutNames.Add(Constants.CashFlowPivotLayoutWeekly, typeof(CashFlow).Name);
            PivotGridSavedLayout.ReservedLayoutNames.Add(Constants.CashFlowPivotLayoutMonthly, typeof(CashFlow).Name);
        }

        #region Application Model Nodes
        public override void AddGeneratorUpdaters(ModelNodesGeneratorUpdaters updaters)
        {
            base.AddGeneratorUpdaters(updaters);
            updaters.Add(new ActionContainerMappingNodesUpdater());
        }
        public class ActionContainerMappingNodesUpdater : ModelNodesGeneratorUpdater<ModelActionContainersGenerator>
        {
            public override void UpdateNode(ModelNode node)
            {
                var mainNode = node as IModelActionToContainerMapping;
                if (mainNode != null)
                {
                    if (mainNode["DetailViewActions"] == null)
                    {
                        var childNode = mainNode.AddNode<IModelActionContainer>();
                        childNode.Id = "DetailViewActions";
                    }
                }
            }
        }
        #endregion

    }

}
