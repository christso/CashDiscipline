using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Validation;

namespace CTMS.Module.ParamObjects.Artf
{
    [NonPersistent]
    [D2NXAF.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    public class ArtfCreatePaymentsParam
    {
        private DateTime _ValueDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [RuleRequiredField("RuleRequiredField for ArtfCreatePaymentsParam",
        Constants.AcceptActionContext, "Value Date cannot be empty")]
        public DateTime ValueDate
        {
            get { return _ValueDate; }
            set { _ValueDate = value; }
        }

        [Size(30)]
        public string PaymentBatchName { get; set; }
    }
}
