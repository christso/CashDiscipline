using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers
{
    public class ShowNavigationItemControllerEx : DevExpress.ExpressApp.SystemModule.ShowNavigationItemController
    {
        protected override void InitializeItems()
        {
            base.InitializeItems();
            SetOfBooks.GetInstance(Application);
        }

    }
}
