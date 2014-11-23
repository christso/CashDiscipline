using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Editors;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security.Strategy;
using CTMS.Module.BusinessObjects.Artf;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects;
using CTMS.Module.ParamObjects.Artf;
using DevExpress.Data.Filtering;
using Xafology.ExpressApp.SystemModule;

namespace CTMS.Module.Controllers.Artf
{
    public partial class ArtfReconCreateTaskViewController : ViewController
    {
        private DevExpress.ExpressApp.Actions.SingleChoiceAction tasksAction;

        private class TaskCaptions
        {
            public const string Tasks = "Tasks";
            public const string Create = "Create";
            public const string Purge = "Purge";
        }

        public ArtfReconCreateTaskViewController()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.TargetObjectType = typeof(CTMS.Module.BusinessObjects.Artf.ArtfRecon);

            this.tasksAction = new DevExpress.ExpressApp.Actions.SingleChoiceAction(this, "Tasks", "Edit");
            this.tasksAction.Caption = TaskCaptions.Tasks;
            this.tasksAction.ConfirmationMessage = null;
            this.tasksAction.ImageName = null;

            DevExpress.ExpressApp.Actions.ChoiceActionItem createChoiceActionItem = new DevExpress.ExpressApp.Actions.ChoiceActionItem();
            createChoiceActionItem.Caption = TaskCaptions.Create;
            createChoiceActionItem.ImageName = null;
            createChoiceActionItem.Shortcut = null;
            createChoiceActionItem.ToolTip = null;
            this.tasksAction.Items.Add(createChoiceActionItem);

            DevExpress.ExpressApp.Actions.ChoiceActionItem purgeChoiceActionItem = new DevExpress.ExpressApp.Actions.ChoiceActionItem();
            purgeChoiceActionItem.Caption = TaskCaptions.Purge;
            purgeChoiceActionItem.ImageName = null;
            purgeChoiceActionItem.Shortcut = null;
            purgeChoiceActionItem.ToolTip = null;
            this.tasksAction.Items.Add(purgeChoiceActionItem);

            this.tasksAction.ItemType = DevExpress.ExpressApp.Actions.SingleChoiceActionItemType.ItemIsOperation;
            this.tasksAction.Execute += new DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventHandler(this.tasksAction_Execute);
        }

        private void tasksAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            ChoiceActionItem activeItem = e.SelectedChoiceActionItem;

            switch (activeItem.Caption)
            {
                case TaskCaptions.Create:
                    CreateAllTasks();
                    break;
                case TaskCaptions.Purge:
                    DeleteTasks();
                    break;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        void CreateAllTasks()
        {
            Session session = ((XPObjectSpace)ObjectSpace).Session;
            System.Collections.IList reconObjects = View.SelectedObjects;
            if (reconObjects.Count == 0)
            {
                var messageBox = new GenericMessageBox("Error: Please select a reconciliation object first before running 'Create Tasks'.");
            }

            List<ArtfRecon> invalidRecons = new List<ArtfRecon>();
            foreach (ArtfRecon reconObject in reconObjects)
            {
                if (!CreateTasks(session, reconObject))
                    invalidRecons.Add(reconObject);
                reconObject.Save();
            }
            ObjectSpace.CommitChanges(); // needed for ASP.NET, but you don't get confirmation of the created records.

            if (invalidRecons.Count > 0)
            {
                var invalidReconStrings = from r in invalidRecons
                                        select r.ReconId + "; " + r.Amount;

                var messageText = "Validation errors were detected for the recon objects below:\r\n"
                    + string.Join("\r\n-", invalidReconStrings);
                new GenericMessageBox("Warning: " + messageText);
            }
        }

        void DeleteTasks()
        {
            Session session = ((XPObjectSpace)ObjectSpace).Session;
            System.Collections.IList reconObjects = View.SelectedObjects;
            if (reconObjects.Count == 0)
            {
                var messageBox = new GenericMessageBox("Please select a reconciliation object first before running 'Delete Tasks'.", "Error");
            }

            foreach (ArtfRecon reconObject in reconObjects)
            {
                session.Delete(reconObject.ArtfTasks);
            }
            ObjectSpace.CommitChanges(); // needed for ASP.NET, but you don't get confirmation of the created records.
        }

        public bool CreateTasks(Session session, ArtfRecon reconObject)
        {
            if (!ValidateRecon(session, reconObject))
                return false;

            // Get Default Task Map for Customer Transfer Type
            System.Collections.ICollection taskMaps = session.GetObjects(session.GetClassInfo(typeof(ArtfReconTaskMap)),
                                        (CriteriaOperator)(
                                            new BinaryOperator("FromCustomerType", reconObject.BankStmt.Account.ArtfCustomerType)
                                            & new BinaryOperator("ToCustomerType", reconObject.CustomerType)),
                                        new SortingCollection(null), 0, false, true);

            // Default Tasks, Valid Tasks
            if (taskMaps.Count == 0)
            {
                ArtfTask t = new ArtfTask(session) { ArtfRecon = reconObject };
                t.Comments = "Unrecognised Recon";
                t.Save();
                return false;
            }
            else if (taskMaps.Count > 1)
            {
                ArtfTask t = new ArtfTask(session) { ArtfRecon = reconObject };
                t.Comments = "More than one Task Map exists for the 'From Customer Type' and 'To Customer Type' criteria";
                t.Save();
                return false;
            }

            // Apply matching Task Map to Task
            foreach (ArtfReconTaskMap taskMap in taskMaps)
            {
                CreateTasksActivityReclass(session, reconObject, taskMap);
                if (taskMap.IsBankTfr)
                {
                    var t = new ArtfFundsTransferTask(session) { ArtfRecon = reconObject };
                    var tc = new BankTransferTaskCreator(session, reconObject, taskMap, t);
                    tc.Create();
                }
                else
                {
                    var t = new ArtfGlJournalTask(session) { ArtfRecon = reconObject };
                    var tc = new GlJournalTaskCreator(session, reconObject, taskMap, t);
                    tc.Create();
                }

                if (taskMap.Comments != null)
                {
                    var t = new ArtfTask(session) { ArtfRecon = reconObject };
                    t.Comments = taskMap.Comments;
                }
            }
            return true;
        }

        private void CreateTasksActivityReclass(Session session, ArtfRecon reconObject, ArtfReconTaskMap taskMap)
        {
            var t = new ArtfActivityReclassTask(session) { ArtfRecon = reconObject };
            
            t.BankStmtReclassAmount = t.ArtfRecon.Amount;

            // compute activity
            if (t.ArtfRecon.TfrToBankAccount != null)
            {
                var accountActivityMap = session.FindObject<AccountActivityMap>(
                        new OperandProperty("Account") == t.ArtfRecon.TfrToBankAccount
                        & new OperandProperty("Activity.ArtfCustomerType") == t.ArtfRecon.CustomerType);
                t.BankStmtReclassToActivity = accountActivityMap.Activity;
            }
            else
            {
                var activity = session.FindObject<Activity>(
                        new OperandProperty("IsArtfDefault") == new OperandValue(true)
                        & new OperandProperty("ArtfCustomerType") == t.ArtfRecon.CustomerType);
                if (activity != null)
                    t.BankStmtReclassToActivity = activity;
                else
                {
                    activity = session.FindObject<Activity>(
                        new BinaryOperator("ArtfCustomerType", t.ArtfRecon.CustomerType));

                    //activity = session.FindObject<Activity>((CriteriaOperator)(
                    //    Activity.Fields.ArtfCustomerType == t.ArtfRecon.CustomerType));
                    if (activity != null)
                        t.BankStmtReclassToActivity = activity;
                }
            }
            t.AssignDefaultTaskOwner();
            t.Save();
        }

        public bool ValidateRecon(Session session, ArtfRecon reconObject)
        {
            // validate data
            BankStmt bankStmt = reconObject.BankStmt;
            if (bankStmt == null)
            {
                new ArtfTask(session)
                {
                    ArtfRecon = reconObject,
                    Comments = "No bank statement line was linked to Recon"
                };
                return false;
            }
            else if (bankStmt.Account.ArtfCustomerType == null)
            {
                new ArtfTask(session)
                {
                    ArtfRecon = reconObject,
                    Comments = string.Format("Source account '{0}' has no Customer Type"
                        + " defined in table Account.",
                        bankStmt.Account.Name)
                };
                return false;
            }
            else if (reconObject.CustomerType == null)
            {
                new ArtfTask(session)
                {
                    ArtfRecon = reconObject,
                    Comments = "Destination account Customer Type cannot be null."
                };
                return false;
            }
            else if (reconObject == null)
            {
                new ArtfTask(session)
                {
                    ArtfRecon = reconObject,
                    Comments = "Task must have a Recon object defined."
                };
                return false;
            }
            return true;
        }
    }
}
