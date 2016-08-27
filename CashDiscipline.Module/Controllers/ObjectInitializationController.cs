using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers
{
    public class ObjectInitializationController : DevExpress.ExpressApp.SystemModule.ShowNavigationItemController
    {
        protected override void InitializeItems()
        {
            base.InitializeItems();
            SetOfBooks.GetInstance(Application);
        }

    }
}
