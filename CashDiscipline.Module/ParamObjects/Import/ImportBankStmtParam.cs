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
    [NavigationItem("Parameters")]
    public class ImportBankStmtParam : BaseObject
    {
        public ImportBankStmtParam(Session session) : base(session)
        {
            session.LockingOption = LockingOption.None;
        }

        // Master			
        [Association("ImportBankStmtParam-ImportBankStmtParamItems"), DevExpress.Xpo.Aggregated]
        public XPCollection<ImportBankStmtParamItem> ImportBankStmtParamItems
        {
            get { return GetCollection<ImportBankStmtParamItem>("ImportBankStmtParamItems"); }
        }

        public static ImportBankStmtParam GetInstance(IObjectSpace objectSpace)
        {
            return BaseObjectHelper.GetInstance<ImportBankStmtParam>(objectSpace);
        }
    }
}