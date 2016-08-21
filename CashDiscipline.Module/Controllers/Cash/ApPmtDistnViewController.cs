using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ApPmtDistnViewController : ViewController
    {
        private const string mapCaption = "Map Selected";
        private const string importCaption = "Import";

        public ApPmtDistnViewController()
        {
            TargetObjectType = typeof(ApPmtDistn);

            var mainAction = new SingleChoiceAction(this, "RunApPmtDistnAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            mainAction.Caption = "Actions";
            mainAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            mainAction.ShowItemsOnClick = true;
            mainAction.Execute += MainAction_Execute;

            var importChoice = new ChoiceActionItem();
            importChoice.Caption = importCaption;
            mainAction.Items.Add(importChoice);

            var mappingChoice = new ChoiceActionItem();
            mappingChoice.Caption = mapCaption;
            mainAction.Items.Add(mappingChoice);

        }

        private void MainAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case mapCaption:
                    MapSelected();
                    break;
                case importCaption:
                    ShowImportForm(e.ShowViewParameters);
                    break;
            }
        }

        private void ShowImportForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportApPmtDistnParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        private void MapSelected()
        {
            var mapper = new ApPmtDistnMapper((XPObjectSpace)ObjectSpace);
            var objs = View.SelectedObjects;
            mapper.Process(objs);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.CustomDeleteObjects += ObjectSpace_CustomDeleteObjects;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.CustomDeleteObjects -= ObjectSpace_CustomDeleteObjects;
            base.OnDeactivated();
        }
        private void ObjectSpace_CustomDeleteObjects(object sender, CustomDeleteObjectsEventArgs e)
        {
            var deleter = new BatchDeleter(ObjectSpace);
            deleter.Delete(e.Objects);
        }
    }
}
