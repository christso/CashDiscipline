using CTMS.Module.BusinessObjects.User;
using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.ParamObjects.User
{
    [AutoCreatableObject]
    [NonPersistent]
    public class SaveUserViewLayoutParam
    {
        private UserViewLayout _UserViewLayout;

        [ModelDefault("LookupProperty", "LayoutName")]
        public UserViewLayout Layout
        {
            get { return _UserViewLayout; }
            set { _UserViewLayout = value; }
        }
    }
}
