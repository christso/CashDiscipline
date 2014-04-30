using System;
using System.ComponentModel;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using GenerateUserFriendlyId.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.HelperClasses;
using System.Collections.Generic;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.ExpressApp.Model;

namespace CTMS.Module.BusinessObjects.Artf
{
    public class ArtfTask : UserFriendlyIdPersistentObject
    {
        private TaskStatusEnum _Status;

        public const string c3Sub = CTMS.Module.Constants.c3Sub;
        public const string cPaging = CTMS.Module.Constants.cPaging;
        public const string cVSub = CTMS.Module.Constants.cVSub;
        public const string cSundryDebtor = CTMS.Module.Constants.cSundryDebtor;
        public const string c3Gateway = CTMS.Module.Constants.c3Gateway;
        public const string cTransferClearing = CTMS.Module.Constants.cTransferClearing;
        public const string cVDebtors = CTMS.Module.Constants.cVDebtors;
        public const string cAnzOp = CTMS.Module.Constants.cAnzOp;
        
        public ArtfTask(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false:
            // if (!IsLoading){
            //    It is now OK to place your initialization code here.
            // }
            // or as an alternative, move your initialization code into the AfterConstruction method.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
            
        }

        [PersistentAlias("concat('T', ToStr(SequentialNumber))")]
        public string TaskId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("TaskId"));
            }
        }

        ArtfRecon _ArtfRecon;
        [Association(@"ArtfRecon-ArtfTasks")]
        [ModelDefault("AllowEdit", "False")]
        public ArtfRecon ArtfRecon
        {
            get { return _ArtfRecon; }
            set 
            {
                SetPropertyValue<ArtfRecon>("ArtfRecon", ref _ArtfRecon, value);
            }
        }

        private string _Comments;
        [Size(SizeAttribute.Unlimited)]
        public string Comments
        {
            get
            {
                return _Comments;
            }
            set
            {
                SetPropertyValue("Comments", ref _Comments, value);
            }
        }

        public TaskStatusEnum Status
        {
            get
            {
                return _Status;
            }
            set
            {
                SetPropertyValue("Status", ref _Status, value);
            }
        }

        [NonPersistent]
        public TaskTypeEnum TaskType
        {
            get
            {
                if (this is ArtfActivityReclassTask) return TaskTypeEnum.Activity;
                if (this is ArtfFundsTransferTask) return TaskTypeEnum.Transfer;
                if (this is ArtfGlJournalTask) return TaskTypeEnum.GenJournal;
                if (this is ArtfLedgerTask) return TaskTypeEnum.Ledger;
                if (this is ArtfReceiptTask) return TaskTypeEnum.Receipt;
                return TaskTypeEnum.General;
            }
        }

        SecuritySystemUser _AssignedTo;
        public SecuritySystemUser AssignedTo
        {
            get
            {
                return _AssignedTo;
            }
            set
            {
                SetPropertyValue("AssignedTo", ref _AssignedTo, value);
            }
        }

        public virtual void AssignDefaultTaskOwner()
        {
            AssignDefaultTaskOwner(TaskTypeEnum.General);
        }

        public void AssignDefaultTaskOwner(TaskTypeEnum taskType)
        {
            var task = this;
            var ownerMap = Session.FindObject<ArtfTaskOwnerDefaults>(new OperandProperty("TaskType") == new OperandValue(taskType));
            if (ownerMap != null)
                task.AssignedTo = ownerMap.AssignedTo;
        }
        protected override void OnSaving()
        {
            base.OnSaving();
            AssignDefaultTaskOwner();
        }
    }

    public enum TaskTypeEnum
    {
        Activity,
        Transfer,
        [DevExpress.ExpressApp.DC.XafDisplayName("Gen Journal")]
        GenJournal,
        Ledger,
        Receipt,
        General
    }

    public enum TaskStatusEnum
    {
        Ready = 0,
        InProgress = 1,
        Complete = 2,
        Invalid = 3
    }

}
