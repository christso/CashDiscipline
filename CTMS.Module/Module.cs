using System;
using System.Collections.Generic;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Validation;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Model;
using CTMS.Module.Model;
using System.Diagnostics;
using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Xpo.Metadata;
using CTMS.Module.Validation;

namespace CTMS.Module
{
    public sealed partial class CTMSModule : ModuleBase
    {
        public CTMSModule()
        {
            InitializeComponent();
        }
        //const string GeneratedEntityName = "ArtfGlJournal";

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
            ValidationRulesRegistrator.RegisterRule(moduleManager, typeof(LayoutReservedNameRule), typeof(IRuleBaseProperties));
            //ValidationRulesRegistrator.RegisterRule(modulesManager, ..., ...);
            //registrator.RegisterRule(typeof(CTMS.Module.Validation.ReconLedgerRule), typeof(IRuleBaseProperties));
        }
        public override void Setup(XafApplication application)
        {
            base.Setup(application);
            //application.SettingUp += application_SettingUp;
            application.CustomProcessShortcut += application_CustomProcessShortcut;
            //XafTypesInfo.Instance.RegisterEntity("ArtfGlJournal", typeof(CTMS.Module.BusinessObjects.Artf.ArtfGlJournal));
        }

        //void application_SettingUp(object sender, SetupEventArgs e)
        //{
        //    DevExpress.Xpo.Metadata.XPDictionary xpDictionary = DevExpress.ExpressApp.Xpo.XpoTypesInfoHelper.GetXpoTypeInfoSource().XPDictionary;
        //    XPClassInfo generatedDcEntity = xpDictionary.GetClassInfo(GetType().Assembly.FullName, "DevExpress.ExpressApp.DC.GeneratedClasses." + GeneratedEntityName);
        //    generatedDcEntity.AddAttribute(new PersistentAttribute("artf.GlJournal"));
        //    XafTypesInfo.Instance.RefreshInfo(generatedDcEntity.ClassType);
        //}

        //public override void CustomizeTypesInfo(ITypesInfo typesInfo)
        //{
        //    base.CustomizeTypesInfo(typesInfo);
        //    typesInfo.RegisterEntity(GeneratedEntityName, typeof(CTMS.Module.BusinessObjects.Artf.ArtfGlJournal));
        //}

        #region AutoCreatableObject
        void application_CustomProcessShortcut(object sender, CustomProcessShortcutEventArgs e)
        {
            if (e.Shortcut != null && !string.IsNullOrEmpty(e.Shortcut.ViewId))
            {
                IModelView modelView = Application.FindModelView(e.Shortcut.ViewId);
                if (modelView is IModelObjectView)
                {
                    ITypeInfo typeInfo = ((IModelObjectView)modelView).ModelClass.TypeInfo;
                    AutoCreatableObjectAttribute attribute = typeInfo.FindAttribute<AutoCreatableObjectAttribute>(true);
                    if (attribute != null && attribute.AutoCreatable)
                    {
                        // create new instance of object of it is marked as AutoCreatable
                        IObjectSpace objSpace = Application.CreateObjectSpace();
                        object obj;
                        if (typeof(XPBaseObject).IsAssignableFrom(typeInfo.Type) ||
                            (typeInfo.IsInterface && typeInfo.IsDomainComponent))
                        {
                            obj = objSpace.FindObject(typeInfo.Type, null);
                            if (obj == null)
                            {
                                obj = objSpace.CreateObject(typeInfo.Type);
                            }
                        }
                        else
                        {
                            obj = typeof(BaseObject).IsAssignableFrom(typeInfo.Type) ? 
                                objSpace.CreateObject(typeInfo.Type) : Activator.CreateInstance(typeInfo.Type);
                        }
                        DetailView detailView = Application.CreateDetailView(objSpace, obj, true);
                        if (attribute.ViewEditMode == ViewEditMode.Edit)
                        {
                            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
                        }
                        e.View = detailView;
                        e.Handled = true;
                    }
                }
            }
        }
        #endregion

        #region Application Model Properties
        public override void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            base.ExtendModelInterfaces(extenders);
            extenders.Add<IModelLayoutItem, IModelLayoutItemIcon>();
            extenders.Add<IModelMember, IModelIcon>();
        }
        #endregion

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
