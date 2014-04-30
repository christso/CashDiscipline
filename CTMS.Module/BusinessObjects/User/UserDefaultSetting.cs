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
using CTMS.Module.HelperClasses;

namespace CTMS.Module.BusinessObjects.User
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class UserDefaultSetting : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public UserDefaultSetting(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        private SecuritySystemUser _AppUser;
        private UserViewLayout _UserViewLayout;

        [RuleUniqueValue("UserDefaultSetting_AppUser_RuleUniqueValue", DefaultContexts.Save)]
        public SecuritySystemUser AppUser
        {
            get
            {
                return _AppUser;
            }
            set
            {
                SetPropertyValue("AppUser", ref _AppUser, value);
            }
        }

        public UserViewLayout UserViewLayout
        {
            get
            {
                return _UserViewLayout;
            }
            set
            {
                SetPropertyValue("UserViewLayout", ref _UserViewLayout, value);
            }
        }

        public static UserDefaultSetting GetUserDefaultSettings(IObjectSpace objSpace)
        {
            var currentUser = StaticHelpers.GetCurrentUser(objSpace);
            var settings = objSpace.FindObject<UserDefaultSetting>(
                CriteriaOperator.Parse("AppUser = ?", currentUser));
            if (settings == null)
            {
                settings = objSpace.CreateObject<UserDefaultSetting>();
                settings.AppUser = currentUser;
                settings.Save();
                objSpace.CommitChanges();
            }
            return settings;
        }
    }
}
