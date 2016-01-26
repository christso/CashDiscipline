using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.BusinessObjects.Setup;
using CTMS.Module.ParamObjects.Cash;
using Xafology.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xafology.Spreadsheet.Attributes;
using DevExpress.Persistent.BaseImpl;

namespace CTMS.Module.BusinessObjects
{
    public class TestObject : BaseObject
    {
        public TestObject(Session session)
            : base(session)
        {
            
        }

        private int _sequentialNumber;

        public int SequentialNumber
        {
            get 
            {
                return _sequentialNumber; 
            }
            set 
            {
                SetPropertyValue("SequentialNumber", ref _sequentialNumber, value); 
            }
        }

        protected override void OnSaving()
        {
            var tmpSequentialNumber = GenerateDistributedId(Session, this);
            if (tmpSequentialNumber >= 0)
            {
                SequentialNumber = tmpSequentialNumber;
            }
            base.OnSaving();
        }

        private static int GenerateDistributedId(Session session, object obj)
        {
            if (!(session is NestedUnitOfWork) && session.IsNewObject(obj))
            {
                int nextSequence = DistributedIdGeneratorHelper.Generate(session.DataLayer, obj.GetType().FullName, string.Empty);
                return nextSequence;
            }
            return -1;
        }

        private decimal _AccountCcyAmt;

        public decimal AccountCcyAmt
        {
            get
            {
                return _AccountCcyAmt;
            }
            set
            {
                SetPropertyValue("AccountCcyAmt", ref _AccountCcyAmt, Math.Round(value, 2));
            }
        }

    }
}
