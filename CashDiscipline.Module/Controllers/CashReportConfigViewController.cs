using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers
{
    public class CashReportConfigViewController : ViewController
    {
        public CashReportConfigViewController()
        {
            TargetObjectType = typeof(CashReportConfig);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            this.ObjectSpace.Committing += ObjectSpace_Committing;
        }

        private void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var objSpace = (XPObjectSpace)View.ObjectSpace;
            var generator = new AccountBalanceGenerator(objSpace);
            var config = (CashReportConfig)View.CurrentObject;
            var acctbalparam = AccountBalanceParam.GetInstance(objSpace);

            acctbalparam.FromDate = config.StartDate.AddDays(-1);
            acctbalparam.ToDate = acctbalparam.FromDate;
            generator.Generate(acctbalparam);
            
        }
    }
}
