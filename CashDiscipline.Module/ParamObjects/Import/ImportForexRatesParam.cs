using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CashDiscipline.Module.ParamObjects.Import
{
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultClassOptions]
    public class ImportForexRatesParam : BaseObject
    {
        public ImportForexRatesParam(Session session) : base(session)
        {
            session.LockingOption = LockingOption.None;
        }

        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                SetPropertyValue("FileName", ref _FileName, value);
            }
        }

        public static ImportForexRatesParam GetInstance(IObjectSpace objectSpace)
        {
            return BaseObjectHelper.GetInstance<ImportForexRatesParam>(objectSpace);
        }
    }
}
