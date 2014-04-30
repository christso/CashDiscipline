// How to remove object's ID from the validation error message
// http://www.devexpress.com/Support/Center/Question/Details/Q376163

using DevExpress.ExpressApp;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers
{
    public class CustomizeValidationMessageController : WindowController
    {
        public CustomizeValidationMessageController()
        {
            TargetWindowType = WindowType.Main;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            Validator.RuleSet.ValidationCompleted += new EventHandler<ValidationCompletedEventArgs>(RuleSet_ValidationCompleted);
        }
        void RuleSet_ValidationCompleted(object sender, ValidationCompletedEventArgs e)
        {
            if (e.Exception != null)
            {
                e.Exception.ObjectHeaderFormat = "";
            }
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Validator.RuleSet.ValidationCompleted -= new EventHandler<ValidationCompletedEventArgs>(RuleSet_ValidationCompleted);
        }
    }
}
