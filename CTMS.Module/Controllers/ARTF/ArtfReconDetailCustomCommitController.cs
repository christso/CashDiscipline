// Prevent the object from being saved, and use custom save behaviour instead
// http://www.devexpress.com/Support/Center/Question/Details/Q423413
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects.Artf;
using D2NXAF.ExpressApp.SystemModule;

namespace CTMS.Module.Controllers.Artf
{
    public class ArtfReconDetailCustomCommitController : ViewController
    {
        List<ArtfRecon> _detailObjectsCache = new List<ArtfRecon>();
        List<string> _warningMessageList = new List<string>();

        public ArtfReconDetailCustomCommitController()
        {
            // TEMPLATE: Specify context for this controller
            TargetObjectType = typeof(ArtfRecon);
            TargetViewType = ViewType.DetailView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.CustomCommitChanges += ObjectSpace_CustomCommitChanges;
            View.ObjectSpace.Committing += ObjectSpace_Committing;
            View.ObjectSpace.Committed += ObjectSpace_Committed;
            View.ObjectSpace.Reloaded += ObjectSpace_Reloaded;
        }
        protected override void OnDeactivated()
        {
            View.ObjectSpace.CustomCommitChanges -= ObjectSpace_CustomCommitChanges;
            View.ObjectSpace.Committing -= ObjectSpace_Committing;
            View.ObjectSpace.Committed -= ObjectSpace_Committed;
            View.ObjectSpace.Reloaded -= ObjectSpace_Reloaded;
            base.OnDeactivated();
        }
        void ObjectSpace_Reloaded(object sender, EventArgs e)
        {
            _detailObjectsCache.Clear();
        }

        void ObjectSpace_Committed(object sender, EventArgs e)
        {
            _detailObjectsCache.Clear();
        }

        void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IObjectSpace os = (IObjectSpace)sender;
            for (int i = os.ModifiedObjects.Count - 1; i >= 0; i--)
            {
                object item = os.ModifiedObjects[i];
                if (typeof(ArtfRecon).IsAssignableFrom(item.GetType()))
                {
                    _detailObjectsCache.Add(item as ArtfRecon);
                    os.RemoveFromModifiedObjects(item);
                }
            }
        }
        void ObjectSpace_CustomCommitChanges(object sender, System.ComponentModel.HandledEventArgs e)
        {
            // TEMPLATE: Implement a custom saving for your Detail objects here.
            bool isUsual = true;
            List<ArtfRecon> objectsToSave = new List<ArtfRecon>();
            foreach (ArtfRecon detailObject in _detailObjectsCache)
            {
                string message;
                if (!CheckArtfReconObject(detailObject, out message))
                {
                    isUsual = false;
                    _warningMessageList.Add(message);
                }
            }
            if (!isUsual)
            {
                // TEMPLATE: display warning message here
                var messageBox = new GenericMessageBox("Warning: " + string.Join( "\r\n", _warningMessageList.ToArray<string>()), messageBox_Accept);
            }

            // save detail objects
            foreach (ArtfRecon detailObject in _detailObjectsCache)
            {
                detailObject.Save();
            }

        }

        void AppendWarningMessage(string message)
        {

        }

        void messageBox_Accept(object sender, ShowViewParameters e)
        {
            foreach (ArtfRecon detailObject in _detailObjectsCache)
            {
                detailObject.Save();
            }
        }

        bool CheckArtfReconObject(ArtfRecon detailObject, out string message)
        {
            // TEMPLATE: implement your validation logic here
            message = "";
            return true;
        }
    }
}
