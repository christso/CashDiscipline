using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Validation;
using Xafology.ExpressApp.PivotGridLayout;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Validation;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;

namespace CashDiscipline.Module
{
    public sealed partial class CashDisciplineModule : ModuleBase
    {
        public CashDisciplineModule()
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

            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.IsSequential = true;
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
