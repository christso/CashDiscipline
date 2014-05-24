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
using DevExpress.ExpressApp.Security.Strategy;

using GenerateUserFriendlyId.Module.BusinessObjects;
using DevExpress.ExpressApp.Actions;
using System.Threading;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class ActionRequest : UserFriendlyIdPersistentObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ActionRequest(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code (check out http://documentation.devexpress.com/#Xaf/CustomDocument2834 for more details).

            //// Cannot get currentUser on a separate thread in ASP.NET
            //SecuritySystemUser currentUser = GlobalHelper.GetCurrentUser(Session);
            //if (currentUser != null)
            //    this.Requestor = currentUser;

            RequestDate = DateTime.Now;
        }

        [PersistentAlias("concat('RQ', ToStr(SequentialNumber))")]
        public string RequestId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("RequestId"));
            }
        }

        // Fields...
        [VisibleInLookupListView(false)]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        public static Dictionary<Guid, CancellationTokenSource> CancellationTokenSources = new Dictionary<Guid, CancellationTokenSource>();

        public void SetCancellationTokenSource(CancellationTokenSource cts)
        {
            if (!CancellationTokenSources.ContainsKey(Oid))
                CancellationTokenSources.Add(Oid, cts);
            else
                CancellationTokenSources[Oid] = cts;
        }

        public bool CancelRequest()
        {
            if (!CancellationTokenSources.ContainsKey(Oid)) return false;
            CancellationTokenSources[Oid].Cancel();
            CancellationTokenSources[Oid].Dispose();
            CancellationTokenSources.Remove(Oid);
            return true;
        }

        private string _RequestLog;
        private string _RequestName;
        private RequestStatus _RequestStatus;
        private DateTime _RequestDate;
        SecuritySystemUser _Requestor;

        public DateTime RequestDate
        {
            get
            {
                return _RequestDate;
            }
            set
            {
                SetPropertyValue("RequestDate", ref _RequestDate, value);
            }
        }
        public string RequestName
        {
            get
            {
                return _RequestName;
            }
            set
            {
                SetPropertyValue("RequestName", ref _RequestName, value);
            }
        }
        public SecuritySystemUser Requestor
        {
            get
            {
                return _Requestor;
            }
            set
            {
                SetPropertyValue("Requestor", ref _Requestor, value);
            }
        }

        public RequestStatus RequestStatus
        {
            get
            {
                return _RequestStatus;
            }
            set
            {
                SetPropertyValue("RequestStatus", ref _RequestStatus, value);
            }
        }

        [Size(SizeAttribute.Unlimited)]
        public string RequestLog
        {
            get
            {
                return _RequestLog;
            }
            set
            {
                SetPropertyValue("RequestLog", ref _RequestLog, value);
            }
        }
    }
    public enum RequestStatus
    {
        Waiting,
        Processing,
        Complete,
        Error,
        Cancelled
    }
}
