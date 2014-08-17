using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CTMS.Module.BusinessObjects.Artf;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.XtraGrid;

namespace CTMS.Module.Controllers.ReconMaster
{
    public class ReconMasterDetailViewController : ViewController
    {   
        public ReconMasterDetailViewController()
        {
            this.TargetObjectType = typeof(CTMS.Module.BusinessObjects.Artf.IReconMaster);
            this.TargetViewType = DevExpress.ExpressApp.ViewType.DetailView;
            this.TypeOfView = typeof(DevExpress.ExpressApp.DetailView);
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            var editors = GetArtfReconPropertyEditors();
            foreach (ListPropertyEditor editor in editors)
            {
                editor.ControlCreated += reconListPropertyEditor_ControlCreated;
            }
        }
        private void reconListPropertyEditor_ControlCreated(Object sender, EventArgs e)
        {
            ListPropertyEditor reconsListPropertyEditor = (ListPropertyEditor)sender;
            NewObjectViewController reconNewObjectViewController = reconsListPropertyEditor.Frame.GetController<NewObjectViewController>();
            ListViewProcessCurrentObjectController listViewProcessCurrentObjectController = reconsListPropertyEditor.Frame.GetController<ListViewProcessCurrentObjectController>();
            reconNewObjectViewController.NewObjectAction.Executing += NewObjectViewController_NewObjectAction_Executing;
            listViewProcessCurrentObjectController.CustomProcessSelectedItem += ListViewProcessCurrentObjectController_CustomProcessSelectedItem;
        }
        private void NewObjectViewController_NewObjectAction_Executing(Object sender, CancelEventArgs e)
        {
            ObjectSpace.CommitChanges();
        }
        private void ListViewProcessCurrentObjectController_CustomProcessSelectedItem(Object sender, CustomProcessListViewSelectedItemEventArgs e)
        {
            ObjectSpace.CommitChanges();
        }
        private List<ListPropertyEditor> GetArtfReconPropertyEditors()
        {
            DetailView detailView = (DetailView)View;
            var editorIds = new List<string>() { "ArtfRecon", "ArtfFromRecon", "ArtfToRecon" };
            var editors = new List<ListPropertyEditor>();
            foreach (string editorId in editorIds)
            {
                var editor = (ListPropertyEditor)detailView.FindItem(editorId);
                if (editor != null)
                    editors.Add(editor);
            }
            return editors;
        }

    }
}

