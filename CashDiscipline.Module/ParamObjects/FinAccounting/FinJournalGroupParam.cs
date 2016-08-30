using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;

namespace CashDiscipline.Module.ParamObjects.FinAccounting
{
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.None)]
    public class FinJournalGroupParam : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public FinJournalGroupParam(Session session)
            : base(session)
        {
            session.LockingOption = LockingOption.None;
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }
        private bool _Include;

        public bool Include
        {
            get
            {
                return _Include;
            }
            set
            {
                SetPropertyValue("Include", ref _Include, value);
            }
        }

        private FinJournalGroup _JournalGroup;

        [ModelDefault("AllowEdit", "False")]
        public FinJournalGroup JournalGroup
        {
            get
            {
                return _JournalGroup;
            }
            set
            {
                SetPropertyValue("JournalGroup", ref _JournalGroup, value);
            }
        }

        private FinGenJournalParam _GenJournalParam;

        [Association("FinGenJournalParam-FinJournalGroupParams")]
        public FinGenJournalParam GenJournalParam
        {
            get
            {
                return _GenJournalParam;
            }
            set
            {
                SetPropertyValue("GenJournalParam", ref _GenJournalParam, value);
            }
        }

        public static void SyncParamObjects(Session session, FinGenJournalParam genJnlParam)
        {
            // get journal groups that already exist in parameters
            var jnlGroupParams = genJnlParam.JournalGroupParams;
            var jnlGroupsInParams = jnlGroupParams.Select(p => p.JournalGroup);
            
            // get journal groups
            var jnlGroupsColl = session.GetObjects(session.GetClassInfo(typeof(FinJournalGroup)),
                null, new SortingCollection(null), 0, false, true);
            var jnlGroups = jnlGroupsColl.Cast<FinJournalGroup>().ToList();

            #region Add to Params
            // get journal groups that don't exist in parameters
            var jnlGroupsToAdd = jnlGroups.Where(jg => !jnlGroupsInParams.Contains(jg));

            // add journal groups to parameters
            foreach (FinJournalGroup jnlGroup in jnlGroupsToAdd)
            {
                var jnlGroupParam = new FinJournalGroupParam(session);
                jnlGroupParam.GenJournalParam = genJnlParam;
                jnlGroupParam.JournalGroup = jnlGroup;
                jnlGroupParam.Save();
            }
            #endregion

            #region Delete from Params
            // get parameters that don't exist in journal groups
            var jnlGroupParamsToDelete = jnlGroupParams.Where(p => !jnlGroups.Contains(p.JournalGroup));

            while (jnlGroupParamsToDelete.Any())
            {
                var jgp = jnlGroupParamsToDelete.First();
                jgp.Delete();
                jgp.Save();
            }
            #endregion

            genJnlParam.Save();
            session.CommitTransaction();
        }

        public static void SyncParamObjects(Session session)
        {
            var genJnlParam = session.FindObject<FinGenJournalParam>(null);
            if (genJnlParam == null)
            {
                genJnlParam = new FinGenJournalParam(session);
            }
            SyncParamObjects(session, genJnlParam);
        }

        public static void SyncParamObjects(IObjectSpace objSpace)
        {
            SyncParamObjects(((XPObjectSpace)objSpace).Session);
        }

    }
}
